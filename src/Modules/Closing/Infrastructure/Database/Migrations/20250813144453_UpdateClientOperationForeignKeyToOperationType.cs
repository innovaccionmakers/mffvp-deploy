using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Closing.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class UpdateClientOperationForeignKeyToOperationType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "subtipo_transaccion_id",
                schema: "cierre",
                table: "operaciones_cliente",
                newName: "tipo_operaciones_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "tipo_operaciones_id",
                schema: "cierre",
                table: "operaciones_cliente",
                newName: "subtipo_transaccion_id");
        }
    }
}
