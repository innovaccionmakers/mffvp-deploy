START TRANSACTION;
ALTER TABLE productos.fondos_voluntarios_pensiones ALTER COLUMN estado TYPE text;

INSERT INTO productos."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250613062316_ChangeStatus', '9.0.3');

COMMIT;

