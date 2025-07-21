using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Closing.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePortfolioValuationPrecisionInClosing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "valor_unidad",
                schema: "cierre",
                table: "valoracion_portafolio",
                type: "numeric(38,16)",
                precision: 38,
                scale: 16,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(19,2)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "valor_unidad",
                schema: "cierre",
                table: "valoracion_portafolio",
                type: "numeric(19,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(38,16)",
                oldPrecision: 38,
                oldScale: 16);
        }
    }
}
