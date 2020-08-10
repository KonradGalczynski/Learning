using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ScheduleWebApiRefitContract;

namespace ContractDistributionNetCoreWebApi.Controllers.Schedule
{
	public interface IScheduleStorage
	{
		Task<Guid> SaveScheduleAsync(List<ScheduleEntry> scheduleEntries);
		List<ScheduleEntry> ReadSchedule(Guid identifier);
	}
}