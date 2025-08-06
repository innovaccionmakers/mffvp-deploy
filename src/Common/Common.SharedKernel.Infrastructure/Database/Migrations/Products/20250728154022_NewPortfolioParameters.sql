ALTER TABLE productos.portafolios DROP COLUMN "CommissionRateType";

INSERT INTO productos."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250728154022_NewPortfolioParameters', '9.0.3');


