ALTER TABLE seguridad.usuarios_roles DROP CONSTRAINT "FK_usuarios_roles_roles_permisos_rol_permiso_id";

ALTER TABLE seguridad.usuarios_roles RENAME COLUMN rol_permiso_id TO rol_id;

ALTER INDEX seguridad."IX_usuarios_roles_rol_permiso_id" RENAME TO "IX_usuarios_roles_rol_id";

ALTER TABLE seguridad.usuarios_roles ADD "RolePermissionId" integer;

CREATE INDEX "IX_usuarios_roles_RolePermissionId" ON seguridad.usuarios_roles ("RolePermissionId");

ALTER TABLE seguridad.usuarios_roles ADD CONSTRAINT "FK_usuarios_roles_roles_permisos_RolePermissionId" FOREIGN KEY ("RolePermissionId") REFERENCES seguridad.roles_permisos (id);

ALTER TABLE seguridad.usuarios_roles ADD CONSTRAINT "FK_usuarios_roles_roles_rol_id" FOREIGN KEY (rol_id) REFERENCES seguridad.roles (id) ON DELETE CASCADE;

INSERT INTO seguridad."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250713130642_UserRoles', '9.0.3');


