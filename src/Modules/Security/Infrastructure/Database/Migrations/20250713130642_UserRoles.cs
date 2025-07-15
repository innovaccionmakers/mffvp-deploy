using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Security.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class UserRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_usuarios_roles_roles_permisos_rol_permiso_id",
                schema: "seguridad",
                table: "usuarios_roles");

            migrationBuilder.RenameColumn(
                name: "rol_permiso_id",
                schema: "seguridad",
                table: "usuarios_roles",
                newName: "rol_id");

            migrationBuilder.RenameIndex(
                name: "IX_usuarios_roles_rol_permiso_id",
                schema: "seguridad",
                table: "usuarios_roles",
                newName: "IX_usuarios_roles_rol_id");

            migrationBuilder.AddColumn<int>(
                name: "RolePermissionId",
                schema: "seguridad",
                table: "usuarios_roles",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_roles_RolePermissionId",
                schema: "seguridad",
                table: "usuarios_roles",
                column: "RolePermissionId");

            migrationBuilder.AddForeignKey(
                name: "FK_usuarios_roles_roles_permisos_RolePermissionId",
                schema: "seguridad",
                table: "usuarios_roles",
                column: "RolePermissionId",
                principalSchema: "seguridad",
                principalTable: "roles_permisos",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_usuarios_roles_roles_rol_id",
                schema: "seguridad",
                table: "usuarios_roles",
                column: "rol_id",
                principalSchema: "seguridad",
                principalTable: "roles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_usuarios_roles_roles_permisos_RolePermissionId",
                schema: "seguridad",
                table: "usuarios_roles");

            migrationBuilder.DropForeignKey(
                name: "FK_usuarios_roles_roles_rol_id",
                schema: "seguridad",
                table: "usuarios_roles");

            migrationBuilder.DropIndex(
                name: "IX_usuarios_roles_RolePermissionId",
                schema: "seguridad",
                table: "usuarios_roles");

            migrationBuilder.DropColumn(
                name: "RolePermissionId",
                schema: "seguridad",
                table: "usuarios_roles");

            migrationBuilder.RenameColumn(
                name: "rol_id",
                schema: "seguridad",
                table: "usuarios_roles",
                newName: "rol_permiso_id");

            migrationBuilder.RenameIndex(
                name: "IX_usuarios_roles_rol_id",
                schema: "seguridad",
                table: "usuarios_roles",
                newName: "IX_usuarios_roles_rol_permiso_id");

            migrationBuilder.AddForeignKey(
                name: "FK_usuarios_roles_roles_permisos_rol_permiso_id",
                schema: "seguridad",
                table: "usuarios_roles",
                column: "rol_permiso_id",
                principalSchema: "seguridad",
                principalTable: "roles_permisos",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
