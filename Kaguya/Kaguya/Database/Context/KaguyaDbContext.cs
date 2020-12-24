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
		public DbSet<PremiumKey> PremiumKeys { get; set; }
		public DbSet<LogConfiguration> LogConfigurations { get; set; }
		public DbSet<Reminder> Reminders { get; set; }
		public DbSet<Rep> Rep { get; set; }
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
			// Keys - Only specify if key is complex (unique by more than 1 property)
			
			// Key: FilteredWord
			modelBuilder.Entity<FilteredWord>().HasKey(w => new { w.ServerId, w.Word });
			
			/* Indexing
			 
			 If indexing properties separately, make a new block. 
			 Only include multiple properties in one block if querying them together.
			
			 Reference (Type)UserRepository.cs under Kaguya.Database.Repositories.
			 Whenever a query has a 'where' clause, we use that predicate to determine 
			 what to index. 
			 
			 Example:
			 _dbContext.Users.AsQueryable().Where(x => x.UserId == <param>) means that we 
			 would need to specify a KaguyaUser index for userid by doing the following:
			 
			 modelBuilder.Entity<KaguyaUser>().HasIndex(u => new
			{
				u.UserId
			});
			
			If we do a where clause with *multiple* predicates, such as 
			(x.UserId == <param> && x.Points < <param 2>), then we would compound the 
			index into one block. This is the only case in which we would combine more 
			than one object parameter into one index block.
			
			Do not make an additional index if the query is against the object's primary key.
			*/

			// Index: AdminAction
			modelBuilder.Entity<AdminAction>().HasIndex(a => new
			{
				a.ServerId
			});

			// Index: CommandHistory
			modelBuilder.Entity<CommandHistory>().HasIndex(ch => new
			{
				ch.UserId,
				ch.ServerId,
				ch.CommandName
			});
			
			// Index: Fish
			modelBuilder.Entity<Fish>().HasIndex(f => new
			{
				f.UserId
			});
			modelBuilder.Entity<Fish>().HasIndex(f => new
			{
				f.ServerId
			});
			modelBuilder.Entity<Fish>().HasIndex(f => new
			{
				f.UserId,
				f.FishType
			});
			modelBuilder.Entity<Fish>().HasIndex(f => new
			{
				f.UserId,
				f.Rarity
			});
			
			// Index: PremiumKey
			modelBuilder.Entity<PremiumKey>().HasIndex(p => new
			{
				p.Key
			});
			
			// Index: Rep
			modelBuilder.Entity<Rep>().HasIndex(r => new
			{
				r.UserId
			});
			
			modelBuilder.Entity<Rep>().HasIndex(r => new
			{
				r.TimeGiven
			});
			
			// Index: Reminders
			modelBuilder.Entity<Reminder>().HasIndex(r => new
			{
				r.UserId
			});
			
			modelBuilder.Entity<Reminder>().HasIndex(r => new
			{
				r.Expiration,
				r.HasTriggered
			});
		}

		partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
	}
}