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

        public MagicMessagebus(
            [Optional]IErrorTracker errorTracker = null,
            IKernel kernel = null)
        {
            this.errorTracker = errorTracker;
            this.kernel = kernel;

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
                method.Invoke(null, new object[] { message });
            }
            else
            {
                if (this.kernel == null)
                {
                    return;
                }

                var service = this.GetService(method);

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
        }

        private object GetService(MethodInfo method)
        {
            var get = typeof(ResolutionExtensions)
                .GetMethods()
                .Where(m2 => m2.Name == "Get")
                .Where(m2 => m2.IsGenericMethod)
                .Where(m2 => m2.GetParameters().Count() == 2)
                .Single();

            var result = get.MakeGenericMethod(method.DeclaringType)
                .Invoke(null, new object[] { this.kernel, new IParameter[] { } });

            return result;
        }
    }
}