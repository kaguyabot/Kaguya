using Kaguya.Database.Model;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace Kaguya.Database.Context
{
	public partial class KaguyaDbContext : DbContext
	{
		public DbSet<AdminAction> AdminActions { get; set; }
		public DbSet<BlacklistedEntity> BlacklistedEntities { get; set; }
		public DbSet<CommandHistory> CommandHistories { get; set; }
		public DbSet<FilteredWord> FilteredWords { get; set; }
		public DbSet<Fish> Fish { get; set; }
		public DbSet<LogConfiguration> LogConfigurations { get; set; }
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
			modelBuilder.Entity<FilteredWord>().HasKey(w => new { w.ServerId, w.Word });
			modelBuilder.Entity<Fish>().HasKey(f => new
			{
				f.FishId
			});
			modelBuilder.Entity<Fish>().HasIndex(f => new
			{
				f.UserId,
				f.ServerId,
				f.ChannelId,
				f.TimeCaught,
				f.FishType,
				f.Rarity
			});
		}

		partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
	}
}