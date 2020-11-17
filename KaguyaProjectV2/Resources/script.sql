create table if not exists eightball
(
    response varchar(500) not null,
    primary key (response)
);

create table if not exists favorite_tracks
(
    user_id bigint unsigned not null,
    track_id text not null,
    track_title text not null,
    track_author text not null,
    track_duration double not null
);

create table if not exists kaguya_server
(
    server_id bigint unsigned not null,
    command_prefix varchar(5) default '$' null,
    command_count bigint unsigned null,
    total_admin_actions int null,
    praise_cooldown int default 24 not null,
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
    warn_log bigint not null,
    unwarn_log bigint not null,
    shadowban_log bigint not null,
    unshadowban_log bigint not null,
    mute_log bigint not null,
    unmute_log bigint null,
    level_announcements_enabled tinyint default 1 not null,
    osu_link_parsing_enabled tinyint default 1 null,
    next_quote_id int default 1 not null,
    premium_expiration double default 0 not null,
    antiraid_punishment_dm text null,
    is_blacklisted bit not null,
    is_currently_purging_messages bit not null,
    custom_greeting text null,
    custom_greeting_enabled tinyint default 0 not null,
    primary key (server_id)
)
    charset=utf8;

create table if not exists antiraid
(
    server_id bigint unsigned not null,
    users int not null,
    seconds int not null,
    action varchar(10) not null,
    primary key (server_id),
    constraint FK_AntiRaid_KaguyaServer_BackReference
        foreign key (server_id) references kaguya_server (server_id)
);

create table if not exists auto_assigned_roles
(
    server_id bigint unsigned not null,
    role_id bigint unsigned not null,
    constraint fk_AutoAssignedRoles_KaguyaServer1
        foreign key (server_id) references kaguya_server (server_id)
)
    charset=utf8;

create index fk_AutoAssignedRoles_KaguyaServer1_idx
    on auto_assigned_roles (server_id);

create table if not exists blacklisted_channels
(
    server_id bigint unsigned not null,
    channel_id bigint unsigned not null,
    expiration double not null,
    constraint fk_BlackListedChannels_KaguyaServer1
        foreign key (server_id) references kaguya_server (server_id)
)
    charset=utf8;

create index fk_BlackListedChannels_KaguyaServer1_idx
    on blacklisted_channels (server_id);

create table if not exists filtered_phrases
(
    server_id bigint unsigned not null,
    phrase text not null,
    constraint fk_FilteredPhrases_KaguyaServer1
        foreign key (server_id) references kaguya_server (server_id)
)
    charset=utf8;

create index fk_FilteredPhrases_KaguyaServer1_idx
    on filtered_phrases (server_id);

create table if not exists kaguya_statistics
(
    kaguya_users int not null,
    guilds int not null,
    guild_users int not null,
    shards int not null,
    commands int not null,
    commands_last24hours int not null,
    fish int not null,
    points bigint not null,
    gambles int not null,
    timestamp datetime null,
    text_channels int not null,
    voice_channels int not null,
    ram_usage double not null,
    latency_ms int not null,
    uptime_seconds mediumtext not null,
    version text null
);

create table if not exists kaguya_user
(
    user_id bigint unsigned default '0' not null,
    experience int default 0 not null,
    fish_exp int default 0 not null,
    points int default 0 not null,
    osu_id int default 0 not null,
    osu_beatmaps_linked int default 0 not null,
    commands_used int default 0 not null,
    total_days_premium int default 0 not null,
    active_ratelimit int default 0 not null,
    ratelimit_warnings int default 0 not null,
    total_upvotes int default 0 not null,
    last_given_exp double default 0 not null,
    last_daily_bonus double default 0 not null,
    last_weekly_bonus double default 0 not null,
    last_given_rep double default 0 not null,
    last_ratelimited double default 0 not null,
    last_fished double default 0 not null,
    exp_chatnotification_typenum int default 3 not null,
    exp_dmnotification_typenum int default 2 not null,
    premium_expiration double default 0 not null,
    primary key (user_id)
)
    comment 'ExpTypePreference: 0 = Global, 1 = Server, 2 = Both, 3 = None' charset=utf8;

create table if not exists command_history
(
    server_id bigint unsigned not null,
    user_id bigint unsigned not null,
    command text not null,
    timestamp datetime not null,
    constraint FK_CommandHistory_KaguyaServer_BackReference
        foreign key (server_id) references kaguya_server (server_id),
    constraint FK_CommandHistory_KaguyaUser_BackReference
        foreign key (user_id) references kaguya_user (user_id)
)
    charset=utf8;

create index FK_CommandHistory_KaguyaServer_BackReference_idx
    on command_history (server_id);

create index fk_CommandHistory_KaguyaUser1_idx
    on command_history (user_id);

create table if not exists fish
(
    fish_id bigint unsigned not null,
    user_id bigint unsigned not null,
    server_id bigint unsigned not null,
    time_caught double not null,
    fish_type int not null,
    fish_string varchar(50) not null,
    value int not null,
    exp int not null,
    sold tinyint not null,
    primary key (fish_id),
    constraint FK_Fish_KaguyaServer_BackReference
        foreign key (server_id) references kaguya_server (server_id),
    constraint FK_Fish_KaguyaUser_BackReference
        foreign key (user_id) references kaguya_user (user_id)
);

create index FK_Fish_KaguyaServer_BackReference_idx
    on fish (server_id);

create table if not exists gamble_history
(
    user_id bigint unsigned not null,
    action int not null,
    action_string text not null,
    bet int not null,
    payout int not null,
    roll int not null,
    time double not null,
    winner tinyint not null,
    constraint FK_GambleHistory_KaguyaUser_BackReference
        foreign key (user_id) references kaguya_user (user_id)
)
    charset=utf8;

create index fk_GambleHistory_KaguyaUser1_idx
    on gamble_history (user_id);

create table if not exists muted_users
(
    server_id bigint unsigned not null,
    user_id bigint unsigned not null,
    expires_at double not null,
    primary key (server_id),
    constraint fk_MutedUsers_KaguyaServer1
        foreign key (server_id) references kaguya_server (server_id),
    constraint fk_MutedUsers_KaguyaUser1
        foreign key (user_id) references kaguya_user (user_id)
)
    charset=utf8;

create index fk_MutedUsers_KaguyaServer1_idx
    on muted_users (server_id);

create index fk_MutedUsers_KaguyaUser1_idx
    on muted_users (user_id);

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
    user_id bigint unsigned not null,
    server_id bigint unsigned not null,
    given_by bigint unsigned not null,
    time_given double not null,
    reason text not null,
    constraint FK_Praise_KaguyaServer_BackReference
        foreign key (server_id) references kaguya_server (server_id)
)
    comment 'Server specific rep.';

create index FK_Praise_KaguyaUser_BackReference_idx
    on praise (user_id);

create index FK_Praise_KaguyaUser_BackReference_idx1
    on praise (server_id);

create table if not exists premium_keys
(
    `key` varchar(50) not null,
    length_in_seconds bigint not null,
    key_creator_id bigint unsigned not null,
    user_id bigint unsigned null,
    server_id bigint unsigned null,
    has_expired tinyint null,
    primary key (`key`)
);

create table if not exists quotes
(
    user_id bigint unsigned not null,
    server_id bigint unsigned not null,
    text text not null,
    timestamp double not null,
    id int not null,
    constraint FK_Quotes_KaguyaServer_BackReference
        foreign key (server_id) references kaguya_server (server_id),
    constraint FK_Quotes_KaguyaUser_BackReference
        foreign key (user_id) references kaguya_user (user_id)
);

create index FK_Quotes_KaguyaServer_BackReference_idx
    on quotes (server_id);

create index FK_Quotes_KaguyaUser_BackReference_idx
    on quotes (user_id);

create table if not exists reaction_roles
(
    role_id bigint unsigned not null,
    message_id bigint unsigned not null,
    server_id bigint unsigned not null,
    emote_name_or_id text null
);

create table if not exists reminders
(
    user_id bigint unsigned not null,
    expiration double not null,
    text text not null,
    has_triggered tinyint not null,
    constraint FK_KaguyaUser_Reminder_BackReference
        foreign key (user_id) references kaguya_user (user_id)
);

create index FK_KaguyaUser_Reminder_BackReference_idx
    on reminders (user_id);

create table if not exists rep
(
    user_id bigint unsigned not null,
    given_by bigint unsigned not null,
    time_given double not null,
    reason text not null,
    constraint FK_Rep_KaguyaUser_BackReference
        foreign key (user_id) references kaguya_user (user_id)
);

create index FK_Rep_KaguyaUser_BackReference_idx
    on rep (user_id);

create table if not exists server_exp
(
    server_id bigint unsigned not null,
    user_id bigint unsigned not null,
    exp int not null,
    latest_exp double not null,
    primary key (server_id, user_id),
    constraint FK_ServerExp_KaguyaUser_BackReference
        foreign key (user_id) references kaguya_user (user_id)
)
    charset=utf8;

create index FK_ServerExp_KaguyaUser_BackReference_idx
    on server_exp (user_id);

create table if not exists server_role_rewards
(
    server_id bigint unsigned not null,
    role_id bigint unsigned not null,
    level int not null,
    constraint FK_ServerRoleRewards_KaguyaServer_BackReference
        foreign key (server_id) references kaguya_server (server_id)
);

create index FK_ServerRoleRewards_KaguyaServer_BackReference_idx
    on server_role_rewards (server_id);

create table if not exists upvotes
(
    vote_id varchar(100) not null,
    bot_id bigint unsigned not null,
    user_id bigint unsigned not null,
    time_voted double not null,
    vote_type varchar(10) not null,
    is_weekend tinyint not null,
    query_params text null,
    reminder_sent tinyint default 0 not null,
    primary key (vote_id)
);

create table if not exists user_blacklists
(
    user_id bigint unsigned not null,
    expiration double not null,
    reason text not null,
    primary key (user_id),
    constraint FK_UserBlacklists_KaguyaUser_BackReference
        foreign key (user_id) references kaguya_user (user_id)
);

create table if not exists warn_settings
(
    server_id bigint unsigned not null,
    mute int default 0 not null,
    kick int default 0 not null,
    shadowban int default 0 not null,
    ban int default 0 not null,
    primary key (server_id),
    constraint FK_WarnSettings_KaguyaServer_BackReference
        foreign key (server_id) references kaguya_server (server_id)
)
    charset=utf8;

create index fk_WarnActions_KaguyaServer1_idx
    on warn_settings (server_id);

create table if not exists warned_users
(
    server_id bigint unsigned not null,
    user_id bigint unsigned not null,
    moderator_name text not null,
    reason text not null,
    date double not null,
    constraint FK_WarnedUsers_KaguyaServer_BackReference
        foreign key (server_id) references kaguya_server (server_id)
)
    charset=utf8;

create index FK_WarnedUsers_KaguyaServer_BackReference_idx
    on warned_users (server_id);