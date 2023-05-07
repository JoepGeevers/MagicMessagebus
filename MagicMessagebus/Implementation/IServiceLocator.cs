namespace MagicMessagebus.Implementation
{
    public interface IServiceLocator
    {
        TService Get<TService>();
    }
}