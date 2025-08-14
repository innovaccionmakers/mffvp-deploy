using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Operations.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBankEntityFromOperationsModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_informacion_auxiliar_bancos_banco_recaudo",
                schema: "operaciones",
                table: "informacion_auxiliar");

            migrationBuilder.DropTable(
                name: "bancos",
                schema: "operaciones");

            migrationBuilder.DropIndex(
                name: "IX_informacion_auxiliar_banco_recaudo",
                schema: "operaciones",
                table: "informacion_auxiliar");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "bancos",
                schema: "operaciones",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    dias_de_canje_cheques = table.Column<int>(type: "integer", nullable: false),
                    codigo_compensacion = table.Column<int>(type: "integer", nullable: false),
                    codigo_homologado = table.Column<string>(type: "text", nullable: false),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    nit = table.Column<string>(type: "text", nullable: false),
                    estado = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bancos", x => x.id);
                });

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
    }
}
