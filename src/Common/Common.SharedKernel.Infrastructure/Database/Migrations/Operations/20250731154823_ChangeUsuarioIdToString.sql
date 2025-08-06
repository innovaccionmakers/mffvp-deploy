ALTER TABLE operaciones.informacion_auxiliar_temporal ALTER COLUMN usuario_id TYPE character varying(50);

ALTER TABLE operaciones.informacion_auxiliar ALTER COLUMN usuario_id TYPE character varying(50);

INSERT INTO operaciones."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250731154823_ChangeUsuarioIdToString', '9.0.3');


