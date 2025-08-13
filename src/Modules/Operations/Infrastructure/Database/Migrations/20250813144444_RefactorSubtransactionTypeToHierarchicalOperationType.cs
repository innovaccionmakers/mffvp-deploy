using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Operations.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class RefactorSubtransactionTypeToHierarchicalOperationType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Quitar el FK antiguo (clientes -> subtipo_transacciones)
            migrationBuilder.DropForeignKey(
                name: "FK_operaciones_clientes_subtipo_transacciones_subtipo_transacc~",
                schema: "operaciones",
                table: "operaciones_clientes");

            // 2) Renombrar columnas en tablas consumidoras
            migrationBuilder.RenameColumn(
                name: "subtipo_transaccion_id",
                schema: "operaciones",
                table: "operaciones_fideicomiso",
                newName: "tipo_operaciones_id");

            migrationBuilder.RenameColumn(
                name: "subtipo_transaccion_id",
                schema: "operaciones",
                table: "operaciones_clientes_temporal",
                newName: "tipo_operaciones_id");

            migrationBuilder.RenameColumn(
                name: "subtipo_transaccion_id",
                schema: "operaciones",
                table: "operaciones_clientes",
                newName: "tipo_operaciones_id");

            migrationBuilder.RenameIndex(
                name: "IX_operaciones_clientes_subtipo_transaccion_id",
                schema: "operaciones",
                table: "operaciones_clientes",
                newName: "IX_operaciones_clientes_tipo_operaciones_id");

            // 3) Crear la nueva tabla
            migrationBuilder.CreateTable(
                name: "tipos_operaciones",
                schema: "operaciones",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    categoria = table.Column<int>(type: "integer", nullable: true), // ID del "padre" (tipo). NULL para tipos raíz
                    naturaleza = table.Column<string>(type: "text", nullable: false),
                    estado = table.Column<string>(type: "text", nullable: false),
                    externo = table.Column<string>(type: "text", nullable: false),
                    visible = table.Column<bool>(type: "boolean", nullable: false),
                    atributos_adicionales = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    codigo_homologado = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tipos_operaciones", x => x.id);
                });

            // 4) Insertar TIPOS (padres) - sin especificar id
            migrationBuilder.Sql(@"
                INSERT INTO operaciones.tipos_operaciones
                    (nombre, categoria, naturaleza, estado, externo, visible, atributos_adicionales, codigo_homologado)
                VALUES
                    ('Aporte',       NULL, 'I', 'A', 'false', TRUE,  '{}'::jsonb, 'AP'),
                    ('Rendimientos', NULL, 'I', 'A', 'false', FALSE, '{}'::jsonb, 'U'),
                    ('Retiro Ágil',  NULL, 'E', 'A', 'false', FALSE, '{}'::jsonb, 'RE');
            ");

            // 5) Insertar SUBTIPOS (hijos) - categoria = id del tipo 'Aporte' (subconsulta)
            migrationBuilder.Sql(@"
                INSERT INTO operaciones.tipos_operaciones
                    (nombre, categoria, naturaleza, estado, externo, visible, atributos_adicionales, codigo_homologado)
                VALUES
                    ('Ninguno',           (SELECT id FROM operaciones.tipos_operaciones WHERE nombre = 'Aporte' LIMIT 1), 'I', 'A', 'false', TRUE,  '{}'::jsonb, 'N'),
                    ('Descuento nomina',  (SELECT id FROM operaciones.tipos_operaciones WHERE nombre = 'Aporte' LIMIT 1), 'I', 'A', 'false', FALSE, '{}'::jsonb, 'DN'),
                    ('Debito Automatico', (SELECT id FROM operaciones.tipos_operaciones WHERE nombre = 'Aporte' LIMIT 1), 'I', 'A', 'false', FALSE, '{}'::jsonb, 'DA');
            ");

            // 6) Remapear referencias en tablas consumidoras:
            //    Usamos la tabla vieja (subtipo_transacciones) para tomar su codigo_homologado
            //    y buscar el nuevo id en tipos_operaciones, antes de borrar la tabla vieja.

            // operaciones_clientes
            migrationBuilder.Sql(@"
                UPDATE operaciones.operaciones_clientes oc
                SET tipo_operaciones_id = toper.id
                FROM operaciones.subtipo_transacciones st
                JOIN operaciones.tipos_operaciones toper
                  ON toper.codigo_homologado = st.codigo_homologado
                WHERE oc.tipo_operaciones_id = st.id;
            ");

            // operaciones_clientes_temporal
            migrationBuilder.Sql(@"
                UPDATE operaciones.operaciones_clientes_temporal oct
                SET tipo_operaciones_id = toper.id
                FROM operaciones.subtipo_transacciones st
                JOIN operaciones.tipos_operaciones toper
                  ON toper.codigo_homologado = st.codigo_homologado
                WHERE oct.tipo_operaciones_id = st.id;
            ");

            // operaciones_fideicomiso
            migrationBuilder.Sql(@"
                UPDATE operaciones.operaciones_fideicomiso ofi
                SET tipo_operaciones_id = toper.id
                FROM operaciones.subtipo_transacciones st
                JOIN operaciones.tipos_operaciones toper
                  ON toper.codigo_homologado = st.codigo_homologado
                WHERE ofi.tipo_operaciones_id = st.id;
            ");

            // 7) Ahora sí, eliminar la tabla vieja (ya nadie la referencia)
            migrationBuilder.DropTable(
                name: "subtipo_transacciones",
                schema: "operaciones");

            // 8) Crear el nuevo FK (clientes -> tipos_operaciones)
            migrationBuilder.AddForeignKey(
                name: "FK_operaciones_clientes_tipos_operaciones_tipo_operaciones_id",
                schema: "operaciones",
                table: "operaciones_clientes",
                column: "tipo_operaciones_id",
                principalSchema: "operaciones",
                principalTable: "tipos_operaciones",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 1) Quitar FK nuevo
            migrationBuilder.DropForeignKey(
                name: "FK_operaciones_clientes_tipos_operaciones_tipo_operaciones_id",
                schema: "operaciones",
                table: "operaciones_clientes");

            // 2) Eliminar nueva tabla
            migrationBuilder.DropTable(
                name: "tipos_operaciones",
                schema: "operaciones");

            // 3) Renombrar columnas de vuelta
            migrationBuilder.RenameColumn(
                name: "tipo_operaciones_id",
                schema: "operaciones",
                table: "operaciones_fideicomiso",
                newName: "subtipo_transaccion_id");

            migrationBuilder.RenameColumn(
                name: "tipo_operaciones_id",
                schema: "operaciones",
                table: "operaciones_clientes_temporal",
                newName: "subtipo_transaccion_id");

            migrationBuilder.RenameColumn(
                name: "tipo_operaciones_id",
                schema: "operaciones",
                table: "operaciones_clientes",
                newName: "subtipo_transaccion_id");

            migrationBuilder.RenameIndex(
                name: "IX_operaciones_clientes_tipo_operaciones_id",
                schema: "operaciones",
                table: "operaciones_clientes",
                newName: "IX_operaciones_clientes_subtipo_transaccion_id");

            // 4) Recrear la tabla antigua (vacía)
            migrationBuilder.CreateTable(
                name: "subtipo_transacciones",
                schema: "operaciones",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    atributos_adicionales = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    categoria = table.Column<Guid>(type: "uuid", nullable: true),
                    externo = table.Column<string>(type: "text", nullable: false),
                    codigo_homologado = table.Column<string>(type: "text", nullable: false),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    naturaleza = table.Column<string>(type: "text", nullable: false),
                    estado = table.Column<string>(type: "text", nullable: false),
                    visible = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subtipo_transacciones", x => x.id);
                });

            // 5) Para evitar errores de FK al recrearlo (tabla vacía),
            //    ponemos a NULL las referencias (rollback seguro).
            migrationBuilder.Sql(@"UPDATE operaciones.operaciones_clientes SET subtipo_transaccion_id = NULL;");
            migrationBuilder.Sql(@"UPDATE operaciones.operaciones_clientes_temporal SET subtipo_transaccion_id = NULL;");
            migrationBuilder.Sql(@"UPDATE operaciones.operaciones_fideicomiso SET subtipo_transaccion_id = NULL;");

            // 6) Volver a crear el FK antiguo (clientes -> subtipo_transacciones)
            migrationBuilder.AddForeignKey(
                name: "FK_operaciones_clientes_subtipo_transacciones_subtipo_transacc~",
                schema: "operaciones",
                table: "operaciones_clientes",
                column: "subtipo_transaccion_id",
                principalSchema: "operaciones",
                principalTable: "subtipo_transacciones",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
