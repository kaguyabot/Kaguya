using Microsoft.EntityFrameworkCore.Migrations;

namespace Kaguya.Migrations
{
    public partial class Server_RemoveIsCurrentlyPurgingMessages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCurrentlyPurgingMessages",
                table: "KaguyaServers");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCurrentlyPurgingMessages",
                table: "KaguyaServers",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }
    }
}
