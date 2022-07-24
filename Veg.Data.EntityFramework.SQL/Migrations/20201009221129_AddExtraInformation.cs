using Microsoft.EntityFrameworkCore.Migrations;

namespace Veg.Data.EntityFramework.SQL.Migrations
{
    public partial class AddExtraInformation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExtraInformation",
                table: "Reports",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExtraInformation",
                table: "Reports");
        }
    }
}
