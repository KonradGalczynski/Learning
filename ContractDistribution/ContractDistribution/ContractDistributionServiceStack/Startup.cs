using ContractDistributionServiceStackWebApi.Services;
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
		// This method gets called by the runtime. Use this method to add services to the container.
		public new void ConfigureServices(IServiceCollection services)
		{
			services.AddHttpClient();
			services.AddSingleton<IScheduleStorage, ScheduleStorage>();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
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
