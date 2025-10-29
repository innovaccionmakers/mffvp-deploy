using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Closing.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class CreateTableYieldToDistribute : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "rendimientos_por_distribuir",
                schema: "cierre",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fideicomiso_id = table.Column<long>(type: "bigint", nullable: false),
                    portafolio_id = table.Column<int>(type: "integer", nullable: false),
                    fecha_cierre = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fecha_aplicacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    participacion = table.Column<decimal>(type: "numeric(38,16)", precision: 38, scale: 16, nullable: false),
                    rendimientos = table.Column<decimal>(type: "numeric(19,2)", precision: 19, scale: 2, nullable: false),
                    concepto = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    fecha_proceso = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rendimientos_por_distribuir", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "rendimientos_por_distribuir",
                schema: "cierre");
        }
    }
}
