START TRANSACTION;
ALTER TABLE cierre.valoracion_portafolio ALTER COLUMN fecha_proceso TYPE timestamp with time zone;

INSERT INTO cierre."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250731195430_AlterClosingDateColumnTypeToTimestamp', '9.0.3');


