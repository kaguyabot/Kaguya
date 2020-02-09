using KaguyaProjectV2.KaguyaBot.Core.Interfaces;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using LinqToDB;
using LinqToDB.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "kaguyaserver")]
    public class Server : IKaguyaQueryable<Server>, IKaguyaUnique<Server>, IServerSearchable<Server>
    {
        [PrimaryKey]
        public ulong ServerId { get; set; }
        [Column(Name = "CommandPrefix"), Nullable]
        public string CommandPrefix { get; set; } = "$";
        [Column(Name = "CommandCount"), NotNull]
        public int TotalCommandCount { get; set; }
        [Column(Name = "TotalAdminActions"), NotNull]
        public int TotalAdminActions { get; set; }
        [Column(Name = "PraiseCooldown")]
        public int PraiseCooldown { get; set; } = 24;
        [Column(Name = "ModLog"), Nullable]
        public ulong ModLog { get; set; }
        [Column(Name = "DeletedMessages"), Nullable]
        public ulong LogDeletedMessages { get; set; }
        [Column(Name = "UpdatedMessages"), Nullable]
        public ulong LogUpdatedMessages { get; set; }
        [Column(Name = "FilteredPhrases"), Nullable]
        public ulong LogFilteredPhrases { get; set; }
        [Column(Name = "UserJoins"), Nullable]
        public ulong LogUserJoins { get; set; }
        [Column(Name = "UserLeaves"), Nullable]
        public ulong LogUserLeaves { get; set; }
        [Column(Name = "Bans"), Nullable]
        public ulong LogBans { get; set; }
        [Column(Name = "Unbans"), Nullable]
        public ulong LogUnbans { get; set; }
        [Column(Name = "VoiceChannelConnections"), Nullable]
        public ulong LogVoiceChannelConnections { get; set; }
        [Column(Name = "LevelAnnouncements"), Nullable]
        public ulong LogLevelAnnouncements { get; set; }
        [Column(Name = "FishLevels"), NotNull]
        public ulong LogFishLevels { get; set; }
        [Column(Name = "Antiraids"), Nullable]
        public ulong LogAntiraids { get; set; }
        [Column(Name = "Greetings"), NotNull]
        public ulong LogGreetings { get; set; }
        [Column(Name = "TwitchNotifications"), Nullable]
        public ulong LogTwitchNotifications { get; set; }
        [Column(Name = "IsBlacklisted"), Nullable]
        public bool IsBlacklisted { get; set; }
        [Column(Name = "CustomGreeting"), Nullable]
        public string CustomGreeting { get; set; }
        [Column(Name = "CustomGreetingIsEnabled"), NotNull]
        public bool CustomGreetingIsEnabled { get; set; }
        [Column(Name = "LevelAnnouncementsEnabled"), NotNull]
        public bool LevelAnnouncementsEnabled { get; set; }
        public double PremiumExpirationDate
        {
            get
            {
                var now = DateTime.Now.ToOADate();
                var allUserKeys = DatabaseQueries.GetAllForServerAsync<PremiumKey>(ServerId).Result;
                if(allUserKeys.Count > 0)
                    return now + allUserKeys.Sum(key => key.Expiration - now);
                return DateTime.MinValue.ToOADate();
            }
        }
        
        /// <summary>
        /// Whether or not the server currently has an active premium subscription.
        /// </summary>
        public bool IsPremium => PremiumExpirationDate > DateTime.Now.ToOADate();

        [Column(Name = "AutoWarnOnBlacklistedPhrase"), NotNull]
        public bool AutoWarnOnBlacklistedPhrase { get; set; } = false;

        /// <summary>
        /// A boolean that determines whether the server is currently purging messages.
        /// We log this so that we don't bombard log channels with messages whenever they are bulk
        /// cleared. Instead, we use this boolean to determine whether to skip the 'Deleted Message'
        /// log event. We log bulk-deletion of messages by checking the audit log instead. This
        /// value is not in the database.
        /// </summary>
        [Column(Name = "IsCurrentlyPurgingMessages"), NotNull]
        public bool IsCurrentlyPurgingMessages { get; set; } = false; //Not in database.
        /// <summary>
        /// FK_KaguyaServer_MutedUsers_BackReference
        /// </summary>
        [Association(ThisKey = "ServerId", OtherKey = "ServerId")]
        public IEnumerable<MutedUser> MutedUsers { get; set; }
        /// <summary>
        /// FK_KaguyaServer_FilteredPhrases_BackReference
        /// </summary>
        [Association(ThisKey = "ServerId", OtherKey = "ServerId")]
        public IEnumerable<FilteredPhrase> FilteredPhrases { get; set; }
        /// <summary>
        /// FK_KaguyaServer_AutoAssignedRoles_BackReference
        /// </summary>
        [Association(ThisKey = "ServerId", OtherKey = "ServerId")]
        public IEnumerable<AutoAssignedRole> AutoAssignedRoles { get; set; }
        /// <summary>
        /// FK_KaguyaServer_BlackListedChannels_BackReference
        /// </summary>
        [Association(ThisKey = "ServerId", OtherKey = "ServerId")]
        public IEnumerable<BlackListedChannel> BlackListedChannels { get; set; }
        /// <summary>
        /// FK_KaguyaServer_WarnedUsers_BackReference
        /// </summary>
        [Association(ThisKey = "ServerId", OtherKey = "ServerId")]
        public IEnumerable<WarnedUser> WarnedUsers { get; set; }
        /// <summary>
        /// FK_KaguyaServer_ServerExp_BackReference
        /// </summary>
        [Association(ThisKey = "ServerId", OtherKey = "ServerId")]
        public IEnumerable<ServerExp> ServerExp { get; set; }

        [Association(ThisKey = "ServerId", OtherKey = "ServerId")]
        public IEnumerable<Praise> Praise { get; set; }

        [Association(ThisKey = "ServerId", OtherKey = "ServerId")]
        public IEnumerable<AntiRaidConfig> AntiRaid { get; set; }
    }
}