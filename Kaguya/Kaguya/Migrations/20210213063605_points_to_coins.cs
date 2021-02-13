using Microsoft.EntityFrameworkCore.Migrations;

namespace Kaguya.Migrations
{
    public partial class points_to_coins : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PremiumKeys",
                table: "PremiumKeys");

            migrationBuilder.RenameColumn(
                name: "Points",
                table: "KaguyaUsers",
                newName: "Coins");

            migrationBuilder.RenameColumn(
                name: "Points",
                table: "KaguyaStatistics",
                newName: "Coins");

            migrationBuilder.RenameColumn(
                name: "Points",
                table: "Giveaways",
                newName: "Coins");

            migrationBuilder.RenameColumn(
                name: "PointValue",
                table: "Fish",
                newName: "CoinValue");

            migrationBuilder.AlterColumn<string>(
                name: "Key",
                table: "PremiumKeys",
                type: "varchar(255) CHARACTER SET utf8mb4",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255) CHARACTER SET utf8mb4");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PremiumKeys",
                table: "PremiumKeys",
                column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PremiumKeys",
                table: "PremiumKeys");

            migrationBuilder.RenameColumn(
                name: "Coins",
                table: "KaguyaUsers",
                newName: "Points");

            migrationBuilder.RenameColumn(
                name: "Coins",
                table: "KaguyaStatistics",
                newName: "Points");

            migrationBuilder.RenameColumn(
                name: "Coins",
                table: "Giveaways",
                newName: "Points");

            migrationBuilder.RenameColumn(
                name: "CoinValue",
                table: "Fish",
                newName: "PointValue");

            migrationBuilder.AlterColumn<string>(
                name: "Key",
                table: "PremiumKeys",
                type: "varchar(255) CHARACTER SET utf8mb4",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(255) CHARACTER SET utf8mb4",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PremiumKeys",
                table: "PremiumKeys",
                columns: new[] { "Id", "Key" });
        }
    }
}
