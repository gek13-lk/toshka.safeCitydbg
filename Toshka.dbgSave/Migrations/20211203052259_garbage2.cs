using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Toshka.dbgSave.Migrations
{
    public partial class garbage2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateExport",
                table: "Garbages",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Fullness",
                table: "Garbages",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateExport",
                table: "Garbages");

            migrationBuilder.DropColumn(
                name: "Fullness",
                table: "Garbages");
        }
    }
}
