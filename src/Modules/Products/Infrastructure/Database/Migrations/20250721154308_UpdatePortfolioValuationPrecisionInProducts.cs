using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Products.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePortfolioValuationPrecisionInProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "valor_unidad",
                schema: "productos",
                table: "valoracion_portafolio_dia",
                type: "numeric(38,16)",
                precision: 38,
                scale: 16,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(19,2)",
                oldPrecision: 19,
                oldScale: 2);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "valor_unidad",
                schema: "productos",
                table: "valoracion_portafolio_dia",
                type: "numeric(19,2)",
                precision: 19,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(38,16)",
                oldPrecision: 38,
                oldScale: 16);
        }
    }
}
