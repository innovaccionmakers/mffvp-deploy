using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operations.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddUnitsToTrustOperationTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "unidades",
                schema: "operaciones",
                table: "operaciones_fideicomiso",
                type: "numeric(38,16)",
                precision: 38,
                scale: 16,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "unidades",
                schema: "operaciones",
                table: "operaciones_fideicomiso");
        }
    }
}
