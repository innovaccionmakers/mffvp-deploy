ALTER TABLE operaciones.subtipo_transacciones ADD atributos_adicionales jsonb NOT NULL DEFAULT ('{}'::jsonb);

ALTER TABLE operaciones.subtipo_transacciones ADD visible boolean NOT NULL DEFAULT TRUE;

INSERT INTO operaciones."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250801204447_AddVisibleAndAttributesToSubtransactionTypes', '9.0.3');


