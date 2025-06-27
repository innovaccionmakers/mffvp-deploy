ALTER TABLE fideicomisos.fideicomisos ADD estado boolean NOT NULL DEFAULT TRUE;

ALTER TABLE fideicomisos.fideicomisos ADD rendimiento_acumulado numeric(19,2) NOT NULL DEFAULT 0.0;

INSERT INTO fideicomisos."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250627131444_RefactorTrustEntityAndIntegrationEvents', '9.0.3');


