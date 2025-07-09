using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Treasury.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "tesoreria");

            migrationBuilder.CreateTable(
                name: "conceptos_tesoreria",
                schema: "tesoreria",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    concepto = table.Column<string>(type: "text", nullable: false),
                    naturaleza = table.Column<string>(type: "text", nullable: false),
                    admite_negativo = table.Column<bool>(type: "boolean", nullable: false),
                    permite_gasto = table.Column<bool>(type: "boolean", nullable: false),
                    cuenta_bancaria = table.Column<bool>(type: "boolean", nullable: false),
                    contraparte = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_proceso = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    observaciones = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_conceptos_tesoreria", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "emisor",
                schema: "tesoreria",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    emisor = table.Column<string>(type: "text", nullable: false),
                    descripcion = table.Column<string>(type: "text", nullable: false),
                    nit = table.Column<float>(type: "real", nullable: false),
                    digito = table.Column<int>(type: "integer", nullable: false),
                    codigo_homologado = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_emisor", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "parametros_configuracion",
                schema: "tesoreria",
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
                        principalSchema: "tesoreria",
                        principalTable: "parametros_configuracion",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "cuenta_bancaria",
                schema: "tesoreria",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    portafolio_id = table.Column<long>(type: "bigint", nullable: false),
                    emisor_id = table.Column<long>(type: "bigint", nullable: false),
                    numero_cuenta = table.Column<string>(type: "text", nullable: false),
                    tipo_cuenta = table.Column<string>(type: "text", nullable: false),
                    observaciones = table.Column<string>(type: "text", nullable: false),
                    fecha_proceso = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cuenta_bancaria", x => x.id);
                    table.ForeignKey(
                        name: "FK_cuenta_bancaria_emisor_emisor_id",
                        column: x => x.emisor_id,
                        principalSchema: "tesoreria",
                        principalTable: "emisor",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "movimientos_tesoreria",
                schema: "tesoreria",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    portafolio_id = table.Column<int>(type: "integer", nullable: false),
                    fecha_proceso = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fecha_cierre = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    concepto_tesoreria_id = table.Column<long>(type: "bigint", nullable: false),
                    valor = table.Column<decimal>(type: "numeric(19,2)", nullable: false),
                    cuenta_bancaria_id = table.Column<long>(type: "bigint", nullable: true),
                    entidad_id = table.Column<long>(type: "bigint", nullable: false),
                    contraparte_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_movimientos_tesoreria", x => x.id);
                    table.ForeignKey(
                        name: "FK_movimientos_tesoreria_conceptos_tesoreria_concepto_tesoreri~",
                        column: x => x.concepto_tesoreria_id,
                        principalSchema: "tesoreria",
                        principalTable: "conceptos_tesoreria",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_movimientos_tesoreria_cuenta_bancaria_cuenta_bancaria_id",
                        column: x => x.cuenta_bancaria_id,
                        principalSchema: "tesoreria",
                        principalTable: "cuenta_bancaria",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_movimientos_tesoreria_emisor_contraparte_id",
                        column: x => x.contraparte_id,
                        principalSchema: "tesoreria",
                        principalTable: "emisor",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_movimientos_tesoreria_emisor_entidad_id",
                        column: x => x.entidad_id,
                        principalSchema: "tesoreria",
                        principalTable: "emisor",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_cuenta_bancaria_emisor_id",
                schema: "tesoreria",
                table: "cuenta_bancaria",
                column: "emisor_id");

            migrationBuilder.CreateIndex(
                name: "IX_movimientos_tesoreria_concepto_tesoreria_id",
                schema: "tesoreria",
                table: "movimientos_tesoreria",
                column: "concepto_tesoreria_id");

            migrationBuilder.CreateIndex(
                name: "IX_movimientos_tesoreria_contraparte_id",
                schema: "tesoreria",
                table: "movimientos_tesoreria",
                column: "contraparte_id");

            migrationBuilder.CreateIndex(
                name: "IX_movimientos_tesoreria_cuenta_bancaria_id",
                schema: "tesoreria",
                table: "movimientos_tesoreria",
                column: "cuenta_bancaria_id");

            migrationBuilder.CreateIndex(
                name: "IX_movimientos_tesoreria_entidad_id",
                schema: "tesoreria",
                table: "movimientos_tesoreria",
                column: "entidad_id");

            migrationBuilder.CreateIndex(
                name: "IX_parametros_configuracion_padre_id",
                schema: "tesoreria",
                table: "parametros_configuracion",
                column: "padre_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "movimientos_tesoreria",
                schema: "tesoreria");

            migrationBuilder.DropTable(
                name: "parametros_configuracion",
                schema: "tesoreria");

            migrationBuilder.DropTable(
                name: "conceptos_tesoreria",
                schema: "tesoreria");

            migrationBuilder.DropTable(
                name: "cuenta_bancaria",
                schema: "tesoreria");

            migrationBuilder.DropTable(
                name: "emisor",
                schema: "tesoreria");
        }
    }
}
