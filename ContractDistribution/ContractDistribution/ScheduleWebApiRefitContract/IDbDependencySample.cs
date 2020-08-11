using System.Threading.Tasks;
using Refit;

namespace ScheduleWebApiRefitContract
{
	public interface IDbDependencySample
	{
		[Post("/api/db")]
		Task<string> SaveItemAsync(string item);

		[Get("/api/db/{itemId}")]
		Task<string> ReadItemByIdAsync(string itemId);
	}
}
