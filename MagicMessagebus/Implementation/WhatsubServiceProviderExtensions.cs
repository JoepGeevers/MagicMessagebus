namespace MagicMessagebus.Implementation
{
    using System;

    using Microsoft.Extensions.DependencyInjection;

    public static class WhatsubServiceProviderExtensions
    {
        // also create example for ninject
        public static IServiceCollection AddWhatsub(this IServiceCollection services)
        {
            Whatsub.Clear();

            return services
                .AddSingleton<Whatsub>()
                .AddSingleton<IServiceLocator, ServiceProviderServiceLocator>();
        }

        public static IServiceCollection WithSubscription<TService, TMessage>(this IServiceCollection services, Action<TService, TMessage> fn)
        {
            Whatsub.Subscribe(fn);

            return services;
        }
    }
}