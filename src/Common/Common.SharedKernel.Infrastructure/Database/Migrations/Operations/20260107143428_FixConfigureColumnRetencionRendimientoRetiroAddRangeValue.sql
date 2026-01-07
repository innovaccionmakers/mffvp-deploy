ALTER TABLE operaciones.operaciones_fideicomiso ALTER COLUMN retencion_contingente_retiro TYPE numeric(19,2);

INSERT INTO operaciones."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260107143428_FixConfigureColumnRetencionRendimientoRetiroAddRangeValue', '9.0.3');


