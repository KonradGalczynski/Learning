using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace ContractDistributionNetCoreWebApi.Controllers.DependencyToRabbitMq
{
	[ApiController]
	[Route("api/rabbitmq")]
	public class RabbitMqDependencySampleController : ControllerBase
	{
		private readonly IConfiguration _configuration;

		public RabbitMqDependencySampleController(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		[HttpPost]
		public Task PublishMessageAsync(string message)
		{
			var factory = new ConnectionFactory
			{
				UserName = _configuration["RabbitMq:User"],
				Password = _configuration["RabbitMq:Password"],
				VirtualHost = _configuration["RabbitMq:VirtualHost"],
				HostName = _configuration["RabbitMq:Hostname"],
				Port = int.Parse(_configuration["RabbitMq:Port"])
			};
			var connection = factory.CreateConnection();
			var channel = connection.CreateModel();
			var exchange = "SampleExchange";
			channel.ExchangeDeclare(exchange, ExchangeType.Direct);
			channel.QueueDeclare("SampleQueue", false, false, false, null);
			channel.QueueBind("SampleQueue", exchange, "key", null);

			var messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(message);
			channel.BasicPublish(exchange, "key", null, messageBodyBytes);

			channel.Close();
			connection.Close();
			return Task.CompletedTask;
		}
	}
}