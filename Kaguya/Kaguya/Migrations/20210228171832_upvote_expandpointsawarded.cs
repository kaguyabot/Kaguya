using Microsoft.EntityFrameworkCore.Migrations;

namespace Kaguya.Migrations
{
    public partial class upvote_expandpointsawarded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CoinsAwarded",
                table: "Upvotes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ExpAwarded",
                table: "Upvotes",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CoinsAwarded",
                table: "Upvotes");

            migrationBuilder.DropColumn(
                name: "ExpAwarded",
                table: "Upvotes");
        }
    }
}
