using Microsoft.EntityFrameworkCore.Migrations;

namespace Kaguya.Migrations
{
    public partial class premiumFix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PremiumKeys",
                table: "PremiumKeys");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PremiumKeys",
                table: "PremiumKeys",
                columns: new[] { "Id", "Key" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PremiumKeys",
                table: "PremiumKeys");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PremiumKeys",
                table: "PremiumKeys",
                column: "Key");
        }
    }
}
