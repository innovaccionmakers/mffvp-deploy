using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Trusts.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddConfigurationParameters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "parametros_configuracion",
                schema: "fideicomisos",
                columns: table => new
                {
                    parametro_configuracion_id = table.Column<int>(type: "integer", nullable: false)
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
                    table.PrimaryKey("PK_parametros_configuracion", x => x.parametro_configuracion_id);
                    table.ForeignKey(
                        name: "FK_parametros_configuracion_parametros_configuracion_padre_id",
                        column: x => x.padre_id,
                        principalSchema: "fideicomisos",
                        principalTable: "parametros_configuracion",
                        principalColumn: "parametro_configuracion_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_parametros_configuracion_padre_id",
                schema: "fideicomisos",
                table: "parametros_configuracion",
                column: "padre_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "parametros_configuracion",
                schema: "fideicomisos");
        }
    }
}
