using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trusts.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddProtectedBalanceAndAgileWithdrawalToTrusts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "disponible_retiro_agil",
                schema: "fideicomisos",
                table: "fideicomisos",
                type: "numeric(19,2)",
                precision: 19,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "saldo_protegido",
                schema: "fideicomisos",
                table: "fideicomisos",
                type: "numeric(19,2)",
                precision: 19,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "disponible_retiro_agil",
                schema: "fideicomisos",
                table: "fideicomisos");

            migrationBuilder.DropColumn(
                name: "saldo_protegido",
                schema: "fideicomisos",
                table: "fideicomisos");
        }
    }
}
