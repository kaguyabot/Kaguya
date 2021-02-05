using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Kaguya.Migrations
{
    public partial class kaguyastatistics : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KaguyaStatistics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Users = table.Column<int>(type: "int", nullable: false),
                    Servers = table.Column<int>(type: "int", nullable: false),
                    Shards = table.Column<int>(type: "int", nullable: false),
                    CommandsExecuted = table.Column<int>(type: "int", nullable: false),
                    Fish = table.Column<int>(type: "int", nullable: false),
                    Points = table.Column<long>(type: "bigint", nullable: false),
                    Gambles = table.Column<int>(type: "int", nullable: false),
                    RamUsageMegabytes = table.Column<double>(type: "double", nullable: false),
                    LatencyMilliseconds = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KaguyaStatistics", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KaguyaStatistics");
        }
    }
}
