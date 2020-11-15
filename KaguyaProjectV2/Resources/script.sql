create table if not exists eightball
(
	Response varchar(500) not null,
	primary key (Response)
);

create table if not exists favorite_tracks
(
	user_id bigint unsigned not null,
	track_id text not null,
	track_title text not null,
	track_author text not null,
	track_duration double not null
);

create table if not exists kaguyaserver
(
	server_id bigint unsigned not null,
	command_prefix varchar(5) default '$' null,
	command_count bigint unsigned null,
	total_admin_actions int(7) null,
	praise_cooldown int(3) default 24 not null,
	deleted_messages_log bigint unsigned null,
	updated_messages_log bigint unsigned null,
	filtered_phrases_log bigint unsigned null,
	user_joins_log bigint unsigned null,
	user_leaves_log bigint unsigned null,
	bans_log bigint unsigned null,
	unbans_log bigint unsigned null,
	voice_channel_connections_log bigint unsigned null,
	level_announcements_log bigint unsigned null,
	fish_levels_log bigint unsigned not null,
	anti_raids_log bigint unsigned null,
	greetings_log bigint default 0 not null,
	warn_log bigint unsigned not null,
	unwarn_log bigint unsigned not null,
	shadowban_log bigint unsigned not null,
	unshadowban_log bigint unsigned not null,
	mute_log bigint unsigned not null,
	unmute_log bigint unsigned not null,
	next_quote_id int(7) default 1 not null,
	premium_expiration double default 0 not null,
	antiraid_punishment_dm text null,
	is_blacklisted bit not null,
	is_currently_purging_messages bit not null,
	custom_greeting text null,
	custom_greeting_enabled tinyint default 0 not null,
	level_announcements_enabled tinyint default 1 not null,
	osu_link_parsing_enabled tinyint default 1 null,
	primary key (server_id)
)
charset=utf8;

create table if not exists antiraid
(
	ServerId bigint unsigned not null,
	Users int(3) not null,
	Seconds int(4) not null,
	Action varchar(10) not null,
	primary key (ServerId),
	constraint FK_AntiRaid_KaguyaServer_BackReference
		foreign key (ServerId) references kaguyaserver (server_id)
);

create table if not exists autoassignedroles
(
	ServerId bigint unsigned not null,
	RoleId bigint unsigned not null,
	constraint fk_AutoAssignedRoles_KaguyaServer1
		foreign key (ServerId) references kaguyaserver (server_id)
)
charset=utf8;

create index fk_AutoAssignedRoles_KaguyaServer1_idx
	on autoassignedroles (ServerId);

create table if not exists blacklistedchannels
(
	ServerId bigint unsigned not null,
	ChannelId bigint unsigned not null,
	Expiration double not null,
	constraint fk_BlackListedChannels_KaguyaServer1
		foreign key (ServerId) references kaguyaserver (server_id)
)
charset=utf8;

create index fk_BlackListedChannels_KaguyaServer1_idx
	on blacklistedchannels (ServerId);

create table if not exists filteredphrases
(
	ServerId bigint unsigned not null,
	Phrase text not null,
	constraint fk_FilteredPhrases_KaguyaServer1
		foreign key (ServerId) references kaguyaserver (server_id)
)
charset=utf8;

create index fk_FilteredPhrases_KaguyaServer1_idx
	on filteredphrases (ServerId);

create table if not exists kaguyauser
(
	UserId bigint unsigned default 0 not null,
	Experience int default 0 not null,
	FishExp int default 0 not null,
	Points int default 0 not null,
	OsuId int default 0 not null,
	CommandUses int default 0 not null,
	TotalDaysSupported int(4) default 0 not null,
	NSFWImages int default 0 not null,
	ActiveRateLimit int(1) default 0 not null,
	RateLimitWarnings int default 0 not null,
	GamblingWins int default 0 not null,
	GamblingLosses int default 0 not null,
	CurrencyAwarded int default 0 not null,
	CurrencyLost int default 0 not null,
	RollWins int default 0 not null,
	RollLosses int default 0 not null,
	QuickdrawWins int default 0 not null,
	QuickDrawLosses int default 0 not null,
	FishBait int(8) default 0 not null,
	TotalUpvotes int(7) default 0 not null,
	LastGivenExp double default 0 not null,
	LastDailyBonus double default 0 not null,
	LastWeeklyBonus double default 0 not null,
	LastGivenRep double default 0 not null,
	LastRatelimited double default 0 not null,
	LastFished double default 0 not null,
	ExpDMNotificationType int(1) default 3 not null,
	ExpChatNotificationType int(1) default 2 not null,
	OsuBeatmapsLinked int(7) default 0 not null,
	PremiumExpiration double default 0 not null,
	primary key (UserId)
)
comment 'ExpTypePreference: 0 = Global, 1 = Server, 2 = Both, 3 = None' charset=utf8;

create table if not exists commandhistory
(
	ServerId bigint unsigned not null,
	UserId bigint unsigned not null,
	Command text not null,
	Timestamp datetime not null,
	constraint FK_CommandHistory_KaguyaServer_BackReference
		foreign key (ServerId) references kaguyaserver (server_id),
	constraint FK_CommandHistory_KaguyaUser_BackReference
		foreign key (UserId) references kaguyauser (UserId)
)
charset=utf8;

create index FK_CommandHistory_KaguyaServer_BackReference_idx
	on commandhistory (ServerId);

create index fk_CommandHistory_KaguyaUser1_idx
	on commandhistory (UserId);

create table if not exists fish
(
	FishId bigint(10) unsigned not null,
	UserId bigint unsigned not null,
	ServerId bigint unsigned not null,
	TimeCaught double not null,
	Fish int(2) not null,
	FishString varchar(50) not null,
	Value int(8) not null,
	Exp int(5) not null,
	Sold tinyint not null,
	primary key (FishId),
	constraint FK_Fish_KaguyaServer_BackReference
		foreign key (ServerId) references kaguyaserver (server_id),
	constraint FK_Fish_KaguyaUser_BackReference
		foreign key (UserId) references kaguyauser (UserId)
);

create index FK_Fish_KaguyaServer_BackReference_idx
	on fish (ServerId);

create table if not exists gamblehistory
(
	UserId bigint unsigned not null,
	Action int(1) not null,
	ActionString text not null,
	Bet int(10) not null,
	Payout int(14) not null,
	Roll int(5) not null,
	Time double not null,
	Winner tinyint not null,
	constraint FK_GambleHistory_KaguyaUser_BackReference
		foreign key (UserId) references kaguyauser (UserId)
)
charset=utf8;

create index fk_GambleHistory_KaguyaUser1_idx
	on gamblehistory (UserId);

create table if not exists mutedusers
(
	ServerId bigint unsigned not null,
	UserId bigint unsigned not null,
	ExpiresAt double not null,
	primary key (ServerId),
	constraint fk_MutedUsers_KaguyaServer1
		foreign key (ServerId) references kaguyaserver (server_id),
	constraint fk_MutedUsers_KaguyaUser1
		foreign key (UserId) references kaguyauser (UserId)
)
charset=utf8;

create index fk_MutedUsers_KaguyaServer1_idx
	on mutedusers (ServerId);

create index fk_MutedUsers_KaguyaUser1_idx
	on mutedusers (UserId);

create table if not exists owner_giveaway
(
	id int auto_increment
		primary key,
	message_id bigint not null,
	channel_id bigint not null,
	exp int not null,
	points int not null,
	expiration double not null,
	has_expired tinyint default 0 not null
);

create table if not exists owner_giveaway_reactions
(
	owner_giveaway_id int not null,
	user_id bigint not null
);

create table if not exists praise
(
	UserId bigint unsigned not null,
	ServerId bigint unsigned not null,
	GivenBy bigint unsigned not null,
	TimeGiven double not null,
	Reason text not null,
	constraint FK_Praise_KaguyaServer_BackReference
		foreign key (ServerId) references kaguyaserver (server_id)
)
comment 'Server specific rep.';

create index FK_Praise_KaguyaUser_BackReference_idx
	on praise (UserId);

create index FK_Praise_KaguyaUser_BackReference_idx1
	on praise (ServerId);

create table if not exists premiumkeys
(
	`key` varchar(50) not null,
	length_in_seconds bigint not null,
	key_creator_id bigint unsigned not null,
	user_id bigint unsigned null,
	server_id bigint unsigned null,
	has_expired tinyint(1) not null,
	primary key (`key`)
);

create table if not exists quotes
(
	UserId bigint unsigned not null,
	ServerId bigint unsigned not null,
	Text text not null,
	TimeStamp double not null,
	Id int(10) not null,
	constraint FK_Quotes_KaguyaServer_BackReference
		foreign key (ServerId) references kaguyaserver (server_id),
	constraint FK_Quotes_KaguyaUser_BackReference
		foreign key (UserId) references kaguyauser (UserId)
);

create index FK_Quotes_KaguyaServer_BackReference_idx
	on quotes (ServerId);

create index FK_Quotes_KaguyaUser_BackReference_idx
	on quotes (UserId);

create table if not exists reactionroles
(
	roleid bigint unsigned not null,
	messageid bigint unsigned not null,
	serverid bigint unsigned not null,
	emotenameid text null
);

create table if not exists reminders
(
	UserId bigint unsigned not null,
	Expiration double not null,
	Text text not null,
	HasTriggered tinyint not null,
	constraint FK_KaguyaUser_Reminder_BackReference
		foreign key (UserId) references kaguyauser (UserId)
);

create index FK_KaguyaUser_Reminder_BackReference_idx
	on reminders (UserId);

create table if not exists rep
(
	UserId bigint unsigned not null,
	GivenBy bigint unsigned not null,
	TimeGiven double not null,
	Reason text not null,
	constraint FK_Rep_KaguyaUser_BackReference
		foreign key (UserId) references kaguyauser (UserId)
);

create index FK_Rep_KaguyaUser_BackReference_idx
	on rep (UserId);

create table if not exists serverexp
(
	Serverid bigint unsigned not null,
	UserId bigint unsigned not null,
	Exp int(10) not null,
	LatestExp double not null,
	primary key (Serverid, UserId),
	constraint FK_ServerExp_KaguyaUser_BackReference
		foreign key (UserId) references kaguyauser (UserId)
)
charset=utf8;

create index FK_ServerExp_KaguyaUser_BackReference_idx
	on serverexp (UserId);

create table if not exists serverrolerewards
(
	ServerId bigint unsigned not null,
	RoleId bigint unsigned not null,
	Level int(5) not null,
	constraint FK_ServerRoleRewards_KaguyaServer_BackReference
		foreign key (ServerId) references kaguyaserver (server_id)
);

create index FK_ServerRoleRewards_KaguyaServer_BackReference_idx
	on serverrolerewards (ServerId);

create table if not exists stats
(
	kaguya_users int null,
	guilds int null,
	guild_users int null,
	shards int null,
	commands int null,
	commands_last24hours int not null,
	fish int null,
	points bigint null,
	gambles int null,
	time_stamp datetime null,
	text_channels int not null,
	voice_channels int not null,
	ram_usage double not null,
	latency_ms int null,
	uptime_seconds mediumtext not null,
	version text not null
);

create table if not exists upvotes
(
	VoteId varchar(100) not null,
	BotId bigint unsigned not null,
	UserId bigint unsigned not null,
	Time double not null,
	VoteType varchar(10) not null,
	IsWeekend tinyint not null,
	QueryParams text null,
	ReminderSent tinyint default 0 not null,
	primary key (VoteId)
);

create table if not exists userblacklists
(
	UserId bigint unsigned not null,
	Expiration double not null,
	Reason text not null,
	primary key (UserId),
	constraint FK_UserBlacklists_KaguyaUser_BackReference
		foreign key (UserId) references kaguyauser (UserId)
);

create table if not exists warnedusers
(
	ServerId bigint unsigned not null,
	UserId bigint unsigned not null,
	ModeratorName text not null,
	Reason text not null,
	Date double not null,
	constraint FK_WarnedUsers_KaguyaServer_BackReference
		foreign key (ServerId) references kaguyaserver (server_id)
)
charset=utf8;

create index FK_WarnedUsers_KaguyaServer_BackReference_idx
	on warnedusers (ServerId);

create table if not exists warnsettings
(
	ServerId bigint unsigned not null,
	Mute int(2) default 0 not null,
	Kick int(2) default 0 not null,
	Shadowban int(2) default 0 not null,
	Ban int(2) default 0 not null,
	primary key (ServerId),
	constraint FK_WarnSettings_KaguyaServer_BackReference
		foreign key (ServerId) references kaguyaserver (server_id)
)
charset=utf8;

create index fk_WarnActions_KaguyaServer1_idx
	on warnsettings (ServerId);


