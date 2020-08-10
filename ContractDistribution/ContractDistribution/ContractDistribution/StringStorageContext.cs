using Microsoft.EntityFrameworkCore;

namespace ContractDistributionNetCoreWebApi
{
	public class StringStorageContext : DbContext
	{
		public StringStorageContext(DbContextOptions options)
			: base(options)
		{
		}

		public DbSet<string> Item { get; set; }
	}
}