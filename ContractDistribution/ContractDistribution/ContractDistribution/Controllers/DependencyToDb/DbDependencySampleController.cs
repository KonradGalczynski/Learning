using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ContractDistributionNetCoreWebApi.Controllers.DependencyToDb
{
	[ApiController]
	[Route("api/db")]
	public class DbDependencySampleController : ControllerBase
	{
		[HttpPost]
		public async Task<Guid> SaveStringAsync(string item)
		{
		}

		[HttpGet("{stringId}")]
		public string GetStringAsync(Guid stringId)
		{
		}
	}
}
