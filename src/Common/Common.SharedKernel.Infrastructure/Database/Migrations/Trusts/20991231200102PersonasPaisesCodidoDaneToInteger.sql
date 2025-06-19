-- Drop column if it already exists
ALTER TABLE IF EXISTS personas.paises
    DROP COLUMN IF EXISTS codigo_dane;

-- Add new column with desired type
ALTER TABLE IF EXISTS personas.paises
    ADD COLUMN codigo_dane integer NOT NULL;
