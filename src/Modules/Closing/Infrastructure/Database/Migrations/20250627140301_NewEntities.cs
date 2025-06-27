using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Closing.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class NewEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "operaciones_cliente",
                schema: "cierre",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fecha_radicacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    afiliado_id = table.Column<int>(type: "integer", nullable: false),
                    objetivo_id = table.Column<int>(type: "integer", nullable: false),
                    portafolio_id = table.Column<int>(type: "integer", nullable: false),
                    valor = table.Column<decimal>(type: "numeric(19,2)", nullable: false),
                    fecha_proceso = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    subtipo_transaccion_id = table.Column<long>(type: "bigint", nullable: false),
                    fecha_aplicacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_operaciones_cliente", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rendimientos",
                schema: "cierre",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    portafolio_id = table.Column<int>(type: "integer", nullable: false),
                    ingresos = table.Column<decimal>(type: "numeric(19,2)", nullable: false),
                    gastos = table.Column<decimal>(type: "numeric(19,2)", nullable: false),
                    comisiones = table.Column<decimal>(type: "numeric(19,2)", nullable: false),
                    costos = table.Column<decimal>(type: "numeric(19,2)", nullable: false),
                    rendimientos_abonar = table.Column<decimal>(type: "numeric(19,2)", nullable: false),
                    fecha_cierre = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fecha_proceso = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    cerrado = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rendimientos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rendimientos_fideicomisos",
                schema: "cierre",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fideicomiso_id = table.Column<int>(type: "integer", nullable: false),
                    portafolio_id = table.Column<int>(type: "integer", nullable: false),
                    fecha_cierre = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    participacion = table.Column<decimal>(type: "numeric(38,16)", nullable: false),
                    unidades = table.Column<decimal>(type: "numeric(38,16)", nullable: false),
                    rendimientos = table.Column<decimal>(type: "numeric(19,2)", nullable: false),
                    saldo_precierre = table.Column<decimal>(type: "numeric(19,2)", nullable: false),
                    saldo_cierre = table.Column<decimal>(type: "numeric(19,2)", nullable: false),
                    ingresos = table.Column<decimal>(type: "numeric(19,2)", nullable: false),
                    gastos = table.Column<decimal>(type: "numeric(19,2)", nullable: false),
                    comisiones = table.Column<decimal>(type: "numeric(19,2)", nullable: false),
                    costo = table.Column<decimal>(type: "numeric(19,2)", nullable: false),
                    capital = table.Column<decimal>(type: "numeric(19,2)", nullable: false),
                    fecha_proceso = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    retencion_contingente = table.Column<decimal>(type: "numeric(19,2)", nullable: false),
                    retencion_rendimiento = table.Column<decimal>(type: "numeric(19,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rendimientos_fideicomisos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "valoracion_portafolio",
                schema: "cierre",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    portafolio_id = table.Column<int>(type: "integer", nullable: false),
                    fecha_cierre = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    valor = table.Column<decimal>(type: "numeric(19,2)", nullable: false),
                    unidades = table.Column<decimal>(type: "numeric(38,16)", nullable: false),
                    valor_unidad = table.Column<decimal>(type: "numeric(19,2)", nullable: false),
                    rendimiento_bruto_unidad = table.Column<decimal>(type: "numeric(38,16)", nullable: false),
                    costo_unidad = table.Column<decimal>(type: "numeric(38,16)", nullable: false),
                    rentabilidad_diaria = table.Column<decimal>(type: "numeric(38,16)", nullable: false),
                    operaciones_entrada = table.Column<decimal>(type: "numeric(19,2)", nullable: false),
                    operaciones_salida = table.Column<decimal>(type: "numeric(19,2)", nullable: false),
                    fecha_proceso = table.Column<DateTime>(type: "date", nullable: false),
                    cerrado = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_valoracion_portafolio", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "detalle_rendimientos",
                schema: "cierre",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    rendimiento_id = table.Column<long>(type: "bigint", nullable: false),
                    portafolio_id = table.Column<int>(type: "integer", nullable: false),
                    fecha_cierre = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fuente = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    concepto = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    ingresos = table.Column<decimal>(type: "numeric(19,2)", nullable: false),
                    gastos = table.Column<decimal>(type: "numeric(19,2)", nullable: false),
                    comisiones = table.Column<decimal>(type: "numeric(19,2)", nullable: false),
                    fecha_proceso = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    cerrado = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_detalle_rendimientos", x => x.id);
                    table.ForeignKey(
                        name: "FK_detalle_rendimientos_rendimientos_rendimiento_id",
                        column: x => x.rendimiento_id,
                        principalSchema: "cierre",
                        principalTable: "rendimientos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_pyg_concepto_id",
                schema: "cierre",
                table: "pyg",
                column: "concepto_id");

            migrationBuilder.CreateIndex(
                name: "IX_detalle_rendimientos_rendimiento_id",
                schema: "cierre",
                table: "detalle_rendimientos",
                column: "rendimiento_id");

            migrationBuilder.AddForeignKey(
                name: "FK_pyg_conceptos_pyg_concepto_id",
                schema: "cierre",
                table: "pyg",
                column: "concepto_id",
                principalSchema: "cierre",
                principalTable: "conceptos_pyg",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_pyg_conceptos_pyg_concepto_id",
                schema: "cierre",
                table: "pyg");

            migrationBuilder.DropTable(
                name: "detalle_rendimientos",
                schema: "cierre");

            migrationBuilder.DropTable(
                name: "operaciones_cliente",
                schema: "cierre");

            migrationBuilder.DropTable(
                name: "rendimientos_fideicomisos",
                schema: "cierre");

            migrationBuilder.DropTable(
                name: "valoracion_portafolio",
                schema: "cierre");

            migrationBuilder.DropTable(
                name: "rendimientos",
                schema: "cierre");

            migrationBuilder.DropIndex(
                name: "IX_pyg_concepto_id",
                schema: "cierre",
                table: "pyg");
        }
    }
}
