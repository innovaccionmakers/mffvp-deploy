using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trusts.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTrustsUnitsTypeAndRemoveAccumulatedYield : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "rendimiento_acumulado",
                schema: "fideicomisos",
                table: "fideicomisos");

            migrationBuilder.AlterColumn<decimal>(
                name: "unidades_totales",
                schema: "fideicomisos",
                table: "fideicomisos",
                type: "numeric(38,16)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "unidades_totales",
                schema: "fideicomisos",
                table: "fideicomisos",
                type: "integer",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(38,16)");

            migrationBuilder.AddColumn<decimal>(
                name: "rendimiento_acumulado",
                schema: "fideicomisos",
                table: "fideicomisos",
                type: "numeric(19,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
