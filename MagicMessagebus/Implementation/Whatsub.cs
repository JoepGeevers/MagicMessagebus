namespace MagicMessagebus.Implementation
{
    using System;
    using System.Collections.Generic;

    public class Whatsub
    {
        private static readonly List<IServiceLocatorSubscription> serviceLocatorSubscription = new List<IServiceLocatorSubscription>();
        private static readonly List<IStaticSubscription> staticSubscription = new List<IStaticSubscription>();

        private readonly IServiceLocator locator;

        public Whatsub(IServiceLocator locator)
        {
            this.locator = locator;
        }

        public static void Clear()
        {
            serviceLocatorSubscription.Clear();
            staticSubscription.Clear();
        }

        public static void Subscribe<TService, TMessage>(Action<TService, TMessage> fn)
            => serviceLocatorSubscription.Add(new Subscription<TService, TMessage>(fn));

        public static void Subscribe<TMessage>(Action<TMessage> fn)
            => staticSubscription.Add(new Subscription<TMessage>(fn));

        public void Publish<TMessage>(TMessage message)
        {
            serviceLocatorSubscription.ForEach(s => s.Invoke(message, locator));
            staticSubscription.ForEach(s => s.Invoke(message));
        }
    }
}