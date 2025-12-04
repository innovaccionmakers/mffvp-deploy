ALTER TABLE fideicomisos.fideicomisos ADD disponible_retiro_agil numeric(19,2) NOT NULL DEFAULT 0.0;

ALTER TABLE fideicomisos.fideicomisos ADD saldo_protegido numeric(19,2) NOT NULL DEFAULT 0.0;

INSERT INTO fideicomisos."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251204144723_AddProtectedBalanceAndAgileWithdrawalToTrusts', '9.0.3');

