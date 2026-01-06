using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operations.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class FixConfigureColumnsAddRangeValue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "valor_solicitado",
                schema: "operaciones",
                table: "operaciones_fideicomiso",
                type: "numeric(19,2)",
                precision: 19,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "retencion_rendimientos_retiro",
                schema: "operaciones",
                table: "operaciones_fideicomiso",
                type: "numeric(19,2)",
                precision: 19,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "rendimientos_pagados",
                schema: "operaciones",
                table: "operaciones_fideicomiso",
                type: "numeric(19,2)",
                precision: 19,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "capital_pagado",
                schema: "operaciones",
                table: "operaciones_fideicomiso",
                type: "numeric(19,2)",
                precision: 19,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldDefaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "valor_solicitado",
                schema: "operaciones",
                table: "operaciones_fideicomiso",
                type: "numeric",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(19,2)",
                oldPrecision: 19,
                oldScale: 2,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "retencion_rendimientos_retiro",
                schema: "operaciones",
                table: "operaciones_fideicomiso",
                type: "numeric",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(19,2)",
                oldPrecision: 19,
                oldScale: 2,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "rendimientos_pagados",
                schema: "operaciones",
                table: "operaciones_fideicomiso",
                type: "numeric",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(19,2)",
                oldPrecision: 19,
                oldScale: 2,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "capital_pagado",
                schema: "operaciones",
                table: "operaciones_fideicomiso",
                type: "numeric",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(19,2)",
                oldPrecision: 19,
                oldScale: 2,
                oldDefaultValue: 0m);
        }
    }
}
