ALTER TABLE operaciones.operaciones_fideicomiso DROP CONSTRAINT "FK_operaciones_fideicomiso_operaciones_clientes_operaciones_cl~";

ALTER TABLE operaciones.operaciones_fideicomiso ALTER COLUMN operaciones_clientes_id DROP NOT NULL;

ALTER TABLE operaciones.operaciones_fideicomiso ADD CONSTRAINT "FK_operaciones_fideicomiso_operaciones_clientes_operaciones_cl~" FOREIGN KEY (operaciones_clientes_id) REFERENCES operaciones.operaciones_clientes (id);

INSERT INTO operaciones."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250801151431_AllowNullClientOperationIdInTrustOperation', '9.0.3');


