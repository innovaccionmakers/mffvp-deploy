ALTER TABLE cierre.operaciones_cliente ADD estado integer NOT NULL DEFAULT 1;

ALTER TABLE cierre.operaciones_cliente ADD fideicomiso_id bigint;

ALTER TABLE cierre.operaciones_cliente ADD operaciones_cliente_id bigint;

ALTER TABLE cierre.operaciones_cliente ADD unidades numeric(38,16);

INSERT INTO cierre."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250922170949_AddUnitsColumnToClientOperations', '9.0.3');

