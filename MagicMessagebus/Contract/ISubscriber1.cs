using System.Net;

namespace MagicMessagebus.Contract
{
    public interface ISubscriber<TMessage>
        where TMessage : class, IMagicMessage
    {
        HttpStatusCode Subscribe(TMessage message);
    }
}