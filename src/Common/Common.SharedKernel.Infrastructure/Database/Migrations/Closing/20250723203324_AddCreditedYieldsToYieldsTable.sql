START TRANSACTION;
ALTER TABLE cierre.rendimientos ADD rendimientos_abonados numeric(19,2) NOT NULL DEFAULT 0.0;

INSERT INTO cierre."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250723203324_AddCreditedYieldsToYieldsTable', '9.0.3');


