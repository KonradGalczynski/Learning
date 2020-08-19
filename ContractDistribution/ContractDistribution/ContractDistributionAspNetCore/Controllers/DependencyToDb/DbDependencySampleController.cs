using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContractDistributionAspNetCoreWebApi.Controllers.DependencyToDb
{
	[ApiController]
	[Route("api/db")]
	public class DbDependencySampleController : ControllerBase
	{
		private readonly StringStorageContext _context;

		public DbDependencySampleController(StringStorageContext context)
		{
			_context = context;
		}
		
		[HttpPost]
		public async Task<string> SaveItemAsync(string item)
		{
			var record = new Record
			{
				Id = Guid.NewGuid().ToString(),
				Item = item
			};
			await _context.Record.AddAsync(record);
			await _context.SaveChangesAsync();
			return record.Id;
		}
		
		[HttpGet("{itemId}")]
		public async Task<string> GetItemAsync(string itemId)
		{
			var element = await _context.Record.FirstOrDefaultAsync(x => x.Id == itemId);
			return element.Item;
		}
	}
}
