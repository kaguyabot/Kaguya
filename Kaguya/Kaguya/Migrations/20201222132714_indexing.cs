using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Kaguya.Migrations
{
    public partial class indexing : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Fish_UserId_ServerId_ChannelId_TimeCaught_FishType_Rarity",
                table: "Fish");

            migrationBuilder.AlterColumn<string>(
                name: "CommandName",
                table: "CommandHistories",
                type: "varchar(255) CHARACTER SET utf8mb4",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext CHARACTER SET utf8mb4",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "PremiumKeys",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Key = table.Column<string>(type: "varchar(255) CHARACTER SET utf8mb4", nullable: true),
                    KeyCreatorId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    LengthInSeconds = table.Column<int>(type: "int", nullable: false),
                    Expiration = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UserId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    ServerId = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PremiumKeys", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Rep_TimeGiven",
                table: "Rep",
                column: "TimeGiven");

            migrationBuilder.CreateIndex(
                name: "IX_Rep_UserId",
                table: "Rep",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Fish_ServerId",
                table: "Fish",
                column: "ServerId");

            migrationBuilder.CreateIndex(
                name: "IX_Fish_UserId",
                table: "Fish",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Fish_UserId_FishType",
                table: "Fish",
                columns: new[] { "UserId", "FishType" });

            migrationBuilder.CreateIndex(
                name: "IX_Fish_UserId_Rarity",
                table: "Fish",
                columns: new[] { "UserId", "Rarity" });

            migrationBuilder.CreateIndex(
                name: "IX_CommandHistories_UserId_ServerId_CommandName",
                table: "CommandHistories",
                columns: new[] { "UserId", "ServerId", "CommandName" });

            migrationBuilder.CreateIndex(
                name: "IX_AdminActions_ServerId",
                table: "AdminActions",
                column: "ServerId");

            migrationBuilder.CreateIndex(
                name: "IX_PremiumKeys_Key",
                table: "PremiumKeys",
                column: "Key");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PremiumKeys");

            migrationBuilder.DropIndex(
                name: "IX_Rep_TimeGiven",
                table: "Rep");

            migrationBuilder.DropIndex(
                name: "IX_Rep_UserId",
                table: "Rep");

            migrationBuilder.DropIndex(
                name: "IX_Fish_ServerId",
                table: "Fish");

            migrationBuilder.DropIndex(
                name: "IX_Fish_UserId",
                table: "Fish");

            migrationBuilder.DropIndex(
                name: "IX_Fish_UserId_FishType",
                table: "Fish");

            migrationBuilder.DropIndex(
                name: "IX_Fish_UserId_Rarity",
                table: "Fish");

            migrationBuilder.DropIndex(
                name: "IX_CommandHistories_UserId_ServerId_CommandName",
                table: "CommandHistories");

            migrationBuilder.DropIndex(
                name: "IX_AdminActions_ServerId",
                table: "AdminActions");

            migrationBuilder.AlterColumn<string>(
                name: "CommandName",
                table: "CommandHistories",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255) CHARACTER SET utf8mb4",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Fish_UserId_ServerId_ChannelId_TimeCaught_FishType_Rarity",
                table: "Fish",
                columns: new[] { "UserId", "ServerId", "ChannelId", "TimeCaught", "FishType", "Rarity" });
        }
    }
}
