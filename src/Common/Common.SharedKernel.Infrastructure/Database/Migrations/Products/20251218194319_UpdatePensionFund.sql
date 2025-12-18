ALTER TABLE productos.fondos_voluntarios_pensiones ADD cod_negocio_sfc integer NOT NULL DEFAULT 0;

INSERT INTO productos."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251218194319_UpdatePensionFund', '9.0.3');


