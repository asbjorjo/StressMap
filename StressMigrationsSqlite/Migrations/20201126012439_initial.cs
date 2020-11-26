using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

namespace StressMigrationsSqlite.Migrations
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "stressrecord",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WsmId = table.Column<string>(type: "TEXT", nullable: false),
                    ISO = table.Column<string>(type: "TEXT", nullable: true),
                    Location = table.Column<Point>(type: "POINTZ", nullable: false)
                        .Annotation("Sqlite:Srid", 4326),
                    Azimuth = table.Column<int>(type: "INTEGER", nullable: false),
                    Quality = table.Column<string>(type: "TEXT", nullable: false),
                    Regime = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stressrecord", x => x.Id);
                    table.UniqueConstraint("AK_stressrecord_WsmId", x => x.WsmId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_stressrecord_Location",
                table: "stressrecord",
                column: "Location");

            migrationBuilder.CreateIndex(
                name: "IX_stressrecord_Quality",
                table: "stressrecord",
                column: "Quality");

            migrationBuilder.CreateIndex(
                name: "IX_stressrecord_Regime",
                table: "stressrecord",
                column: "Regime");

            migrationBuilder.CreateIndex(
                name: "IX_stressrecord_Type",
                table: "stressrecord",
                column: "Type");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "stressrecord");
        }
    }
}
