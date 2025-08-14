ALTER TABLE operaciones.informacion_auxiliar DROP CONSTRAINT "FK_informacion_auxiliar_bancos_banco_recaudo";

DROP TABLE operaciones.bancos;

DROP INDEX operaciones."IX_informacion_auxiliar_banco_recaudo";

INSERT INTO operaciones."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250814185402_RemoveBankEntityFromOperationsModule', '9.0.3');


