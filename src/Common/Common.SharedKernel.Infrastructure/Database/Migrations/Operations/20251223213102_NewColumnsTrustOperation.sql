ALTER TABLE operaciones.operaciones_fideicomiso ADD capital_pagado numeric NOT NULL DEFAULT 0.0;

ALTER TABLE operaciones.operaciones_fideicomiso ADD rendimientos_pagados numeric NOT NULL DEFAULT 0.0;

ALTER TABLE operaciones.operaciones_fideicomiso ADD retencion_contingente_retiro numeric NOT NULL DEFAULT 0.0;

ALTER TABLE operaciones.operaciones_fideicomiso ADD retencion_rendimientos_retiro numeric NOT NULL DEFAULT 0.0;

ALTER TABLE operaciones.operaciones_fideicomiso ADD valor_solicitado numeric NOT NULL DEFAULT 0.0;

CREATE INDEX "IX_operaciones_clientes_operaciones_cliente_id" ON operaciones.operaciones_clientes (operaciones_cliente_id);

ALTER TABLE operaciones.operaciones_clientes ADD CONSTRAINT "FK_operaciones_clientes_operaciones_clientes_operaciones_clien~" FOREIGN KEY (operaciones_cliente_id) REFERENCES operaciones.operaciones_clientes (id) ON DELETE RESTRICT;

INSERT INTO operaciones."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251223213102_NewColumnsTrustOperation', '9.0.3');


