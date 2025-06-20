using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operations.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddRelationToBankInOperation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_informacion_auxiliar_bancos_BankId",
                schema: "operaciones",
                table: "informacion_auxiliar");

            migrationBuilder.DropIndex(
                name: "IX_informacion_auxiliar_BankId",
                schema: "operaciones",
                table: "informacion_auxiliar");

            migrationBuilder.DropColumn(
                name: "BankId",
                schema: "operaciones",
                table: "informacion_auxiliar");

            migrationBuilder.CreateIndex(
                name: "IX_informacion_auxiliar_banco_recaudo",
                schema: "operaciones",
                table: "informacion_auxiliar",
                column: "banco_recaudo");

            migrationBuilder.AddForeignKey(
                name: "FK_informacion_auxiliar_bancos_banco_recaudo",
                schema: "operaciones",
                table: "informacion_auxiliar",
                column: "banco_recaudo",
                principalSchema: "operaciones",
                principalTable: "bancos",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_informacion_auxiliar_bancos_banco_recaudo",
                schema: "operaciones",
                table: "informacion_auxiliar");

            migrationBuilder.DropIndex(
                name: "IX_informacion_auxiliar_banco_recaudo",
                schema: "operaciones",
                table: "informacion_auxiliar");

            migrationBuilder.AddColumn<int>(
                name: "BankId",
                schema: "operaciones",
                table: "informacion_auxiliar",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_informacion_auxiliar_BankId",
                schema: "operaciones",
                table: "informacion_auxiliar",
                column: "BankId");

            migrationBuilder.AddForeignKey(
                name: "FK_informacion_auxiliar_bancos_BankId",
                schema: "operaciones",
                table: "informacion_auxiliar",
                column: "BankId",
                principalSchema: "operaciones",
                principalTable: "bancos",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
