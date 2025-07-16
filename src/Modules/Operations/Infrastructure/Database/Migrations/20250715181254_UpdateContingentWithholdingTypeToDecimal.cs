using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operations.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class UpdateContingentWithholdingTypeToDecimal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "retencion_contingente",
                schema: "operaciones",
                table: "informacion_auxiliar_temporal",
                type: "numeric(19,2)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<decimal>(
                name: "retencion_contingente",
                schema: "operaciones",
                table: "informacion_auxiliar",
                type: "numeric(19,2)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "retencion_contingente",
                schema: "operaciones",
                table: "informacion_auxiliar_temporal",
                type: "integer",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(19,2)");

            migrationBuilder.AlterColumn<int>(
                name: "retencion_contingente",
                schema: "operaciones",
                table: "informacion_auxiliar",
                type: "integer",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(19,2)");
        }
    }
}
