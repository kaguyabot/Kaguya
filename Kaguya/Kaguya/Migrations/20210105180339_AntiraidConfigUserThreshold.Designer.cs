﻿// <auto-generated />
using System;
using Kaguya.Database.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Kaguya.Migrations
{
    [DbContext(typeof(KaguyaDbContext))]
    [Migration("20210105180339_AntiraidConfigUserThreshold")]
    partial class AntiraidConfigUserThreshold
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 64)
                .HasAnnotation("ProductVersion", "5.0.1");

            modelBuilder.Entity("Kaguya.Database.Model.AdminAction", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Action")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<ulong>("ActionedUserId")
                        .HasColumnType("bigint unsigned");

                    b.Property<DateTime?>("Expiration")
                        .HasColumnType("datetime(6)");

                    b.Property<bool>("IsHidden")
                        .HasColumnType("tinyint(1)");

                    b.Property<ulong>("ModeratorId")
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("Reason")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<ulong>("ServerId")
                        .HasColumnType("bigint unsigned");

                    b.HasKey("Id");

                    b.HasIndex("ServerId");

                    b.HasIndex("ActionedUserId", "ServerId", "Expiration");

                    b.ToTable("AdminActions");
                });

            modelBuilder.Entity("Kaguya.Database.Model.AntiRaidConfig", b =>
                {
                    b.Property<ulong>("ServerId")
                        .HasColumnType("bigint unsigned");

                    b.Property<int>("Action")
                        .HasColumnType("int");

                    b.Property<string>("AntiraidPunishmentDirectMessage")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<bool>("Enabled")
                        .HasColumnType("tinyint(1)");

                    b.Property<DateTime?>("Expiration")
                        .HasColumnType("datetime(6)");

                    b.Property<uint>("Seconds")
                        .HasColumnType("int unsigned");

                    b.Property<uint>("UserThreshold")
                        .HasColumnType("int unsigned");

                    b.HasKey("ServerId");

                    b.ToTable("AntiRaidConfigs");
                });

            modelBuilder.Entity("Kaguya.Database.Model.BlacklistedEntity", b =>
                {
                    b.Property<ulong>("EntityId")
                        .HasColumnType("bigint unsigned");

                    b.Property<int>("EntityType")
                        .HasColumnType("int");

                    b.Property<DateTime?>("ExpirationTime")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Reason")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.HasKey("EntityId");

                    b.ToTable("BlacklistedEntities");
                });

            modelBuilder.Entity("Kaguya.Database.Model.CommandHistory", b =>
                {
                    b.Property<uint>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int unsigned");

                    b.Property<string>("CommandName")
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4");

                    b.Property<string>("ErrorMessage")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<bool>("ExecutedSuccessfully")
                        .HasColumnType("tinyint(1)");

                    b.Property<DateTime>("ExecutionTime")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Message")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<ulong>("ServerId")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong>("UserId")
                        .HasColumnType("bigint unsigned");

                    b.HasKey("Id");

                    b.HasIndex("UserId", "ServerId", "CommandName");

                    b.ToTable("CommandHistories");
                });

            modelBuilder.Entity("Kaguya.Database.Model.FilteredWord", b =>
                {
                    b.Property<ulong>("ServerId")
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("Word")
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4");

                    b.Property<int>("FilterReaction")
                        .HasColumnType("int");

                    b.HasKey("ServerId", "Word");

                    b.ToTable("FilteredWords");
                });

            modelBuilder.Entity("Kaguya.Database.Model.Fish", b =>
                {
                    b.Property<long>("FishId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<int>("BaseCost")
                        .HasColumnType("int");

                    b.Property<ulong>("ChannelId")
                        .HasColumnType("bigint unsigned");

                    b.Property<int>("CostOfPlay")
                        .HasColumnType("int");

                    b.Property<int>("ExpValue")
                        .HasColumnType("int");

                    b.Property<int>("FishType")
                        .HasColumnType("int");

                    b.Property<int>("PointValue")
                        .HasColumnType("int");

                    b.Property<int>("Rarity")
                        .HasColumnType("int");

                    b.Property<string>("RarityString")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<ulong>("ServerId")
                        .HasColumnType("bigint unsigned");

                    b.Property<DateTime>("TimeCaught")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("TypeString")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<ulong>("UserId")
                        .HasColumnType("bigint unsigned");

                    b.HasKey("FishId");

                    b.HasIndex("ServerId");

                    b.HasIndex("UserId");

                    b.HasIndex("UserId", "FishType");

                    b.HasIndex("UserId", "Rarity");

                    b.ToTable("Fish");
                });

            modelBuilder.Entity("Kaguya.Database.Model.KaguyaServer", b =>
                {
                    b.Property<ulong>("ServerId")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong?>("AntiRaidServerId")
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("CommandPrefix")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("CustomGreeting")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<bool>("CustomGreetingIsEnabled")
                        .HasColumnType("tinyint(1)");

                    b.Property<DateTime>("DateFirstTracked")
                        .HasColumnType("datetime(6)");

                    b.Property<bool>("IsCurrentlyPurgingMessages")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("LevelAnnouncementsEnabled")
                        .HasColumnType("tinyint(1)");

                    b.Property<ulong>("MuteRoleId")
                        .HasColumnType("bigint unsigned");

                    b.Property<int>("NextQuoteId")
                        .HasColumnType("int");

                    b.Property<bool>("OsuLinkParsingEnabled")
                        .HasColumnType("tinyint(1)");

                    b.Property<int>("PraiseCooldown")
                        .HasColumnType("int");

                    b.Property<DateTime?>("PremiumExpiration")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("TotalAdminActions")
                        .HasColumnType("int");

                    b.Property<int>("TotalCommandCount")
                        .HasColumnType("int");

                    b.HasKey("ServerId");

                    b.HasIndex("AntiRaidServerId");

                    b.ToTable("Servers");
                });

            modelBuilder.Entity("Kaguya.Database.Model.KaguyaUser", b =>
                {
                    b.Property<ulong>("UserId")
                        .HasColumnType("bigint unsigned");

                    b.Property<int>("ActiveRateLimit")
                        .HasColumnType("int");

                    b.Property<DateTime?>("BlacklistExpiration")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime>("DateFirstTracked")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("ExpNotificationType")
                        .HasColumnType("int");

                    b.Property<int>("FishExp")
                        .HasColumnType("int");

                    b.Property<int>("GlobalExp")
                        .HasColumnType("int");

                    b.Property<DateTime?>("LastBlacklisted")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime?>("LastDailyBonus")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime?>("LastFished")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime?>("LastGivenExp")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime?>("LastGivenRep")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime?>("LastRatelimited")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime?>("LastWeeklyBonus")
                        .HasColumnType("datetime(6)");

                    b.Property<int?>("OsuGameMode")
                        .HasColumnType("int");

                    b.Property<long?>("OsuId")
                        .HasColumnType("bigint");

                    b.Property<int>("Points")
                        .HasColumnType("int");

                    b.Property<DateTime?>("PremiumExpiration")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("RateLimitWarnings")
                        .HasColumnType("int");

                    b.Property<int>("TotalCommandUses")
                        .HasColumnType("int");

                    b.Property<int>("TotalDaysPremium")
                        .HasColumnType("int");

                    b.Property<int>("TotalPremiumRedemptions")
                        .HasColumnType("int");

                    b.Property<int>("TotalUpvotes")
                        .HasColumnType("int");

                    b.HasKey("UserId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Kaguya.Database.Model.LogConfiguration", b =>
                {
                    b.Property<ulong>("ServerId")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong>("Bans")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong>("MessageDeleted")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong>("MessageUpdated")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong>("Shadowbans")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong>("UnBans")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong>("Unshadowbans")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong>("Unwarns")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong>("UserJoins")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong>("UserLeaves")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong>("VoiceUpdates")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong>("Warns")
                        .HasColumnType("bigint unsigned");

                    b.HasKey("ServerId");

                    b.ToTable("LogConfigurations");
                });

            modelBuilder.Entity("Kaguya.Database.Model.PremiumKey", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<DateTime?>("Expiration")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Key")
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4");

                    b.Property<ulong>("KeyCreatorId")
                        .HasColumnType("bigint unsigned");

                    b.Property<int>("LengthInSeconds")
                        .HasColumnType("int");

                    b.Property<ulong>("ServerId")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong>("UserId")
                        .HasColumnType("bigint unsigned");

                    b.HasKey("Id");

                    b.HasIndex("Key");

                    b.ToTable("PremiumKeys");
                });

            modelBuilder.Entity("Kaguya.Database.Model.Reminder", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<DateTime>("Expiration")
                        .HasColumnType("datetime(6)");

                    b.Property<bool>("HasTriggered")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("Text")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<ulong>("UserId")
                        .HasColumnType("bigint unsigned");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.HasIndex("Expiration", "HasTriggered");

                    b.ToTable("Reminders");
                });

            modelBuilder.Entity("Kaguya.Database.Model.Rep", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<ulong>("GivenBy")
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("Reason")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<DateTime>("TimeGiven")
                        .HasColumnType("datetime(6)");

                    b.Property<ulong>("UserId")
                        .HasColumnType("bigint unsigned");

                    b.HasKey("Id");

                    b.HasIndex("TimeGiven");

                    b.HasIndex("UserId");

                    b.ToTable("Rep");
                });

            modelBuilder.Entity("Kaguya.Database.Model.KaguyaServer", b =>
                {
                    b.HasOne("Kaguya.Database.Model.AntiRaidConfig", "AntiRaid")
                        .WithMany()
                        .HasForeignKey("AntiRaidServerId");

                    b.Navigation("AntiRaid");
                });
#pragma warning restore 612, 618
        }
    }
}
