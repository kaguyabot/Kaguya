using Microsoft.EntityFrameworkCore.Migrations;

namespace Kaguya.Migrations
{
    public partial class fish_raritystring : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FishTypeString",
                table: "Fish",
                newName: "TypeString");

            migrationBuilder.AddColumn<string>(
                name: "RarityString",
                table: "Fish",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RarityString",
                table: "Fish");

            migrationBuilder.RenameColumn(
                name: "TypeString",
                table: "Fish",
                newName: "FishTypeString");
        }
    }
}
