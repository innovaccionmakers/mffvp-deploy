# === Configuration ===
$startupProject = "src/API/MFFVP.Api/MFFVP.Api.csproj"
$baseOutput = "src/Common/Common.SharedKernel.Infrastructure/Database/Migrations"

# Define modules
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

    # Migrations are in parent of the .csproj folder
    $migrationsPath = "$($projectPath.Substring(0, $projectPath.LastIndexOf('/')))/Database/Migrations"
    $outputPath = "$baseOutput\$folderName"
    New-Item -ItemType Directory -Path $outputPath -Force | Out-Null

    Write-Host "`n🔍 Processing $folderName ($context)"

    # Filter and sort real migration files (excluding .Designer.cs)
    $migrations = Get-ChildItem -Path $migrationsPath -Filter *.cs -ErrorAction SilentlyContinue |
        Where-Object { $_.BaseName -match '^[0-9]{14}_[^\.]+$' } |
        Sort-Object Name |
        Select-Object -ExpandProperty BaseName

    # Ensure always treated as array
    $migrations = @($migrations)

    if ($migrations.Count -eq 0) {
        Write-Host "⚠️  No migrations found for $folderName"
        continue
    }

    # Special case: only one migration
    if ($migrations.Count -eq 1) {
        $to = $migrations[0]
        $sqlFile = "$outputPath\$to.sql"

        if (-Not (Test-Path $sqlFile)) {
            Write-Host "🛠 Generating initial script: 0 → $to"
            dotnet ef migrations script 0 $to `
                --context $context `
                -s $startupProject `
                -p "$projectPath.csproj" `
                -o $sqlFile
            Write-Host "✅ Created: $sqlFile"
        } else {
            Write-Host "⏩ Exists: $sqlFile — skipping"
        }

        continue
    }

    # Generate incremental scripts from one migration to the next
    for ($i = 0; $i -lt $migrations.Count; $i++) {
        $from = if ($i -eq 0) { "0" } else { $migrations[$i - 1] }
        $to = $migrations[$i]
        $sqlFile = "$outputPath\$to.sql"

        if (-Not (Test-Path $sqlFile)) {
            Write-Host "🛠 Generating script: $from → $to"
            dotnet ef migrations script $from $to `
                --context $context `
                -s $startupProject `
                -p "$projectPath.csproj" `
                -o $sqlFile
            Write-Host "✅ Created: $sqlFile"
        } else {
            Write-Host "⏩ Exists: $sqlFile — skipping"
        }
    }
}
