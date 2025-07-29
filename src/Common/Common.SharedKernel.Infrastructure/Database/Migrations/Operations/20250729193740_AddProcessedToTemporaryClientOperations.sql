ALTER TABLE operaciones.operaciones_clientes_temporal ADD procesado boolean NOT NULL DEFAULT FALSE;

INSERT INTO operaciones."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250729193740_AddProcessedToTemporaryClientOperations', '9.0.3');


