namespace Whatsub
{
	using System;
	using System.Collections.Generic;
	using System.Threading;

	public class Whatsub
	{
		private static IServiceLocator locator;
		private static readonly List<ISubscription> subscriptions = new List<ISubscription>();
		private static readonly List<ISubscription2> subscriptions2 = new List<ISubscription2>();

		public Whatsub(IServiceLocator l)
		{
			locator = l;
		}

		public void Subscribe<TService, TMessage>(Action<TService, TMessage> fn)
			=> subscriptions.Add(new Subscription<TService, TMessage>(fn));

		public static void Subscribe<TMessage>(Action<TMessage> fn)
			=> subscriptions2.Add(new Subscription<TMessage>(fn));

		public static void Publish<TMessage>(TMessage message)
		{
			subscriptions.ForEach(s => s.InvokeIf(message, locator));
			subscriptions2.ForEach(s => s.InvokeIf(message));
		}
	}

	internal class Wow
	{
		public Wow()
		{
		}

		internal void Add<TMessage>(Action<TMessage> fn)
		{

		}
	}

	class TypeDictionary
	{
		private readonly Dictionary<Type, List<T>> dictionary = new Dictionary<Type, List<T>>();

		public void Add<T>(T item)
		{
			Type key = typeof(T);

			if (!dictionary.ContainsKey(key))
			{
				dictionary[key] = new List<T>();
			}

			dictionary[key].Add(item);
		}

		public List<T> Get()
		{
			Type key = typeof(T);

			if (dictionary.TryGetValue(key, out List<T> items))
			{
				return items;
			}
			else
			{
				return new List<T>();
			}
		}
	}
}