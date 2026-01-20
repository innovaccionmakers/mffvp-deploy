using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Products.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddAgileWithdrawalFieldsToBalances : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE productos.portafolios ALTER COLUMN permite_retiro_agil DROP DEFAULT;
                ALTER TABLE productos.portafolios ALTER COLUMN permite_retiro_agil TYPE boolean USING false;
                ALTER TABLE productos.portafolios ALTER COLUMN permite_retiro_agil SET DEFAULT false;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE productos.portafolios ALTER COLUMN permite_retiro_agil DROP DEFAULT;
                ALTER TABLE productos.portafolios ALTER COLUMN permite_retiro_agil TYPE text USING permite_retiro_agil::text;
                ALTER TABLE productos.portafolios ALTER COLUMN permite_retiro_agil SET DEFAULT '';
            ");
        }
    }
}
