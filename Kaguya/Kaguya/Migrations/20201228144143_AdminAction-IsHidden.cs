using Microsoft.EntityFrameworkCore.Migrations;

namespace Kaguya.Migrations
{
    public partial class AdminActionIsHidden : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsHidden",
                table: "AdminActions",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_AdminActions_ActionedUserId_ServerId_Expiration",
                table: "AdminActions",
                columns: new[] { "ActionedUserId", "ServerId", "Expiration" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AdminActions_ActionedUserId_ServerId_Expiration",
                table: "AdminActions");

            migrationBuilder.DropColumn(
                name: "IsHidden",
                table: "AdminActions");
        }
    }
}
