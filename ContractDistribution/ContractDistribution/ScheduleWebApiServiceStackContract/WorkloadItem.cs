using System;

namespace ScheduleWebApiServiceStackContract
{
	public class WorkloadItem
	{
		public Guid Identifier { get; set; }
		public int DurationInHours { get; set; }
	}
}