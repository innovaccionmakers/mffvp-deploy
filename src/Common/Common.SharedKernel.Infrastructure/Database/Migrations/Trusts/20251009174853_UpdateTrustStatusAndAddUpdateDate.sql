ALTER TABLE fideicomisos.fideicomisos ADD estado_temp integer NOT NULL DEFAULT 1;

UPDATE fideicomisos.fideicomisos 
                  SET estado_temp = 1;

ALTER TABLE fideicomisos.fideicomisos DROP COLUMN estado;

ALTER TABLE fideicomisos.fideicomisos RENAME COLUMN estado_temp TO estado;

ALTER TABLE fideicomisos.fideicomisos ADD fecha_actualizacion timestamp with time zone;

INSERT INTO fideicomisos."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251009174853_UpdateTrustStatusAndAddUpdateDate', '9.0.3');

