using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operations.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class FixConfigureColumnRetencionRendimientoRetiroAddRangeValue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "retencion_contingente_retiro",
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
                name: "retencion_contingente_retiro",
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
