
namespace MagicMessagebus.Implementation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Threading.Tasks;

    using Geevers.Infrastructure;
    using Newtonsoft.Json;
    using Ninject;

    using Contract;

    public class MagicMessagebus : IMagicMessagebus
    {
        internal readonly IMagicMessagebusAssemblyFilter assemblyFilter;
        internal readonly IErrorTracker errorTracker;
        private readonly IKernel ninject;
        private readonly IServiceProvider dotnet;


        public MagicMessagebus(IServiceProvider dotnet, IMagicMessagebusAssemblyFilter assemblyFilter = null, IErrorTracker errorTracker = null)
            : this(assemblyFilter, errorTracker, dotnet) { }

        [Inject]
        public MagicMessagebus(IKernel ninject, [Optional] IMagicMessagebusAssemblyFilter assemblyFilter, [Optional] IErrorTracker errorTracker)
            : this(assemblyFilter, errorTracker, ninject) { }

        public MagicMessagebus(IMagicMessagebusAssemblyFilter assemblyFilter = null, IErrorTracker errorTracker = null, IServiceProvider dotnet = null, IKernel ninject = null)
        {
            this.errorTracker = errorTracker ?? new ExplodingErrorTracker();
            this.assemblyFilter = assemblyFilter ?? new DefaultAssemblyFilter();

            this.ninject = ninject;
            this.dotnet = dotnet;

            if (Map == null)
            {
                lock (key)
                {
                    if (Map == null)
                    {
                        this.CreateMap();
                    }
                }

                this.Publish(new StartupStaticSelftest(), true);
                this.Publish(new StartupInstanceSelftest(), true);
            }
        }

        [Obsolete("Should only be used in tests when you now new assemblies are loaded after MagicMessagebus has been initialized")]
        public void Reset()
        {
            lock (key)
            {
                this.CreateMap();
            }
        }

        private void CreateMap()
        {
            Map = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => false == a.IsDynamic)
                .Where(a => a.FullName.StartsWith("MagicMessagebus,") || this.assemblyFilter.ScanForSubcriptions(a))
                .SelectMany(a =>
                {
                    try
                    {
                        return a.GetTypes();
                    }
                    catch (Exception e)
                    {
                        e.Data.Add("Assembly", a.FullName);
                        e.Data.Add("Version", this.GetType().Assembly.GetName().Version.ToString());

                        this.errorTracker.Track(e);
                    }

                    return new Type[0];
                })
                .Where(t => t.IsClass || t.IsInterface)
                .SelectMany(t => t.GetMethods())
                .Where(m => m.Name == "Subscribe")
                .Where(m => m.IsStatic || m.DeclaringType.IsInterface) // so it works for static methods or interfaces, nothing else
                .Where(m =>
                {
                    var parameters = m.GetParameters();

                    if (parameters.Count() != 1)
                    {
                        return false;
                    }

                    return parameters.Single().ParameterType
                        .GetInterfaces()
                        .Where(i => i.Name == nameof(IMagicMessage))
                        .Any();
                })
                .GroupBy(m => m.GetParameters().Single().ParameterType) // this might not work. you might need the fullname of the parametertype because assemblies may differ
                .ToList();
        }

        private static readonly object key = new object();

        internal static List<IGrouping<Type, MethodInfo>> Map { get; set; } // static because tests fail if mapped twice in one application, which should never be necessary anyway

        void IMagicMessagebus.Publish(IMagicMessage message)
        {
            this.Publish(message, false);
        }

        public static void Publish(IMagicMessage message)
        {
            new MagicMessagebus().Publish(message, false);
        }

        private void Publish(IMagicMessage message, bool selftest)
        {
            var subscriptions = Map
                .Where(m => m.Key.Equals(message.GetType()))
                .SelectMany(g => g)
                .ToList();

            if (subscriptions.Any())
            {
                subscriptions.ForEach(m => this.Invoke(m, message, selftest));
            }
            else
            {
                if (selftest)
                {
                    this.errorTracker.Track(new MagicMessagebusException($"MagicMessagebus is NOT healthy. Selftest failed for message `{message.GetType()}`. Please contact `nuget.org/packages/MagicMessagebus`"));
                }
                else
                {
                    this.errorTracker.Track(new MagicMessagebusException($"No subscriptions found for message `{message.GetType()}`"));
                }
            }
        }

        private void Invoke(MethodInfo method, IMagicMessage message, bool selftest)
        {
            if (method.IsStatic)
            {
                this.Invoke(method, message, null, selftest);
            }
            else
            {
                var service = this.GetService(method);

                if (service == null)
                {
                    return;
                }

                this.Invoke(method, message, service, selftest);
            }
        }

        private void Invoke(MethodInfo method, IMagicMessage message, object service, bool invokeSynchronously)
        {
            if (invokeSynchronously)
            {
                Invoke(method, message, service);
            }
            else
            {
                Task.Run(() =>
                {
                    Invoke(method, message, service);
                });
            }
        }

        private void Invoke(MethodInfo method, IMagicMessage message, object service)
        {
            try
            {
                var result = method.Invoke(service, new object[] { message });

                this.VerifyInitializationTestMessage(message, result);

                if (result is HttpStatusCode)
                {
                    var status = (HttpStatusCode)result;

                    if (false == status.IsSuccessStatusCode())
                    {
                        throw new MagicMessagebusException("A subscriber to the MagicMessagebus returned an unsuccessfull status code")
                        {
                            Data = {
                                    { "Service", service?.GetType().Name },
                                    { "Method", method.Name },
                                    { "Status", $"{(int)status} {status.ToString()}" },
                                    { "Type", message.GetType().Name },
                                    { "Message", JsonConvert.SerializeObject(message, Formatting.Indented) },
                            },
                        };
                    }
                }
            }
            catch (Exception e)
            {
                this.errorTracker.Track(e);
            }
        }

        private void VerifyInitializationTestMessage(IMagicMessage message, object result)
        {
            if (message is StartupStaticSelftest)
            {
                if (false == result is HttpStatusCode)
                {
                    throw new MagicMessagebusException("MagicMessagebus is not working as it should. Startup selftest failed: Static subscription did not return an HttpStatusCode");
                }

                var status = (HttpStatusCode)result;

                if (status != (HttpStatusCode)299)
                {
                    throw new MagicMessagebusException("MagicMessagebus is not working as it should. Startup selftest failed: Static subscription did not return the expected HttpStatusCode");
                }
            }

            if (message is StartupInstanceSelftest)
            {
                if (false == result is HttpStatusCode)
                {
                    throw new MagicMessagebusException("MagicMessagebus is not working as it should. Startup selftest failed: Instance subscription did not return an HttpStatusCode");
                }

                var status = (HttpStatusCode)result;

                if (status != (HttpStatusCode)299)
                {
                    throw new MagicMessagebusException("MagicMessagebus is not working as it should. Startup selftest failed: Instance subscription did not return the expected HttpStatusCode");
                }
            }
        }

        private object GetService(MethodInfo method)
        {
            return this.ninject?.Get(method.DeclaringType)
                ?? this.dotnet?.GetService(method.DeclaringType)
                ?? null;
        }

        public static HttpStatusCode Subscribe(StartupStaticSelftest message)
        {
            return (HttpStatusCode)299;
        }

        public HttpStatusCode Subscribe(StartupInstanceSelftest message)
        {
            return (HttpStatusCode)299;
        }
    }
}