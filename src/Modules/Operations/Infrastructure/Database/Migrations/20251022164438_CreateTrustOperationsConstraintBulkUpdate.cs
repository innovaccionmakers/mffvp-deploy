using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operations.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class CreateTrustOperationsConstraintBulkUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddUniqueConstraint(
                name: "ux_operaciones_fideicomiso_portafolio_fideicomiso_fecha_tipo",
                schema: "operaciones",
                table: "operaciones_fideicomiso",
                columns: new[] { "portafolio_id", "fideicomiso_id", "fecha_proceso", "tipo_operaciones_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "ux_operaciones_fideicomiso_portafolio_fideicomiso_fecha_tipo",
                schema: "operaciones",
                table: "operaciones_fideicomiso");
        }
    }
}
