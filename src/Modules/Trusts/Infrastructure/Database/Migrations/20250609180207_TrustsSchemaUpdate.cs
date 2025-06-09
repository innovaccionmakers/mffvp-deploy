using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Trusts.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class TrustsSchemaUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "historicos_fideicomisos",
                schema: "fideicomisos");

            migrationBuilder.DropColumn(
                name: "cliente_id",
                schema: "fideicomisos",
                table: "fideicomisos");

            migrationBuilder.DropColumn(
                name: "porcentaje_retencion_contingente",
                schema: "fideicomisos",
                table: "fideicomisos");

            migrationBuilder.AlterColumn<decimal>(
                name: "saldo_total",
                schema: "fideicomisos",
                table: "fideicomisos",
                type: "numeric(19,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "retencion_rendimiento",
                schema: "fideicomisos",
                table: "fideicomisos",
                type: "numeric(19,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "retencion_contingente",
                schema: "fideicomisos",
                table: "fideicomisos",
                type: "numeric(19,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "rendimiento",
                schema: "fideicomisos",
                table: "fideicomisos",
                type: "numeric(19,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "disponible",
                schema: "fideicomisos",
                table: "fideicomisos",
                type: "numeric(19,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "capital",
                schema: "fideicomisos",
                table: "fideicomisos",
                type: "numeric(19,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AddColumn<long>(
                name: "operaciones_cliente_id",
                schema: "fideicomisos",
                table: "fideicomisos",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "operaciones_cliente_id",
                schema: "fideicomisos",
                table: "fideicomisos");

            migrationBuilder.AlterColumn<decimal>(
                name: "saldo_total",
                schema: "fideicomisos",
                table: "fideicomisos",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(19,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "retencion_rendimiento",
                schema: "fideicomisos",
                table: "fideicomisos",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(19,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "retencion_contingente",
                schema: "fideicomisos",
                table: "fideicomisos",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(19,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "rendimiento",
                schema: "fideicomisos",
                table: "fideicomisos",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(19,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "disponible",
                schema: "fideicomisos",
                table: "fideicomisos",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(19,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "capital",
                schema: "fideicomisos",
                table: "fideicomisos",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(19,2)");

            migrationBuilder.AddColumn<int>(
                name: "cliente_id",
                schema: "fideicomisos",
                table: "fideicomisos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "porcentaje_retencion_contingente",
                schema: "fideicomisos",
                table: "fideicomisos",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "historicos_fideicomisos",
                schema: "fideicomisos",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    rendimiento = table.Column<decimal>(type: "numeric", nullable: false),
                    comercial_id = table.Column<string>(type: "text", nullable: false),
                    TrustId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_historicos_fideicomisos", x => x.id);
                    table.ForeignKey(
                        name: "FK_historicos_fideicomisos_fideicomisos_TrustId",
                        column: x => x.TrustId,
                        principalSchema: "fideicomisos",
                        principalTable: "fideicomisos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_historicos_fideicomisos_TrustId",
                schema: "fideicomisos",
                table: "historicos_fideicomisos",
                column: "TrustId");
        }
    }
}
