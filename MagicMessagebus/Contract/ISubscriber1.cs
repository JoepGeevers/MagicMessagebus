using System.Net;

namespace MagicMessagebus.Contract
{
    public interface ISubscriber<TMessage>
        where TMessage : IMagicMessage
    {
        HttpStatusCode Foo(TMessage message);
    }
}