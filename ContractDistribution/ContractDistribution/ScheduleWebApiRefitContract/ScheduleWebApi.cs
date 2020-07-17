using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Refit;

namespace ScheduleWebApiRefitContract
{
	public interface IScheduleWebApi
	{
		[Post("/api/schedule")]
		Task<Guid> CreateSchedule();

		[Get("/api/schedule/{scheduleId}")]
		Task<List<ScheduleEntry>> GetScheduleById(Guid scheduleId);
	}
}
