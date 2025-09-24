ALTER TABLE contabilidad.auxiliar_contable DROP COLUMN nit;

INSERT INTO contabilidad."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250924162015_RemoveNitColumn', '9.0.3');


