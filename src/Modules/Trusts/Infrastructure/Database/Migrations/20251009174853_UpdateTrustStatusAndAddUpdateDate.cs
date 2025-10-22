using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trusts.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTrustStatusAndAddUpdateDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Agregar una columna temporal para el nuevo estado
            migrationBuilder.AddColumn<int>(
                name: "estado_temp",
                schema: "fideicomisos",
                table: "fideicomisos",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            // Actualizar todos los registros existentes con el valor 1
            migrationBuilder.Sql(
                @"UPDATE fideicomisos.fideicomisos 
                  SET estado_temp = 1;");

            // Eliminar la columna antigua
            migrationBuilder.DropColumn(
                name: "estado",
                schema: "fideicomisos",
                table: "fideicomisos");

            // Renombrar la columna temporal al nombre original
            migrationBuilder.RenameColumn(
                name: "estado_temp",
                schema: "fideicomisos",
                table: "fideicomisos",
                newName: "estado");

            // Agregar la columna de fecha de actualización
            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_actualizacion",
                schema: "fideicomisos",
                table: "fideicomisos",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Eliminar la columna de fecha de actualización
            migrationBuilder.DropColumn(
                name: "fecha_actualizacion",
                schema: "fideicomisos",
                table: "fideicomisos");

            // Agregar columna temporal boolean
            migrationBuilder.AddColumn<bool>(
                name: "estado_temp",
                schema: "fideicomisos",
                table: "fideicomisos",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            // Convertir los valores integer a boolean, asumiendo que cualquier valor > 0 es true
            migrationBuilder.Sql(
                @"UPDATE fideicomisos.fideicomisos 
                  SET estado_temp = CASE 
                      WHEN estado > 0 THEN true 
                      ELSE false 
                  END;");

            // Eliminar la columna integer
            migrationBuilder.DropColumn(
                name: "estado",
                schema: "fideicomisos",
                table: "fideicomisos");

            // Renombrar la columna temporal
            migrationBuilder.RenameColumn(
                name: "estado_temp",
                schema: "fideicomisos",
                table: "fideicomisos",
                newName: "estado");
        }
    }
}