using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ScheduleWebApiRefitContract;

namespace ContractDistributionAspNetCoreWebApi.Controllers.Schedule
{
	[ApiController]
	[Route("api/schedule")]
	public class ScheduleController : ControllerBase
	{
		private readonly IConfiguration _configuration;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly ILogger<ScheduleController> _logger;
		private readonly IScheduleStorage _scheduleStorage;

		public ScheduleController(IConfiguration configuration,
			IHttpClientFactory httpClientFactory,
			IScheduleStorage scheduleStorage,
			ILogger<ScheduleController> logger)
		{
			_configuration = configuration;
			_httpClientFactory = httpClientFactory;
			_scheduleStorage = scheduleStorage;
			_logger = logger;
		}

		[HttpPost]
		public async Task<Guid> CreateSchedule(List<WorkloadItem> workloadItems)
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
			return await _scheduleStorage.SaveScheduleAsync(CalculateScheduleFor(workloadItems, plannedMaintenanceWindow));
		}

		private static List<ScheduleEntry> CalculateScheduleFor(IEnumerable<WorkloadItem> workloadItems, 
			MaintenanceWindow plannedMaintenanceWindow)
		{
			return workloadItems.Select(x => new ScheduleEntry
			{
				Identifier = x.Identifier,
				Order = x.DurationInHours >= plannedMaintenanceWindow.LengthInHours ? 2 : 1
			}).ToList();
		}

		[HttpGet("{scheduleId}")]
		public List<ScheduleEntry> GetScheduleById(Guid scheduleId)
		{
			_logger.LogInformation("Get schedule requested.");

			return _scheduleStorage.ReadSchedule(scheduleId);
		}
	}
}