UPDATE contabilidad.auxiliar_contable SET periodo = '' WHERE periodo IS NULL;
ALTER TABLE contabilidad.auxiliar_contable ALTER COLUMN periodo SET NOT NULL;
ALTER TABLE contabilidad.auxiliar_contable ALTER COLUMN periodo SET DEFAULT '';

UPDATE contabilidad.auxiliar_contable SET fecha = TIMESTAMPTZ '-infinity' WHERE fecha IS NULL;
ALTER TABLE contabilidad.auxiliar_contable ALTER COLUMN fecha SET NOT NULL;
ALTER TABLE contabilidad.auxiliar_contable ALTER COLUMN fecha SET DEFAULT TIMESTAMPTZ '-infinity';

UPDATE contabilidad.auxiliar_contable SET digito_verificacion = 0 WHERE digito_verificacion IS NULL;
ALTER TABLE contabilidad.auxiliar_contable ALTER COLUMN digito_verificacion SET NOT NULL;
ALTER TABLE contabilidad.auxiliar_contable ALTER COLUMN digito_verificacion SET DEFAULT 0;

ALTER TABLE contabilidad.auxiliar_contable ADD portafolio_id integer NOT NULL DEFAULT 0;

INSERT INTO contabilidad."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251020165954_AddPortfolioIdColumn', '9.0.3');


