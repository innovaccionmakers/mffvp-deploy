ALTER TABLE cierre.operaciones_cliente RENAME COLUMN subtipo_transaccion_id TO tipo_operaciones_id;

INSERT INTO cierre."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250813144453_UpdateClientOperationForeignKeyToOperationType', '9.0.3');


