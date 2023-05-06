namespace MagicMessagebus.Implementation
{
    using System;

    using Microsoft.Extensions.DependencyInjection;

    public class Subscription<TService, TMessage> : IServiceLocatorSubscription
    {
        public readonly Action<TService, TMessage> fn;

        public Subscription(Action<TService, TMessage> fn) => this.fn = fn;

        public void Invoke<T>(T message, IServiceProvider provider)
        {
            if (typeof(T).Equals(typeof(TMessage)))
            {
                var service = provider.GetService<TService>();

                this.fn.Invoke(service, (TMessage)(object)message);
            }
        }
    }

    public class Subscription<TMessage> : IStaticSubscription
    {
        public readonly Action<TMessage> fn;

        public Subscription(Action<TMessage> fn) => this.fn = fn;

        public void Invoke<T>(T message)
        {
            if (typeof(T).Equals(typeof(TMessage)))
            {
                this.fn.Invoke((TMessage)(object)message);
            }
        }
    }
}