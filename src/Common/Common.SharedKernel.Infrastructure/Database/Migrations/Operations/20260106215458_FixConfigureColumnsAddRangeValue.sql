ALTER TABLE operaciones.operaciones_fideicomiso ALTER COLUMN valor_solicitado TYPE numeric(19,2);

ALTER TABLE operaciones.operaciones_fideicomiso ALTER COLUMN retencion_rendimientos_retiro TYPE numeric(19,2);

ALTER TABLE operaciones.operaciones_fideicomiso ALTER COLUMN rendimientos_pagados TYPE numeric(19,2);

ALTER TABLE operaciones.operaciones_fideicomiso ALTER COLUMN capital_pagado TYPE numeric(19,2);

INSERT INTO operaciones."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260106215458_FixConfigureColumnsAddRangeValue', '9.0.3');


