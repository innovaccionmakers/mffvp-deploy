ALTER TABLE cierre.operaciones_cliente ADD causal_id integer;

INSERT INTO cierre."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251009183114_AddCauseIdToClosingClientOperation', '9.0.3');

