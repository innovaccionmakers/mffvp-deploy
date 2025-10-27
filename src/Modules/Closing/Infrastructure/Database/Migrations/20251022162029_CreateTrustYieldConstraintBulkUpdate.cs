using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Closing.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class CreateTrustYieldConstraintBulkUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddUniqueConstraint(
                name: "ux_rendimientos_fideicomisos_portafolio_fideicomiso_fecha",
                schema: "cierre",
                table: "rendimientos_fideicomisos",
                columns: new[] { "portafolio_id", "fideicomiso_id", "fecha_cierre" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "ux_rendimientos_fideicomisos_portafolio_fideicomiso_fecha",
                schema: "cierre",
                table: "rendimientos_fideicomisos");
        }
    }
}
