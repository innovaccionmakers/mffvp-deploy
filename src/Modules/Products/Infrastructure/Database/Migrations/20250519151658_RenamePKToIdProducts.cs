using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Products.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class RenamePKToIdProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "portafolio_id",
                schema: "productos",
                table: "portafolios",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "plan_id",
                schema: "productos",
                table: "planes",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "parametro_configuracion_id",
                schema: "productos",
                table: "parametros_configuracion",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "objetivo_id",
                schema: "productos",
                table: "objetivos",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "alternativa_id",
                schema: "productos",
                table: "alternativas",
                newName: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "id",
                schema: "productos",
                table: "portafolios",
                newName: "portafolio_id");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "productos",
                table: "planes",
                newName: "plan_id");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "productos",
                table: "parametros_configuracion",
                newName: "parametro_configuracion_id");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "productos",
                table: "objetivos",
                newName: "objetivo_id");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "productos",
                table: "alternativas",
                newName: "alternativa_id");
        }
    }
}
