using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace StressMigrationsPostgreSQL.Migrations
{
    public partial class plates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Point>(
                name: "Location",
                table: "stressrecord",
                type: "geometry (pointz, 4326)",
                nullable: false,
                oldClrType: typeof(Point),
                oldType: "geometry (pointz)");

            migrationBuilder.CreateTable(
                name: "stressplate",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Outline = table.Column<Polygon>(type: "geometry (polygon, 4326)", nullable: false)
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

            migrationBuilder.AlterColumn<Point>(
                name: "Location",
                table: "stressrecord",
                type: "geometry (pointz)",
                nullable: false,
                oldClrType: typeof(Point),
                oldType: "geometry (pointz, 4326)");
        }
    }
}
