using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Accounting.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddNewTablesConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "configuraciones_generales",
                schema: "contabilidad",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    portafolio_Id = table.Column<int>(type: "integer", nullable: false),
                    codigo_contable = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    centro_costos = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_configuraciones_generales", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "consecutivos_archivo",
                schema: "contabilidad",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fecha_generacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    consecutivo = table.Column<int>(type: "integer", nullable: false),
                    fecha_actual = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_consecutivos_archivo", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "configuraciones_generales",
                schema: "contabilidad");

            migrationBuilder.DropTable(
                name: "consecutivos_archivo",
                schema: "contabilidad");
        }
    }
}
