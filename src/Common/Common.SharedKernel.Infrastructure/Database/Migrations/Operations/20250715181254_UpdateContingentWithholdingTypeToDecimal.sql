ALTER TABLE operaciones.informacion_auxiliar_temporal ALTER COLUMN retencion_contingente TYPE numeric(19,2);

ALTER TABLE operaciones.informacion_auxiliar ALTER COLUMN retencion_contingente TYPE numeric(19,2);

INSERT INTO operaciones."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250715181254_UpdateContingentWithholdingTypeToDecimal', '9.0.3');


