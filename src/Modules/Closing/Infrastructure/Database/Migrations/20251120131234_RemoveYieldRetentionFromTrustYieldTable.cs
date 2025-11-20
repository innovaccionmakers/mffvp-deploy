using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Closing.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class RemoveYieldRetentionFromTrustYieldTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "retencion_rendimiento",
                schema: "cierre",
                table: "rendimientos_fideicomisos");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "retencion_rendimiento",
                schema: "cierre",
                table: "rendimientos_fideicomisos",
                type: "numeric(19,2)",
                precision: 19,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }
    }
}
