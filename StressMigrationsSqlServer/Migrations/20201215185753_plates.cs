using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

namespace StressMigrationsSqlServer.Migrations
{
    public partial class plates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "stressplate",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Outline = table.Column<Polygon>(type: "geometry", nullable: false)
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
        }
    }
}
