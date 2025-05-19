using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace People.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class RenamePKToIdPeople : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "persona_id",
                schema: "personas",
                table: "personas",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "parametro_configuracion_id",
                schema: "personas",
                table: "parametros_configuracion",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "pais_id",
                schema: "personas",
                table: "paises",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "codgrupo_id",
                schema: "personas",
                table: "ciuu",
                newName: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "id",
                schema: "personas",
                table: "personas",
                newName: "persona_id");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "personas",
                table: "parametros_configuracion",
                newName: "parametro_configuracion_id");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "personas",
                table: "paises",
                newName: "pais_id");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "personas",
                table: "ciuu",
                newName: "codgrupo_id");
        }
    }
}
