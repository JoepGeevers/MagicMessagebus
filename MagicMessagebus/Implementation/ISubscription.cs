namespace MagicMessagebus.Implementation
{
    using System;

    public interface IServiceLocatorSubscription
    {
        void Invoke<TMessage>(TMessage message, IServiceLocator locator);
    }

    public interface IStaticSubscription
    {
        void Invoke<TMessage>(TMessage message);
    }
}