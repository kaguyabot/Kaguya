using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Kaguya.Migrations
{
    public partial class AntiraidConfig : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AntiRaidConfig_Servers_ServerId",
                table: "AntiRaidConfig");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AntiRaidConfig",
                table: "AntiRaidConfig");

            migrationBuilder.RenameTable(
                name: "AntiRaidConfig",
                newName: "AntiRaidConfigs");

            migrationBuilder.AddColumn<ulong>(
                name: "AntiRaidServerId",
                table: "Servers",
                type: "bigint unsigned",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Expiration",
                table: "AntiRaidConfigs",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_AntiRaidConfigs",
                table: "AntiRaidConfigs",
                column: "ServerId");

            migrationBuilder.CreateIndex(
                name: "IX_Servers_AntiRaidServerId",
                table: "Servers",
                column: "AntiRaidServerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Servers_AntiRaidConfigs_AntiRaidServerId",
                table: "Servers",
                column: "AntiRaidServerId",
                principalTable: "AntiRaidConfigs",
                principalColumn: "ServerId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Servers_AntiRaidConfigs_AntiRaidServerId",
                table: "Servers");

            migrationBuilder.DropIndex(
                name: "IX_Servers_AntiRaidServerId",
                table: "Servers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AntiRaidConfigs",
                table: "AntiRaidConfigs");

            migrationBuilder.DropColumn(
                name: "AntiRaidServerId",
                table: "Servers");

            migrationBuilder.DropColumn(
                name: "Expiration",
                table: "AntiRaidConfigs");

            migrationBuilder.RenameTable(
                name: "AntiRaidConfigs",
                newName: "AntiRaidConfig");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AntiRaidConfig",
                table: "AntiRaidConfig",
                column: "ServerId");

            migrationBuilder.AddForeignKey(
                name: "FK_AntiRaidConfig_Servers_ServerId",
                table: "AntiRaidConfig",
                column: "ServerId",
                principalTable: "Servers",
                principalColumn: "ServerId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
