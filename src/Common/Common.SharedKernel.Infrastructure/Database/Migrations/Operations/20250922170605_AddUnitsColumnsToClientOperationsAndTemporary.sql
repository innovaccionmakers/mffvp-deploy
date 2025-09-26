ALTER TABLE operaciones.operaciones_clientes_temporal ADD estado integer NOT NULL DEFAULT 1;

ALTER TABLE operaciones.operaciones_clientes_temporal ADD fideicomiso_id bigint;

ALTER TABLE operaciones.operaciones_clientes_temporal ADD operaciones_cliente_id bigint;

ALTER TABLE operaciones.operaciones_clientes_temporal ADD unidades numeric(38,16);

ALTER TABLE operaciones.operaciones_clientes ADD estado integer NOT NULL DEFAULT 1;

ALTER TABLE operaciones.operaciones_clientes ADD fideicomiso_id bigint;

ALTER TABLE operaciones.operaciones_clientes ADD operaciones_cliente_id bigint;

ALTER TABLE operaciones.operaciones_clientes ADD unidades numeric(38,16);

INSERT INTO operaciones."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250922170605_AddUnitsColumnsToClientOperationsAndTemporary', '9.0.3');

