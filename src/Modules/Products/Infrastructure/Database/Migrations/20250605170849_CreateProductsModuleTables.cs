using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Products.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class CreateProductsModuleTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "monto_minimo_inicial",
                schema: "productos",
                table: "portafolios",
                newName: "aporte_minimo_inicial");

            migrationBuilder.AlterColumn<int>(
                name: "id",
                schema: "productos",
                table: "portafolios",
                type: "integer",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<decimal>(
                name: "aporte_minimo_adicional",
                schema: "productos",
                table: "portafolios",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_actual",
                schema: "productos",
                table: "portafolios",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "porcentaje_comision",
                schema: "productos",
                table: "portafolios",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "tipo_tasa_comision",
                schema: "productos",
                table: "portafolios",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "id",
                schema: "productos",
                table: "planes",
                type: "integer",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "id",
                schema: "productos",
                table: "objetivos",
                type: "integer",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "comercial_id",
                schema: "productos",
                table: "objetivos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "oficina_actual_id",
                schema: "productos",
                table: "objetivos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "oficina_apertura_id",
                schema: "productos",
                table: "objetivos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "saldo",
                schema: "productos",
                table: "objetivos",
                type: "numeric(19,2)",
                precision: 19,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<int>(
                name: "id",
                schema: "productos",
                table: "alternativas",
                type: "integer",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "codigo_homologado)",
                schema: "productos",
                table: "alternativas",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "planes_fondo_id",
                schema: "productos",
                table: "alternativas",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "alternativas_portafolios",
                schema: "productos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AlternativeId = table.Column<int>(type: "integer", nullable: false),
                    PortfolioId = table.Column<int>(type: "integer", nullable: false),
                    estado = table.Column<string>(type: "text", nullable: false),
                    recaudador = table.Column<bool>(type: "boolean", nullable: false)
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
                    estado = table.Column<string>(type: "text", nullable: false),
                    prefijo = table.Column<string>(type: "text", nullable: false),
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
                    tipo_identificacion = table.Column<int>(type: "integer", nullable: false),
                    identificacion = table.Column<int>(type: "integer", nullable: false),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    nombre_corto = table.Column<string>(type: "text", nullable: false),
                    estado = table.Column<string>(type: "text", nullable: false),
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
                    estado = table.Column<string>(type: "text", nullable: false),
                    prefijo = table.Column<string>(type: "text", nullable: false),
                    codigo_homologado = table.Column<string>(type: "text", nullable: false),
                    ciudad_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_oficinas", x => x.id);
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
                    estado = table.Column<string>(type: "text", nullable: false)
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
                name: "IX_planes_fondo_fondo_id",
                schema: "productos",
                table: "planes_fondo",
                column: "fondo_id");

            migrationBuilder.CreateIndex(
                name: "IX_planes_fondo_plan_id",
                schema: "productos",
                table: "planes_fondo",
                column: "plan_id");

            migrationBuilder.AddForeignKey(
                name: "FK_alternativas_planes_fondo_planes_fondo_id",
                schema: "productos",
                table: "alternativas",
                column: "planes_fondo_id",
                principalSchema: "productos",
                principalTable: "planes_fondo",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_objetivos_alternativas_alternativa_id",
                schema: "productos",
                table: "objetivos",
                column: "alternativa_id",
                principalSchema: "productos",
                principalTable: "alternativas",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_objetivos_comerciales_comercial_id",
                schema: "productos",
                table: "objetivos",
                column: "comercial_id",
                principalSchema: "productos",
                principalTable: "comerciales",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_alternativas_planes_fondo_planes_fondo_id",
                schema: "productos",
                table: "alternativas");

            migrationBuilder.DropForeignKey(
                name: "FK_objetivos_alternativas_alternativa_id",
                schema: "productos",
                table: "objetivos");

            migrationBuilder.DropForeignKey(
                name: "FK_objetivos_comerciales_comercial_id",
                schema: "productos",
                table: "objetivos");

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
                name: "comerciales",
                schema: "productos");

            migrationBuilder.DropTable(
                name: "oficinas",
                schema: "productos");

            migrationBuilder.DropTable(
                name: "planes_fondo",
                schema: "productos");

            migrationBuilder.DropTable(
                name: "fondos_voluntarios_pensiones",
                schema: "productos");

            migrationBuilder.DropIndex(
                name: "IX_objetivos_alternativa_id",
                schema: "productos",
                table: "objetivos");

            migrationBuilder.DropIndex(
                name: "IX_objetivos_comercial_id",
                schema: "productos",
                table: "objetivos");

            migrationBuilder.DropIndex(
                name: "IX_alternativas_planes_fondo_id",
                schema: "productos",
                table: "alternativas");

            migrationBuilder.DropColumn(
                name: "aporte_minimo_adicional",
                schema: "productos",
                table: "portafolios");

            migrationBuilder.DropColumn(
                name: "fecha_actual",
                schema: "productos",
                table: "portafolios");

            migrationBuilder.DropColumn(
                name: "porcentaje_comision",
                schema: "productos",
                table: "portafolios");

            migrationBuilder.DropColumn(
                name: "tipo_tasa_comision",
                schema: "productos",
                table: "portafolios");

            migrationBuilder.DropColumn(
                name: "comercial_id",
                schema: "productos",
                table: "objetivos");

            migrationBuilder.DropColumn(
                name: "oficina_actual_id",
                schema: "productos",
                table: "objetivos");

            migrationBuilder.DropColumn(
                name: "oficina_apertura_id",
                schema: "productos",
                table: "objetivos");

            migrationBuilder.DropColumn(
                name: "saldo",
                schema: "productos",
                table: "objetivos");

            migrationBuilder.DropColumn(
                name: "codigo_homologado)",
                schema: "productos",
                table: "alternativas");

            migrationBuilder.DropColumn(
                name: "planes_fondo_id",
                schema: "productos",
                table: "alternativas");

            migrationBuilder.RenameColumn(
                name: "aporte_minimo_inicial",
                schema: "productos",
                table: "portafolios",
                newName: "monto_minimo_inicial");

            migrationBuilder.AlterColumn<long>(
                name: "id",
                schema: "productos",
                table: "portafolios",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "id",
                schema: "productos",
                table: "planes",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "id",
                schema: "productos",
                table: "objetivos",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "id",
                schema: "productos",
                table: "alternativas",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        }
    }
}
