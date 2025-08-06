ALTER TABLE productos.portafolios ALTER COLUMN vir_retiro_minimo TYPE numeric(19,2);

ALTER TABLE productos.portafolios ALTER COLUMN vir_retiro_max_parcial TYPE numeric(19,2);

INSERT INTO productos."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250805195537_DataTypeSetting', '9.0.3');


