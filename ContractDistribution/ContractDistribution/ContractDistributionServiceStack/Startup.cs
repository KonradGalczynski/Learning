using ContractDistributionServiceStackWebApi.Services.Schedule;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServiceStack;

namespace ContractDistributionServiceStackWebApi
{
	public class Startup : ModularStartup
	{
		public new void ConfigureServices(IServiceCollection services)
		{
			services.AddHttpClient();
			services.AddSingleton<IScheduleStorage, ScheduleStorage>();
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseServiceStack(new AppHost
			{
				AppSettings = new NetCoreAppSettings(Configuration)
			});
		}
	}
}
