using Microsoft.EntityFrameworkCore.Migrations;

namespace Kaguya.Migrations
{
    public partial class AntiraidConfigNumberTypes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActionString",
                table: "AntiRaidConfigs");

            migrationBuilder.AlterColumn<uint>(
                name: "Users",
                table: "AntiRaidConfigs",
                type: "int unsigned",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<uint>(
                name: "Seconds",
                table: "AntiRaidConfigs",
                type: "int unsigned",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Users",
                table: "AntiRaidConfigs",
                type: "int",
                nullable: false,
                oldClrType: typeof(uint),
                oldType: "int unsigned");

            migrationBuilder.AlterColumn<int>(
                name: "Seconds",
                table: "AntiRaidConfigs",
                type: "int",
                nullable: false,
                oldClrType: typeof(uint),
                oldType: "int unsigned");

            migrationBuilder.AddColumn<string>(
                name: "ActionString",
                table: "AntiRaidConfigs",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true);
        }
    }
}
