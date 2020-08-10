using ContractDistributionServiceStackWebApi.Services.Schedule;
using Funq;
using ServiceStack;

namespace ContractDistributionServiceStackWebApi
{
	public class AppHost : AppHostBase
	{
		public AppHost()
			: base("ContractDistributionServiceStack", typeof(ScheduleService).Assembly) { }

		public override void Configure(Container container)
		{
			SetConfig(new HostConfig
			{
				DefaultRedirectPath = "/metadata",
				DebugMode = AppSettings.Get(nameof(HostConfig.DebugMode), false)
			});
		}
    }
}