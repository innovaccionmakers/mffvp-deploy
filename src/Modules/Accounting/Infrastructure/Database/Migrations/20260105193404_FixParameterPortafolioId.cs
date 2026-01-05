using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Accounting.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class FixParameterPortafolioId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "portafolio_Id",
                schema: "contabilidad",
                table: "configuraciones_generales",
                newName: "portafolio_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "portafolio_id",
                schema: "contabilidad",
                table: "configuraciones_generales",
                newName: "portafolio_Id");
        }
    }
}
