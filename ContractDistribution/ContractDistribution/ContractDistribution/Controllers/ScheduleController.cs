using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ScheduleWebApiRefitContract;

namespace ContractDistribution.Controllers
{
	[ApiController]
	[Route("api/schedule")]
	public class ScheduleController : ControllerBase
	{
		private readonly ILogger<ScheduleController> _logger;

		public ScheduleController(ILogger<ScheduleController> logger)
		{
			_logger = logger;
		}

		[HttpPost]
		public Guid CreateSchedule()
		{
			_logger.LogInformation("Create schedule requested.");
			return Guid.NewGuid();
		}

		[HttpGet("{scheduleId}")]
		public List<ScheduleEntry> GetScheduleById(Guid scheduleId)
		{
			_logger.LogInformation("Get schedule requested.");
			
			return new List<ScheduleEntry>();
		}
	}
}
