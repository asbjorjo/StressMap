using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

namespace StressMigrationsSqlite.Migrations
{
    public partial class plates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Sqlite:InitSpatialMetaData", true);

            migrationBuilder.CreateTable(
                name: "stressplate",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Outline = table.Column<Polygon>(type: "polygon", nullable: false)
                        .Annotation("Sqlite:Srid", 4326)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stressplate", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_stressplate_Name",
                table: "stressplate",
                column: "Name");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "stressplate");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Sqlite:InitSpatialMetaData", true);
        }
    }
}
