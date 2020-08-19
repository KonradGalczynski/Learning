using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ContractDistributionAspNetCoreWebApi;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using Refit;
using ScheduleWebApiRefitContract;
using TddXt.AnyRoot.Strings;
using Xunit;
using static TddXt.AnyRoot.Root;

namespace ScheduleWebApiRefitTests
{
	public class RabbitMqDependencySampleControllerTests : IDisposable
	{
		private readonly CancellationTokenSource _cts;
		private const string RabbitMqDependencySampleBaseUrl = "http://localhost:3333";

		public RabbitMqDependencySampleControllerTests()
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
					webBuilder.ConfigureAppConfiguration(builder =>
					{
						var configurationOverrides = new List<KeyValuePair<string, string>>
						{
							new KeyValuePair<string, string>("RabbitMq:User", "guest"),
							new KeyValuePair<string, string>("RabbitMq:Password", "guest"),
							new KeyValuePair<string, string>("RabbitMq:VirtualHost", "/"),
							new KeyValuePair<string, string>("RabbitMq:Hostname", "localhost"),
							new KeyValuePair<string, string>("RabbitMq:Port", "5672")
						};
						builder.AddInMemoryCollection(configurationOverrides);
					});
					webBuilder.UseUrls(RabbitMqDependencySampleBaseUrl);
				});


		[Fact]
		public async Task ShouldPublishMessage()
		{
			var rabbitMqDependencySample = RestService.For<IRabbitMqDependencySample>(RabbitMqDependencySampleBaseUrl);
			var message = Any.String();

			await rabbitMqDependencySample.PublishMessageAsync(message);

			Thread.Sleep(5000);
			AssertMessageReceived(message);
		}

		private static void AssertMessageReceived(string message)
		{
			var factory = new ConnectionFactory
			{
				UserName = "guest",
				Password = "guest",
				VirtualHost = "/",
				HostName = "localhost",
				Port = 5672
			};
			var connection = factory.CreateConnection();
			var channel = connection.CreateModel();
			var result = channel.BasicGet("SampleQueue", false);
			if (result != null)
			{
				var bodyString = System.Text.Encoding.UTF8.GetString(result.Body.ToArray());
				channel.BasicAck(result.DeliveryTag, false);
				bodyString.Should().Be(message);
			}

			channel.Close();
			connection.Close();
		}

		public void Dispose()
		{
			_cts.Cancel();
		}
	}
}
