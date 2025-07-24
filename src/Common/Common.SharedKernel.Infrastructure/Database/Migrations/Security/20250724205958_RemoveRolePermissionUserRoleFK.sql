DO $$
BEGIN
    -- Drop constraint if it exists
    IF EXISTS (
        SELECT 1
        FROM information_schema.table_constraints
        WHERE constraint_schema = 'seguridad'
          AND table_name = 'usuarios_roles'
          AND constraint_name = 'FK_usuarios_roles_roles_permisos_RolePermissionId'
    ) THEN
        ALTER TABLE seguridad.usuarios_roles DROP CONSTRAINT "FK_usuarios_roles_roles_permisos_RolePermissionId";
    END IF;
END $$;

DO $$
BEGIN
    -- Drop index if it exists
    IF EXISTS (
        SELECT 1
        FROM pg_indexes
        WHERE schemaname = 'seguridad'
          AND indexname = 'IX_usuarios_roles_RolePermissionId'
    ) THEN
        DROP INDEX seguridad."IX_usuarios_roles_RolePermissionId";
    END IF;
END $$;

DO $$
BEGIN
    -- Drop column if it exists
    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'seguridad'
          AND table_name = 'usuarios_roles'
          AND column_name = 'RolePermissionId'
    ) THEN
        ALTER TABLE seguridad.usuarios_roles DROP COLUMN "RolePermissionId";
    END IF;
END $$;

DO $$
BEGIN
    -- Insert migration history if not already present
    IF NOT EXISTS (
        SELECT 1
        FROM seguridad."__EFMigrationsHistory"
        WHERE "MigrationId" = '20250724205958_RemoveRolePermissionUserRoleFK'
    ) THEN
        INSERT INTO seguridad."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
        VALUES ('20250724205958_RemoveRolePermissionUserRoleFK', '9.0.3');
    END IF;
END $$;
