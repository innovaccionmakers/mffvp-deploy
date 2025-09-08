ALTER TABLE operaciones.informacion_auxiliar 
                ALTER COLUMN cuenta_recaudo TYPE varchar 
                USING cuenta_recaudo::varchar;
            

INSERT INTO operaciones."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250905171433_AlterTableAuxiliaryInformation', '9.0.3');


