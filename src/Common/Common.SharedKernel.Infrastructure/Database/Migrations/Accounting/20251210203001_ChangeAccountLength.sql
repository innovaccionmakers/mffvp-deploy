ALTER TABLE contabilidad.auxiliar_contable ALTER COLUMN identificador TYPE uuid;

ALTER TABLE contabilidad.auxiliar_contable ALTER COLUMN cuenta TYPE character varying(20);

INSERT INTO contabilidad."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251210203001_ChangeAccountLength', '9.0.3');


