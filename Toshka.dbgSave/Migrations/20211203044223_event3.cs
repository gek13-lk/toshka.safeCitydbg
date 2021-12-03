using Microsoft.EntityFrameworkCore.Migrations;

namespace Toshka.dbgSave.Migrations
{
    public partial class event3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MainType",
                table: "Events",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Cameras",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MainType",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Cameras");
        }
    }
}
