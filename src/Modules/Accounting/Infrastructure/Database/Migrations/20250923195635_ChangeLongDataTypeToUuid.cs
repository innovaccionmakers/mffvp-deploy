using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Accounting.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class ChangeLongDataTypeToUuid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {            
            migrationBuilder.DropColumn(
                name: "identificador",
                schema: "contabilidad",
                table: "auxiliar_contable");
            
            migrationBuilder.AddColumn<Guid>(
                name: "identificador",
                schema: "contabilidad",
                table: "auxiliar_contable",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "identificador",
                schema: "contabilidad",
                table: "auxiliar_contable");
            
            migrationBuilder.AddColumn<long>(
                name: "identificador",
                schema: "contabilidad",
                table: "auxiliar_contable",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }
    }
}
