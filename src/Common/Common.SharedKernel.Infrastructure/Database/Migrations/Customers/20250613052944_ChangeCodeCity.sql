ALTER TABLE personas.municipios RENAME COLUMN codigo_ciudad TO codigo_municipio;

INSERT INTO personas."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250613052944_ChangeCodeCity', '9.0.3');


