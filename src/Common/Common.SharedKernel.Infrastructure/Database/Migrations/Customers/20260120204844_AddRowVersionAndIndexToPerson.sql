ALTER TABLE personas.personas ADD row_version bigint NOT NULL DEFAULT ((extract(epoch from clock_timestamp()) * 1000)::BIGINT);

ALTER TABLE personas.paises ALTER COLUMN codigo_dane TYPE integer;

CREATE INDEX idx_row_version ON personas.personas (row_version);

INSERT INTO personas."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260120204844_AddRowVersionAndIndexToPerson', '9.0.3');


