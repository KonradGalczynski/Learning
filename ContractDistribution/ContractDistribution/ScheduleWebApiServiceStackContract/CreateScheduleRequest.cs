﻿using System.Collections.Generic;
using System.Threading.Tasks;
using ServiceStack;

namespace ScheduleWebApiServiceStackContract
{
	[Route("/api/schedule", "POST")]
	public class CreateScheduleRequest : IReturn<CreateScheduleResponse>
	{
		public List<WorkloadItem> WorkloadItems { get; set; }
	}
}
