using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Customers.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class ChangeCodeCity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "codigo_ciudad",
                schema: "personas",
                table: "municipios",
                newName: "codigo_municipio");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "codigo_municipio",
                schema: "personas",
                table: "municipios",
                newName: "codigo_ciudad");
        }
    }
}
