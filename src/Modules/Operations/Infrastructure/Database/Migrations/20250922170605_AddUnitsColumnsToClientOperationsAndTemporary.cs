using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operations.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddUnitsColumnsToClientOperationsAndTemporary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "estado",
                schema: "operaciones",
                table: "operaciones_clientes_temporal",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<long>(
                name: "fideicomiso_id",
                schema: "operaciones",
                table: "operaciones_clientes_temporal",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "operaciones_cliente_id",
                schema: "operaciones",
                table: "operaciones_clientes_temporal",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "unidades",
                schema: "operaciones",
                table: "operaciones_clientes_temporal",
                type: "numeric(38,16)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "estado",
                schema: "operaciones",
                table: "operaciones_clientes",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<long>(
                name: "fideicomiso_id",
                schema: "operaciones",
                table: "operaciones_clientes",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "operaciones_cliente_id",
                schema: "operaciones",
                table: "operaciones_clientes",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "unidades",
                schema: "operaciones",
                table: "operaciones_clientes",
                type: "numeric(38,16)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "estado",
                schema: "operaciones",
                table: "operaciones_clientes_temporal");

            migrationBuilder.DropColumn(
                name: "fideicomiso_id",
                schema: "operaciones",
                table: "operaciones_clientes_temporal");

            migrationBuilder.DropColumn(
                name: "operaciones_cliente_id",
                schema: "operaciones",
                table: "operaciones_clientes_temporal");

            migrationBuilder.DropColumn(
                name: "unidades",
                schema: "operaciones",
                table: "operaciones_clientes_temporal");

            migrationBuilder.DropColumn(
                name: "estado",
                schema: "operaciones",
                table: "operaciones_clientes");

            migrationBuilder.DropColumn(
                name: "fideicomiso_id",
                schema: "operaciones",
                table: "operaciones_clientes");

            migrationBuilder.DropColumn(
                name: "operaciones_cliente_id",
                schema: "operaciones",
                table: "operaciones_clientes");

            migrationBuilder.DropColumn(
                name: "unidades",
                schema: "operaciones",
                table: "operaciones_clientes");
        }
    }
}
