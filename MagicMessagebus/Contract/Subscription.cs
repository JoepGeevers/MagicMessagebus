namespace MagicMessagebus.Contract
{
    public class Subscription<TService, TMessage>
        where TService : class, ISubscriber<TMessage>
        where TMessage : class, IMagicMessage
    {
    }
}