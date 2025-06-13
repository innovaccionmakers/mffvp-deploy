using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operations.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class ChangeRelationOrigin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_origenaportes_modorigen_origen_id",
                schema: "operaciones",
                table: "origenaportes_modorigen");

            migrationBuilder.CreateIndex(
                name: "IX_origenaportes_modorigen_origen_id",
                schema: "operaciones",
                table: "origenaportes_modorigen",
                column: "origen_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_origenaportes_modorigen_origen_id",
                schema: "operaciones",
                table: "origenaportes_modorigen");

            migrationBuilder.CreateIndex(
                name: "IX_origenaportes_modorigen_origen_id",
                schema: "operaciones",
                table: "origenaportes_modorigen",
                column: "origen_id",
                unique: true);
        }
    }
}
