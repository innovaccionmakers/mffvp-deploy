using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Customers.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddRowVersionAndIndexToPerson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "row_version",
                schema: "personas",
                table: "personas",
                type: "bigint",
                nullable: false,
                defaultValueSql: "(extract(epoch from clock_timestamp()) * 1000)::BIGINT");

            migrationBuilder.AlterColumn<int>(
                name: "codigo_dane",
                schema: "personas",
                table: "paises",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "idx_row_version",
                schema: "personas",
                table: "personas",
                column: "row_version");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_row_version",
                schema: "personas",
                table: "personas");

            migrationBuilder.DropColumn(
                name: "row_version",
                schema: "personas",
                table: "personas");

            migrationBuilder.AlterColumn<string>(
                name: "codigo_dane",
                schema: "personas",
                table: "paises",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}
