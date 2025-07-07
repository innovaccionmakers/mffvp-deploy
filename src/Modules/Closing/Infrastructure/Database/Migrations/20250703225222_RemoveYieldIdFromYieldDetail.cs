using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Closing.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class RemoveYieldIdFromYieldDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_detalle_rendimientos_rendimientos_rendimiento_id",
                schema: "cierre",
                table: "detalle_rendimientos");

            migrationBuilder.DropIndex(
                name: "IX_detalle_rendimientos_rendimiento_id",
                schema: "cierre",
                table: "detalle_rendimientos");

            migrationBuilder.DropColumn(
                name: "rendimiento_id",
                schema: "cierre",
                table: "detalle_rendimientos");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "rendimiento_id",
                schema: "cierre",
                table: "detalle_rendimientos",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_detalle_rendimientos_rendimiento_id",
                schema: "cierre",
                table: "detalle_rendimientos",
                column: "rendimiento_id");

            migrationBuilder.AddForeignKey(
                name: "FK_detalle_rendimientos_rendimientos_rendimiento_id",
                schema: "cierre",
                table: "detalle_rendimientos",
                column: "rendimiento_id",
                principalSchema: "cierre",
                principalTable: "rendimientos",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
