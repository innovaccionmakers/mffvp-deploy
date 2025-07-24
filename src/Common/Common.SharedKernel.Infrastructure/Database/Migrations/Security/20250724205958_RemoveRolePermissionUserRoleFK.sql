ALTER TABLE seguridad.usuarios_roles DROP CONSTRAINT "FK_usuarios_roles_roles_permisos_RolePermissionId";

DROP INDEX seguridad."IX_usuarios_roles_RolePermissionId";

ALTER TABLE seguridad.usuarios_roles DROP COLUMN "RolePermissionId";

INSERT INTO seguridad."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250724205958_RemoveRolePermissionUserRoleFK', '9.0.3');


