#!/usr/bin/env bash
set -euo pipefail

# === Configuración ===
startupProject="src/API/MFFVP.Api/MFFVP.Api.csproj"
baseOutput="src/Common/Common.SharedKernel.Infrastructure/Database/Migrations"

modules=(
  "TrustsDbContext|src/Modules/Trusts/Infrastructure/Trusts.Infrastructure|Trusts"
  "ProductsDbContext|src/Modules/Products/Infrastructure/Products.Infrastructure|Products"
  "CustomersDbContext|src/Modules/Customers/Infrastructure/Customers.Infrastructure|Customers"
  "OperationsDbContext|src/Modules/Operations/Infrastructure/Operations.Infrastructure|Operations"
  "ClosingDbContext|src/Modules/Closing/Infrastructure/Closing.Infrastructure|Closing"
  "AssociateDbContext|src/Modules/Associate/Infrastructure/Associate.Infrastructure|Associate"
  "TreasuryDbContext|src/Modules/Treasury/Infrastructure/Treasury.Infrastructure|Treasury"
  "SecurityDbContext|src/Modules/Security/Infrastructure/Security.Infrastructure|Security"
)

# ---------- Bucle principal ----------
for mod in "${modules[@]}"; do
  IFS='|' read -r context projectPath folderName <<<"$mod"
  schemaName="$(echo "$folderName" | tr '[:upper:]' '[:lower:]')"

  migrationsPath="${projectPath%/*}/Database/Migrations"
  outputPath="$baseOutput/$folderName"
  mkdir -p "$outputPath"

  echo ""
  echo "▶ Procesando $folderName   (DbContext = $context)"

  # --------- Recolectar migraciones SIN mapfile y SIN abortar si no hay ---------
  migrations=()
  while IFS= read -r mig; do
    migrations+=("$mig")
  done < <(
    if [[ -d "$migrationsPath" ]]; then
      find "$migrationsPath" -maxdepth 1 -type f -name '*.cs' \
        -exec basename {} .cs \; |
        grep -E '^[0-9]{14}_[^.]+$' | sort || true
    fi
  )
  # ------------------------------------------------------------------------------

  if (( ${#migrations[@]} == 0 )); then
    echo "   ⚠ No se encontraron migraciones para $folderName"
    continue
  fi

  # Preamble PostgreSQL (ajustamos el esquema)
preamble="DO \$EF\$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_extension WHERE extname = 'uuid-ossp') THEN
        CREATE EXTENSION \"uuid-ossp\";
    END IF;
END \$EF\$;

DO \$EF\$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_namespace WHERE nspname = '$schemaName') THEN
        CREATE SCHEMA $schemaName;
    END IF;
END \$EF\$;"

  # ---------- Generación de scripts ----------
  if (( ${#migrations[@]} == 1 )); then
    from="0"
    to="${migrations[0]}"
    sqlFile="$outputPath/$to.sql"

    if [[ ! -f "$sqlFile" ]]; then
      echo "   ▸ Generando script inicial: $from → $to"
      dotnet ef migrations script "$from" "$to" \
        --context "$context" \
        -s "$startupProject" \
        -p "$projectPath.csproj" \
        -o "$sqlFile"

      { echo "$preamble"; echo ""; cat "$sqlFile"; } >"${sqlFile}.tmp" && mv "${sqlFile}.tmp" "$sqlFile"

      sed -i '' -e 's/\$EF\$/$$/g' \
               -e '/^START TRANSACTION;$/d' \
               -e '/^COMMIT;$/d' "$sqlFile"

      echo "   ✔ Creado: $sqlFile"
    else
      echo "   ⏩ Existe: $sqlFile — se omite"
    fi

    continue
  fi

  prev="0"
  for to in "${migrations[@]}"; do
    from="$prev"
    sqlFile="$outputPath/$to.sql"

    if [[ ! -f "$sqlFile" ]]; then
      echo "   ▸ Generando script: $from → $to"
      dotnet ef migrations script "$from" "$to" \
        --context "$context" \
        -s "$startupProject" \
        -p "$projectPath.csproj" \
        -o "$sqlFile"

      if [[ "$from" == "0" ]]; then
        { echo "$preamble"; echo ""; cat "$sqlFile"; } >"${sqlFile}.tmp" && mv "${sqlFile}.tmp" "$sqlFile"
      fi

      sed -i '' -e 's/\$EF\$/$$/g' \
               -e '/^START TRANSACTION;$/d' \
               -e '/^COMMIT;$/d' "$sqlFile"

      echo "   ✔ Creado: $sqlFile"
    else
      echo "   ⏩ Existe: $sqlFile — se omite"
    fi
    prev="$to"
  done
done

echo ""
echo "🏁  Generación completada."