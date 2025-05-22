using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Energycom.Ingestion.Migrations
{
    /// <inheritdoc />
    public partial class RemovePreParsedValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "parsed",
                table: "readings");

            migrationBuilder.DropColumn(
                name: "unit",
                table: "readings");

            migrationBuilder.DropColumn(
                name: "value",
                table: "readings");

            migrationBuilder.RenameColumn(
                name: "reading_date",
                table: "readings",
                newName: "ingestion_date");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ingestion_date",
                table: "readings",
                newName: "reading_date");

            migrationBuilder.AddColumn<bool>(
                name: "parsed",
                table: "readings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "unit",
                table: "readings",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "value",
                table: "readings",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
