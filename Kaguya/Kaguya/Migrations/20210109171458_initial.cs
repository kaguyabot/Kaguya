using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Kaguya.Migrations
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdminActions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ServerId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    ModeratorId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    ActionedUserId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    Action = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    Reason = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    Expiration = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsHidden = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsSystemAction = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminActions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AntiRaidConfigs",
                columns: table => new
                {
                    ServerId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    UserThreshold = table.Column<uint>(type: "int unsigned", nullable: false),
                    Seconds = table.Column<uint>(type: "int unsigned", nullable: false),
                    Action = table.Column<int>(type: "int", nullable: false),
                    PunishmentLength = table.Column<TimeSpan>(type: "time(6)", nullable: true),
                    AntiraidPunishmentDirectMessage = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    Enabled = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AntiRaidConfigs", x => x.ServerId);
                });

            migrationBuilder.CreateTable(
                name: "BlacklistedEntities",
                columns: table => new
                {
                    EntityId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    EntityType = table.Column<int>(type: "int", nullable: false),
                    ExpirationTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Reason = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlacklistedEntities", x => x.EntityId);
                });

            migrationBuilder.CreateTable(
                name: "CommandHistories",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    ServerId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    CommandName = table.Column<string>(type: "varchar(255) CHARACTER SET utf8mb4", nullable: true),
                    Message = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    ExecutedSuccessfully = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ErrorMessage = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    ExecutionTime = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommandHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FilteredWords",
                columns: table => new
                {
                    ServerId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    Word = table.Column<string>(type: "varchar(255) CHARACTER SET utf8mb4", nullable: false),
                    FilterReaction = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilteredWords", x => new { x.ServerId, x.Word });
                });

            migrationBuilder.CreateTable(
                name: "Fish",
                columns: table => new
                {
                    FishId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    ServerId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    ChannelId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    TimeCaught = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ExpValue = table.Column<int>(type: "int", nullable: false),
                    PointValue = table.Column<int>(type: "int", nullable: false),
                    CostOfPlay = table.Column<int>(type: "int", nullable: false),
                    BaseCost = table.Column<int>(type: "int", nullable: false),
                    FishType = table.Column<int>(type: "int", nullable: false),
                    Rarity = table.Column<int>(type: "int", nullable: false),
                    RarityString = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    TypeString = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fish", x => x.FishId);
                });

            migrationBuilder.CreateTable(
                name: "LogConfigurations",
                columns: table => new
                {
                    ServerId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    Antiraids = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    Bans = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    UnBans = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    Warns = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    Unwarns = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    Shadowbans = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    Unshadowbans = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    UserJoins = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    UserLeaves = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    VoiceUpdates = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    MessageDeleted = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    MessageUpdated = table.Column<ulong>(type: "bigint unsigned", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogConfigurations", x => x.ServerId);
                });

            migrationBuilder.CreateTable(
                name: "PremiumKeys",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Key = table.Column<string>(type: "varchar(255) CHARACTER SET utf8mb4", nullable: true),
                    KeyCreatorId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    LengthInSeconds = table.Column<int>(type: "int", nullable: false),
                    Expiration = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UserId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    ServerId = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PremiumKeys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Reminders",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    Text = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    Expiration = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    HasTriggered = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reminders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Rep",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    GivenBy = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    TimeGiven = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Reason = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rep", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    GlobalExp = table.Column<int>(type: "int", nullable: false),
                    FishExp = table.Column<int>(type: "int", nullable: false),
                    Points = table.Column<int>(type: "int", nullable: false),
                    OsuId = table.Column<long>(type: "bigint", nullable: true),
                    OsuGameMode = table.Column<int>(type: "int", nullable: true),
                    TotalCommandUses = table.Column<int>(type: "int", nullable: false),
                    TotalDaysPremium = table.Column<int>(type: "int", nullable: false),
                    TotalPremiumRedemptions = table.Column<int>(type: "int", nullable: false),
                    ActiveRateLimit = table.Column<int>(type: "int", nullable: false),
                    RateLimitWarnings = table.Column<int>(type: "int", nullable: false),
                    TotalUpvotes = table.Column<int>(type: "int", nullable: false),
                    DateFirstTracked = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    LastGivenExp = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    LastDailyBonus = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    LastWeeklyBonus = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    LastGivenRep = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    LastRatelimited = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    LastFished = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    LastBlacklisted = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    PremiumExpiration = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    BlacklistExpiration = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ExpNotificationType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "Servers",
                columns: table => new
                {
                    ServerId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    CommandPrefix = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    TotalCommandCount = table.Column<int>(type: "int", nullable: false),
                    TotalAdminActions = table.Column<int>(type: "int", nullable: false),
                    PraiseCooldown = table.Column<int>(type: "int", nullable: false),
                    NextQuoteId = table.Column<int>(type: "int", nullable: false),
                    MuteRoleId = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    ShadowbanRoleId = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    DateFirstTracked = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    PremiumExpiration = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsCurrentlyPurgingMessages = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CustomGreeting = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    CustomGreetingIsEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    LevelAnnouncementsEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    OsuLinkParsingEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AntiRaidServerId = table.Column<ulong>(type: "bigint unsigned", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Servers", x => x.ServerId);
                    table.ForeignKey(
                        name: "FK_Servers_AntiRaidConfigs_AntiRaidServerId",
                        column: x => x.AntiRaidServerId,
                        principalTable: "AntiRaidConfigs",
                        principalColumn: "ServerId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdminActions_ActionedUserId_ServerId_Expiration",
                table: "AdminActions",
                columns: new[] { "ActionedUserId", "ServerId", "Expiration" });

            migrationBuilder.CreateIndex(
                name: "IX_AdminActions_ServerId",
                table: "AdminActions",
                column: "ServerId");

            migrationBuilder.CreateIndex(
                name: "IX_CommandHistories_UserId_ServerId_CommandName",
                table: "CommandHistories",
                columns: new[] { "UserId", "ServerId", "CommandName" });

            migrationBuilder.CreateIndex(
                name: "IX_Fish_ServerId",
                table: "Fish",
                column: "ServerId");

            migrationBuilder.CreateIndex(
                name: "IX_Fish_UserId",
                table: "Fish",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Fish_UserId_FishType",
                table: "Fish",
                columns: new[] { "UserId", "FishType" });

            migrationBuilder.CreateIndex(
                name: "IX_Fish_UserId_Rarity",
                table: "Fish",
                columns: new[] { "UserId", "Rarity" });

            migrationBuilder.CreateIndex(
                name: "IX_PremiumKeys_Key",
                table: "PremiumKeys",
                column: "Key");

            migrationBuilder.CreateIndex(
                name: "IX_Reminders_Expiration_HasTriggered",
                table: "Reminders",
                columns: new[] { "Expiration", "HasTriggered" });

            migrationBuilder.CreateIndex(
                name: "IX_Reminders_UserId",
                table: "Reminders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Rep_TimeGiven",
                table: "Rep",
                column: "TimeGiven");

            migrationBuilder.CreateIndex(
                name: "IX_Rep_UserId",
                table: "Rep",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Servers_AntiRaidServerId",
                table: "Servers",
                column: "AntiRaidServerId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdminActions");

            migrationBuilder.DropTable(
                name: "BlacklistedEntities");

            migrationBuilder.DropTable(
                name: "CommandHistories");

            migrationBuilder.DropTable(
                name: "FilteredWords");

            migrationBuilder.DropTable(
                name: "Fish");

            migrationBuilder.DropTable(
                name: "LogConfigurations");

            migrationBuilder.DropTable(
                name: "PremiumKeys");

            migrationBuilder.DropTable(
                name: "Reminders");

            migrationBuilder.DropTable(
                name: "Rep");

            migrationBuilder.DropTable(
                name: "Servers");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "AntiRaidConfigs");
        }
    }
}
