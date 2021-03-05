using Microsoft.EntityFrameworkCore.Migrations;

namespace Kaguya.Migrations
{
    public partial class Server_LevelNotifications : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LevelAnnouncementsEnabled",
                table: "KaguyaServers");

            migrationBuilder.AddColumn<int>(
                name: "LevelNotifications",
                table: "KaguyaServers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LevelNotifications",
                table: "KaguyaServers");

            migrationBuilder.AddColumn<bool>(
                name: "LevelAnnouncementsEnabled",
                table: "KaguyaServers",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }
    }
}
