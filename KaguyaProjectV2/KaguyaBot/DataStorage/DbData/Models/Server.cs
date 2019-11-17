using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "kaguyaserver")]
    public class Server
    {
        [PrimaryKey]
        public ulong Id { get; set; }
        [Column(Name = "CommandPrefix"), Nullable]
        public string CommandPrefix { get; set; } = "$";
        [Column(Name = "CommandCount"), NotNull]
        public int TotalCommandCount { get; set; }
        [Column(Name = "KaguyaServerLog"), Nullable]
        public ulong LogKaguyaServerLog { get; set; }
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
        [Column(Name = "Kicks"), Nullable]
        public ulong LogKicks { get; set; }
        [Column(Name = "Shadowbans"), Nullable]
        public ulong LogShadowbans { get; set; }
        [Column(Name = "Unshadowbans"), Nullable]
        public ulong LogUnshadowbans { get; set; }
        [Column(Name = "Warns"), Nullable]
        public ulong LogWarns { get; set; }
        [Column(Name = "Unwarns"), Nullable]
        public ulong LogUnwarns { get; set; }
        [Column(Name = "VoiceChannelConnections"), Nullable]
        public ulong LogVoiceChannelConnections { get; set; }
        [Column(Name = "LevelAnnouncements"), Nullable]
        public ulong LogLevelAnnouncements { get; set; }
        [Column(Name = "Antiraids"), Nullable]
        public ulong LogAntiraids { get; set; }
        [Column(Name = "TwitchNotifications"), Nullable]
        public ulong LogTwitchNotifications { get; set; }
        [Column(Name = "YoutubeNotifications"), Nullable]
        public ulong LogYouTubeNotifications { get; set; }
        [Column(Name = "RedditNotifications"), Nullable]
        public ulong LogRedditNotifications { get; set; }
        [Column(Name = "TwitterNotifications"), Nullable]
        public ulong LogTwitterNotifications { get; set; }
        [Column(Name = "IsBlacklisted"), NotNull]
        public bool IsBlacklisted { get; set; }
        [Column(Name = "IsPremium"), NotNull]
        public bool IsPremium { get; set; }
        [Column(Name = "AutoWarnOnBlacklistedPhrase"), NotNull]
        public bool AutoWarnOnBlacklistedPhrase { get; set; } = false;
        /// <summary>
        /// FK_KaguyaServer_MutedUsers_BackReference
        /// </summary>
        [Association(ThisKey = "Id", OtherKey = "ServerId")]
        public IEnumerable<MutedUser> MutedUsers { get; set; }
        /// <summary>
        /// FK_KaguyaServer_WarnActions_BackReference
        /// </summary>
        [Association(ThisKey = "Id", OtherKey = "ServerId")]
        public IEnumerable<WarnAction> WarnActions { get; set; }
        /// <summary>
        /// FK_KaguyaServer_FilteredPhrases_BackReference
        /// </summary>
        [Association(ThisKey = "Id", OtherKey = "ServerId")]
        public IEnumerable<FilteredPhrase> FilteredPhrases { get; set; }
        /// <summary>
        /// FK_KaguyaServer_AutoAssignedRoles_BackReference
        /// </summary>
        [Association(ThisKey = "Id", OtherKey = "ServerId")]
        public IEnumerable<AutoAssignedRole> AutoAssignedRoles { get; set; }
        /// <summary>
        /// FK_KaguyaServer_BlackListedChannels_BackReference
        /// </summary>
        [Association(ThisKey = "Id", OtherKey = "ServerId")]
        public IEnumerable<BlackListedChannel> BlackListedChannels { get; set; }
    }
}