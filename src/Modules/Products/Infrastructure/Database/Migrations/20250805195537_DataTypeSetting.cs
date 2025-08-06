using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Products.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class DataTypeSetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "vir_retiro_minimo",
                schema: "productos",
                table: "portafolios",
                type: "numeric(19,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "vir_retiro_max_parcial",
                schema: "productos",
                table: "portafolios",
                type: "numeric(19,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "vir_retiro_minimo",
                schema: "productos",
                table: "portafolios",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(19,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "vir_retiro_max_parcial",
                schema: "productos",
                table: "portafolios",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(19,2)");
        }
    }
}
