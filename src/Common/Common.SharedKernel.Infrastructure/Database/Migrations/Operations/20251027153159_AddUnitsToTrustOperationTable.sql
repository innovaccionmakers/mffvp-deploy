ALTER TABLE operaciones.operaciones_fideicomiso ADD unidades numeric(38,16) NOT NULL DEFAULT 0.0;

INSERT INTO operaciones."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251027153159_AddUnitsToTrustOperationTable', '9.0.3');

