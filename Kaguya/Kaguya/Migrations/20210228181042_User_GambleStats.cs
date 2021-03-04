using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Kaguya.Migrations
{
    public partial class User_GambleStats : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TotalUpvotes",
                table: "KaguyaUsers",
                newName: "TotalUpvotesTopGg");

            migrationBuilder.RenameColumn(
                name: "LastUpvoted",
                table: "KaguyaUsers",
                newName: "LastUpvotedTopGg");

            migrationBuilder.AddColumn<int>(
                name: "GrossGambleCoinLosses",
                table: "KaguyaUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GrossGambleCoinWinnings",
                table: "KaguyaUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastUpvotedDiscordBoats",
                table: "KaguyaUsers",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalGambleLosses",
                table: "KaguyaUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalGambleWins",
                table: "KaguyaUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalGambles",
                table: "KaguyaUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalUpvotesDiscordBoats",
                table: "KaguyaUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GrossGambleCoinLosses",
                table: "KaguyaUsers");

            migrationBuilder.DropColumn(
                name: "GrossGambleCoinWinnings",
                table: "KaguyaUsers");

            migrationBuilder.DropColumn(
                name: "LastUpvotedDiscordBoats",
                table: "KaguyaUsers");

            migrationBuilder.DropColumn(
                name: "TotalGambleLosses",
                table: "KaguyaUsers");

            migrationBuilder.DropColumn(
                name: "TotalGambleWins",
                table: "KaguyaUsers");

            migrationBuilder.DropColumn(
                name: "TotalGambles",
                table: "KaguyaUsers");

            migrationBuilder.DropColumn(
                name: "TotalUpvotesDiscordBoats",
                table: "KaguyaUsers");

            migrationBuilder.RenameColumn(
                name: "TotalUpvotesTopGg",
                table: "KaguyaUsers",
                newName: "TotalUpvotes");

            migrationBuilder.RenameColumn(
                name: "LastUpvotedTopGg",
                table: "KaguyaUsers",
                newName: "LastUpvoted");
        }
    }
}
