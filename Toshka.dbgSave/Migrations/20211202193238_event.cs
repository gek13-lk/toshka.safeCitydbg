using Microsoft.EntityFrameworkCore.Migrations;

namespace Toshka.dbgSave.Migrations
{
    public partial class @event : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Markup",
                table: "Cameras",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PeopleDeg",
                table: "Cameras",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Subtype",
                table: "Cameras",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCamera",
                table: "Cameras",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Markup",
                table: "Cameras");

            migrationBuilder.DropColumn(
                name: "PeopleDeg",
                table: "Cameras");

            migrationBuilder.DropColumn(
                name: "Subtype",
                table: "Cameras");

            migrationBuilder.DropColumn(
                name: "IsCamera",
                table: "Cameras");
        }
    }
}
