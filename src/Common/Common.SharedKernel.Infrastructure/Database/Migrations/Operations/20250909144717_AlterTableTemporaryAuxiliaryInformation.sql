ALTER TABLE operaciones.informacion_auxiliar_temporal ALTER COLUMN cuenta_recaudo TYPE text;

ALTER TABLE operaciones.informacion_auxiliar ALTER COLUMN cuenta_recaudo TYPE text;

INSERT INTO operaciones."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250909144717_AlterTableTemporaryAuxiliaryInformation', '9.0.3');


