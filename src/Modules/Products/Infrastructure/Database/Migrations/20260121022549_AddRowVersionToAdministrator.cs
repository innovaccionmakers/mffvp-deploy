using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Products.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddRowVersionToAdministrator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "row_version",
                schema: "productos",
                table: "administradores",
                type: "bigint",
                nullable: false,
                defaultValueSql: "(extract(epoch from clock_timestamp()) * 1000)::BIGINT");

            migrationBuilder.CreateIndex(
                name: "idx_administradores_row_version",
                schema: "productos",
                table: "administradores",
                column: "row_version");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_administradores_row_version",
                schema: "productos",
                table: "administradores");

            migrationBuilder.DropColumn(
                name: "row_version",
                schema: "productos",
                table: "administradores");
        }
    }
}
