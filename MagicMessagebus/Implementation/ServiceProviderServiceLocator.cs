namespace MagicMessagebus.Implementation
{
    using System;

    using Microsoft.Extensions.DependencyInjection;

    public class ServiceProviderServiceLocator : IServiceLocator
    {
        private readonly IServiceProvider provider;

        public ServiceProviderServiceLocator(IServiceProvider provider)
        {
            this.provider=provider;
        }

        public TService Get<TService>()
        {
            return this.provider.GetRequiredService<TService>();
        }
    }
}