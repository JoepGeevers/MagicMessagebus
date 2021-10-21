namespace MagicMessagebus.Contract
{
    using System;
    using System.Net;

    public interface IMagicMessagebus
    {
        Action<IMagicMessage> Subscriptions { get; set; }

        void Publish(IMagicMessage message);
        void Subscribe<T>(Action<T> subscribe);
        void Subscribe<TService, TMessage>()
            where TService : class, ISubscriber<TMessage>
            where TMessage : class, IMagicMessage;

        void Subscribe<TSubscription>()
            where TSubscription : ISubscriber<IMagicMessage>;
        void SubScribe2<TService, TMessage>(Subscription<TService, TMessage> s)
            where TService : class, ISubscriber<TMessage>
            where TMessage : class, IMagicMessage;
        void Subscribe3<TMessage>(Action<TMessage> subscribe);
        void Subscribe4(Action<IMagicMessage> subscribe);
        void Subscribe5(Action<IMagicMessage> subscribe);
        void Subscribe6(Func<IMagicMessage, HttpStatusCode> function);
    }
}