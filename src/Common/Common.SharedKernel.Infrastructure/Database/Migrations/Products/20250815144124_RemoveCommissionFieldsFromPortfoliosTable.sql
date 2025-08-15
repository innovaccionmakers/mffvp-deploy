ALTER TABLE productos.portafolios DROP COLUMN porcentaje_comision;

ALTER TABLE productos.portafolios DROP COLUMN tipo_tasa_comision;

INSERT INTO productos."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250815144124_RemoveCommissionFieldsFromPortfoliosTable', '9.0.3');


