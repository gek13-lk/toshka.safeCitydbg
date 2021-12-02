using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Toshka.dbgSave.Migrations
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ModelsInput",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RentalDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Weekend = table.Column<float>(type: "real", nullable: false),
                    Fullness = table.Column<float>(type: "real", nullable: false),
                    Export = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModelsInput", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ModelsOutput",
                columns: table => new
                {
                    ForecastedRentals = table.Column<float[]>(type: "real[]", nullable: true),
                    LowerBoundRentals = table.Column<float[]>(type: "real[]", nullable: true),
                    UpperBoundRentals = table.Column<float[]>(type: "real[]", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "TelegramUsers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChatId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelegramUsers", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ModelsInput");

            migrationBuilder.DropTable(
                name: "ModelsOutput");

            migrationBuilder.DropTable(
                name: "TelegramUsers");
        }
    }
}
