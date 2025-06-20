using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Closing.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class ChangeNameShema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "cierre");

            migrationBuilder.RenameTable(
                name: "pyg",
                schema: "closing",
                newName: "pyg",
                newSchema: "cierre");

            migrationBuilder.RenameTable(
                name: "parametros_configuracion",
                schema: "closing",
                newName: "parametros_configuracion",
                newSchema: "cierre");

            migrationBuilder.RenameTable(
                name: "conceptos_pyg",
                schema: "closing",
                newName: "conceptos_pyg",
                newSchema: "cierre");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "closing");

            migrationBuilder.RenameTable(
                name: "pyg",
                schema: "cierre",
                newName: "pyg",
                newSchema: "closing");

            migrationBuilder.RenameTable(
                name: "parametros_configuracion",
                schema: "cierre",
                newName: "parametros_configuracion",
                newSchema: "closing");

            migrationBuilder.RenameTable(
                name: "conceptos_pyg",
                schema: "cierre",
                newName: "conceptos_pyg",
                newSchema: "closing");
        }
    }
}
