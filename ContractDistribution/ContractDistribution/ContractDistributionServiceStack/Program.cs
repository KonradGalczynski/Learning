using Microsoft.Extensions.Hosting;
using ServiceStack;

namespace ContractDistributionServiceStackWebApi
{
	public class Program
	{
		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(builder =>
				{
					builder.UseModularStartup<Startup>();
				});
    }
}
