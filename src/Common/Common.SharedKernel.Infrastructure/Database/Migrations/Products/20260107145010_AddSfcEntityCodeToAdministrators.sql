ALTER TABLE productos.administradores ADD codigo_entidad_sfc character varying(6) NOT NULL DEFAULT '';

INSERT INTO productos."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260107145010_AddSfcEntityCodeToAdministrators', '9.0.3');

