using Microsoft.EntityFrameworkCore.Migrations;

namespace Kaguya.Migrations
{
    public partial class undologtest : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SomeNewLog",
                table: "LogConfigurations");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<ulong>(
                name: "SomeNewLog",
                table: "LogConfigurations",
                type: "bigint unsigned",
                nullable: false,
                defaultValue: 0ul);
        }
    }
}
