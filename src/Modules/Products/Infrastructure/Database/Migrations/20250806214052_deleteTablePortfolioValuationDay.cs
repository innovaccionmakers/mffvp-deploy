using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Products.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class deleteTablePortfolioValuationDay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "valoracion_portafolio_dia",
                schema: "productos");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "valoracion_portafolio_dia",
                schema: "productos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    portfolio_id = table.Column<int>(type: "integer", nullable: false),
                    fecha_cierre = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    rentabilidad_diaria = table.Column<decimal>(type: "numeric(38,16)", precision: 38, scale: 16, nullable: false),
                    rendimiento_bruto_unidad = table.Column<decimal>(type: "numeric(38,16)", precision: 38, scale: 16, nullable: false),
                    operaciones_entrada = table.Column<decimal>(type: "numeric(19,2)", precision: 19, scale: 2, nullable: false),
                    operaciones_salida = table.Column<decimal>(type: "numeric(19,2)", precision: 19, scale: 2, nullable: false),
                    fecha_proceso = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    costo_unidad = table.Column<decimal>(type: "numeric(38,16)", precision: 38, scale: 16, nullable: false),
                    valor_unidad = table.Column<decimal>(type: "numeric(38,16)", precision: 38, scale: 16, nullable: false),
                    unidades = table.Column<decimal>(type: "numeric(38,16)", precision: 38, scale: 16, nullable: false),
                    valor = table.Column<decimal>(type: "numeric(19,2)", precision: 19, scale: 2, nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_valoracion_portafolio_dia_portfolio_id",
                schema: "productos",
                table: "valoracion_portafolio_dia",
                column: "portfolio_id");
        }
    }
}
