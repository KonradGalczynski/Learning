using System;
using System.Collections.Generic;
using ServiceStack;

namespace ScheduleWebApiServiceStackContract
{
	[Route("/api/schedule/{ScheduleId}", "GET")]
	public class GetScheduleByIdRequest : IReturn<List<ScheduleEntry>>
	{
		public Guid ScheduleId { get; set; }
	}
}
