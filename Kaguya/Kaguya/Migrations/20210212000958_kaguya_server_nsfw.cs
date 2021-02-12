using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Kaguya.Migrations
{
    public partial class kaguya_server_nsfw : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsNsfwAllowed",
                table: "KaguyaServers",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "NsfwAllowanceTime",
                table: "KaguyaServers",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<ulong>(
                name: "NsfwAllowedId",
                table: "KaguyaServers",
                type: "bigint unsigned",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsNsfwAllowed",
                table: "KaguyaServers");

            migrationBuilder.DropColumn(
                name: "NsfwAllowanceTime",
                table: "KaguyaServers");

            migrationBuilder.DropColumn(
                name: "NsfwAllowedId",
                table: "KaguyaServers");
        }
    }
}
