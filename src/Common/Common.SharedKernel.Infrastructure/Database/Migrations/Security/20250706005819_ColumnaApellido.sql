ALTER TABLE seguridad.usuarios RENAME COLUMN segundo_nombre TO apellido;

INSERT INTO seguridad."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250706005819_ColumnaApellido', '9.0.3');


