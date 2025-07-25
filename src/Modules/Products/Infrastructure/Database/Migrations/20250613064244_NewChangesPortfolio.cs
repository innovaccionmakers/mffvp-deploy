﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Products.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class NewChangesPortfolio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "porcentaje_comision",
                schema: "productos",
                table: "portafolios",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "tipo_tasa_comision",
                schema: "productos",
                table: "portafolios",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "porcentaje_comision",
                schema: "productos",
                table: "portafolios");

            migrationBuilder.DropColumn(
                name: "tipo_tasa_comision",
                schema: "productos",
                table: "portafolios");
        }
    }
}
