namespace MagicMessagebus.Implementation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    using Ninject;
    using Ninject.Parameters;

    using Contract;

    public class MagicMessagebus : IMagicMessagebus
    {
        private readonly IErrorTracker errorTracker;
        private readonly IKernel kernel;
        private readonly IServiceProvider serviceProvider;

        public MagicMessagebus() : this(null, null, null) { }
        public MagicMessagebus(IErrorTracker errorTracker) : this(errorTracker, null, null) { }
        public MagicMessagebus(IKernel kernel) : this(null, kernel, null) { }
        public MagicMessagebus(IErrorTracker errorTracker, IKernel kernel) : this(errorTracker, kernel, null) { }
        public MagicMessagebus(IServiceProvider serviceProvider) : this(null, null, serviceProvider) { }
        public MagicMessagebus(IErrorTracker errorTracker, IServiceProvider serviceProvider) : this(errorTracker, null, serviceProvider) { }

        public MagicMessagebus(
            IErrorTracker errorTracker,
            IKernel kernel,
            IServiceProvider serviceProvider)
        {
            this.errorTracker = errorTracker;
            this.kernel = kernel;
            this.serviceProvider = serviceProvider;

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
                var service = this.GetServiceFromNinject(method)
                    ?? this.GetServiceFromServiceProvider(method);

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
                    method.Invoke(service, new object[] { message });
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

        private object GetServiceFromNinject(MethodInfo method)
        {
            if (this.kernel == null)
            {
                return null;
            }

            return this.kernel.Get(method.DeclaringType);
        }

        private object GetServiceFromServiceProvider(MethodInfo method)
        {
            if (this.serviceProvider == null)
            {
                return null;
            }

            return this.serviceProvider.GetService(method.DeclaringType);
        }
    }
}