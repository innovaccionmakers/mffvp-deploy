using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Operations.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class RefactorSubtransactionTypeToHierarchicalOperationType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_operaciones_clientes_subtipo_transacciones_subtipo_transacc~",
                schema: "operaciones",
                table: "operaciones_clientes");

            migrationBuilder.DropTable(
                name: "subtipo_transacciones",
                schema: "operaciones");

            migrationBuilder.RenameColumn(
                name: "subtipo_transaccion_id",
                schema: "operaciones",
                table: "operaciones_fideicomiso",
                newName: "tipo_operaciones_id");

            migrationBuilder.RenameColumn(
                name: "subtipo_transaccion_id",
                schema: "operaciones",
                table: "operaciones_clientes_temporal",
                newName: "tipo_operaciones_id");

            migrationBuilder.RenameColumn(
                name: "subtipo_transaccion_id",
                schema: "operaciones",
                table: "operaciones_clientes",
                newName: "tipo_operaciones_id");

            migrationBuilder.RenameIndex(
                name: "IX_operaciones_clientes_subtipo_transaccion_id",
                schema: "operaciones",
                table: "operaciones_clientes",
                newName: "IX_operaciones_clientes_tipo_operaciones_id");

            migrationBuilder.CreateTable(
                name: "tipos_operaciones",
                schema: "operaciones",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    categoria = table.Column<int>(type: "integer", nullable: true),
                    naturaleza = table.Column<string>(type: "text", nullable: false),
                    estado = table.Column<string>(type: "text", nullable: false),
                    externo = table.Column<string>(type: "text", nullable: false),
                    visible = table.Column<bool>(type: "boolean", nullable: false),
                    atributos_adicionales = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    codigo_homologado = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tipos_operaciones", x => x.id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_operaciones_clientes_tipos_operaciones_tipo_operaciones_id",
                schema: "operaciones",
                table: "operaciones_clientes",
                column: "tipo_operaciones_id",
                principalSchema: "operaciones",
                principalTable: "tipos_operaciones",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_operaciones_clientes_tipos_operaciones_tipo_operaciones_id",
                schema: "operaciones",
                table: "operaciones_clientes");

            migrationBuilder.DropTable(
                name: "tipos_operaciones",
                schema: "operaciones");

            migrationBuilder.RenameColumn(
                name: "tipo_operaciones_id",
                schema: "operaciones",
                table: "operaciones_fideicomiso",
                newName: "subtipo_transaccion_id");

            migrationBuilder.RenameColumn(
                name: "tipo_operaciones_id",
                schema: "operaciones",
                table: "operaciones_clientes_temporal",
                newName: "subtipo_transaccion_id");

            migrationBuilder.RenameColumn(
                name: "tipo_operaciones_id",
                schema: "operaciones",
                table: "operaciones_clientes",
                newName: "subtipo_transaccion_id");

            migrationBuilder.RenameIndex(
                name: "IX_operaciones_clientes_tipo_operaciones_id",
                schema: "operaciones",
                table: "operaciones_clientes",
                newName: "IX_operaciones_clientes_subtipo_transaccion_id");

            migrationBuilder.CreateTable(
                name: "subtipo_transacciones",
                schema: "operaciones",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    atributos_adicionales = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    categoria = table.Column<Guid>(type: "uuid", nullable: true),
                    externo = table.Column<string>(type: "text", nullable: false),
                    codigo_homologado = table.Column<string>(type: "text", nullable: false),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    naturaleza = table.Column<string>(type: "text", nullable: false),
                    estado = table.Column<string>(type: "text", nullable: false),
                    visible = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subtipo_transacciones", x => x.id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_operaciones_clientes_subtipo_transacciones_subtipo_transacc~",
                schema: "operaciones",
                table: "operaciones_clientes",
                column: "subtipo_transaccion_id",
                principalSchema: "operaciones",
                principalTable: "subtipo_transacciones",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
