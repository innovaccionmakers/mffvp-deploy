ALTER TABLE contabilidad.conceptos DROP COLUMN cuenta_contra_credito;

ALTER TABLE contabilidad.conceptos DROP COLUMN cuenta_contra_debito;

INSERT INTO contabilidad."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251002210318_DropColumnsConceptsTable', '9.0.3');


