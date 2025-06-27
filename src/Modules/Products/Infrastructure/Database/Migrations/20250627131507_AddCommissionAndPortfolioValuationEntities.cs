using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Products.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddCommissionAndPortfolioValuationEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "comisiones",
                schema: "productos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    portfolio_id = table.Column<int>(type: "integer", nullable: false),
                    fecha_proceso = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    concepto = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    modalidad = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    tipo_comision = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    periodo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    base_calculo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    regla_calculo = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    activo = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_comisiones", x => x.id);
                    table.ForeignKey(
                        name: "FK_comisiones_portafolios_portfolio_id",
                        column: x => x.portfolio_id,
                        principalSchema: "productos",
                        principalTable: "portafolios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "valoracion_portafolio_dia",
                schema: "productos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    portfolio_id = table.Column<int>(type: "integer", nullable: false),
                    fecha_cierre = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    valor = table.Column<decimal>(type: "numeric(19,2)", precision: 19, scale: 2, nullable: false),
                    unidades = table.Column<decimal>(type: "numeric(38,16)", precision: 38, scale: 16, nullable: false),
                    valor_unidad = table.Column<decimal>(type: "numeric(19,2)", precision: 19, scale: 2, nullable: false),
                    rendimiento_bruto_unidad = table.Column<decimal>(type: "numeric(38,16)", precision: 38, scale: 16, nullable: false),
                    costo_unidad = table.Column<decimal>(type: "numeric(38,16)", precision: 38, scale: 16, nullable: false),
                    rentabilidad_diaria = table.Column<decimal>(type: "numeric(38,16)", precision: 38, scale: 16, nullable: false),
                    operaciones_entrada = table.Column<decimal>(type: "numeric(19,2)", precision: 19, scale: 2, nullable: false),
                    operaciones_salida = table.Column<decimal>(type: "numeric(19,2)", precision: 19, scale: 2, nullable: false),
                    fecha_proceso = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_valoracion_portafolio_dia", x => x.id);
                    table.ForeignKey(
                        name: "FK_valoracion_portafolio_dia_portafolios_portfolio_id",
                        column: x => x.portfolio_id,
                        principalSchema: "productos",
                        principalTable: "portafolios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "comisiones_acumuladas",
                schema: "productos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    portfolio_id = table.Column<int>(type: "integer", nullable: false),
                    comisiones_id = table.Column<int>(type: "integer", nullable: false),
                    valor_acumulado = table.Column<decimal>(type: "numeric(19,2)", precision: 19, scale: 2, nullable: false),
                    valor_pagado = table.Column<decimal>(type: "numeric(19,2)", precision: 19, scale: 2, nullable: false),
                    valor_pendiente = table.Column<decimal>(type: "numeric(19,2)", precision: 19, scale: 2, nullable: false),
                    fecha_cierre = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fecha_pago = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fecha_proceso = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_comisiones_acumuladas", x => x.id);
                    table.ForeignKey(
                        name: "FK_comisiones_acumuladas_comisiones_comisiones_id",
                        column: x => x.comisiones_id,
                        principalSchema: "productos",
                        principalTable: "comisiones",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_comisiones_portfolio_id",
                schema: "productos",
                table: "comisiones",
                column: "portfolio_id");

            migrationBuilder.CreateIndex(
                name: "IX_comisiones_acumuladas_comisiones_id",
                schema: "productos",
                table: "comisiones_acumuladas",
                column: "comisiones_id");

            migrationBuilder.CreateIndex(
                name: "IX_valoracion_portafolio_dia_portfolio_id",
                schema: "productos",
                table: "valoracion_portafolio_dia",
                column: "portfolio_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "comisiones_acumuladas",
                schema: "productos");

            migrationBuilder.DropTable(
                name: "valoracion_portafolio_dia",
                schema: "productos");

            migrationBuilder.DropTable(
                name: "comisiones",
                schema: "productos");
        }
    }
}
