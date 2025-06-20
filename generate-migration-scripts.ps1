# === Configuration ===
$startupProject = "src/API/MFFVP.Api/MFFVP.Api.csproj"
$baseOutput = "src/Common/Common.SharedKernel.Infrastructure/Database/Migrations"

# Define modules and schema (schema = folder name lowercased)
$modules = @(
    @{ DbContext = "TrustsDbContext"; Project = "src/Modules/Trusts/Infrastructure/Trusts.Infrastructure"; Folder = "Trusts" },
    @{ DbContext = "ProductsDbContext"; Project = "src/Modules/Products/Infrastructure/Products.Infrastructure"; Folder = "Products" },
    @{ DbContext = "CustomersDbContext"; Project = "src/Modules/Customers/Infrastructure/Customers.Infrastructure"; Folder = "Customers" },
    @{ DbContext = "OperationsDbContext"; Project = "src/Modules/Operations/Infrastructure/Operations.Infrastructure"; Folder = "Operations" },
    @{ DbContext = "ClosingDbContext"; Project = "src/Modules/Closing/Infrastructure/Closing.Infrastructure"; Folder = "Closing" },
    @{ DbContext = "AssociateDbContext"; Project = "src/Modules/Associate/Infrastructure/Associate.Infrastructure"; Folder = "Associate" }
)

foreach ($module in $modules) {
    $context = $module.DbContext
    $projectPath = $module.Project
    $folderName = $module.Folder
    $schemaName = $folderName.ToLower()

    $migrationsPath = "$($projectPath.Substring(0, $projectPath.LastIndexOf('/')))/Database/Migrations"
    $outputPath = "$baseOutput\$folderName"
    New-Item -ItemType Directory -Path $outputPath -Force | Out-Null

    Write-Host "`nProcessing $folderName ($context)"

    $migrations = Get-ChildItem -Path $migrationsPath -Filter *.cs -ErrorAction SilentlyContinue |
        Where-Object { $_.BaseName -match '^[0-9]{14}_[^\.]+$' } |
        Sort-Object Name |
        Select-Object -ExpandProperty BaseName

    $migrations = @($migrations)

    if ($migrations.Count -eq 0) {
        Write-Host "No migrations found for $folderName"
        continue
    }

    # PostgreSQL-safe preamble without escaping issues
    $preamble = @'
DO $EF$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_extension WHERE extname = 'uuid-ossp') THEN
        CREATE EXTENSION "uuid-ossp";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_namespace WHERE nspname = '{schema}') THEN
        CREATE SCHEMA {schema};
    END IF;
END $EF$;
'@ -replace '{schema}', $schemaName

    if ($migrations.Count -eq 1) {
        $to = $migrations[0]
        $sqlFile = "$outputPath\$to.sql"

        if (-Not (Test-Path $sqlFile)) {
            Write-Host "Generating initial script: 0 → $to"
            dotnet ef migrations script 0 $to `
                --context $context `
                -s $startupProject `
                -p "$projectPath.csproj" `
                -o $sqlFile

            $content = Get-Content $sqlFile -Raw
            $finalScript = $preamble + "`r`n" + $content
            $finalScript = $finalScript -replace '\$EF\$', '$$$$'
            $finalScript = $finalScript -replace 'START TRANSACTION;\s*', ''
            $finalScript = $finalScript -replace 'COMMIT;\s*', ''
            Set-Content -Path $sqlFile -Value $finalScript -Encoding UTF8

            Write-Host "Created: $sqlFile"
        } else {
            Write-Host "Exists: $sqlFile — skipping"
        }

        continue
    }

    for ($i = 0; $i -lt $migrations.Count; $i++) {
        $from = if ($i -eq 0) { "0" } else { $migrations[$i - 1] }
        $to = $migrations[$i]
        $sqlFile = "$outputPath\$to.sql"

        if (-Not (Test-Path $sqlFile)) {
            Write-Host "Generating script: $from → $to"
            dotnet ef migrations script $from $to `
                --context $context `
                -s $startupProject `
                -p "$projectPath.csproj" `
                -o $sqlFile

            $content = Get-Content $sqlFile -Raw
            $finalScript = if ($from -eq "0") { $preamble + "`r`n" + $content } else { $content }
            $finalScript = $finalScript -replace '\$EF\$', '$$$$'
            $finalScript = $finalScript -replace 'START TRANSACTION;\s*', ''
            $finalScript = $finalScript -replace 'COMMIT;\s*', ''
            Set-Content -Path $sqlFile -Value $finalScript -Encoding UTF8

            Write-Host "Created: $sqlFile"
        } else {
            Write-Host "Exists: $sqlFile — skipping"
        }
    }
}
