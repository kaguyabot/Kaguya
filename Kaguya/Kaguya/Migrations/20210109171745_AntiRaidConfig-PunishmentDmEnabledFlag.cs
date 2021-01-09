using Microsoft.EntityFrameworkCore.Migrations;

namespace Kaguya.Migrations
{
    public partial class AntiRaidConfigPunishmentDmEnabledFlag : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Enabled",
                table: "AntiRaidConfigs",
                newName: "PunishmentDmEnabled");

            migrationBuilder.AddColumn<bool>(
                name: "ConfigEnabled",
                table: "AntiRaidConfigs",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConfigEnabled",
                table: "AntiRaidConfigs");

            migrationBuilder.RenameColumn(
                name: "PunishmentDmEnabled",
                table: "AntiRaidConfigs",
                newName: "Enabled");
        }
    }
}
