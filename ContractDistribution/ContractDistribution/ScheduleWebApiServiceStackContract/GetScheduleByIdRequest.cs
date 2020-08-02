using System;
using ServiceStack;

namespace ScheduleWebApiServiceStackContract
{
	[Route("/api/schedule/{ScheduleId}", "GET")]
	public class GetScheduleByIdRequest : IReturn<GetScheduleByIdResponse>
	{
		public Guid ScheduleId { get; set; }
	}
}
