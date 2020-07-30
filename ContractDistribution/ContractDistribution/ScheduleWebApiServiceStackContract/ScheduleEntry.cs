using System;

namespace ScheduleWebApiServiceStackContract
{
	public class ScheduleEntry
	{
		public Guid Identifier { get; set; }
		public int Order { get; set; }
	}
}