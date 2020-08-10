using Microsoft.EntityFrameworkCore;

namespace ContractDistributionNetCoreWebApi.Controllers.DependencyToDb
{
	public class StringStorageContext : DbContext
	{
		public StringStorageContext(DbContextOptions options)
			: base(options)
		{
		}

		public DbSet<Record> Record { get; set; }


	}
}