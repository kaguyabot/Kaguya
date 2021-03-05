using Microsoft.EntityFrameworkCore.Migrations;

namespace Kaguya.Migrations
{
    public partial class Server_RemoveAntiraidLink : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KaguyaServers_AntiRaidConfigs_AntiRaidServerId",
                table: "KaguyaServers");

            migrationBuilder.DropIndex(
                name: "IX_KaguyaServers_AntiRaidServerId",
                table: "KaguyaServers");

            migrationBuilder.DropColumn(
                name: "ExpNotificationType",
                table: "KaguyaUsers");

            migrationBuilder.DropColumn(
                name: "AntiRaidServerId",
                table: "KaguyaServers");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ExpNotificationType",
                table: "KaguyaUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<ulong>(
                name: "AntiRaidServerId",
                table: "KaguyaServers",
                type: "bigint unsigned",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_KaguyaServers_AntiRaidServerId",
                table: "KaguyaServers",
                column: "AntiRaidServerId");

            migrationBuilder.AddForeignKey(
                name: "FK_KaguyaServers_AntiRaidConfigs_AntiRaidServerId",
                table: "KaguyaServers",
                column: "AntiRaidServerId",
                principalTable: "AntiRaidConfigs",
                principalColumn: "ServerId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
