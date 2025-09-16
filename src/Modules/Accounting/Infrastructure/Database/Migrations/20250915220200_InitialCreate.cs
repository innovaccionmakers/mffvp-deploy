using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Accounting.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "contabilidad");

            migrationBuilder.CreateTable(
                name: "auxiliar_contable",
                schema: "contabilidad",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    identificacion = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    digito_verificacion = table.Column<int>(type: "integer", nullable: true),
                    nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    periodo = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: true),
                    cuenta = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: true),
                    fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    nit = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: true),
                    detalle = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    tipo = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    valor = table.Column<decimal>(type: "numeric(19,2)", precision: 19, scale: 2, nullable: false, defaultValue: 0m),
                    naturaleza = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    identificador = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_auxiliar_contable", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "conceptos",
                schema: "contabilidad",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    portafolio_id = table.Column<int>(type: "integer", nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    cuenta_contra_credito = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    cuenta_contra_debito = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    cuenta_debito = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    cuenta_credito = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_conceptos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "consecutivos",
                schema: "contabilidad",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    naturaleza = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    documento_fuente = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    consecutivo = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_consecutivos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "parametros_configuracion",
                schema: "contabilidad",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    uuid = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    padre_id = table.Column<int>(type: "integer", nullable: true),
                    estado = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    tipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "categoria"),
                    editable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    sistema = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    metadata = table.Column<JsonDocument>(type: "jsonb", nullable: false, defaultValueSql: "'{}'"),
                    codigo_homologacion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_parametros_configuracion", x => x.id);
                    table.ForeignKey(
                        name: "FK_parametros_configuracion_parametros_configuracion_padre_id",
                        column: x => x.padre_id,
                        principalSchema: "contabilidad",
                        principalTable: "parametros_configuracion",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tesoreria",
                schema: "contabilidad",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    portafolio_id = table.Column<int>(type: "integer", nullable: false),
                    cuenta_bancaria = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    cuenta_debito = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    cuenta_credito = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tesoreria", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "transacciones_pasivas",
                schema: "contabilidad",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    portafolio_id = table.Column<int>(type: "integer", nullable: false),
                    tipo_operaciones_id = table.Column<long>(type: "bigint", nullable: false),
                    cuenta_debito = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    cuenta_credito = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    cuenta_contra_credito = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    cuenta_contra_debito = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transacciones_pasivas", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_parametros_configuracion_padre_id",
                schema: "contabilidad",
                table: "parametros_configuracion",
                column: "padre_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "auxiliar_contable",
                schema: "contabilidad");

            migrationBuilder.DropTable(
                name: "conceptos",
                schema: "contabilidad");

            migrationBuilder.DropTable(
                name: "consecutivos",
                schema: "contabilidad");

            migrationBuilder.DropTable(
                name: "parametros_configuracion",
                schema: "contabilidad");

            migrationBuilder.DropTable(
                name: "tesoreria",
                schema: "contabilidad");

            migrationBuilder.DropTable(
                name: "transacciones_pasivas",
                schema: "contabilidad");
        }
    }
}
