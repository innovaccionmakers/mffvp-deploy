ALTER TABLE contabilidad.configuraciones_generales RENAME COLUMN "portafolio_Id" TO portafolio_id;

INSERT INTO contabilidad."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260105193404_FixParameterPortafolioId', '9.0.3');


