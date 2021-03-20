namespace MagicMessagebus.Contract
{
    using System.Net;

    public interface IMagicMessagebus
    {
        void Publish(IMagicMessage message);
        HttpStatusCode Subscribe(StartupInstanceSelftest message);
    }
}