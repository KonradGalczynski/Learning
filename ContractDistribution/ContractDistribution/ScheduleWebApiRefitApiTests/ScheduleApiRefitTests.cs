using System;
using System.Threading;
using System.Threading.Tasks;
using ContractDistribution;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Refit;
using ScheduleWebApiRefitContract;
using Xunit;

namespace ScheduleWebApiRefitApiTests
{
	public class ScheduleApiRefitTests : IDisposable
	{
		private readonly CancellationTokenSource _cts;
		private const string ScheduleWebApiBaseUrl = "http://localhost:3333";

		public ScheduleApiRefitTests()
		{
			string[] args = { };
			_cts = new CancellationTokenSource();
			Task.Run(() => CreateHostBuilder(args).Build().Run(), _cts.Token);
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseStartup<Startup>();
					webBuilder.UseUrls(ScheduleWebApiBaseUrl);
				});

		[Fact]
		public async Task ShouldCreateScheduleWhenRequested()
		{
			var scheduleWebAPi = RestService.For<IScheduleWebApi>(ScheduleWebApiBaseUrl);

			var result = await scheduleWebAPi.CreateSchedule();

			result.Should().NotBe(Guid.Empty);
		}

		[Fact]
		public async Task ShouldRetrieveCreatedSchedule()
		{
			var scheduleWebAPi = RestService.For<IScheduleWebApi>(ScheduleWebApiBaseUrl);
			var scheduleId = await scheduleWebAPi.CreateSchedule();

			var result = await scheduleWebAPi.GetScheduleById(scheduleId);

			result.Should().BeEmpty();
		}

		public void Dispose()
		{
			_cts.Cancel();
		}
	}
}
