ALTER TABLE productos.portafolios ALTER COLUMN permite_retiro_agil DROP DEFAULT;
ALTER TABLE productos.portafolios ALTER COLUMN permite_retiro_agil TYPE boolean USING false;
ALTER TABLE productos.portafolios ALTER COLUMN permite_retiro_agil SET DEFAULT false;


INSERT INTO productos."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260119022025_AddAgileWithdrawalFieldsToBalances', '9.0.3');

