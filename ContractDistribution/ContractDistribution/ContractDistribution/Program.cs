using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace ContractDistributionRefit
{
	public class Program
	{
		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseStartup<Startup>();
				});
	}
}

// testing levels
//   tests as living specification - xunit and description of module behavior
//   quick hint about behavior coverage as a metric for checking quality of your tests
//   what types of tests are the most important ones when it comes to web services
//   how service testing is different than testing with existing hosts for example test host
//   how performing the same test scenario on different levels help with finding rootcause 
//     of problems
//   rest contract distribution
//     code first vs spec first
//     code first approaches
//       interface based - refit
//       dto based - service stack
//       openapi - c# and typescript clients
//     services testing in each approach
//     service testing when svc depends on:
//       other rest service
//       database - sql, redis
//       queue mechanism - rabbit
//       streaming platform - kafka
//   non-rest contract distribution


