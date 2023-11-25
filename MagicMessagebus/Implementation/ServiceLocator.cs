namespace Whatsub
{
    using System;

    using Microsoft.Extensions.DependencyInjection;

    public class ServiceLocator : IServiceLocator
    {
        private readonly IServiceProvider provider;

		public ServiceLocator(IServiceProvider provider) => this.provider = provider;

		public TService Get<TService>() => this.provider.GetRequiredService<TService>();
	}
}