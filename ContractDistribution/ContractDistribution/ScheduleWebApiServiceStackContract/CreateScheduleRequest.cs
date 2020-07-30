using System;
using System.Collections.Generic;
using ServiceStack;

namespace ScheduleWebApiServiceStackContract
{
	[Route("/api/schedule", "POST")]
	public class CreateScheduleRequest : IReturn<Guid>
	{
		public List<WorkloadItem> WorkloadItems { get; set; }

	}
}
