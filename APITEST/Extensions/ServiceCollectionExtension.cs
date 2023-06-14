using APITEST.Infrastructure.IServices;
using APITEST.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace APITEST.Extensions
{
	public static class ServiceCollectionExtension
	{
		public static void AddSevices(this IServiceCollection services)
		{
			services.TryAddScoped<IAccountService, AccountService>();
		}
	}
}
