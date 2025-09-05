using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operations.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AlterTableAuxiliaryInformation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE operaciones.informacion_auxiliar 
                ALTER COLUMN cuenta_recaudo TYPE varchar 
                USING cuenta_recaudo::varchar;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE operaciones.informacion_auxiliar 
                ALTER COLUMN cuenta_recaudo TYPE integer 
                USING cuenta_recaudo::integer;
            ");
        }
    }
}
