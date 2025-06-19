ALTER TABLE operaciones.informacion_auxiliar DROP CONSTRAINT "FK_informacion_auxiliar_bancos_BankId";

DROP INDEX operaciones."IX_informacion_auxiliar_BankId";

ALTER TABLE operaciones.informacion_auxiliar DROP COLUMN "BankId";

CREATE INDEX "IX_informacion_auxiliar_banco_recaudo" ON operaciones.informacion_auxiliar (banco_recaudo);

ALTER TABLE operaciones.informacion_auxiliar ADD CONSTRAINT "FK_informacion_auxiliar_bancos_banco_recaudo" FOREIGN KEY (banco_recaudo) REFERENCES operaciones.bancos (id) ON DELETE CASCADE;

INSERT INTO operaciones."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250618165452_AddRelationToBankInOperation', '9.0.3');


