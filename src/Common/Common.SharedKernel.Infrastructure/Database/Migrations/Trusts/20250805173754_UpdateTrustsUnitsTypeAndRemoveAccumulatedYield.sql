START TRANSACTION;
ALTER TABLE fideicomisos.fideicomisos DROP COLUMN rendimiento_acumulado;

ALTER TABLE fideicomisos.fideicomisos ALTER COLUMN unidades_totales TYPE numeric(38,16);

INSERT INTO fideicomisos."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250805173754_UpdateTrustsUnitsTypeAndRemoveAccumulatedYield', '9.0.3');


