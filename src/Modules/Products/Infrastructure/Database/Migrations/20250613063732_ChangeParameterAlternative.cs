using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Products.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class ChangeParameterAlternative : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "codigo_homologado)",
                schema: "productos",
                table: "alternativas",
                newName: "codigo_homologado");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "codigo_homologado",
                schema: "productos",
                table: "alternativas",
                newName: "codigo_homologado)");
        }
    }
}
