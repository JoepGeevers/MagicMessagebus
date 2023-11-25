namespace Whatsub
{
    using System;

    using Microsoft.Extensions.DependencyInjection;

	// also create example for ninject
	public static class WhatsubServiceProviderExtensions
    {
		public static IServiceCollection AddWhatsub(this IServiceCollection services)
			=> services
				.AddSingleton<Whatsub>()
				.AddSingleton<IServiceLocator, ServiceLocator>();

		public static IServiceCollection WithSubscription<TService, TMessage>(this IServiceCollection services, Action<TService, TMessage> fn)
        {
            Whatsub.Subscribe(fn);

            return services;
        }

		public static Foo<TMessage> Subscribe<TMessage>(this IServiceCollection services) => new Foo<TMessage>(services);
	}

	public class Foo<TMessage>
	{
		private readonly IServiceCollection services;

		public Foo(IServiceCollection services) => this.services = services;

		public IServiceCollection To<TService>(Action<TService, TMessage> fn)
		{
			Whatsub.Subscribe(fn);

			return this.services;
		}
	}
}