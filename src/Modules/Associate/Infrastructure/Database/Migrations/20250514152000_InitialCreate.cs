using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Associate.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "afiliado");

            migrationBuilder.CreateTable(
                name: "activacion_afiliado",
                schema: "afiliado",
                columns: table => new
                {
                    activación_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tipo_identificacion = table.Column<string>(type: "text", nullable: false),
                    identificacion = table.Column<string>(type: "text", nullable: false),
                    pendionado = table.Column<bool>(type: "boolean", nullable: false),
                    cumple_requisitos_pension = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_activación = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_activacion_afiliado", x => x.activación_id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "activacion_afiliado",
                schema: "afiliado");
        }
    }
}
