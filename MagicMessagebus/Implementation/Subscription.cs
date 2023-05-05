namespace MagicMessagebus.Implementation
{
    using System;

    using Microsoft.Extensions.DependencyInjection;

    internal class Subscription<TService, TMessage> : ISubscription
    {
        public readonly Action<TService, TMessage> fn;

        public Subscription(Action<TService, TMessage> fn)
        {
            this.fn = fn;
        }

        public void CallIfMatch<T>(T message, IServiceProvider provider)
        {
            if (typeof(T).Equals(typeof(TMessage)))
            {
                var service = provider.GetService<TService>();

                this.fn.Invoke(service, (TMessage)(object)message);
            }
        }
    }
}