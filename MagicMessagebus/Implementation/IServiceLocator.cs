namespace Whatsub
{
    public interface IServiceLocator
    {
        TService Get<TService>();
    }
}