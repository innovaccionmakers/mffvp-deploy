START TRANSACTION;
DROP TABLE operaciones."Banks";

INSERT INTO operaciones."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250613184529_DeleteTableBanksProducts', '9.0.3');

COMMIT;

