using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Trusts.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "fideicomisos");

            migrationBuilder.CreateTable(
                name: "fideicomisos",
                schema: "fideicomisos",
                columns: table => new
                {
                    fideicomiso_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    afiliado_id = table.Column<int>(type: "integer", nullable: false),
                    cliente_id = table.Column<int>(type: "integer", nullable: false),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    objetivo_id = table.Column<int>(type: "integer", nullable: false),
                    portafolio_id = table.Column<int>(type: "integer", nullable: false),
                    saldo_total = table.Column<decimal>(type: "numeric", nullable: false),
                    unidades_totales = table.Column<int>(type: "integer", nullable: false),
                    capital = table.Column<decimal>(type: "numeric", nullable: false),
                    rendimiento = table.Column<decimal>(type: "numeric", nullable: false),
                    condicion_tributaria = table.Column<int>(type: "integer", nullable: false),
                    retencion_contingente = table.Column<decimal>(type: "numeric", nullable: false),
                    retencion_rendimiento = table.Column<decimal>(type: "numeric", nullable: false),
                    disponible = table.Column<decimal>(type: "numeric", nullable: false),
                    porcentaje_retencion_contingente = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fideicomisos", x => x.fideicomiso_id);
                });

            migrationBuilder.CreateTable(
                name: "historicos_fideicomisos",
                schema: "fideicomisos",
                columns: table => new
                {
                    historico_fideicomiso_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TrustId = table.Column<long>(type: "bigint", nullable: false),
                    rendimiento = table.Column<decimal>(type: "numeric", nullable: false),
                    fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    comercial_id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_historicos_fideicomisos", x => x.historico_fideicomiso_id);
                    table.ForeignKey(
                        name: "FK_historicos_fideicomisos_fideicomisos_TrustId",
                        column: x => x.TrustId,
                        principalSchema: "fideicomisos",
                        principalTable: "fideicomisos",
                        principalColumn: "fideicomiso_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_historicos_fideicomisos_TrustId",
                schema: "fideicomisos",
                table: "historicos_fideicomisos",
                column: "TrustId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "historicos_fideicomisos",
                schema: "fideicomisos");

            migrationBuilder.DropTable(
                name: "fideicomisos",
                schema: "fideicomisos");
        }
    }
}
