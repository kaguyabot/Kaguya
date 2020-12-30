using Microsoft.EntityFrameworkCore.Migrations;

namespace Kaguya.Migrations
{
    public partial class WordFilterUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WordFilters");

            migrationBuilder.CreateTable(
                name: "FilteredWords",
                columns: table => new
                {
                    ServerId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    Word = table.Column<string>(type: "varchar(255) CHARACTER SET utf8mb4", nullable: false),
                    FilterReaction = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilteredWords", x => new { x.ServerId, x.Word });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FilteredWords");

            migrationBuilder.CreateTable(
                name: "WordFilters",
                columns: table => new
                {
                    ServerId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    Word = table.Column<string>(type: "varchar(255) CHARACTER SET utf8mb4", nullable: false),
                    FilterReaction = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WordFilters", x => new { x.ServerId, x.Word });
                });
        }
    }
}
