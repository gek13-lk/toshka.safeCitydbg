using Microsoft.EntityFrameworkCore.Migrations;

namespace Toshka.dbgSave.Migrations
{
    public partial class markups : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "focalLength",
                table: "Cameras",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "imageSensorSize",
                table: "Cameras",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "focalLength",
                table: "Cameras");

            migrationBuilder.DropColumn(
                name: "imageSensorSize",
                table: "Cameras");
        }
    }
}
