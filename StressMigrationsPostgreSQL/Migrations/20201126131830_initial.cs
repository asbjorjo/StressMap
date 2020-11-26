using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace StressMigrationsPostgreSQL.Migrations
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.CreateTable(
                name: "stressrecord",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WsmId = table.Column<string>(type: "text", nullable: false),
                    ISO = table.Column<string>(type: "text", nullable: true),
                    Location = table.Column<Point>(type: "geometry (pointz)", nullable: false),
                    Azimuth = table.Column<int>(type: "integer", nullable: false),
                    Quality = table.Column<string>(type: "text", nullable: false),
                    Regime = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false)
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
