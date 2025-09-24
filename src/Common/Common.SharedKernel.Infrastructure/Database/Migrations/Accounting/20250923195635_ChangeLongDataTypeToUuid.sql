ALTER TABLE contabilidad.auxiliar_contable DROP COLUMN identificador;

ALTER TABLE contabilidad.auxiliar_contable ADD identificador uuid NOT NULL DEFAULT (gen_random_uuid());

INSERT INTO contabilidad."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250923195635_ChangeLongDataTypeToUuid', '9.0.3');


