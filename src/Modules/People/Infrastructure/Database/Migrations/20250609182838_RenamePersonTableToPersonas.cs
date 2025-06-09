using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace People.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class RenamePersonTableToPersonas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_persona_ciuu_actividad_economica_id",
                schema: "personas",
                table: "persona");

            migrationBuilder.DropForeignKey(
                name: "FK_persona_paises_pais_id",
                schema: "personas",
                table: "persona");

            migrationBuilder.DropPrimaryKey(
                name: "PK_persona",
                schema: "personas",
                table: "persona");

            migrationBuilder.RenameTable(
                name: "persona",
                schema: "personas",
                newName: "personas",
                newSchema: "personas");

            migrationBuilder.RenameIndex(
                name: "IX_persona_pais_id",
                schema: "personas",
                table: "personas",
                newName: "IX_personas_pais_id");

            migrationBuilder.RenameIndex(
                name: "IX_persona_actividad_economica_id",
                schema: "personas",
                table: "personas",
                newName: "IX_personas_actividad_economica_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_personas",
                schema: "personas",
                table: "personas",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_personas_ciuu_actividad_economica_id",
                schema: "personas",
                table: "personas",
                column: "actividad_economica_id",
                principalSchema: "personas",
                principalTable: "ciuu",
                principalColumn: "codgrupo",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_personas_paises_pais_id",
                schema: "personas",
                table: "personas",
                column: "pais_id",
                principalSchema: "personas",
                principalTable: "paises",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_personas_ciuu_actividad_economica_id",
                schema: "personas",
                table: "personas");

            migrationBuilder.DropForeignKey(
                name: "FK_personas_paises_pais_id",
                schema: "personas",
                table: "personas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_personas",
                schema: "personas",
                table: "personas");

            migrationBuilder.RenameTable(
                name: "personas",
                schema: "personas",
                newName: "persona",
                newSchema: "personas");

            migrationBuilder.RenameIndex(
                name: "IX_personas_pais_id",
                schema: "personas",
                table: "persona",
                newName: "IX_persona_pais_id");

            migrationBuilder.RenameIndex(
                name: "IX_personas_actividad_economica_id",
                schema: "personas",
                table: "persona",
                newName: "IX_persona_actividad_economica_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_persona",
                schema: "personas",
                table: "persona",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_persona_ciuu_actividad_economica_id",
                schema: "personas",
                table: "persona",
                column: "actividad_economica_id",
                principalSchema: "personas",
                principalTable: "ciuu",
                principalColumn: "codgrupo",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_persona_paises_pais_id",
                schema: "personas",
                table: "persona",
                column: "pais_id",
                principalSchema: "personas",
                principalTable: "paises",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
