using Microsoft.EntityFrameworkCore.Migrations;

namespace Kaguya.Migrations
{
    public partial class View_UserExperienceRank : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE VIEW IF NOT EXISTS KaguyaUserRanks AS " +
            "SELECT ROW_NUMBER() OVER(ORDER BY GlobalExp DESC) AS 'rank', UserId, GlobalExp " + 
            "FROM KaguyaUsers");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW IF EXISTS KaguyaUserRanks;");
        }
    }
}
