using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Products.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "productos");

            migrationBuilder.CreateTable(
                name: "bancos",
                schema: "productos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bancos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ciudades",
                schema: "productos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ciudades", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "comerciales",
                schema: "productos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    prefijo = table.Column<string>(type: "text", nullable: false),
                    estado = table.Column<string>(type: "text", nullable: false),
                    codigo_homologado = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_comerciales", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "fondos_voluntarios_pensiones",
                schema: "productos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tipo_documento = table.Column<int>(type: "integer", nullable: false),
                    identificacion = table.Column<int>(type: "integer", nullable: false),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    nombre_corto = table.Column<string>(type: "text", nullable: false),
                    estado = table.Column<int>(type: "integer", nullable: false),
                    codigo_homologado = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fondos_voluntarios_pensiones", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "oficinas",
                schema: "productos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    prefijo = table.Column<string>(type: "text", nullable: false),
                    ciudad_id = table.Column<int>(type: "integer", nullable: false),
                    estado = table.Column<int>(type: "integer", nullable: false),
                    codigo_homologado = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_oficinas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "parametros_configuracion",
                schema: "productos",
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
                        principalSchema: "productos",
                        principalTable: "parametros_configuracion",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "planes",
                schema: "productos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    descripcion = table.Column<string>(type: "text", nullable: false),
                    codigo_homologado = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_planes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "portafolios",
                schema: "productos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    nombre_corto = table.Column<string>(type: "text", nullable: false),
                    modalidad_id = table.Column<int>(type: "integer", nullable: false),
                    aporte_minimo_inicial = table.Column<decimal>(type: "numeric", nullable: false),
                    aporte_minimo_adicional = table.Column<decimal>(type: "numeric", nullable: false),
                    fecha_actual = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    codigo_homologacion = table.Column<string>(type: "text", nullable: false),
                    estado = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_portafolios", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "planes_fondo",
                schema: "productos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    plan_id = table.Column<int>(type: "integer", nullable: false),
                    fondo_id = table.Column<int>(type: "integer", nullable: false),
                    estado = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_planes_fondo", x => x.id);
                    table.ForeignKey(
                        name: "FK_planes_fondo_fondos_voluntarios_pensiones_fondo_id",
                        column: x => x.fondo_id,
                        principalSchema: "productos",
                        principalTable: "fondos_voluntarios_pensiones",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_planes_fondo_planes_plan_id",
                        column: x => x.plan_id,
                        principalSchema: "productos",
                        principalTable: "planes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "alternativas",
                schema: "productos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tipo_alternativa_id = table.Column<int>(type: "integer", nullable: false),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    descripcion = table.Column<string>(type: "text", nullable: false),
                    planes_fondo_id = table.Column<int>(type: "integer", nullable: false),
                    estado = table.Column<string>(type: "text", nullable: false),
                    codigo_homologado = table.Column<string>(name: "codigo_homologado)", type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_alternativas", x => x.id);
                    table.ForeignKey(
                        name: "FK_alternativas_planes_fondo_planes_fondo_id",
                        column: x => x.planes_fondo_id,
                        principalSchema: "productos",
                        principalTable: "planes_fondo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "alternativas_portafolios",
                schema: "productos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AlternativeId = table.Column<int>(type: "integer", nullable: false),
                    PortfolioId = table.Column<int>(type: "integer", nullable: false),
                    recaudador = table.Column<bool>(type: "boolean", nullable: false),
                    estado = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_alternativas_portafolios", x => x.id);
                    table.ForeignKey(
                        name: "FK_alternativas_portafolios_alternativas_AlternativeId",
                        column: x => x.AlternativeId,
                        principalSchema: "productos",
                        principalTable: "alternativas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_alternativas_portafolios_portafolios_PortfolioId",
                        column: x => x.PortfolioId,
                        principalSchema: "productos",
                        principalTable: "portafolios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "objetivos",
                schema: "productos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tipo_objetivo_id = table.Column<int>(type: "integer", nullable: false),
                    afiliado_id = table.Column<int>(type: "integer", nullable: false),
                    alternativa_id = table.Column<int>(type: "integer", nullable: false),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    comercial_id = table.Column<int>(type: "integer", nullable: false),
                    oficina_apertura_id = table.Column<int>(type: "integer", nullable: false),
                    oficina_actual_id = table.Column<int>(type: "integer", nullable: false),
                    saldo = table.Column<decimal>(type: "numeric(19,2)", precision: 19, scale: 2, nullable: false),
                    estado = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_objetivos", x => x.id);
                    table.ForeignKey(
                        name: "FK_objetivos_alternativas_alternativa_id",
                        column: x => x.alternativa_id,
                        principalSchema: "productos",
                        principalTable: "alternativas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_objetivos_comerciales_comercial_id",
                        column: x => x.comercial_id,
                        principalSchema: "productos",
                        principalTable: "comerciales",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_alternativas_planes_fondo_id",
                schema: "productos",
                table: "alternativas",
                column: "planes_fondo_id");

            migrationBuilder.CreateIndex(
                name: "IX_alternativas_portafolios_AlternativeId",
                schema: "productos",
                table: "alternativas_portafolios",
                column: "AlternativeId");

            migrationBuilder.CreateIndex(
                name: "IX_alternativas_portafolios_PortfolioId",
                schema: "productos",
                table: "alternativas_portafolios",
                column: "PortfolioId");

            migrationBuilder.CreateIndex(
                name: "IX_objetivos_alternativa_id",
                schema: "productos",
                table: "objetivos",
                column: "alternativa_id");

            migrationBuilder.CreateIndex(
                name: "IX_objetivos_comercial_id",
                schema: "productos",
                table: "objetivos",
                column: "comercial_id");

            migrationBuilder.CreateIndex(
                name: "IX_parametros_configuracion_padre_id",
                schema: "productos",
                table: "parametros_configuracion",
                column: "padre_id");

            migrationBuilder.CreateIndex(
                name: "IX_planes_fondo_fondo_id",
                schema: "productos",
                table: "planes_fondo",
                column: "fondo_id");

            migrationBuilder.CreateIndex(
                name: "IX_planes_fondo_plan_id",
                schema: "productos",
                table: "planes_fondo",
                column: "plan_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "alternativas_portafolios",
                schema: "productos");

            migrationBuilder.DropTable(
                name: "bancos",
                schema: "productos");

            migrationBuilder.DropTable(
                name: "ciudades",
                schema: "productos");

            migrationBuilder.DropTable(
                name: "objetivos",
                schema: "productos");

            migrationBuilder.DropTable(
                name: "oficinas",
                schema: "productos");

            migrationBuilder.DropTable(
                name: "parametros_configuracion",
                schema: "productos");

            migrationBuilder.DropTable(
                name: "portafolios",
                schema: "productos");

            migrationBuilder.DropTable(
                name: "alternativas",
                schema: "productos");

            migrationBuilder.DropTable(
                name: "comerciales",
                schema: "productos");

            migrationBuilder.DropTable(
                name: "planes_fondo",
                schema: "productos");

            migrationBuilder.DropTable(
                name: "fondos_voluntarios_pensiones",
                schema: "productos");

            migrationBuilder.DropTable(
                name: "planes",
                schema: "productos");
        }
    }
}
