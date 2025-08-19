ALTER TABLE productos.oficinas ADD centro_costos text NOT NULL DEFAULT '';

INSERT INTO productos."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250815184947_AddCostCenterToOfficesTable', '9.0.3');


