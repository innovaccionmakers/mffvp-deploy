using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Operations.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceTrustWithdrawalWithTrustOperation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "operaciones_retiro_fideicomiso",
                schema: "operaciones");

            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_aplicacion",
                schema: "operaciones",
                table: "operaciones_clientes",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "operaciones_fideicomiso",
                schema: "operaciones",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    operaciones_clientes_id = table.Column<long>(type: "bigint", nullable: false),
                    fideicomiso_id = table.Column<long>(type: "bigint", nullable: false),
                    valor = table.Column<decimal>(type: "numeric", nullable: false),
                    subtipo_transaccion_id = table.Column<long>(type: "bigint", nullable: false),
                    portafolio_id = table.Column<int>(type: "integer", nullable: false),
                    fecha_radicacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fecha_proceso = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fecha_aplicacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_operaciones_fideicomiso", x => x.id);
                    table.ForeignKey(
                        name: "FK_operaciones_fideicomiso_operaciones_clientes_operaciones_cl~",
                        column: x => x.operaciones_clientes_id,
                        principalSchema: "operaciones",
                        principalTable: "operaciones_clientes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_operaciones_fideicomiso_operaciones_clientes_id",
                schema: "operaciones",
                table: "operaciones_fideicomiso",
                column: "operaciones_clientes_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "operaciones_fideicomiso",
                schema: "operaciones");

            migrationBuilder.DropColumn(
                name: "fecha_aplicacion",
                schema: "operaciones",
                table: "operaciones_clientes");

            migrationBuilder.CreateTable(
                name: "operaciones_retiro_fideicomiso",
                schema: "operaciones",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    operaciones_clientes_id = table.Column<long>(type: "bigint", nullable: false),
                    valor = table.Column<decimal>(type: "numeric", nullable: false),
                    fideicomiso_id = table.Column<long>(type: "bigint", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_operaciones_retiro_fideicomiso_operaciones_clientes_id",
                schema: "operaciones",
                table: "operaciones_retiro_fideicomiso",
                column: "operaciones_clientes_id");
        }
    }
}
