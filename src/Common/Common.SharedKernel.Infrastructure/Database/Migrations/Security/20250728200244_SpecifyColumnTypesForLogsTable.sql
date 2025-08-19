START TRANSACTION;
ALTER TABLE seguridad.logs ALTER COLUMN usuario TYPE varchar(256);

ALTER TABLE seguridad.logs ALTER COLUMN maquina TYPE varchar(50);

ALTER TABLE seguridad.logs ALTER COLUMN ip TYPE varchar(50);

ALTER TABLE seguridad.logs ALTER COLUMN descripcion TYPE varchar(200);

ALTER TABLE seguridad.logs ALTER COLUMN accion TYPE varchar(200);

INSERT INTO seguridad."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250728200244_SpecifyColumnTypesForLogsTable', '9.0.3');


