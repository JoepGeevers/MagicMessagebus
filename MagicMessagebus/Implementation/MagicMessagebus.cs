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
        private readonly IErrorTracker errorTracker;
        private readonly IKernel ninject;
        private readonly IServiceProvider dotnet;

        public MagicMessagebus() : this(null, null, null) { }
        public MagicMessagebus(IErrorTracker errorTracker) : this(errorTracker, null, null) { }
        public MagicMessagebus(IKernel kernel) : this(null, kernel, null) { }
        public MagicMessagebus(IErrorTracker errorTracker, IKernel kernel) : this(errorTracker, kernel, null) { }
        public MagicMessagebus(IServiceProvider serviceProvider) : this(null, null, serviceProvider) { }
        public MagicMessagebus(IErrorTracker errorTracker, IServiceProvider serviceProvider) : this(errorTracker, null, serviceProvider) { }

        public MagicMessagebus(
            IErrorTracker errorTracker,
            IKernel ninject,
            IServiceProvider dotnet)
        {
            this.errorTracker = errorTracker;

            this.ninject = ninject;
            this.dotnet = dotnet;

            if (Map == null)
            {
                lock (key)
                {
                    if (Map == null)
                    {
                        Map = AppDomain.CurrentDomain.GetAssemblies()
                            .Where(a => false == a.IsDynamic)
                            .SelectMany(a =>
                            {
                                try
                                {
                                    return a.GetExportedTypes();
                                }
                                catch (Exception e)
                                {
                                    if (this.errorTracker == null)
                                    {
                                        throw;
                                    }

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
                }
            }
        }

        private static readonly object key = new object();

        private static List<IGrouping<Type, MethodInfo>> Map { get; set; } // static because tests fail if mapped twice in one application, which should never be necessary anyway

        public void Publish(IMagicMessage message)
        {
            Map
                .Single(m => m.Key.Equals(message.GetType()))
                .Select(g => g)
                .ToList()
                .ForEach(m => this.Invoke(m, message));
        }

        private void Invoke(MethodInfo method, IMagicMessage message)
        {
            if (method.IsStatic)
            {
                this.Invoke(method, message, null);
            }
            else
            {
                var service = this.GetService(method);

                if (service == null)
                {
                    return;
                }

                this.Invoke(method, message, service);
            }
        }

        private void Invoke(MethodInfo method, IMagicMessage message, object service)
        {
            Task.Run(() =>
            {
                try
                {
                    var result = method.Invoke(service, new object[] { message });

                    if (result is HttpStatusCode)
                    {
                        var status = (HttpStatusCode)result;

                        if (false == status.IsSuccessStatusCode())
                        {
                            throw new MagicMessagebusException("A subscriber to the MagicMessagebus returned an unsuccessfull status code")
                            {
                                Data = {
                                    { "Service", service.GetType().Name },
                                    { "Method", method.Name },
                                    { "Message", JsonConvert.SerializeObject(message, Formatting.Indented) },
                                    { "Status", status },
                                },
                            };
                        }
                    }
                }
                catch (Exception e)
                {
                    if (this.errorTracker == null)
                    {
                        throw;
                    }

                    this.errorTracker.Track(e);
                }
            });
        }

        private object GetService(MethodInfo method)
        {
            return this.ninject?.Get(method.DeclaringType)
                ?? this.dotnet?.GetService(method.DeclaringType)
                ?? null;
        }
    }
}