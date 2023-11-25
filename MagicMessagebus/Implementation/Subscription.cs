namespace Whatsub
{
	using System;

	public class Subscription<TMessage> : ISubscription2
	{
		public readonly Action<TMessage> fn;

		public Subscription(Action<TMessage> fn) => this.fn = fn;

		public void InvokeIf<T>(T message)
		{
			if (typeof(T).Equals(typeof(TMessage)))
			{
				this.fn.Invoke((TMessage)(object)message);
			}
		}
	}


	public class Subscription<TService, TMessage> : ISubscription
	{
		public readonly Action<TService, TMessage> fn;

		public Subscription(Action<TService, TMessage> fn) => this.fn = fn;

		public void InvokeIf<T>(T message, IServiceLocator locator)
		{
			if (typeof(T).Equals(typeof(TMessage)))
			{
				var service = locator.Get<TService>();

				this.fn.Invoke(service, (TMessage)(object)message);
			}
		}
	}
}