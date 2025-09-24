using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Accounting.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class RemoveNitColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "nit",
                schema: "contabilidad",
                table: "auxiliar_contable");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "nit",
                schema: "contabilidad",
                table: "auxiliar_contable",
                type: "character varying(12)",
                maxLength: 12,
                nullable: true);
        }
    }
}
