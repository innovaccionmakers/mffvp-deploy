DROP INDEX operaciones."IX_origenaportes_modorigen_origen_id";

CREATE INDEX "IX_origenaportes_modorigen_origen_id" ON operaciones.origenaportes_modorigen (origen_id);

INSERT INTO operaciones."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250613185452_ChangeRelationOrigin', '9.0.3');


