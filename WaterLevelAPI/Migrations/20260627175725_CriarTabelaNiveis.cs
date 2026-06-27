using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WaterLevelAPI.Migrations
{
    /// <inheritdoc />
    public partial class CriarTabelaNiveis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WaterLevels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DeviceId = table.Column<string>(type: "TEXT", nullable: false),
                    CurrentLevel = table.Column<double>(type: "REAL", nullable: false),
                    MinLevel = table.Column<double>(type: "REAL", nullable: false),
                    MaxLevel = table.Column<double>(type: "REAL", nullable: false),
                    TimesTamp = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WaterLevels", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WaterLevels");
        }
    }
}
