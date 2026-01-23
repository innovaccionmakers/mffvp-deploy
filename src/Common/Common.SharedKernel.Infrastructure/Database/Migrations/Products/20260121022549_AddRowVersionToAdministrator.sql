ALTER TABLE productos.administradores ADD row_version bigint NOT NULL DEFAULT ((extract(epoch from clock_timestamp()) * 1000)::BIGINT);

CREATE INDEX idx_administradores_row_version ON productos.administradores (row_version);

INSERT INTO productos."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260121022549_AddRowVersionToAdministrator', '9.0.3');


