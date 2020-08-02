using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ScheduleWebApiServiceStackContract;
using ServiceStack;

namespace ContractDistributionServiceStack.Services
{
	public class ScheduleService : Service
	{
		private readonly IScheduleStorage _scheduleStorage;

		public ScheduleService(
			IScheduleStorage scheduleStorage)
		{
			_scheduleStorage = scheduleStorage;
		}

		public async Task<object> CreateSchedule(CreateScheduleRequest request)
		{
			var response = new CreateScheduleResponse
			{
				//Id = await _scheduleStorage.SaveScheduleAsync(CalculateScheduleFor(request.WorkloadItems, plannedMaintenanceWindow))
			};
			return response;
		}

		private static List<ScheduleEntry> CalculateScheduleFor(List<WorkloadItem> workloadItems, 
			MaintenanceWindow plannedMaintenanceWindow)
		{
			return workloadItems.Select(x => new ScheduleEntry
			{
				Identifier = x.Identifier,
				Order = x.DurationInHours >= plannedMaintenanceWindow.LengthInHours ? 2 : 1
			}).ToList();
		}

		//public object GetScheduleById(GetScheduleByIdRequest request)
		//{
		//	_logger.LogInformation("Get schedule requested.");

		//	var response = new GetScheduleByIdResponse
		//	{
		//		Schedule = _scheduleStorage.ReadSchedule(request.ScheduleId)
		//	};
		//	return response;
		//}

		public object Any(Hello request)
		{
			return new HelloResponse { Result = $"Hello, {request.Name}!" };
		}
	}
}