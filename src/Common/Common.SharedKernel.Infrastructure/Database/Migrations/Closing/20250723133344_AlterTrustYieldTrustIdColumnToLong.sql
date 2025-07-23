START TRANSACTION;
ALTER TABLE cierre.rendimientos_fideicomisos ALTER COLUMN fideicomiso_id TYPE bigint;

INSERT INTO cierre."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250723133344_AlterTrustYieldTrustIdColumnToLong', '9.0.3');


