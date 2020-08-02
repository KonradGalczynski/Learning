using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ScheduleWebApiServiceStackContract;

namespace ContractDistributionServiceStack.Services
{
	public class ScheduleStorage : IScheduleStorage
	{
		private readonly Dictionary<Guid, List<ScheduleEntry>> _storage = new Dictionary<Guid, List<ScheduleEntry>>();
		
		public Task<Guid> SaveScheduleAsync(List<ScheduleEntry> scheduleEntries)
		{
			var identifier = Guid.NewGuid();
			_storage.Add(identifier, scheduleEntries);
			return Task.FromResult(identifier);
		}

		public List<ScheduleEntry> ReadSchedule(Guid identifier)
		{
			return _storage[identifier];
		}
	}
}