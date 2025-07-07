ALTER TABLE cierre.operaciones_cliente ALTER COLUMN id DROP IDENTITY;

INSERT INTO cierre."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250707165235_UpdateClientOperationIdToManualGeneration', '9.0.3');


