namespace Whatsub
{
    public interface ISubscription
    {
        void InvokeIf<TMessage>(TMessage message, IServiceLocator locator);
    }
}