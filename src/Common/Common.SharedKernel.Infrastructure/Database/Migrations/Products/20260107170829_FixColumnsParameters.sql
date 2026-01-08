ALTER TABLE productos.salarios_minimos ALTER COLUMN valor TYPE numeric(19,2);
ALTER TABLE productos.salarios_minimos ALTER COLUMN valor SET DEFAULT 0.0;

ALTER TABLE productos.salarios_minimos ALTER COLUMN anio TYPE character varying(4);

INSERT INTO productos."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260107170829_FixColumnsParameters', '9.0.3');


