using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operations.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class NewColumnsTrustOperation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "capital_pagado",
                schema: "operaciones",
                table: "operaciones_fideicomiso",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "rendimientos_pagados",
                schema: "operaciones",
                table: "operaciones_fideicomiso",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "retencion_contingente_retiro",
                schema: "operaciones",
                table: "operaciones_fideicomiso",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "retencion_rendimientos_retiro",
                schema: "operaciones",
                table: "operaciones_fideicomiso",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "valor_solicitado",
                schema: "operaciones",
                table: "operaciones_fideicomiso",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_operaciones_clientes_operaciones_cliente_id",
                schema: "operaciones",
                table: "operaciones_clientes",
                column: "operaciones_cliente_id");

            migrationBuilder.AddForeignKey(
                name: "FK_operaciones_clientes_operaciones_clientes_operaciones_clien~",
                schema: "operaciones",
                table: "operaciones_clientes",
                column: "operaciones_cliente_id",
                principalSchema: "operaciones",
                principalTable: "operaciones_clientes",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_operaciones_clientes_operaciones_clientes_operaciones_clien~",
                schema: "operaciones",
                table: "operaciones_clientes");

            migrationBuilder.DropIndex(
                name: "IX_operaciones_clientes_operaciones_cliente_id",
                schema: "operaciones",
                table: "operaciones_clientes");

            migrationBuilder.DropColumn(
                name: "capital_pagado",
                schema: "operaciones",
                table: "operaciones_fideicomiso");

            migrationBuilder.DropColumn(
                name: "rendimientos_pagados",
                schema: "operaciones",
                table: "operaciones_fideicomiso");

            migrationBuilder.DropColumn(
                name: "retencion_contingente_retiro",
                schema: "operaciones",
                table: "operaciones_fideicomiso");

            migrationBuilder.DropColumn(
                name: "retencion_rendimientos_retiro",
                schema: "operaciones",
                table: "operaciones_fideicomiso");

            migrationBuilder.DropColumn(
                name: "valor_solicitado",
                schema: "operaciones",
                table: "operaciones_fideicomiso");
        }
    }
}
