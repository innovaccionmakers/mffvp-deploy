using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Products.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddSfcEntityCodeToAdministrators : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "codigo_entidad_sfc",
                schema: "productos",
                table: "administradores",
                type: "character varying(6)",
                maxLength: 6,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "codigo_entidad_sfc",
                schema: "productos",
                table: "administradores");
        }
    }
}
