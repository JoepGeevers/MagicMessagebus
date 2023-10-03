namespace Whatsub
{
    using System;

    using Microsoft.Extensions.DependencyInjection;

    public static class WhatsubServiceProviderExtensions
    {
        // also create example for ninject
        public static IServiceCollection AddWhatsub(this IServiceCollection services)
        {
            return services
                .AddSingleton<Whatsub>()
                .AddSingleton<IServiceLocator, ServiceProviderServiceLocator>();
        }

        public static IServiceCollection WithSubscription<TService, TMessage>(this IServiceCollection services, Func<TService, TMessage, Status> fn)
        {
            Whatsub.Subscribe(fn);

            return services;
        }
    }

    public enum Status
    {
        OK = 200,
        Created = 201,
        Accepted = 202,
        NoContent = 204,
        BadRequest = 400,
        Unauthorized = 401,
        Forbidden = 403,
        NotFound = 404,
        Conflict = 409,
        InternalServerError = 500,
        NotImplemented = 501,
    }
}