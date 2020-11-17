using KaguyaProjectV2.KaguyaBot.Core.Interfaces;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using LinqToDB;
using LinqToDB.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "kaguya_server")]
    public class Server : IKaguyaQueryable<Server>, IKaguyaUnique<Server>, IServerSearchable<Server>
    {
        [PrimaryKey]
        [Column(Name = "server_id")]
        public ulong ServerId { get; set; }

        [Column(Name = "command_prefix")]
        [Nullable]
        public string CommandPrefix { get; set; } = "$";

        [Column(Name = "command_count")]
        [NotNull]
        public int TotalCommandCount { get; set; }

        [Column(Name = "total_admin_actions")]
        [NotNull]
        public int TotalAdminActions { get; set; }

        [Column(Name = "praise_cooldown")]
        public int PraiseCooldown { get; set; } = 24;

        [Column(Name = "deleted_messages_log")]
        [Nullable]
        public ulong LogDeletedMessages { get; set; }

        [Column(Name = "updated_messages_log")]
        [Nullable]
        public ulong LogUpdatedMessages { get; set; }

        [Column(Name = "filtered_phrases_log")]
        [Nullable]
        public ulong LogFilteredPhrases { get; set; }

        [Column(Name = "user_joins_log")]
        [Nullable]
        public ulong LogUserJoins { get; set; }

        [Column(Name = "user_leaves_log")]
        [Nullable]
        public ulong LogUserLeaves { get; set; }

        [Column(Name = "bans_log")]
        [Nullable]
        public ulong LogBans { get; set; }

        [Column(Name = "unbans_log")]
        [Nullable]
        public ulong LogUnbans { get; set; }

        [Column(Name = "voice_channel_connections_log")]
        [Nullable]
        public ulong LogVoiceChannelConnections { get; set; }

        [Column(Name = "level_announcements_log")]
        [Nullable]
        public ulong LogLevelAnnouncements { get; set; }

        [Column(Name = "fish_levels_log")]
        [NotNull]
        public ulong LogFishLevels { get; set; }

        [Column(Name = "anti_raids_log")]
        [Nullable]
        public ulong LogAntiraids { get; set; }

        [Column(Name = "greetings_log")]
        [NotNull]
        public ulong LogGreetings { get; set; }
        
        [Column(Name = "warn_log")]
        [NotNull]
        public ulong LogWarns { get; set; }
        
        [Column(Name = "unwarn_log")]
        [NotNull]
        public ulong LogUnwarns { get; set; }
        
        [Column(Name = "shadowban_log")]
        [NotNull]
        public ulong LogShadowbans { get; set; }
        
        [Column(Name = "unshadowban_log")]
        [NotNull]
        public ulong LogUnshadowbans { get; set; }
        
        [Column(Name = "mute_log")]
        [NotNull]
        public ulong LogMutes { get; set; }
        
        [Column(Name = "unmute_log")]
        [NotNull]
        public ulong LogUnmutes { get; set; }
        
        [Column(Name = "is_blacklisted")]
        [Nullable]
        public bool IsBlacklisted { get; set; }

        [Column(Name = "custom_greeting")]
        [Nullable]
        public string CustomGreeting { get; set; }

        [Column(Name = "custom_greeting_enabled")]
        [NotNull]
        public bool CustomGreetingIsEnabled { get; set; }

        [Column(Name = "level_announcements_enabled")]
        [NotNull]
        public bool LevelAnnouncementsEnabled { get; set; } = true;

        [Column(Name = "osu_link_parsing_enabled")]
        [NotNull]
        public bool OsuLinkParsingEnabled { get; set; } = true;

        [Column(Name = "premium_expiration")]
        [NotNull]
        public double PremiumExpiration { get; set; }

        /// <summary>
        /// Upon anti-raid execution, if this value is set, Kaguya will send a DM to whoever was punished
        /// by the anti-raid service with this property as the message's content.
        /// </summary>
        [Column(Name = "antiraid_punishment_dm")]
        public string AntiraidPunishmentDirectMessage { get; set; }

        /// <summary>
        /// Whether or not the server currently has an active premium subscription.
        /// </summary>
        public bool IsPremium => PremiumExpiration > DateTime.Now.ToOADate();

        /// <summary>
        /// A boolean that determines whether the server is currently purging messages.
        /// We log this so that we don't bombard log channels with messages whenever they are bulk
        /// cleared. Instead, we use this boolean to determine whether to skip the 'Deleted Message'
        /// log event. We log bulk-deletion of messages by checking the audit log instead. This
        /// value is not in the database.
        /// </summary>
        [Column(Name = "is_currently_purging_messages")]
        [NotNull]
        public bool IsCurrentlyPurgingMessages { get; set; }

        [Column(Name = "next_quote_id")]
        [NotNull]
        public int NextQuoteId { get; set; } = 1;

        /// <summary>
        /// FK_AntiRaid_KaguyaServer_BackReference
        /// </summary>
        [Association(ThisKey = "ServerId", OtherKey = "ServerId")]
        public AntiRaidConfig AntiRaid { get; set; }

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
        /// FK_CommandHistory_KaguyaServer_BackReference
        /// </summary>
        [Association(ThisKey = "ServerId", OtherKey = "ServerId")]
        public IEnumerable<CommandHistory> CommandHistory { get; set; }

        /// <summary>
        /// FK_KaguyaServer_FilteredPhrases_BackReference
        /// </summary>
        [Association(ThisKey = "ServerId", OtherKey = "ServerId")]
        public IEnumerable<FilteredPhrase> FilteredPhrases { get; set; }

        /// <summary>
        /// FK_Fish_KaguyaServer_BackReference
        /// </summary>
        [Association(ThisKey = "ServerId", OtherKey = "ServerId")]
        public IEnumerable<Fish> Fish { get; set; }

        /// <summary>
        /// FK_KaguyaServer_MutedUsers_BackReference
        /// </summary>
        [Association(ThisKey = "ServerId", OtherKey = "ServerId")]
        public IEnumerable<MutedUser> MutedUsers { get; set; }

        /// <summary>
        /// FK_Praise_KaguyaServer_BackReference
        /// </summary>
        [Association(ThisKey = "ServerId", OtherKey = "ServerId")]
        public IEnumerable<Praise> Praise { get; set; }

        /// <summary>
        /// FK_Quotes_KaguyaServer_BackReference
        /// </summary>
        /// <value></value>
        [Association(ThisKey = "ServerId", OtherKey = "ServerId")]
        public IEnumerable<Quote> Quotes { get; set; }

        /// <summary>
        /// FK_ServerRoleRewards_KaguyaServer_BackReference
        /// </summary>
        [Association(ThisKey = "ServerId", OtherKey = "ServerId")]
        public IEnumerable<ServerRoleReward> RoleRewards { get; set; }

        /// <summary>
        /// FK_KaguyaServer_ServerExp_BackReference
        /// </summary>
        [Association(ThisKey = "ServerId", OtherKey = "ServerId")]
        public IEnumerable<ServerExp> ServerExp { get; set; }

        /// <summary>
        /// FK_WarnedUsers_KaguyaServer_BackReference
        /// </summary>
        [Association(ThisKey = "ServerId", OtherKey = "ServerId")]
        public IEnumerable<WarnedUser> WarnedUsers { get; set; }

        /// <summary>
        /// FK_WarnSettings_KaguyaServer_BackReference
        /// </summary>
        [Association(ThisKey = "ServerId", OtherKey = "ServerId")]
        public WarnSetting WarnSettings { get; set; }
    }
}