using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Operations.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "operaciones");

            migrationBuilder.CreateTable(
                name: "Bank",
                schema: "operaciones",
                columns: table => new
                {
                    BankId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nit = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CompensationCode = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    HomologatedCode = table.Column<string>(type: "text", nullable: false),
                    CheckClearingDays = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bank", x => x.BankId);
                });

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
                name: "parametros_configuracion",
                schema: "operaciones",
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
                        principalSchema: "operaciones",
                        principalTable: "parametros_configuracion",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "subtipo_transacciones",
                schema: "operaciones",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    categoria = table.Column<Guid>(type: "uuid", nullable: false),
                    naturaleza = table.Column<string>(type: "text", nullable: false),
                    estado = table.Column<string>(type: "text", nullable: false),
                    externo = table.Column<string>(type: "text", nullable: false),
                    codigo_homologado = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subtipo_transacciones", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "OriginMode",
                schema: "operaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OriginId = table.Column<int>(type: "integer", nullable: false),
                    ModalityOriginId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OriginMode", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OriginMode_origen_aportes_OriginId",
                        column: x => x.OriginId,
                        principalSchema: "operaciones",
                        principalTable: "origen_aportes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "operaciones_clientes",
                schema: "operaciones",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fecha_radicacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    afiliado_id = table.Column<int>(type: "integer", nullable: false),
                    objetivo_id = table.Column<int>(type: "integer", nullable: false),
                    portafolio_id = table.Column<int>(type: "integer", nullable: false),
                    valor = table.Column<decimal>(type: "numeric", nullable: false),
                    fecha_proceso = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    subtipo_transaccion_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_operaciones_clientes", x => x.id);
                    table.ForeignKey(
                        name: "FK_operaciones_clientes_subtipo_transacciones_subtipo_transacc~",
                        column: x => x.subtipo_transaccion_id,
                        principalSchema: "operaciones",
                        principalTable: "subtipo_transacciones",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "informacion_auxiliar",
                schema: "operaciones",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    operacion_cliente_id = table.Column<long>(type: "bigint", nullable: false),
                    origen_id = table.Column<int>(type: "integer", nullable: false),
                    metodo_recaudo_id = table.Column<int>(type: "integer", nullable: false),
                    forma_pago_id = table.Column<int>(type: "integer", nullable: false),
                    cuenta_recaudo = table.Column<int>(type: "integer", nullable: false),
                    detalle_forma_pago = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    estado_certificacion_id = table.Column<int>(type: "integer", nullable: false),
                    condicion_tributaria_id = table.Column<int>(type: "integer", nullable: false),
                    retencion_contingente = table.Column<int>(type: "integer", nullable: false),
                    medio_verificable = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    banco_recaudo = table.Column<int>(type: "integer", nullable: false),
                    fecha_consignacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    usuario_comercial = table.Column<string>(type: "text", nullable: false),
                    modalidad_origen_id = table.Column<int>(type: "integer", nullable: false),
                    ciudad_id = table.Column<int>(type: "integer", nullable: false),
                    canal_id = table.Column<int>(type: "integer", nullable: false),
                    usuario_id = table.Column<int>(type: "integer", nullable: false),
                    BankId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_informacion_auxiliar", x => x.id);
                    table.ForeignKey(
                        name: "FK_informacion_auxiliar_Bank_BankId",
                        column: x => x.BankId,
                        principalSchema: "operaciones",
                        principalTable: "Bank",
                        principalColumn: "BankId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_informacion_auxiliar_canales_canal_id",
                        column: x => x.canal_id,
                        principalSchema: "operaciones",
                        principalTable: "canales",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_informacion_auxiliar_operaciones_clientes_operacion_cliente~",
                        column: x => x.operacion_cliente_id,
                        principalSchema: "operaciones",
                        principalTable: "operaciones_clientes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_informacion_auxiliar_origen_aportes_origen_id",
                        column: x => x.origen_id,
                        principalSchema: "operaciones",
                        principalTable: "origen_aportes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
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

            migrationBuilder.CreateIndex(
                name: "IX_informacion_auxiliar_BankId",
                schema: "operaciones",
                table: "informacion_auxiliar",
                column: "BankId");

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
                name: "IX_operaciones_clientes_subtipo_transaccion_id",
                schema: "operaciones",
                table: "operaciones_clientes",
                column: "subtipo_transaccion_id");

            migrationBuilder.CreateIndex(
                name: "IX_operaciones_retiro_fideicomiso_operaciones_clientes_id",
                schema: "operaciones",
                table: "operaciones_retiro_fideicomiso",
                column: "operaciones_clientes_id");

            migrationBuilder.CreateIndex(
                name: "IX_OriginMode_OriginId",
                schema: "operaciones",
                table: "OriginMode",
                column: "OriginId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_parametros_configuracion_padre_id",
                schema: "operaciones",
                table: "parametros_configuracion",
                column: "padre_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "informacion_auxiliar",
                schema: "operaciones");

            migrationBuilder.DropTable(
                name: "operaciones_retiro_fideicomiso",
                schema: "operaciones");

            migrationBuilder.DropTable(
                name: "OriginMode",
                schema: "operaciones");

            migrationBuilder.DropTable(
                name: "parametros_configuracion",
                schema: "operaciones");

            migrationBuilder.DropTable(
                name: "Bank",
                schema: "operaciones");

            migrationBuilder.DropTable(
                name: "canales",
                schema: "operaciones");

            migrationBuilder.DropTable(
                name: "operaciones_clientes",
                schema: "operaciones");

            migrationBuilder.DropTable(
                name: "origen_aportes",
                schema: "operaciones");

            migrationBuilder.DropTable(
                name: "subtipo_transacciones",
                schema: "operaciones");
        }
    }
}
