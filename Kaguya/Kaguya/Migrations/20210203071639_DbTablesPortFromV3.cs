using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Kaguya.Migrations
{
    public partial class DbTablesPortFromV3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Servers_AntiRaidConfigs_AntiRaidServerId",
                table: "Servers");

            migrationBuilder.DropIndex(
                name: "IX_Rep_TimeGiven",
                table: "Rep");

            migrationBuilder.DropIndex(
                name: "IX_Reminders_Expiration_HasTriggered",
                table: "Reminders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PremiumKeys",
                table: "PremiumKeys");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Servers",
                table: "Servers");

            migrationBuilder.DropColumn(
                name: "RarityString",
                table: "Fish");

            migrationBuilder.DropColumn(
                name: "TypeString",
                table: "Fish");

            migrationBuilder.DropColumn(
                name: "NextQuoteId",
                table: "Servers");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "KaguyaUsers");

            migrationBuilder.RenameTable(
                name: "Servers",
                newName: "KaguyaServers");

            migrationBuilder.RenameColumn(
                name: "OsuLinkParsingEnabled",
                table: "KaguyaServers",
                newName: "AutomaticOsuLinkParsingEnabled");

            migrationBuilder.RenameIndex(
                name: "IX_Servers_AntiRaidServerId",
                table: "KaguyaServers",
                newName: "IX_KaguyaServers_AntiRaidServerId");

            migrationBuilder.AlterColumn<string>(
                name: "Key",
                table: "PremiumKeys",
                type: "varchar(255) CHARACTER SET utf8mb4",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(255) CHARACTER SET utf8mb4",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "CommandHistories",
                type: "int",
                nullable: false,
                oldClrType: typeof(uint),
                oldType: "int unsigned")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<string>(
                name: "Action",
                table: "AdminActions",
                type: "varchar(255) CHARACTER SET utf8mb4",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext CHARACTER SET utf8mb4",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Timestamp",
                table: "AdminActions",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_PremiumKeys",
                table: "PremiumKeys",
                column: "Key");

            migrationBuilder.AddPrimaryKey(
                name: "PK_KaguyaUsers",
                table: "KaguyaUsers",
                column: "UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_KaguyaServers",
                table: "KaguyaServers",
                column: "ServerId");

            migrationBuilder.CreateTable(
                name: "AutoAssignedRoles",
                columns: table => new
                {
                    ServerId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    RoleId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    Delay = table.Column<TimeSpan>(type: "time(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutoAssignedRoles", x => new { x.ServerId, x.RoleId });
                });

            migrationBuilder.CreateTable(
                name: "Eightballs",
                columns: table => new
                {
                    Phrase = table.Column<string>(type: "varchar(255) CHARACTER SET utf8mb4", nullable: false),
                    Outlook = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Eightballs", x => x.Phrase);
                });

            migrationBuilder.CreateTable(
                name: "FavoriteTracks",
                columns: table => new
                {
                    UserId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    SongId = table.Column<string>(type: "varchar(255) CHARACTER SET utf8mb4", nullable: false),
                    ServerId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FavoriteTracks", x => new { x.UserId, x.SongId });
                });

            migrationBuilder.CreateTable(
                name: "GambleHistories",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    ServerId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    Action = table.Column<int>(type: "int", nullable: false),
                    AmountBet = table.Column<int>(type: "int", nullable: false),
                    AmountRewarded = table.Column<int>(type: "int", nullable: false),
                    IsWinner = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GambleHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Giveaways",
                columns: table => new
                {
                    MessageId = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ServerId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    ChannelId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    Exp = table.Column<int>(type: "int", nullable: true),
                    Points = table.Column<int>(type: "int", nullable: true),
                    Item = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    Amount = table.Column<int>(type: "int", nullable: false),
                    Expiration = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Giveaways", x => x.MessageId);
                });

            migrationBuilder.CreateTable(
                name: "Quotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ServerId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    UserId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    Text = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quotes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReactionRoles",
                columns: table => new
                {
                    MessageId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    RoleId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    ServerId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    Emote = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    IsStandardEmoji = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReactionRoles", x => new { x.MessageId, x.RoleId });
                });

            migrationBuilder.CreateTable(
                name: "RoleRewards",
                columns: table => new
                {
                    RoleId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    ServerId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleRewards", x => x.RoleId);
                });

            migrationBuilder.CreateTable(
                name: "ServerExperience",
                columns: table => new
                {
                    ServerId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    UserId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    Exp = table.Column<int>(type: "int", nullable: false),
                    LastGivenExp = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerExperience", x => new { x.ServerId, x.UserId });
                });

            migrationBuilder.CreateTable(
                name: "Upvotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    IsWeekend = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ExtraArgs = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Upvotes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WarnConfigurations",
                columns: table => new
                {
                    ServerId = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MuteNum = table.Column<int>(type: "int", nullable: false),
                    KickNum = table.Column<int>(type: "int", nullable: false),
                    ShadowbanNum = table.Column<int>(type: "int", nullable: false),
                    BanNum = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarnConfigurations", x => x.ServerId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reminders_UserId_Expiration",
                table: "Reminders",
                columns: new[] { "UserId", "Expiration" });

            migrationBuilder.CreateIndex(
                name: "IX_FilteredWords_ServerId",
                table: "FilteredWords",
                column: "ServerId");

            migrationBuilder.CreateIndex(
                name: "IX_AdminActions_ActionedUserId_ServerId_Action_Expiration",
                table: "AdminActions",
                columns: new[] { "ActionedUserId", "ServerId", "Action", "Expiration" });

            migrationBuilder.CreateIndex(
                name: "IX_AdminActions_ServerId_Action",
                table: "AdminActions",
                columns: new[] { "ServerId", "Action" });

            migrationBuilder.CreateIndex(
                name: "IX_KaguyaUsers_ActiveRateLimit",
                table: "KaguyaUsers",
                column: "ActiveRateLimit");

            migrationBuilder.CreateIndex(
                name: "IX_FavoriteTracks_UserId",
                table: "FavoriteTracks",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GambleHistories_ServerId",
                table: "GambleHistories",
                column: "ServerId");

            migrationBuilder.CreateIndex(
                name: "IX_GambleHistories_UserId",
                table: "GambleHistories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GambleHistories_UserId_IsWinner",
                table: "GambleHistories",
                columns: new[] { "UserId", "IsWinner" });

            migrationBuilder.CreateIndex(
                name: "IX_Giveaways_Expiration",
                table: "Giveaways",
                column: "Expiration");

            migrationBuilder.CreateIndex(
                name: "IX_Giveaways_ServerId_Expiration",
                table: "Giveaways",
                columns: new[] { "ServerId", "Expiration" });

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_ServerId",
                table: "Quotes",
                column: "ServerId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleRewards_ServerId",
                table: "RoleRewards",
                column: "ServerId");

            migrationBuilder.CreateIndex(
                name: "IX_Upvotes_UserId",
                table: "Upvotes",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_KaguyaServers_AntiRaidConfigs_AntiRaidServerId",
                table: "KaguyaServers",
                column: "AntiRaidServerId",
                principalTable: "AntiRaidConfigs",
                principalColumn: "ServerId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KaguyaServers_AntiRaidConfigs_AntiRaidServerId",
                table: "KaguyaServers");

            migrationBuilder.DropTable(
                name: "AutoAssignedRoles");

            migrationBuilder.DropTable(
                name: "Eightballs");

            migrationBuilder.DropTable(
                name: "FavoriteTracks");

            migrationBuilder.DropTable(
                name: "GambleHistories");

            migrationBuilder.DropTable(
                name: "Giveaways");

            migrationBuilder.DropTable(
                name: "Quotes");

            migrationBuilder.DropTable(
                name: "ReactionRoles");

            migrationBuilder.DropTable(
                name: "RoleRewards");

            migrationBuilder.DropTable(
                name: "ServerExperience");

            migrationBuilder.DropTable(
                name: "Upvotes");

            migrationBuilder.DropTable(
                name: "WarnConfigurations");

            migrationBuilder.DropIndex(
                name: "IX_Reminders_UserId_Expiration",
                table: "Reminders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PremiumKeys",
                table: "PremiumKeys");

            migrationBuilder.DropIndex(
                name: "IX_FilteredWords_ServerId",
                table: "FilteredWords");

            migrationBuilder.DropIndex(
                name: "IX_AdminActions_ActionedUserId_ServerId_Action_Expiration",
                table: "AdminActions");

            migrationBuilder.DropIndex(
                name: "IX_AdminActions_ServerId_Action",
                table: "AdminActions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_KaguyaUsers",
                table: "KaguyaUsers");

            migrationBuilder.DropIndex(
                name: "IX_KaguyaUsers_ActiveRateLimit",
                table: "KaguyaUsers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_KaguyaServers",
                table: "KaguyaServers");

            migrationBuilder.DropColumn(
                name: "Timestamp",
                table: "AdminActions");

            migrationBuilder.RenameTable(
                name: "KaguyaUsers",
                newName: "Users");

            migrationBuilder.RenameTable(
                name: "KaguyaServers",
                newName: "Servers");

            migrationBuilder.RenameColumn(
                name: "AutomaticOsuLinkParsingEnabled",
                table: "Servers",
                newName: "OsuLinkParsingEnabled");

            migrationBuilder.RenameIndex(
                name: "IX_KaguyaServers_AntiRaidServerId",
                table: "Servers",
                newName: "IX_Servers_AntiRaidServerId");

            migrationBuilder.AlterColumn<string>(
                name: "Key",
                table: "PremiumKeys",
                type: "varchar(255) CHARACTER SET utf8mb4",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255) CHARACTER SET utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "RarityString",
                table: "Fish",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TypeString",
                table: "Fish",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true);

            migrationBuilder.AlterColumn<uint>(
                name: "Id",
                table: "CommandHistories",
                type: "int unsigned",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<string>(
                name: "Action",
                table: "AdminActions",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255) CHARACTER SET utf8mb4",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NextQuoteId",
                table: "Servers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PremiumKeys",
                table: "PremiumKeys",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Servers",
                table: "Servers",
                column: "ServerId");

            migrationBuilder.CreateIndex(
                name: "IX_Rep_TimeGiven",
                table: "Rep",
                column: "TimeGiven");

            migrationBuilder.CreateIndex(
                name: "IX_Reminders_Expiration_HasTriggered",
                table: "Reminders",
                columns: new[] { "Expiration", "HasTriggered" });

            migrationBuilder.AddForeignKey(
                name: "FK_Servers_AntiRaidConfigs_AntiRaidServerId",
                table: "Servers",
                column: "AntiRaidServerId",
                principalTable: "AntiRaidConfigs",
                principalColumn: "ServerId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
