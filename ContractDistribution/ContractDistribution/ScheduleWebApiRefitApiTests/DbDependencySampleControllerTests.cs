using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ContractDistributionAspNetCoreWebApi;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Refit;
using ScheduleWebApiRefitContract;
using TddXt.AnyRoot.Strings;
using Xunit;
using static TddXt.AnyRoot.Root;

namespace ScheduleWebApiRefitTests
{
	public class DbDependencySampleControllerTests : IDisposable
	{
		private readonly CancellationTokenSource _cts;
		private const string DbDependencySampleBaseUrl = "http://localhost:3333";
		private const string LocalDbConnectionString = "Server=(localdb)\\mssqllocaldb;Database=StringStorageContext;Trusted_Connection=True;MultipleActiveResultSets=true;Integrated Security=sspi";

		public DbDependencySampleControllerTests()
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
							new KeyValuePair<string, string>("ConnectionStrings:StringStorageContext", LocalDbConnectionString)
						};
						builder.AddInMemoryCollection(configurationOverrides);
					});
					webBuilder.UseUrls(DbDependencySampleBaseUrl);
				});


		[Fact]
		public async Task ShouldReadSavedItem()
		{
			var dbDependencySample = RestService.For<IDbDependencySample>(DbDependencySampleBaseUrl);
			var item = Any.String();

			var itemId = await dbDependencySample.SaveItemAsync(item);
			var result = await dbDependencySample.ReadItemByIdAsync(itemId);

			result.Should().Be(item);
		}

		public void Dispose()
		{
			_cts.Cancel();
			using var connection = new SqlConnection(LocalDbConnectionString);
			connection.Open();
			var deleteCommand = new SqlCommand("DELETE FROM Record", connection);
			deleteCommand.ExecuteNonQuery();
			connection.Close();
		}
	}
}
