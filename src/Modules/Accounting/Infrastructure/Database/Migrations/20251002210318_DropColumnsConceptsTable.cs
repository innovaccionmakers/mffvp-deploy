using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Accounting.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class DropColumnsConceptsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cuenta_contra_credito",
                schema: "contabilidad",
                table: "conceptos");

            migrationBuilder.DropColumn(
                name: "cuenta_contra_debito",
                schema: "contabilidad",
                table: "conceptos");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "cuenta_contra_credito",
                schema: "contabilidad",
                table: "conceptos",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cuenta_contra_debito",
                schema: "contabilidad",
                table: "conceptos",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);
        }
    }
}
