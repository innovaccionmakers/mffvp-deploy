using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Treasury.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AlterTableEmisorAddNewColumnBank : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "banco",
                schema: "tesoreria",
                table: "emisor",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "banco",
                schema: "tesoreria",
                table: "emisor");
        }
    }
}
