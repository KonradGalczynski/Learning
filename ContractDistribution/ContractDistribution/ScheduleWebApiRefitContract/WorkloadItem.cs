using System;

namespace ScheduleWebApiRefitContract
{
	public class WorkloadItem
	{
		public Guid Identifier { get; set; }
		public int DurationInHours { get; set; }
	}
}