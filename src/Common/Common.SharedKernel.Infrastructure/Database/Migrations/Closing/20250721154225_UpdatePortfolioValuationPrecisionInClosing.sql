START TRANSACTION;
ALTER TABLE cierre.valoracion_portafolio ALTER COLUMN valor_unidad TYPE numeric(38,16);

INSERT INTO cierre."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250721154225_UpdatePortfolioValuationPrecisionInClosing', '9.0.3');


