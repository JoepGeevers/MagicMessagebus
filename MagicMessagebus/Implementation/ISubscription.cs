namespace MagicMessagebus.Implementation
{
    using System;

    internal interface ISubscription
    {
        void CallIfMatch<T>(T message, IServiceProvider provider);
    }
}