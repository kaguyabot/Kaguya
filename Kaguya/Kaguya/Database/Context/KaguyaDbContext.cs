using Kaguya.Database.Model;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace Kaguya.Database.Context
{
	public partial class KaguyaDbContext : DbContext
	{
		// All DbSets are listed here in alphabetical order.
		public DbSet<AdminAction> AdminActions { get; set; }
		public DbSet<AntiRaidConfig> AntiRaidConfigs { get; set; }
		public DbSet<AutoAssignedRole> AutoAssignedRoles { get; set; }
		public DbSet<BlacklistedEntity> BlacklistedEntities { get; set; }
		public DbSet<CommandHistory> CommandHistories { get; set; }
		public DbSet<Eightball> Eightballs { get; set; }
		public DbSet<FavoriteTrack> FavoriteTracks { get; set; }
		public DbSet<FilteredWord> FilteredWords { get; set; }
		public DbSet<Fish> Fish { get; set; }
		public DbSet<GambleHistory> GambleHistories { get; set; }
		public DbSet<Giveaway> Giveaways { get; set; }
		public DbSet<KaguyaServer> KaguyaServers { get; set; }
		public DbSet<KaguyaStatistics> KaguyaStatistics { get; set; }
		public DbSet<KaguyaUser> KaguyaUsers { get; set; }
		public DbSet<LogConfiguration> LogConfigurations { get; set; }
		public DbSet<PremiumKey> PremiumKeys { get; set; }
		public DbSet<Quote> Quotes { get; set; }
		public DbSet<ReactionRole> ReactionRoles { get; set; }
		public DbSet<Reminder> Reminders { get; set; }
		public DbSet<Rep> Rep { get; set; }
		public DbSet<RoleReward> RoleRewards { get; set; }
		public DbSet<ServerExperience> ServerExperience { get; set; }
		public DbSet<Upvote> Upvotes { get; set; }
		public DbSet<WarnConfiguration> WarnConfigurations { get; set; }

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

			modelBuilder.Entity<AutoAssignedRole>().HasKey(x => new
			{
				x.ServerId,
				x.RoleId
			});
			
			modelBuilder.Entity<FavoriteTrack>().HasKey(x => new
			{
				x.UserId,
				x.SongId
			});

			modelBuilder.Entity<FilteredWord>().HasKey(x => new
			{
				x.ServerId,
				x.Word
			});
			
			modelBuilder.Entity<ReactionRole>().HasKey(x => new
			{
				x.MessageId,
				x.RoleId
			});
			
			modelBuilder.Entity<ServerExperience>().HasKey(x => new
			{
				x.ServerId,
				x.UserId
			});
			
			/* Indexing
			 
			 If indexing properties separately, make a new block. 
			 Only include multiple properties in one block if querying them together.
			
			 Reference (Type)Repository.cs under Kaguya.Database.Repositories.
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
			modelBuilder.Entity<AdminAction>().HasIndex(x => new
			{
				x.ServerId
			});
			
			modelBuilder.Entity<AdminAction>().HasIndex(x => new
			{
				x.ServerId,
				x.Action
			});
			
			modelBuilder.Entity<AdminAction>().HasIndex(x => new
			{
				x.ActionedUserId,
				x.ServerId,
				x.Expiration
			});
			
			modelBuilder.Entity<AdminAction>().HasIndex(x => new
			{
				x.ActionedUserId,
				x.ServerId,
				x.Action,
				x.Expiration
			});

			// Index: CommandHistory
			modelBuilder.Entity<CommandHistory>().HasIndex(x => new
			{
				x.UserId,
				x.ServerId,
				x.CommandName
			});
			
			// Index: FavoriteTrack
			modelBuilder.Entity<FavoriteTrack>().HasIndex(x => new
			{
				x.UserId
			});
			
			// Index: FilteredWord
			modelBuilder.Entity<FilteredWord>().HasIndex(x => new
			{
				x.ServerId
			});

			// Index: Fish
			modelBuilder.Entity<Fish>().HasIndex(x => new
			{
				x.UserId
			});
			
			modelBuilder.Entity<Fish>().HasIndex(x => new
			{
				x.ServerId
			});
			
			modelBuilder.Entity<Fish>().HasIndex(x => new
			{
				x.UserId,
				x.FishType
			});
			
			modelBuilder.Entity<Fish>().HasIndex(x => new
			{
				x.UserId,
				x.Rarity
			});
			
			// Index: GambleHistory
			modelBuilder.Entity<GambleHistory>().HasIndex(x => new
			{
				x.UserId
			});
			
			modelBuilder.Entity<GambleHistory>().HasIndex(x => new
			{
				x.ServerId
			});
			
			modelBuilder.Entity<GambleHistory>().HasIndex(x => new
			{
				x.UserId,
				x.IsWinner
			});
			
			// Index: Giveaway
			modelBuilder.Entity<Giveaway>().HasIndex(x => new
			{
				x.Expiration
			});
			
			modelBuilder.Entity<Giveaway>().HasIndex(x => new
			{
				x.ServerId,
				x.Expiration
			});
			
			// Index: KaguyaUser
			modelBuilder.Entity<KaguyaUser>().HasIndex(x => new
			{
				x.ActiveRateLimit
			});
			
			// Index: PremiumKey
			modelBuilder.Entity<PremiumKey>().HasIndex(x => new
			{
				x.Key
			});
			
			// Index: Quote
			modelBuilder.Entity<Quote>().HasIndex(x => new
			{
				x.ServerId
			});
			
			// Index: Reminder
			modelBuilder.Entity<Reminder>().HasIndex(x => new
			{
				x.UserId
			});
			
			modelBuilder.Entity<Reminder>().HasIndex(x => new
			{
				x.UserId,
				x.Expiration // Utilized in the "NeedsDelivery" method of the Reminder class.
			});
			
			// Index: Rep
			modelBuilder.Entity<Rep>().HasIndex(x => new
			{
				x.UserId
			});
			
			// Index: RoleReward
			modelBuilder.Entity<RoleReward>().HasIndex(x => new
			{
				x.ServerId
			});
			
			// Index: Upvote
			modelBuilder.Entity<Upvote>().HasIndex(x => new
			{
				x.UserId
			});
		}

		partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
	}
}