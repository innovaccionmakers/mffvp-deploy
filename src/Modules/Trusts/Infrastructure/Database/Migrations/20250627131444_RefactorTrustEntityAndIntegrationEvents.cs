using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trusts.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class RefactorTrustEntityAndIntegrationEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "estado",
                schema: "fideicomisos",
                table: "fideicomisos",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<decimal>(
                name: "rendimiento_acumulado",
                schema: "fideicomisos",
                table: "fideicomisos",
                type: "numeric(19,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "estado",
                schema: "fideicomisos",
                table: "fideicomisos");

            migrationBuilder.DropColumn(
                name: "rendimiento_acumulado",
                schema: "fideicomisos",
                table: "fideicomisos");
        }
    }
}
