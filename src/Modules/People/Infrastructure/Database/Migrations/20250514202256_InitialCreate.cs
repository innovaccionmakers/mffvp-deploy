using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace People.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "personas");

            migrationBuilder.CreateTable(
                name: "ciuu",
                schema: "personas",
                columns: table => new
                {
                    codgrupo_id = table.Column<string>(type: "text", nullable: false),
                    descripcion = table.Column<string>(type: "text", nullable: false),
                    codigo_ciiu = table.Column<string>(type: "text", nullable: false),
                    codigo_division = table.Column<string>(type: "text", nullable: false),
                    nombre_division = table.Column<string>(type: "text", nullable: false),
                    nombre_grupo = table.Column<string>(type: "text", nullable: false),
                    codigo_clase = table.Column<string>(type: "text", nullable: false),
                    codigo_homologado = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ciuu", x => x.codgrupo_id);
                });

            migrationBuilder.CreateTable(
                name: "paises",
                schema: "personas",
                columns: table => new
                {
                    pais_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    nombre_abreviado = table.Column<string>(type: "text", nullable: false),
                    codigo_dane = table.Column<string>(type: "text", nullable: false),
                    codigo_homologado = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_paises", x => x.pais_id);
                });

            migrationBuilder.CreateTable(
                name: "personas",
                schema: "personas",
                columns: table => new
                {
                    persona_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tipo_documento = table.Column<string>(type: "text", nullable: false),
                    codigo_homologado = table.Column<string>(type: "text", nullable: false),
                    identificacion = table.Column<string>(type: "text", nullable: false),
                    primer_nombre = table.Column<string>(type: "text", nullable: false),
                    segundo_nombre = table.Column<string>(type: "text", nullable: false),
                    primer_apellido = table.Column<string>(type: "text", nullable: false),
                    segundo_apellido = table.Column<string>(type: "text", nullable: false),
                    fecha_expedicion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ciudad_expedicion_id = table.Column<int>(type: "integer", nullable: false),
                    fecha_nacimiento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ciudad_nacimiento_id = table.Column<int>(type: "integer", nullable: false),
                    celular = table.Column<string>(type: "text", nullable: false),
                    nombre_completo = table.Column<string>(type: "text", nullable: false),
                    estado_civil_id = table.Column<int>(type: "integer", nullable: false),
                    sexo_id = table.Column<int>(type: "integer", nullable: false),
                    CountryId = table.Column<int>(type: "integer", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    EconomicActivityId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_personas", x => x.persona_id);
                    table.ForeignKey(
                        name: "FK_personas_ciuu_EconomicActivityId",
                        column: x => x.EconomicActivityId,
                        principalSchema: "personas",
                        principalTable: "ciuu",
                        principalColumn: "codgrupo_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_personas_paises_CountryId",
                        column: x => x.CountryId,
                        principalSchema: "personas",
                        principalTable: "paises",
                        principalColumn: "pais_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_personas_CountryId",
                schema: "personas",
                table: "personas",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_personas_EconomicActivityId",
                schema: "personas",
                table: "personas",
                column: "EconomicActivityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "personas",
                schema: "personas");

            migrationBuilder.DropTable(
                name: "ciuu",
                schema: "personas");

            migrationBuilder.DropTable(
                name: "paises",
                schema: "personas");
        }
    }
}
