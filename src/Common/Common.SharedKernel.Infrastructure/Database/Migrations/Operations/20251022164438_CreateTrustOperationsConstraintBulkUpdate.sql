ALTER TABLE operaciones.operaciones_fideicomiso ADD CONSTRAINT ux_operaciones_fideicomiso_portafolio_fideicomiso_fecha_tipo UNIQUE (portafolio_id, fideicomiso_id, fecha_proceso, tipo_operaciones_id);

INSERT INTO operaciones."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251022164438_CreateTrustOperationsConstraintBulkUpdate', '9.0.3');


