using System.Collections.Generic;

namespace ScheduleWebApiServiceStackContract
{
	public class GetScheduleByIdResponse
	{
		public List<ScheduleEntry> Schedule { get; set; }
	}
}