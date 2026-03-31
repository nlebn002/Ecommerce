param(
    [switch]$Preview
)

$ErrorActionPreference = 'Stop'

# Logistics schema changes are intentionally manual. The service does not apply
# EF Core migrations on startup, so use this script to create reviewed
# migrations and apply them explicitly in local/dev workflows.

function Pause-BeforeExit {
    Write-Host ''
    Read-Host 'Press Enter to close'
}

try {
    $repoRoot = Split-Path -Path $PSScriptRoot -Parent
    $infrastructureProject = Join-Path $repoRoot 'src\Services\Logistics\Ecommerce.LogisticsService.Infrastructure\Ecommerce.LogisticsService.Infrastructure.csproj'
    $startupProject = Join-Path $repoRoot 'src\Services\Logistics\Ecommerce.LogisticsService.Api\Ecommerce.LogisticsService.Api.csproj'
    $migrationsDirectory = Join-Path $repoRoot 'src\Services\Logistics\Ecommerce.LogisticsService.Infrastructure\Migrations'

    if (-not (Test-Path -LiteralPath $infrastructureProject)) {
        throw "Infrastructure project not found: $infrastructureProject"
    }

    if (-not (Test-Path -LiteralPath $startupProject)) {
        throw "Startup project not found: $startupProject"
    }

    if (-not (Test-Path -LiteralPath $migrationsDirectory)) {
        New-Item -ItemType Directory -Path $migrationsDirectory | Out-Null
    }

    $versionPattern = '^\d{14}_logistics_(?<version>\d{3})\.cs$'
    $versions =
        Get-ChildItem -LiteralPath $migrationsDirectory -File |
        Where-Object { $_.Name -notlike '*.Designer.cs' } |
        ForEach-Object {
            if ($_.Name -match $versionPattern) {
                [int]$Matches['version']
            }
        }

    $nextVersion = if ($versions) {
        ($versions | Measure-Object -Maximum).Maximum + 1
    }
    else {
        1
    }

    $migrationName = 'logistics_{0}' -f $nextVersion.ToString('000')
    $arguments = @(
        'ef'
        'migrations'
        'add'
        $migrationName
        '--project'
        $infrastructureProject
        '--startup-project'
        $startupProject
        '--output-dir'
        'Migrations'
    )

    Write-Host "Migration name: $migrationName"
    Write-Host ''
    Write-Host "Command:"
    Write-Host "dotnet $($arguments -join ' ')"
    Write-Host ''

    if ($Preview) {
        Write-Host 'Preview mode. No migration was created.'
        return
    }

    $env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'
    $env:DOTNET_CLI_HOME = Join-Path $repoRoot '.dotnet'

    & dotnet @arguments

    if ($LASTEXITCODE -ne 0) {
        throw "dotnet ef failed with exit code $LASTEXITCODE."
    }

    Write-Host ''
    Write-Host 'Migration created successfully.'
}
catch {
    Write-Host ''
    Write-Host 'Migration creation failed.' -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
}
finally {
    Pause-BeforeExit
}
