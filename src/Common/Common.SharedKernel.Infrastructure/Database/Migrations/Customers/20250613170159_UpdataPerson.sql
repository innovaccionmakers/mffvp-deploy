START TRANSACTION;
ALTER TABLE personas.personas ADD fecha_nacimiento timestamp with time zone NOT NULL DEFAULT TIMESTAMPTZ '-infinity';

INSERT INTO personas."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250613170159_UpdataPerson', '9.0.3');

COMMIT;

