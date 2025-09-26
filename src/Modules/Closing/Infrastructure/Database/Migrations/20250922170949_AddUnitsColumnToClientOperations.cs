using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Closing.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddUnitsColumnToClientOperations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "estado",
                schema: "cierre",
                table: "operaciones_cliente",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<long>(
                name: "fideicomiso_id",
                schema: "cierre",
                table: "operaciones_cliente",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "operaciones_cliente_id",
                schema: "cierre",
                table: "operaciones_cliente",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "unidades",
                schema: "cierre",
                table: "operaciones_cliente",
                type: "numeric(38,16)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "estado",
                schema: "cierre",
                table: "operaciones_cliente");

            migrationBuilder.DropColumn(
                name: "fideicomiso_id",
                schema: "cierre",
                table: "operaciones_cliente");

            migrationBuilder.DropColumn(
                name: "operaciones_cliente_id",
                schema: "cierre",
                table: "operaciones_cliente");

            migrationBuilder.DropColumn(
                name: "unidades",
                schema: "cierre",
                table: "operaciones_cliente");
        }
    }
}
