namespace MagicMessagebus.Implementation
{
    using System;

    public interface IServiceLocatorSubscription
    {
        void Invoke<TMessage>(TMessage message, IServiceProvider provider = null);
    }

    public interface IStaticSubscription
    {
        void Invoke<TMessage>(TMessage message);
    }
}