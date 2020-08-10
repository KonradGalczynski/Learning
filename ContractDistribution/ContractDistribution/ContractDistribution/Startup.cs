using ContractDistributionNetCoreWebApi.Controllers.Schedule;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ContractDistributionNetCoreWebApi
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddControllers();
			services.AddHttpClient();
			services.AddSingleton<IScheduleStorage, ScheduleStorage>();
			services.AddSwaggerDocument();
			services.AddDbContext<StringStorageContext>(options =>
				options.UseSqlServer(Configuration.GetConnectionString("StringStorageContext")));
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseHttpsRedirection();

			app.UseRouting();

			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});

			app.UseOpenApi();
			app.UseSwaggerUi3();
		}
	}

	public class StringStorageContext : DbContext
	{
		public StringStorageContext(
			DbContextOptions<RazorPagesMovieContext> options)
			: base(options)
		{
		}

		public DbSet<string> Item { get; set; }
	}
}
