using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ContractDistributionServiceStackWebApi;
using ContractDistributionServiceStackWebApi.Services;
using ContractDistributionServiceStackWebApi.Services.Schedule;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using ScheduleWebApiServiceStackContract;
using ServiceStack;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;
using static TddXt.AnyRoot.Root;

namespace ScheduleWebApiServiceStackTests
{
	public class ScheduleApiServiceStackTests : IDisposable
	{
		private readonly CancellationTokenSource _cts;
		private readonly WireMockServer _mockServer;
		private const string ScheduleWebApiBaseUrl = "http://localhost:3333";

		public ScheduleApiServiceStackTests()
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
					webBuilder.UseModularStartup<Startup>();
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
			var scheduleWebApiClient = new JsonServiceClient(ScheduleWebApiBaseUrl);

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

			var workloadItems = new List<WorkloadItem>
			{
				Any.Instance<WorkloadItem>(),
				Any.Instance<WorkloadItem>()
			};
			var createScheduleRequest = new CreateScheduleRequest
			{
				WorkloadItems = workloadItems
			};

			var result = await scheduleWebApiClient.PostAsync(createScheduleRequest).ConfigureAwait(false);

			result.Id.Should().NotBe(Guid.Empty);
		}

		[Fact]
		public async Task ShouldRetrieveCreatedSchedule()
		{
			var scheduleWebApiClient = new JsonServiceClient(ScheduleWebApiBaseUrl);

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
			var createScheduleRequest = new CreateScheduleRequest
			{
				WorkloadItems = workloadItems
			};

			var createScheduleResponse = await scheduleWebApiClient.PostAsync(createScheduleRequest).ConfigureAwait(false);

			var getScheduleByIdRequest = new GetScheduleByIdRequest
			{
				ScheduleId = createScheduleResponse.Id
			};
			var getScheduleByIdResponse = await scheduleWebApiClient.GetAsync(getScheduleByIdRequest).ConfigureAwait(false);

			getScheduleByIdResponse.Schedule.Count.Should().Be(2);
			getScheduleByIdResponse.Schedule.Should().Contain(x => x.Identifier == workloadItems[0].Identifier &&
			                             x.Order == 2);
			getScheduleByIdResponse.Schedule.Should().Contain(x => x.Identifier == workloadItems[1].Identifier &&
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
