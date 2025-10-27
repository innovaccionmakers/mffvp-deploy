ALTER TABLE cierre.rendimientos_fideicomisos ADD CONSTRAINT ux_rendimientos_fideicomisos_portafolio_fideicomiso_fecha UNIQUE (portafolio_id, fideicomiso_id, fecha_cierre);

INSERT INTO cierre."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251022162029_CreateTrustYieldConstraintBulkUpdate', '9.0.3');


