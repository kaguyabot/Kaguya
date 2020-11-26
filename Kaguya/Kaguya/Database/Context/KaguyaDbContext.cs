using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Kaguya;
using Kaguya.Database.Model;

#nullable disable

namespace Kaguya.Database.Context
{
	public partial class KaguyaDbContext : DbContext
	{
		public DbSet<AdminAction> AdminActions { get; set; }
		public DbSet<KaguyaServer> Servers { get; set; }
		public DbSet<KaguyaUser> Users { get; set; }
		public DbSet<BlacklistedEntity> BlacklistedEntities { get; set; }
		public DbSet<WordFilter> WordFilters { get; set; }

		public KaguyaDbContext(DbContextOptions<KaguyaDbContext> options)
			: base(options)
		{
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			if (!optionsBuilder.IsConfigured)
			{
			}
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			OnModelCreatingPartial(modelBuilder);
			modelBuilder.Entity<WordFilter>().HasKey(w => new { w.ServerId, w.Word });
		}

		partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
	}
}