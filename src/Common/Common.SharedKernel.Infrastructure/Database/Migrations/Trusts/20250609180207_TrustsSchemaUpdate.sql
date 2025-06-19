DROP TABLE fideicomisos.historicos_fideicomisos;

ALTER TABLE fideicomisos.fideicomisos DROP COLUMN cliente_id;

ALTER TABLE fideicomisos.fideicomisos DROP COLUMN porcentaje_retencion_contingente;

ALTER TABLE fideicomisos.fideicomisos ALTER COLUMN saldo_total TYPE numeric(19,2);

ALTER TABLE fideicomisos.fideicomisos ALTER COLUMN retencion_rendimiento TYPE numeric(19,2);

ALTER TABLE fideicomisos.fideicomisos ALTER COLUMN retencion_contingente TYPE numeric(19,2);

ALTER TABLE fideicomisos.fideicomisos ALTER COLUMN rendimiento TYPE numeric(19,2);

ALTER TABLE fideicomisos.fideicomisos ALTER COLUMN disponible TYPE numeric(19,2);

ALTER TABLE fideicomisos.fideicomisos ALTER COLUMN capital TYPE numeric(19,2);

ALTER TABLE fideicomisos.fideicomisos ADD operaciones_cliente_id bigint NOT NULL DEFAULT 0;

INSERT INTO fideicomisos."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250609180207_TrustsSchemaUpdate', '9.0.3');


