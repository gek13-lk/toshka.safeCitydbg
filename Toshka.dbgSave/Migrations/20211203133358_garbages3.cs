using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Toshka.dbgSave.Migrations
{
    public partial class garbages3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Video",
                table: "Garbages",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CameraFilterMarkups",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CameraId = table.Column<long>(type: "bigint", nullable: false),
                    dlx = table.Column<int>(type: "integer", nullable: false),
                    ulx = table.Column<int>(type: "integer", nullable: false),
                    dly = table.Column<int>(type: "integer", nullable: false),
                    uly = table.Column<int>(type: "integer", nullable: false),
                    drx = table.Column<int>(type: "integer", nullable: false),
                    dry = table.Column<int>(type: "integer", nullable: false),
                    ury = table.Column<int>(type: "integer", nullable: false),
                    urx = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CameraFilterMarkups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Exports",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exports", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CameraFilterMarkups");

            migrationBuilder.DropTable(
                name: "Exports");

            migrationBuilder.DropColumn(
                name: "Video",
                table: "Garbages");
        }
    }
}
