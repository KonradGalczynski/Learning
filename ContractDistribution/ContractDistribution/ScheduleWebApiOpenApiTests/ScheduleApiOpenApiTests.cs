using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ContractDistributionOpenApiWebApi;
using ContractDistributionOpenApiWebApi.Controllers;
using ContractDistributionOpenApiWebApi.Controllers.Schedule;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using OpenApiSheduleApiClient;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;
using static TddXt.AnyRoot.Root;

namespace ScheduleWebApiOpenApiTests
{
	public class ScheduleApiOpenApiTests : IDisposable
	{
		private readonly CancellationTokenSource _cts;
		private readonly WireMockServer _mockServer;
		private const string ScheduleWebApiBaseUrl = "http://localhost:3333";

		public ScheduleApiOpenApiTests()
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

			var httpClient = new HttpClient();
			var scheduleWebApiClient = new ScheduleClient(httpClient)
			{
				BaseUrl = ScheduleWebApiBaseUrl
			};

			var result = await scheduleWebApiClient.CreateScheduleAsync(workloadItems);

			result.Should().NotBe(Guid.Empty);
		}

		[Fact]
		public async Task ShouldRetrieveCreatedSchedule()
		{
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

			var httpClient = new HttpClient();
			var scheduleWebApiClient = new ScheduleClient(httpClient)
			{
				BaseUrl = ScheduleWebApiBaseUrl
			};

			var scheduleId = await scheduleWebApiClient.CreateScheduleAsync(workloadItems);

			var result = await scheduleWebApiClient.GetScheduleByIdAsync(scheduleId);

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
