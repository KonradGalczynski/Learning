using ContractDistributionServiceStack.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServiceStack;

namespace ContractDistributionServiceStack
{
	public class Startup : ModularStartup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}


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

			app.UseHttpsRedirection();

			app.UseServiceStack(new AppHost
			{
				AppSettings = new NetCoreAppSettings(Configuration)
			});
		}
	}
}
