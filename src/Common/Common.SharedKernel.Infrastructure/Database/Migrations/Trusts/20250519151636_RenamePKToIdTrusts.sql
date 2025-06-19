ALTER TABLE fideicomisos.parametros_configuracion RENAME COLUMN parametro_configuracion_id TO id;

ALTER TABLE fideicomisos.historicos_fideicomisos RENAME COLUMN historico_fideicomiso_id TO id;

ALTER TABLE fideicomisos.fideicomisos RENAME COLUMN fideicomiso_id TO id;

INSERT INTO fideicomisos."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250519151636_RenamePKToIdTrusts', '9.0.3');


