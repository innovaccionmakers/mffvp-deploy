using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Products.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddRowVersionToPensionFund : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "row_version",
                schema: "productos",
                table: "fondos_voluntarios_pensiones",
                type: "bigint",
                nullable: false,
                defaultValueSql: "(extract(epoch from clock_timestamp()) * 1000)::BIGINT");

            migrationBuilder.CreateIndex(
                name: "idx_fondos_voluntarios_pensiones_row_version",
                schema: "productos",
                table: "fondos_voluntarios_pensiones",
                column: "row_version");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_fondos_voluntarios_pensiones_row_version",
                schema: "productos",
                table: "fondos_voluntarios_pensiones");

            migrationBuilder.DropColumn(
                name: "row_version",
                schema: "productos",
                table: "fondos_voluntarios_pensiones");
        }
    }
}
