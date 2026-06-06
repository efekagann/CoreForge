# CoreForge Setup Script — Windows PowerShell
# Usage: .\setup.ps1 [-Seed] [-SkipDocker] [-SkipMigrations]
param(
    [switch]$Seed,
    [switch]$SkipDocker,
    [switch]$SkipMigrations
)

$ErrorActionPreference = "Stop"

function Write-Step { param($msg) Write-Host "`n==> $msg" -ForegroundColor Cyan }
function Write-OK   { param($msg) Write-Host "    OK: $msg" -ForegroundColor Green }
function Write-Warn { param($msg) Write-Host "    WARN: $msg" -ForegroundColor Yellow }

Write-Host "`nCoreForge Setup`n" -ForegroundColor Magenta

# ── 1. Prerequisites ──────────────────────────────────────────────────────────
Write-Step "Checking prerequisites"

$dotnetVersion = dotnet --version 2>$null
if (-not $dotnetVersion) { throw "dotnet SDK not found. Install .NET 9 SDK from https://dotnet.microsoft.com/download" }
Write-OK ".NET SDK: $dotnetVersion"

if (-not $SkipDocker) {
    $dockerVersion = docker --version 2>$null
    if (-not $dockerVersion) { throw "Docker not found. Install Docker Desktop from https://docker.com" }
    Write-OK "Docker: $dockerVersion"
}

# ── 2. Docker Compose ─────────────────────────────────────────────────────────
if (-not $SkipDocker) {
    Write-Step "Starting Docker containers (PostgreSQL + Redis)"
    docker compose up -d
    Write-OK "Containers started"

    Write-Host "    Waiting for PostgreSQL to be ready..." -ForegroundColor Gray
    $retries = 0
    do {
        Start-Sleep -Seconds 2
        $retries++
        $ready = docker exec coreforge_postgres pg_isready -U postgres 2>$null
    } while (-not ($ready -like "*accepting connections*") -and $retries -lt 15)

    if ($retries -ge 15) { Write-Warn "PostgreSQL may not be ready yet. Continuing anyway." }
    else { Write-OK "PostgreSQL is ready" }
}

# ── 3. Restore packages ───────────────────────────────────────────────────────
Write-Step "Restoring NuGet packages"
dotnet restore
Write-OK "Packages restored"

# ── 4. Local tools ────────────────────────────────────────────────────────────
Write-Step "Restoring local dotnet tools (dotnet-ef 9.x)"
dotnet tool restore
Write-OK "Tools restored"

# ── 5. Build ──────────────────────────────────────────────────────────────────
Write-Step "Building solution"
dotnet build --no-restore -c Release
Write-OK "Build succeeded"

# ── 6. Migrations ─────────────────────────────────────────────────────────────
if (-not $SkipMigrations) {
    Write-Step "Running EF Core migrations"
    Push-Location "src\CoreForge.WebAPI"
    dotnet ef database update --project ..\CoreForge.Infrastructure -- --environment Development
    Pop-Location
    Write-OK "Migrations applied"
}

# ── 7. Seed ───────────────────────────────────────────────────────────────────
if ($Seed) {
    Write-Step "Seeding sample data"
    Push-Location "src\CoreForge.WebAPI"
    dotnet run --no-build -c Release -- --seed
    Pop-Location
    Write-Host ""
    Write-OK "Seed complete. Sample credentials:"
    Write-Host "    SuperAdmin : admin@coreforge.com / Admin@1234!" -ForegroundColor White
    Write-Host "    Acme Admin : admin@acme.com      / Test@1234!" -ForegroundColor White
    Write-Host "    Acme User  : user@acme.com       / Test@1234!" -ForegroundColor White
    Write-Host "    Globex Admin: admin@globex.com   / Test@1234!" -ForegroundColor White
    Write-Host "    Globex User : user@globex.com    / Test@1234!" -ForegroundColor White
}

# ── Done ──────────────────────────────────────────────────────────────────────
Write-Host "`nSetup complete!`n" -ForegroundColor Magenta
Write-Host "Start the API: " -NoNewline
Write-Host "cd src\CoreForge.WebAPI && dotnet run" -ForegroundColor Yellow
Write-Host "Swagger UI:    " -NoNewline
Write-Host "https://localhost:7001/swagger" -ForegroundColor Yellow
