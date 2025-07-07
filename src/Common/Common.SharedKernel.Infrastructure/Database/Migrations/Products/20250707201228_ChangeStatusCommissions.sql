ALTER TABLE productos.comisiones RENAME COLUMN activo TO estado;

INSERT INTO productos."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250707201228_ChangeStatusCommissions', '9.0.3');


