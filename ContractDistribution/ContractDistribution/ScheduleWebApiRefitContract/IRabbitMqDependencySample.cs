using System.Threading.Tasks;
using Refit;

namespace ScheduleWebApiRefitContract
{
	public interface IRabbitMqDependencySample
	{
		[Post("/api/rabbitmq")]
		Task PublishMessageAsync(string message);
	}
}