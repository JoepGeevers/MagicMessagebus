namespace Whatsub
{
    using System;

    public class Subscription<TService, TMessage> : ISubscription
    {
        public readonly Func<TService, TMessage, Status> fn;

        public Subscription(Func<TService, TMessage, Status> fn) => this.fn = fn;

        public void InvokeIf<T>(T message, IServiceLocator locator)
        {
            if (typeof(T).Equals(typeof(TMessage)))
            {
                var service = locator.Get<TService>();

                this.fn.Invoke(service, (TMessage)(object)message);
            }
        }
    }
}