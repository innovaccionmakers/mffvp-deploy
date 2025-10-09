ALTER TABLE operaciones.operaciones_clientes_temporal ADD causal_id integer;

ALTER TABLE operaciones.operaciones_clientes ADD causal_id integer;

INSERT INTO operaciones."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251009182937_AddCauseIdToClientOperation', '9.0.3');

