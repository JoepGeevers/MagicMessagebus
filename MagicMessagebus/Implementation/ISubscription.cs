namespace Whatsub
{
    public interface ISubscription
    {
        void InvokeIf<TMessage>(TMessage message, IServiceLocator locator);
    }

	public interface ISubscription2
	{
		void InvokeIf<TMessage>(TMessage message);
	}
}