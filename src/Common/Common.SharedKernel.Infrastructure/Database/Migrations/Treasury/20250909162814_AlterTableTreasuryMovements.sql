ALTER TABLE tesoreria.movimientos_tesoreria ALTER COLUMN entidad_id DROP NOT NULL;

INSERT INTO tesoreria."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250909162814_AlterTableTreasuryMovements', '9.0.3');


