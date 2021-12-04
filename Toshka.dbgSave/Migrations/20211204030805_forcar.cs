using Microsoft.EntityFrameworkCore.Migrations;

namespace Toshka.dbgSave.Migrations
{
    public partial class forcar : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "forCar",
                table: "CameraFilterMarkups",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "forCar",
                table: "CameraFilterMarkups");
        }
    }
}
