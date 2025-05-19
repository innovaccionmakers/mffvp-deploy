using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trusts.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class RenamePKToIdTrusts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "parametro_configuracion_id",
                schema: "fideicomisos",
                table: "parametros_configuracion",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "historico_fideicomiso_id",
                schema: "fideicomisos",
                table: "historicos_fideicomisos",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "fideicomiso_id",
                schema: "fideicomisos",
                table: "fideicomisos",
                newName: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "id",
                schema: "fideicomisos",
                table: "parametros_configuracion",
                newName: "parametro_configuracion_id");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "fideicomisos",
                table: "historicos_fideicomisos",
                newName: "historico_fideicomiso_id");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "fideicomisos",
                table: "fideicomisos",
                newName: "fideicomiso_id");
        }
    }
}
