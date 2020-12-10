using Microsoft.EntityFrameworkCore.Migrations;

namespace Kaguya.Migrations
{
    public partial class LogConfiguration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LogConfigurations",
                columns: table => new
                {
                    ServerId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    Bans = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    UnBans = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    Warns = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    Unwarns = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    Shadowbans = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    Unshadowbans = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    UserJoins = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    UserLeaves = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    VoiceUpdates = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    MessageDeleted = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    MessageUpdated = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogConfigurations", x => x.ServerId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LogConfigurations");
        }
    }
}
