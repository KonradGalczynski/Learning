using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ContractDistributionNetCoreWebApi;
using ContractDistributionNetCoreWebApi.Controllers;
using ContractDistributionNetCoreWebApi.Controllers.Schedule;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Refit;
using ScheduleWebApiRefitContract;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;
using static TddXt.AnyRoot.Root;

namespace ScheduleWebApiRefitTests
{
	public class ScheduleApiRefitTests : IDisposable
	{
		private readonly CancellationTokenSource _cts;
		private readonly WireMockServer _mockServer;
		private const string ScheduleWebApiBaseUrl = "http://localhost:3333";

		public ScheduleApiRefitTests()
		{
			string[] args = { };
			_cts = new CancellationTokenSource();
			_mockServer = WireMockServer.Start(5678);
			Task.Run(() => CreateHostBuilder(args).Build().Run(), _cts.Token);
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseStartup<Startup>();
					webBuilder.ConfigureAppConfiguration(builder =>
					{
						var configurationOverrides = new List<KeyValuePair<string, string>>
						{
							new KeyValuePair<string, string>("ServiceLocatorBaseAddress", "http://localhost:5678")
						};
						builder.AddInMemoryCollection(configurationOverrides);
					});
					webBuilder.UseUrls(ScheduleWebApiBaseUrl);
				});

		[Fact]
		public async Task ShouldCreateScheduleWhenRequested()
		{
			var scheduleWebAPi = RestService.For<IScheduleWebApi>(ScheduleWebApiBaseUrl);
			var workloadItems = new List<WorkloadItem>
			{
				Any.Instance<WorkloadItem>(),
				Any.Instance<WorkloadItem>()
			};
			
			var maintenanceWindowServiceLocation = new ServiceLocation
			{
				Location = new Uri("http://localhost:5678")
			};
			_mockServer.Given(
				Request.Create().UsingGet().WithPath("/MaintenanceWindowService"))
				.RespondWith(
					Response.Create().WithSuccess().WithBodyAsJson(maintenanceWindowServiceLocation));
			
			var maintenanceWindows = Any.Instance<MaintenanceWindow>();
			_mockServer.Given(
				Request.Create().UsingGet().WithPath("/Planned"))
				.RespondWith(
					Response.Create().WithSuccess().WithBodyAsJson(maintenanceWindows));

			var result = await scheduleWebAPi.CreateScheduleAsync(workloadItems);

			result.Should().NotBe(Guid.Empty);
		}

		[Fact]
		public async Task ShouldRetrieveCreatedSchedule()
		{
			var scheduleWebAPi = RestService.For<IScheduleWebApi>(ScheduleWebApiBaseUrl);
			var workloadItems = new List<WorkloadItem>
			{
				new WorkloadItem
				{
					Identifier = Guid.NewGuid(),
					DurationInHours = 7
				},
				new WorkloadItem
				{
					Identifier = Guid.NewGuid(),
					DurationInHours = 3
				}
			};

			var maintenanceWindowServiceLocation = new ServiceLocation
			{
				Location = new Uri("http://localhost:5678")
			};
			_mockServer.Given(
					Request.Create().UsingGet().WithPath("/MaintenanceWindowService"))
				.RespondWith(
					Response.Create().WithSuccess().WithBodyAsJson(maintenanceWindowServiceLocation));

			var maintenanceWindows = new MaintenanceWindow
			{
				LengthInHours = 5
			};
			_mockServer.Given(
					Request.Create().UsingGet().WithPath("/Planned"))
				.RespondWith(
					Response.Create().WithSuccess().WithBodyAsJson(maintenanceWindows));

			var scheduleId = await scheduleWebAPi.CreateScheduleAsync(workloadItems);

			var result = await scheduleWebAPi.GetScheduleByIdAsync(scheduleId);

			result.Count.Should().Be(2);
			result.Should().Contain(x => x.Identifier == workloadItems[0].Identifier &&
			                             x.Order == 2);
			result.Should().Contain(x => x.Identifier == workloadItems[1].Identifier &&
			                             x.Order == 1);
		}

		public void Dispose()
		{
			_cts.Cancel();
			_mockServer.Stop();
			_mockServer.Dispose();
		}
	}
}
