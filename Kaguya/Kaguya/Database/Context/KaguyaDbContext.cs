using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
		public DbSet<KaguyaServer> Servers { get; set; }
		public DbSet<KaguyaUser> Users { get; set; }


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
		}

		partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
	}
}