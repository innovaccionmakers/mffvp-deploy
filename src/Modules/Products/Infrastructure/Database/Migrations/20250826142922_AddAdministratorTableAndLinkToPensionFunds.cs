using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Products.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddAdministratorTableAndLinkToPensionFunds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "tipo_documento",
                schema: "productos",
                table: "fondos_voluntarios_pensiones",
                newName: "tipo_identificacion");

            migrationBuilder.AlterColumn<string>(
                name: "identificacion",
                schema: "productos",
                table: "fondos_voluntarios_pensiones",
                type: "character varying(25)",
                maxLength: 25,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "digito",
                schema: "productos",
                table: "fondos_voluntarios_pensiones",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "administradores",
                schema: "productos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    identificacion = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    tipo_identificacion = table.Column<int>(type: "integer", nullable: false),
                    digito = table.Column<int>(type: "integer", nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    estado = table.Column<string>(type: "text", nullable: false),
                    codigo_entidad = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    tipo_entidad = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_administradores", x => x.id);
                });

            migrationBuilder.Sql(@"
                INSERT INTO productos.administradores
                    (id, identificacion, tipo_identificacion, digito, nombre, estado, codigo_entidad, tipo_entidad)
                VALUES
                    (
                        1,
                        '800150280',
                        (SELECT id FROM productos.parametros_configuracion 
                         WHERE tipo = 'TipoDocumento' AND nombre = 'Nit de la Sociedad' LIMIT 1),
                        0,
                        'Fiduciaria Bancolombia',
                        'A',
                        '00531',
                        (SELECT id FROM productos.parametros_configuracion 
                         WHERE tipo = 'TipoEntidad' AND nombre = 'SOCIEDADES FIDUCIARIAS' LIMIT 1)
                    )
                ON CONFLICT DO NOTHING;
            ");

            migrationBuilder.AddColumn<int>(
                name: "administrador_id",
                schema: "productos",
                table: "fondos_voluntarios_pensiones",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE productos.fondos_voluntarios_pensiones
                SET administrador_id = 1
                WHERE administrador_id IS NULL;
            ");

            migrationBuilder.AlterColumn<int>(
                name: "administrador_id",
                schema: "productos",
                table: "fondos_voluntarios_pensiones",
                type: "integer",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_fondos_voluntarios_pensiones_administrador_id",
                schema: "productos",
                table: "fondos_voluntarios_pensiones",
                column: "administrador_id");

            migrationBuilder.AddForeignKey(
                name: "FK_fondos_voluntarios_pensiones_administradores_administrador_id",
                schema: "productos",
                table: "fondos_voluntarios_pensiones",
                column: "administrador_id",
                principalSchema: "productos",
                principalTable: "administradores",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_fondos_voluntarios_pensiones_administradores_administrador_id",
                schema: "productos",
                table: "fondos_voluntarios_pensiones");

            migrationBuilder.DropTable(
                name: "administradores",
                schema: "productos");

            migrationBuilder.DropIndex(
                name: "IX_fondos_voluntarios_pensiones_administrador_id",
                schema: "productos",
                table: "fondos_voluntarios_pensiones");

            migrationBuilder.DropColumn(
                name: "administrador_id",
                schema: "productos",
                table: "fondos_voluntarios_pensiones");

            migrationBuilder.DropColumn(
                name: "digito",
                schema: "productos",
                table: "fondos_voluntarios_pensiones");

            migrationBuilder.RenameColumn(
                name: "tipo_identificacion",
                schema: "productos",
                table: "fondos_voluntarios_pensiones",
                newName: "tipo_documento");

            migrationBuilder.AlterColumn<int>(
                name: "identificacion",
                schema: "productos",
                table: "fondos_voluntarios_pensiones",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(25)",
                oldMaxLength: 25);
        }
    }
}
