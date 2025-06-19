ALTER TABLE productos.alternativas RENAME COLUMN "codigo_homologado)" TO codigo_homologado;

INSERT INTO productos."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250613063732_ChangeParameterAlternative', '9.0.3');


