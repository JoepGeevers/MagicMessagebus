namespace Whatsub
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;

    public class Whatsub
    {
        private readonly IServiceLocator locator;
        private static readonly List<ISubscription> subscriptions = new List<ISubscription>();

        public Whatsub(IServiceLocator locator) => this.locator = locator;

        public static void Subscribe<TService, TMessage>(Func<TService, TMessage, Status> fn)
            => subscriptions.Add(new Subscription<TService, TMessage>(fn));

        public void Publish<TMessage>(TMessage message)
            => subscriptions.ForEach(s => s.InvokeIf(message, locator));
    }
}