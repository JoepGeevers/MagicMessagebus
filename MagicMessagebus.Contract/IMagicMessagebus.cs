namespace MagicMessagebus.Contract
{
    public interface IMagicMessagebus
    {
        void Publish(IMagicMessage message);
    }
}
