using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Closing.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AlterTrustYieldTrustIdColumnToLong : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "fideicomiso_id",
                schema: "cierre",
                table: "rendimientos_fideicomisos",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "fideicomiso_id",
                schema: "cierre",
                table: "rendimientos_fideicomisos",
                type: "integer",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");
        }
    }
}
