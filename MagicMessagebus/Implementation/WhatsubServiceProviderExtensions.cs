namespace MagicMessagebus.Implementation
{
    using System;

    using Microsoft.Extensions.DependencyInjection;

    public static class WhatsubServiceProviderExtensions
    {
        public static IServiceCollection AddMagicMessagebus(this IServiceCollection services)
            => services.AddSingleton<Whatsub>();

        public static IServiceCollection WithSubscription<TService, TMessage>(this IServiceCollection services, Action<TService, TMessage> fn)
        {
            Whatsub.Subscribe(fn);

            return services;
        }
    }
}