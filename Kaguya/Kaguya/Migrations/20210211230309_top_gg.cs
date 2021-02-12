using Microsoft.EntityFrameworkCore.Migrations;

namespace Kaguya.Migrations
{
    public partial class top_gg : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ExtraArgs",
                table: "Upvotes",
                newName: "Type");

            migrationBuilder.AddColumn<ulong>(
                name: "BotId",
                table: "Upvotes",
                type: "bigint unsigned",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AddColumn<string>(
                name: "QueryParams",
                table: "Upvotes",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ReminderSent",
                table: "Upvotes",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BotId",
                table: "Upvotes");

            migrationBuilder.DropColumn(
                name: "QueryParams",
                table: "Upvotes");

            migrationBuilder.DropColumn(
                name: "ReminderSent",
                table: "Upvotes");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "Upvotes",
                newName: "ExtraArgs");
        }
    }
}
