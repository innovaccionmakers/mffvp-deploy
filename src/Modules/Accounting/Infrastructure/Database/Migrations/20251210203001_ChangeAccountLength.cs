using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Accounting.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class ChangeAccountLength : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "identificador",
                schema: "contabilidad",
                table: "auxiliar_contable",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "UUID");

            migrationBuilder.AlterColumn<string>(
                name: "cuenta",
                schema: "contabilidad",
                table: "auxiliar_contable",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(6)",
                oldMaxLength: 6,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "identificador",
                schema: "contabilidad",
                table: "auxiliar_contable",
                type: "UUID",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "cuenta",
                schema: "contabilidad",
                table: "auxiliar_contable",
                type: "character varying(6)",
                maxLength: 6,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);
        }
    }
}
