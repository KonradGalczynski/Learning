using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ScheduleWebApiServiceStackContract;
using ServiceStack;

namespace ContractDistributionServiceStack.Services
{
	public class ScheduleService : Service
	{
		private readonly IConfiguration _configuration;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly ILogger<ScheduleService> _logger;
		private readonly IScheduleStorage _scheduleStorage;

		public ScheduleService(IConfiguration configuration,
			IHttpClientFactory httpClientFactory,
			IScheduleStorage scheduleStorage,
			ILogger<ScheduleService> logger)
		{
			_configuration = configuration;
			_httpClientFactory = httpClientFactory;
			_scheduleStorage = scheduleStorage;
			_logger = logger;
		}

		public async Task<object> Post(CreateScheduleRequest request)
		{
			_logger.LogInformation("Create schedule requested.");
			using var httpClient = _httpClientFactory.CreateClient();
			var getMaintenanceWindowServiceUriRequest = new HttpRequestMessage(HttpMethod.Get, $"{_configuration["ServiceLocatorBaseAddress"]}/MaintenanceWindowService");
			var maintenanceWindowServiceUriResponse = await httpClient.SendAsync(getMaintenanceWindowServiceUriRequest).ConfigureAwait(false);
			var maintenanceWindowServiceUri = JsonConvert.DeserializeObject<ServiceLocation>(await maintenanceWindowServiceUriResponse.Content.ReadAsStringAsync()
				.ConfigureAwait(false));
			var getPlannedMaintenanceWindowRequest = new HttpRequestMessage(HttpMethod.Get, $"{maintenanceWindowServiceUri.Location}Planned");
			var plannedMaintenanceWindowResponse = await httpClient.SendAsync(getPlannedMaintenanceWindowRequest).ConfigureAwait(false);
			var plannedMaintenanceWindow =
				JsonConvert.DeserializeObject<MaintenanceWindow>(await plannedMaintenanceWindowResponse.Content.ReadAsStringAsync().ConfigureAwait(false));
			var response = new CreateScheduleResponse
			{
				Id = await _scheduleStorage.SaveScheduleAsync(CalculateScheduleFor(request.WorkloadItems, plannedMaintenanceWindow))
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

		public object Get(GetScheduleByIdRequest request)
		{
			_logger.LogInformation("Get schedule requested.");

			var response = new GetScheduleByIdResponse
			{
				Schedule = _scheduleStorage.ReadSchedule(request.ScheduleId)
			};
			return response;
		}
	}
}