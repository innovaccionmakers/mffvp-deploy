using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operations.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddCauseIdToClientOperation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "causal_id",
                schema: "operaciones",
                table: "operaciones_clientes_temporal",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "causal_id",
                schema: "operaciones",
                table: "operaciones_clientes",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "causal_id",
                schema: "operaciones",
                table: "operaciones_clientes_temporal");

            migrationBuilder.DropColumn(
                name: "causal_id",
                schema: "operaciones",
                table: "operaciones_clientes");
        }
    }
}
