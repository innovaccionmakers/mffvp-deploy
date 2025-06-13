using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Operations.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class ChangeName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_informacion_auxiliar_Bank_BankId",
                schema: "operaciones",
                table: "informacion_auxiliar");

            migrationBuilder.DropForeignKey(
                name: "FK_OriginMode_origen_aportes_OriginId",
                schema: "operaciones",
                table: "OriginMode");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OriginMode",
                schema: "operaciones",
                table: "OriginMode");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Bank",
                schema: "operaciones",
                table: "Bank");

            migrationBuilder.RenameTable(
                name: "OriginMode",
                schema: "operaciones",
                newName: "origenaportes_modorigen",
                newSchema: "operaciones");

            migrationBuilder.RenameTable(
                name: "Bank",
                schema: "operaciones",
                newName: "bancos",
                newSchema: "operaciones");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "operaciones",
                table: "origenaportes_modorigen",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "OriginId",
                schema: "operaciones",
                table: "origenaportes_modorigen",
                newName: "origen_id");

            migrationBuilder.RenameColumn(
                name: "ModalityOriginId",
                schema: "operaciones",
                table: "origenaportes_modorigen",
                newName: "modalidad_origen_id");

            migrationBuilder.RenameIndex(
                name: "IX_OriginMode_OriginId",
                schema: "operaciones",
                table: "origenaportes_modorigen",
                newName: "IX_origenaportes_modorigen_origen_id");

            migrationBuilder.RenameColumn(
                name: "Nit",
                schema: "operaciones",
                table: "bancos",
                newName: "nit");

            migrationBuilder.RenameColumn(
                name: "Status",
                schema: "operaciones",
                table: "bancos",
                newName: "estado");

            migrationBuilder.RenameColumn(
                name: "Name",
                schema: "operaciones",
                table: "bancos",
                newName: "nombre");

            migrationBuilder.RenameColumn(
                name: "HomologatedCode",
                schema: "operaciones",
                table: "bancos",
                newName: "codigo_homologado");

            migrationBuilder.RenameColumn(
                name: "CompensationCode",
                schema: "operaciones",
                table: "bancos",
                newName: "codigo_compensacion");

            migrationBuilder.RenameColumn(
                name: "CheckClearingDays",
                schema: "operaciones",
                table: "bancos",
                newName: "dias_de_canje_cheques");

            migrationBuilder.RenameColumn(
                name: "BankId",
                schema: "operaciones",
                table: "bancos",
                newName: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_origenaportes_modorigen",
                schema: "operaciones",
                table: "origenaportes_modorigen",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_bancos",
                schema: "operaciones",
                table: "bancos",
                column: "id");

            migrationBuilder.CreateTable(
                name: "Banks",
                schema: "operaciones",
                columns: table => new
                {
                    BankId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Banks", x => x.BankId);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_informacion_auxiliar_bancos_BankId",
                schema: "operaciones",
                table: "informacion_auxiliar",
                column: "BankId",
                principalSchema: "operaciones",
                principalTable: "bancos",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_origenaportes_modorigen_origen_aportes_origen_id",
                schema: "operaciones",
                table: "origenaportes_modorigen",
                column: "origen_id",
                principalSchema: "operaciones",
                principalTable: "origen_aportes",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_informacion_auxiliar_bancos_BankId",
                schema: "operaciones",
                table: "informacion_auxiliar");

            migrationBuilder.DropForeignKey(
                name: "FK_origenaportes_modorigen_origen_aportes_origen_id",
                schema: "operaciones",
                table: "origenaportes_modorigen");

            migrationBuilder.DropTable(
                name: "Banks",
                schema: "operaciones");

            migrationBuilder.DropPrimaryKey(
                name: "PK_origenaportes_modorigen",
                schema: "operaciones",
                table: "origenaportes_modorigen");

            migrationBuilder.DropPrimaryKey(
                name: "PK_bancos",
                schema: "operaciones",
                table: "bancos");

            migrationBuilder.RenameTable(
                name: "origenaportes_modorigen",
                schema: "operaciones",
                newName: "OriginMode",
                newSchema: "operaciones");

            migrationBuilder.RenameTable(
                name: "bancos",
                schema: "operaciones",
                newName: "Bank",
                newSchema: "operaciones");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "operaciones",
                table: "OriginMode",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "origen_id",
                schema: "operaciones",
                table: "OriginMode",
                newName: "OriginId");

            migrationBuilder.RenameColumn(
                name: "modalidad_origen_id",
                schema: "operaciones",
                table: "OriginMode",
                newName: "ModalityOriginId");

            migrationBuilder.RenameIndex(
                name: "IX_origenaportes_modorigen_origen_id",
                schema: "operaciones",
                table: "OriginMode",
                newName: "IX_OriginMode_OriginId");

            migrationBuilder.RenameColumn(
                name: "nit",
                schema: "operaciones",
                table: "Bank",
                newName: "Nit");

            migrationBuilder.RenameColumn(
                name: "nombre",
                schema: "operaciones",
                table: "Bank",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "estado",
                schema: "operaciones",
                table: "Bank",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "dias_de_canje_cheques",
                schema: "operaciones",
                table: "Bank",
                newName: "CheckClearingDays");

            migrationBuilder.RenameColumn(
                name: "codigo_homologado",
                schema: "operaciones",
                table: "Bank",
                newName: "HomologatedCode");

            migrationBuilder.RenameColumn(
                name: "codigo_compensacion",
                schema: "operaciones",
                table: "Bank",
                newName: "CompensationCode");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "operaciones",
                table: "Bank",
                newName: "BankId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OriginMode",
                schema: "operaciones",
                table: "OriginMode",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Bank",
                schema: "operaciones",
                table: "Bank",
                column: "BankId");

            migrationBuilder.AddForeignKey(
                name: "FK_informacion_auxiliar_Bank_BankId",
                schema: "operaciones",
                table: "informacion_auxiliar",
                column: "BankId",
                principalSchema: "operaciones",
                principalTable: "Bank",
                principalColumn: "BankId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OriginMode_origen_aportes_OriginId",
                schema: "operaciones",
                table: "OriginMode",
                column: "OriginId",
                principalSchema: "operaciones",
                principalTable: "origen_aportes",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
