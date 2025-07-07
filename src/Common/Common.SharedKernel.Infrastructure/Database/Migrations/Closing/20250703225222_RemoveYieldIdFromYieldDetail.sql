ALTER TABLE cierre.detalle_rendimientos DROP CONSTRAINT "FK_detalle_rendimientos_rendimientos_rendimiento_id";

DROP INDEX cierre."IX_detalle_rendimientos_rendimiento_id";

ALTER TABLE cierre.detalle_rendimientos DROP COLUMN rendimiento_id;

INSERT INTO cierre."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250703225222_RemoveYieldIdFromYieldDetail', '9.0.3');


