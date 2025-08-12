ALTER TABLE tesoreria.emisor ADD banco boolean NOT NULL DEFAULT FALSE;

INSERT INTO tesoreria."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250806155127_AlterTableEmisorAddNewColumnBank', '9.0.3');


