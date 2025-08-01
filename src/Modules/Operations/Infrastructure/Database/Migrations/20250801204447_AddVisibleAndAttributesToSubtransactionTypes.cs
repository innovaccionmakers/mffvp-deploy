using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operations.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddVisibleAndAttributesToSubtransactionTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<JsonDocument>(
                name: "atributos_adicionales",
                schema: "operaciones",
                table: "subtipo_transacciones",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'{}'::jsonb");

            migrationBuilder.AddColumn<bool>(
                name: "visible",
                schema: "operaciones",
                table: "subtipo_transacciones",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "atributos_adicionales",
                schema: "operaciones",
                table: "subtipo_transacciones");

            migrationBuilder.DropColumn(
                name: "visible",
                schema: "operaciones",
                table: "subtipo_transacciones");
        }
    }
}
