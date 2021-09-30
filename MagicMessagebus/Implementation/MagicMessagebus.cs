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

        // I apologize, but I have not found a way to make optional parameters play nice together with .NET DI and Ninject
        public MagicMessagebus(                                                                    ) : this(null,         null,    null  ) { }
        public MagicMessagebus(IErrorTracker errorTracker                                          ) : this(errorTracker, null,    null  ) { }
        public MagicMessagebus(                            IKernel ninject                         ) : this(null,         ninject, null  ) { }
        public MagicMessagebus(IErrorTracker errorTracker, IKernel ninject                         ) : this(errorTracker, ninject, null  ) { }
        public MagicMessagebus(                                             IServiceProvider dotnet) : this(null,         null,    dotnet) { }
        public MagicMessagebus(IErrorTracker errorTracker,                  IServiceProvider dotnet) : this(errorTracker, null,    dotnet) { }
        public MagicMessagebus(                            IKernel ninject, IServiceProvider dotnet) : this(null,         ninject, dotnet) { }
        public MagicMessagebus(IErrorTracker errorTracker, IKernel ninject, IServiceProvider dotnet)
        {
            this.errorTracker = errorTracker ?? new ExplodingErrorTracker();

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
                                    return a.GetTypes();
                                }
                                catch (Exception e)
                                {
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
                                    .Where(i => i.Name == nameof(global::MagicMessagebus.Contract.IMagicMessage))
                                    .Any();
                            })
                            .GroupBy(m => m.GetParameters().Single().ParameterType) // this might not work. you might need the fullname of the parametertype because assemblies may differ
                            .ToList();
                    }
                }

                this.Publish(new StartupStaticSelftest(), true);
                this.Publish(new StartupInstanceSelftest(), true);
            }
        }

        private static readonly object key = new object();

        internal static List<IGrouping<Type, MethodInfo>> Map { get; set; } // static because tests fail if mapped twice in one application, which should never be necessary anyway
        public Action<IMagicMessage> Subscriptions { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void Publish(IMagicMessage message)
        {
            this.Publish(message, false);
        }

        private void Publish(IMagicMessage message, bool selftest)
        {
            //Map
            //    .Single(m => m.Key.Equals(message.GetType()))
            //    .Select(g => g)
            //    .ToList()
            //    .ForEach(m => this.Invoke(m, message, selftest));

            foreach (var subscriber in subscribers)
            {
                if (subscriber.Value.GetType().Equals(message.GetType()))
                {
                    var service = (ISubscriber<IMagicMessage>)this.GetService(subscriber.Key);

                    service.Subscribe(message);
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
                var service = this.GetService(method.DeclaringType);

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

        private object GetService(Type type)
        {
            return this.ninject?.Get(type)
                ?? this.dotnet?.GetService(type)
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

        public void Subscribe<T>(Action<T> subscribe)
        {
        }

        public void Subscribe<TService, TMessage>()
            where TService : ISubscriber<TMessage>
            where TMessage : IMagicMessage
        {
            subscribers.Add(new KeyValuePair<Type, Type>(typeof(TService), typeof(TMessage)));
        }

        private static readonly List<KeyValuePair<Type, Type>> subscribers = new List<KeyValuePair<Type, Type>>();

        private TService GetService<TService>()
            where TService: class
        {
            return this.ninject?.Get<TService>()
                ?? (TService)this.dotnet.GetService(typeof(TService))
                ?? default;
        }

        public void Subscribe<TSubscription>() where TSubscription : ISubscriber<IMagicMessage>
        {
            throw new NotImplementedException();
        }

        public void SubScribe2<TService, TMessage>(Subscription<TService, TMessage> s)
            where TService : ISubscriber<TMessage>
            where TMessage : IMagicMessage
        {
            throw new NotImplementedException();
        }

        public void Subscribe3<TMessage>(Action<TMessage> subscribe)
        {
            subscribe.GetMethodInfo();
        }

        public void Subscribe4(Action<IMagicMessage> subscribe)
        {
            var info = subscribe.GetMethodInfo();
        }

        public void Subscribe5(Action<IMagicMessage> subscribe)
        {
            throw new NotImplementedException();
        }

        public void Subscribe6(Func<IMagicMessage, HttpStatusCode> function)
        {
            throw new NotImplementedException();
        }
    }
}