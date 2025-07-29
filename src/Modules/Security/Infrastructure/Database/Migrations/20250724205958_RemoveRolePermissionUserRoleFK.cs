using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Security.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRolePermissionUserRoleFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_usuarios_roles_roles_permisos_RolePermissionId",
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
        }
    }
}
