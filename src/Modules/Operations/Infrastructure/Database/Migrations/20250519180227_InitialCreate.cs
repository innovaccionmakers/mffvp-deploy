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
                    banco_recaudo = table.Column<string>(type: "text", nullable: false),
                    fecha_consignacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    usuario_comercial = table.Column<string>(type: "text", nullable: false),
                    ciudad = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_informacion_auxiliar", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "operaciones_clientes",
                schema: "operaciones",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    afiliado_id = table.Column<int>(type: "integer", nullable: false),
                    objetivo_id = table.Column<int>(type: "integer", nullable: false),
                    portafolio_id = table.Column<int>(type: "integer", nullable: false),
                    valor = table.Column<decimal>(type: "numeric", nullable: false),
                    subtipotransaccion_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_operaciones_clientes", x => x.id);
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
                name: "operaciones_clientes",
                schema: "operaciones");

            migrationBuilder.DropTable(
                name: "parametros_configuracion",
                schema: "operaciones");
        }
    }
}
