using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Energycom.Ingestion.Migrations
{
    /// <inheritdoc />
    public partial class MeterSites : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "site_id",
                table: "meters",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "sites",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Grid = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    latitude = table.Column<decimal>(type: "numeric", nullable: false),
                    longitude = table.Column<decimal>(type: "numeric", nullable: false),
                    altitude = table.Column<decimal>(type: "numeric", nullable: false),
                    time_zone = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sites", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_meters_site_id",
                table: "meters",
                column: "site_id");

            migrationBuilder.AddForeignKey(
                name: "fk_meters_sites_site_id",
                table: "meters",
                column: "site_id",
                principalTable: "sites",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_meters_sites_site_id",
                table: "meters");

            migrationBuilder.DropTable(
                name: "sites");

            migrationBuilder.DropIndex(
                name: "ix_meters_site_id",
                table: "meters");

            migrationBuilder.DropColumn(
                name: "site_id",
                table: "meters");
        }
    }
}
