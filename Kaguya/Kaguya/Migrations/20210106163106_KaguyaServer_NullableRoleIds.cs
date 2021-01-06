using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Kaguya.Migrations
{
    public partial class KaguyaServer_NullableRoleIds : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Expiration",
                table: "AntiRaidConfigs");

            migrationBuilder.AlterColumn<ulong>(
                name: "MuteRoleId",
                table: "Servers",
                type: "bigint unsigned",
                nullable: true,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned");

            migrationBuilder.AddColumn<ulong>(
                name: "ShadowbanRoleId",
                table: "Servers",
                type: "bigint unsigned",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "PunishmentLength",
                table: "AntiRaidConfigs",
                type: "time(6)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShadowbanRoleId",
                table: "Servers");

            migrationBuilder.DropColumn(
                name: "PunishmentLength",
                table: "AntiRaidConfigs");

            migrationBuilder.AlterColumn<ulong>(
                name: "MuteRoleId",
                table: "Servers",
                type: "bigint unsigned",
                nullable: false,
                defaultValue: 0ul,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Expiration",
                table: "AntiRaidConfigs",
                type: "datetime(6)",
                nullable: true);
        }
    }
}
