DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM pg_namespace WHERE nspname = 'associate') THEN
        EXECUTE 'DROP SCHEMA associate CASCADE';
    END IF;

    IF EXISTS (SELECT 1 FROM pg_namespace WHERE nspname = 'customers') THEN
        EXECUTE 'DROP SCHEMA customers CASCADE';
    END IF;

    IF EXISTS (SELECT 1 FROM pg_namespace WHERE nspname = 'operations') THEN
        EXECUTE 'DROP SCHEMA operations CASCADE';
    END IF;

    IF EXISTS (SELECT 1 FROM pg_namespace WHERE nspname = 'products') THEN
        EXECUTE 'DROP SCHEMA products CASCADE';
    END IF;

    IF EXISTS (SELECT 1 FROM pg_namespace WHERE nspname = 'trusts') THEN
        EXECUTE 'DROP SCHEMA trusts CASCADE';
    END IF;
END $$;
