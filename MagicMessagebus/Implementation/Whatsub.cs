namespace MagicMessagebus.Implementation
{
    using System;
    using System.Collections.Generic;

    public class Whatsub : IWhatsub
    {
        private static readonly List<IServiceLocatorSubscription> serviceLocatorSubscription = new List<IServiceLocatorSubscription>();
        private static readonly List<IStaticSubscription> staticSubscription = new List<IStaticSubscription>();

        // change this into our own IServiceLocator and have the extensions wrap the actual container
        private readonly IServiceProvider dotnet;

        public Whatsub(IServiceProvider serviceProvider) => this.dotnet = serviceProvider;

        public static void Subscribe<TService, TMessage>(Action<TService, TMessage> fn)
            => serviceLocatorSubscription.Add(new Subscription<TService, TMessage>(fn));

        public static void Subscribe<TMessage>(Action<TMessage> fn)
            => staticSubscription.Add(new Subscription<TMessage>(fn));

        void IWhatsub.Publish<TMessage>(TMessage message)
        {
            serviceLocatorSubscription.ForEach(s => s.Invoke(message, this.dotnet));

            Publish(message);
        }

        public static void Publish<TMessage>(TMessage message)
        {
            staticSubscription.ForEach(s => s.Invoke(message));
        }
    }

    public interface IWhatsub
    {
        void Publish<TMessage>(TMessage message);
    }
}