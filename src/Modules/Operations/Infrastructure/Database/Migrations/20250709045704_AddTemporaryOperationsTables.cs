using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Operations.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddTemporaryOperationsTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "operaciones_clientes_temporal",
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
                    subtipo_transaccion_id = table.Column<long>(type: "bigint", nullable: false),
                    fecha_aplicacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_operaciones_clientes_temporal", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "informacion_auxiliar_temporal",
                schema: "operaciones",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    operacion_cliente_temporal_id = table.Column<long>(type: "bigint", nullable: false),
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
                    usuario_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_informacion_auxiliar_temporal", x => x.id);
                    table.ForeignKey(
                        name: "FK_informacion_auxiliar_temporal_operaciones_clientes_temporal~",
                        column: x => x.operacion_cliente_temporal_id,
                        principalSchema: "operaciones",
                        principalTable: "operaciones_clientes_temporal",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_informacion_auxiliar_temporal_operacion_cliente_temporal_id",
                schema: "operaciones",
                table: "informacion_auxiliar_temporal",
                column: "operacion_cliente_temporal_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "informacion_auxiliar_temporal",
                schema: "operaciones");

            migrationBuilder.DropTable(
                name: "operaciones_clientes_temporal",
                schema: "operaciones");
        }
    }
}
