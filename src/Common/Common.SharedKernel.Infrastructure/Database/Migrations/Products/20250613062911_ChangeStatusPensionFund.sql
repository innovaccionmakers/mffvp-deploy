ALTER TABLE productos.planes_fondo ALTER COLUMN estado TYPE text;

ALTER TABLE productos.oficinas ALTER COLUMN estado TYPE text;

ALTER TABLE productos.objetivos ALTER COLUMN estado TYPE text;

INSERT INTO productos."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250613062911_ChangeStatusPensionFund', '9.0.3');


