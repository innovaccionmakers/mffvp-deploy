ALTER TABLE cierre.valoracion_portafolio ADD valor_inicial numeric(19,2) NOT NULL DEFAULT 0.0;

INSERT INTO cierre."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251031152332_AddInitialValueToPortfolioValuationTable', '9.0.3');

