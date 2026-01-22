ALTER TABLE productos.fondos_voluntarios_pensiones ADD row_version bigint NOT NULL DEFAULT ((extract(epoch from clock_timestamp()) * 1000)::BIGINT);

CREATE INDEX idx_fondos_voluntarios_pensiones_row_version ON productos.fondos_voluntarios_pensiones (row_version);

INSERT INTO productos."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260121024052_AddRowVersionToPensionFund', '9.0.3');


