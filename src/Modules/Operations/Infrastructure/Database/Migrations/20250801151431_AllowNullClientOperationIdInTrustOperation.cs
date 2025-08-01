using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operations.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AllowNullClientOperationIdInTrustOperation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_operaciones_fideicomiso_operaciones_clientes_operaciones_cl~",
                schema: "operaciones",
                table: "operaciones_fideicomiso");

            migrationBuilder.AlterColumn<long>(
                name: "operaciones_clientes_id",
                schema: "operaciones",
                table: "operaciones_fideicomiso",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddForeignKey(
                name: "FK_operaciones_fideicomiso_operaciones_clientes_operaciones_cl~",
                schema: "operaciones",
                table: "operaciones_fideicomiso",
                column: "operaciones_clientes_id",
                principalSchema: "operaciones",
                principalTable: "operaciones_clientes",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_operaciones_fideicomiso_operaciones_clientes_operaciones_cl~",
                schema: "operaciones",
                table: "operaciones_fideicomiso");

            migrationBuilder.AlterColumn<long>(
                name: "operaciones_clientes_id",
                schema: "operaciones",
                table: "operaciones_fideicomiso",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_operaciones_fideicomiso_operaciones_clientes_operaciones_cl~",
                schema: "operaciones",
                table: "operaciones_fideicomiso",
                column: "operaciones_clientes_id",
                principalSchema: "operaciones",
                principalTable: "operaciones_clientes",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
