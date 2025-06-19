ALTER TABLE productos.portafolios ADD porcentaje_comision numeric NOT NULL DEFAULT 0.0;

ALTER TABLE productos.portafolios ADD tipo_tasa_comision integer NOT NULL DEFAULT 0;

INSERT INTO productos."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250613064244_NewChangesPortfolio', '9.0.3');


