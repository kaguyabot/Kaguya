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
                name: "BlacklistedEntities",
                columns: table => new
                {
                    EntityId = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    EntityType = table.Column<int>(type: "int", nullable: false),
                    ExpirationTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Reason = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlacklistedEntities", x => x.EntityId);
                });

            migrationBuilder.CreateTable(
                name: "Servers",
                columns: table => new
                {
                    ServerId = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CommandPrefix = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    TotalCommandCount = table.Column<int>(type: "int", nullable: false),
                    TotalAdminActions = table.Column<int>(type: "int", nullable: false),
                    PraiseCooldown = table.Column<int>(type: "int", nullable: false),
                    NextQuoteId = table.Column<int>(type: "int", nullable: false),
                    PremiumExpiration = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsCurrentlyPurgingMessages = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CustomGreeting = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    CustomGreetingIsEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    LevelAnnouncementsEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    OsuLinkParsingEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Servers", x => x.ServerId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Experience = table.Column<int>(type: "int", nullable: false),
                    FishExp = table.Column<int>(type: "int", nullable: false),
                    Points = table.Column<int>(type: "int", nullable: false),
                    OsuId = table.Column<int>(type: "int", nullable: true),
                    TotalCommandUses = table.Column<int>(type: "int", nullable: false),
                    TotalDaysPremium = table.Column<int>(type: "int", nullable: false),
                    ActiveRateLimit = table.Column<int>(type: "int", nullable: false),
                    RateLimitWarnings = table.Column<int>(type: "int", nullable: false),
                    TotalUpvotes = table.Column<int>(type: "int", nullable: false),
                    LastGivenExp = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    LastDailyBonus = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    LastWeeklyBonus = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    LastGivenRep = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    LastRatelimited = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    LastFished = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ExpNotificationType = table.Column<int>(type: "int", nullable: false),
                    PremiumExpiration = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "WordFilters",
                columns: table => new
                {
                    ServerId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    Word = table.Column<string>(type: "varchar(255) CHARACTER SET utf8mb4", nullable: false),
                    FilterReaction = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WordFilters", x => new { x.ServerId, x.Word });
                });

            migrationBuilder.CreateTable(
                name: "AntiRaidConfig",
                columns: table => new
                {
                    ServerId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    Users = table.Column<int>(type: "int", nullable: false),
                    Seconds = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    Enabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AntiraidPunishmentDirectMessage = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AntiRaidConfig", x => x.ServerId);
                    table.ForeignKey(
                        name: "FK_AntiRaidConfig_Servers_ServerId",
                        column: x => x.ServerId,
                        principalTable: "Servers",
                        principalColumn: "ServerId",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AntiRaidConfig");

            migrationBuilder.DropTable(
                name: "BlacklistedEntities");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "WordFilters");

            migrationBuilder.DropTable(
                name: "Servers");
        }
    }
}
