using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace People.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class CreatePeopleModuleTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_personas_ciuu_EconomicActivityId",
                schema: "personas",
                table: "personas");

            migrationBuilder.DropForeignKey(
                name: "FK_personas_paises_CountryId",
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

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "personas",
                table: "ciuu",
                newName: "codgrupo");

            migrationBuilder.RenameColumn(
                name: "EconomicActivityId",
                schema: "personas",
                table: "persona",
                newName: "actividad_economica_id");

            migrationBuilder.RenameColumn(
                name: "CountryId",
                schema: "personas",
                table: "persona",
                newName: "pais_id");

            migrationBuilder.RenameIndex(
                name: "IX_personas_EconomicActivityId",
                schema: "personas",
                table: "persona",
                newName: "IX_persona_actividad_economica_id");

            migrationBuilder.RenameIndex(
                name: "IX_personas_CountryId",
                schema: "personas",
                table: "persona",
                newName: "IX_persona_pais_id");

            migrationBuilder.AddColumn<bool>(
                name: "estado",
                schema: "personas",
                table: "persona",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_persona",
                schema: "personas",
                table: "persona",
                column: "id");

            migrationBuilder.CreateTable(
                name: "ciudad",
                schema: "personas",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ciudad", x => x.id);
                });

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_persona_ciuu_actividad_economica_id",
                schema: "personas",
                table: "persona");

            migrationBuilder.DropForeignKey(
                name: "FK_persona_paises_pais_id",
                schema: "personas",
                table: "persona");

            migrationBuilder.DropTable(
                name: "ciudad",
                schema: "personas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_persona",
                schema: "personas",
                table: "persona");

            migrationBuilder.DropColumn(
                name: "estado",
                schema: "personas",
                table: "persona");

            migrationBuilder.RenameTable(
                name: "persona",
                schema: "personas",
                newName: "personas",
                newSchema: "personas");

            migrationBuilder.RenameColumn(
                name: "codgrupo",
                schema: "personas",
                table: "ciuu",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "pais_id",
                schema: "personas",
                table: "personas",
                newName: "CountryId");

            migrationBuilder.RenameColumn(
                name: "actividad_economica_id",
                schema: "personas",
                table: "personas",
                newName: "EconomicActivityId");

            migrationBuilder.RenameIndex(
                name: "IX_persona_pais_id",
                schema: "personas",
                table: "personas",
                newName: "IX_personas_CountryId");

            migrationBuilder.RenameIndex(
                name: "IX_persona_actividad_economica_id",
                schema: "personas",
                table: "personas",
                newName: "IX_personas_EconomicActivityId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_personas",
                schema: "personas",
                table: "personas",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_personas_ciuu_EconomicActivityId",
                schema: "personas",
                table: "personas",
                column: "EconomicActivityId",
                principalSchema: "personas",
                principalTable: "ciuu",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_personas_paises_CountryId",
                schema: "personas",
                table: "personas",
                column: "CountryId",
                principalSchema: "personas",
                principalTable: "paises",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
