using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Operations.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class CreateOperationsModuleTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ciudad",
                schema: "operaciones",
                table: "informacion_auxiliar");

            migrationBuilder.RenameColumn(
                name: "subtipotransaccion_id",
                schema: "operaciones",
                table: "operaciones_clientes",
                newName: "subtipo_transaccion_id");

            migrationBuilder.RenameColumn(
                name: "fecha",
                schema: "operaciones",
                table: "operaciones_clientes",
                newName: "fecha_radicacion");

            migrationBuilder.AlterColumn<long>(
                name: "subtipo_transaccion_id",
                schema: "operaciones",
                table: "operaciones_clientes",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_causacion",
                schema: "operaciones",
                table: "operaciones_clientes",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_proceso",
                schema: "operaciones",
                table: "operaciones_clientes",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "canal_id",
                schema: "operaciones",
                table: "informacion_auxiliar",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ciudad_id",
                schema: "operaciones",
                table: "informacion_auxiliar",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "oficinas_id",
                schema: "operaciones",
                table: "informacion_auxiliar",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "usuario_id",
                schema: "operaciones",
                table: "informacion_auxiliar",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "canales",
                schema: "operaciones",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    codigo_homologado = table.Column<string>(type: "text", nullable: false),
                    sistema = table.Column<bool>(type: "boolean", nullable: false),
                    estado = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_canales", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "operaciones_retiro_fideicomiso",
                schema: "operaciones",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    operaciones_clientes_id = table.Column<long>(type: "bigint", nullable: false),
                    fideicomiso_id = table.Column<long>(type: "bigint", nullable: false),
                    valor = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_operaciones_retiro_fideicomiso", x => x.id);
                    table.ForeignKey(
                        name: "FK_operaciones_retiro_fideicomiso_operaciones_clientes_operaci~",
                        column: x => x.operaciones_clientes_id,
                        principalSchema: "operaciones",
                        principalTable: "operaciones_clientes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "origen_aportes",
                schema: "operaciones",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    obligatoriedad_originador = table.Column<bool>(type: "boolean", nullable: false),
                    exige_certificacion = table.Column<bool>(type: "boolean", nullable: false),
                    exige_retencion_contingente = table.Column<bool>(type: "boolean", nullable: false),
                    estado = table.Column<string>(type: "text", nullable: false),
                    codigo_homologado = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_origen_aportes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "subtipo_transacciones",
                schema: "operaciones",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    categoria = table.Column<string>(type: "text", nullable: false),
                    naturaleza = table.Column<string>(type: "text", nullable: false),
                    estado = table.Column<string>(type: "text", nullable: false),
                    externo = table.Column<string>(type: "text", nullable: false),
                    codigo_homologado = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subtipo_transacciones", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_operaciones_clientes_subtipo_transaccion_id",
                schema: "operaciones",
                table: "operaciones_clientes",
                column: "subtipo_transaccion_id");

            migrationBuilder.CreateIndex(
                name: "IX_informacion_auxiliar_canal_id",
                schema: "operaciones",
                table: "informacion_auxiliar",
                column: "canal_id");

            migrationBuilder.CreateIndex(
                name: "IX_informacion_auxiliar_operacion_cliente_id",
                schema: "operaciones",
                table: "informacion_auxiliar",
                column: "operacion_cliente_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_informacion_auxiliar_origen_id",
                schema: "operaciones",
                table: "informacion_auxiliar",
                column: "origen_id");

            migrationBuilder.CreateIndex(
                name: "IX_operaciones_retiro_fideicomiso_operaciones_clientes_id",
                schema: "operaciones",
                table: "operaciones_retiro_fideicomiso",
                column: "operaciones_clientes_id");

            migrationBuilder.AddForeignKey(
                name: "FK_informacion_auxiliar_canales_canal_id",
                schema: "operaciones",
                table: "informacion_auxiliar",
                column: "canal_id",
                principalSchema: "operaciones",
                principalTable: "canales",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_informacion_auxiliar_operaciones_clientes_operacion_cliente~",
                schema: "operaciones",
                table: "informacion_auxiliar",
                column: "operacion_cliente_id",
                principalSchema: "operaciones",
                principalTable: "operaciones_clientes",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_informacion_auxiliar_origen_aportes_origen_id",
                schema: "operaciones",
                table: "informacion_auxiliar",
                column: "origen_id",
                principalSchema: "operaciones",
                principalTable: "origen_aportes",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_informacion_auxiliar_canales_canal_id",
                schema: "operaciones",
                table: "informacion_auxiliar");

            migrationBuilder.DropForeignKey(
                name: "FK_informacion_auxiliar_operaciones_clientes_operacion_cliente~",
                schema: "operaciones",
                table: "informacion_auxiliar");

            migrationBuilder.DropForeignKey(
                name: "FK_informacion_auxiliar_origen_aportes_origen_id",
                schema: "operaciones",
                table: "informacion_auxiliar");

            migrationBuilder.DropForeignKey(
                name: "FK_operaciones_clientes_subtipo_transacciones_subtipo_transacc~",
                schema: "operaciones",
                table: "operaciones_clientes");

            migrationBuilder.DropTable(
                name: "canales",
                schema: "operaciones");

            migrationBuilder.DropTable(
                name: "operaciones_retiro_fideicomiso",
                schema: "operaciones");

            migrationBuilder.DropTable(
                name: "origen_aportes",
                schema: "operaciones");

            migrationBuilder.DropTable(
                name: "subtipo_transacciones",
                schema: "operaciones");

            migrationBuilder.DropIndex(
                name: "IX_operaciones_clientes_subtipo_transaccion_id",
                schema: "operaciones",
                table: "operaciones_clientes");

            migrationBuilder.DropIndex(
                name: "IX_informacion_auxiliar_canal_id",
                schema: "operaciones",
                table: "informacion_auxiliar");

            migrationBuilder.DropIndex(
                name: "IX_informacion_auxiliar_operacion_cliente_id",
                schema: "operaciones",
                table: "informacion_auxiliar");

            migrationBuilder.DropIndex(
                name: "IX_informacion_auxiliar_origen_id",
                schema: "operaciones",
                table: "informacion_auxiliar");

            migrationBuilder.DropColumn(
                name: "fecha_causacion",
                schema: "operaciones",
                table: "operaciones_clientes");

            migrationBuilder.DropColumn(
                name: "fecha_proceso",
                schema: "operaciones",
                table: "operaciones_clientes");

            migrationBuilder.DropColumn(
                name: "canal_id",
                schema: "operaciones",
                table: "informacion_auxiliar");

            migrationBuilder.DropColumn(
                name: "ciudad_id",
                schema: "operaciones",
                table: "informacion_auxiliar");

            migrationBuilder.DropColumn(
                name: "oficinas_id",
                schema: "operaciones",
                table: "informacion_auxiliar");

            migrationBuilder.DropColumn(
                name: "usuario_id",
                schema: "operaciones",
                table: "informacion_auxiliar");

            migrationBuilder.RenameColumn(
                name: "subtipo_transaccion_id",
                schema: "operaciones",
                table: "operaciones_clientes",
                newName: "subtipotransaccion_id");

            migrationBuilder.RenameColumn(
                name: "fecha_radicacion",
                schema: "operaciones",
                table: "operaciones_clientes",
                newName: "fecha");

            migrationBuilder.AlterColumn<int>(
                name: "subtipotransaccion_id",
                schema: "operaciones",
                table: "operaciones_clientes",
                type: "integer",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<string>(
                name: "ciudad",
                schema: "operaciones",
                table: "informacion_auxiliar",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
