ALTER TABLE operaciones.subtipo_transacciones ALTER COLUMN categoria DROP NOT NULL;

INSERT INTO operaciones."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250804144147_MakeSubtransactionCategoryNullable', '9.0.3');


