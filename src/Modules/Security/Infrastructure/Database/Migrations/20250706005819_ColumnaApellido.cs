using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Security.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class ColumnaApellido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "segundo_nombre",
                schema: "seguridad",
                table: "usuarios",
                newName: "apellido");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "apellido",
                schema: "seguridad",
                table: "usuarios",
                newName: "segundo_nombre");
        }
    }
}
