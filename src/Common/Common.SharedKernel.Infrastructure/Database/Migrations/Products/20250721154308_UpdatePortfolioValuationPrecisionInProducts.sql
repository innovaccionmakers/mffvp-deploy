START TRANSACTION;
ALTER TABLE productos.valoracion_portafolio_dia ALTER COLUMN valor_unidad TYPE numeric(38,16);

INSERT INTO productos."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250721154308_UpdatePortfolioValuationPrecisionInProducts', '9.0.3');


