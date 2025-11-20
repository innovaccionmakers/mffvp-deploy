ALTER TABLE cierre.rendimientos_fideicomisos DROP COLUMN retencion_rendimiento;

INSERT INTO cierre."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251120131234_RemoveYieldRetentionFromTrustYieldTable', '9.0.3');


