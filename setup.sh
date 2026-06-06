#!/usr/bin/env bash
# CoreForge Setup Script — Linux / macOS
# Usage: ./setup.sh [--seed] [--skip-docker] [--skip-migrations]
set -euo pipefail

SEED=false
SKIP_DOCKER=false
SKIP_MIGRATIONS=false

for arg in "$@"; do
  case $arg in
    --seed)             SEED=true ;;
    --skip-docker)      SKIP_DOCKER=true ;;
    --skip-migrations)  SKIP_MIGRATIONS=true ;;
  esac
done

step()  { echo; echo "==> $1"; }
ok()    { echo "    OK: $1"; }
warn()  { echo "    WARN: $1"; }

echo ""
echo "CoreForge Setup"
echo ""

# ── 1. Prerequisites ──────────────────────────────────────────────────────────
step "Checking prerequisites"

if ! command -v dotnet &>/dev/null; then
  echo "ERROR: dotnet SDK not found. Install .NET 9 SDK from https://dotnet.microsoft.com/download"
  exit 1
fi
ok ".NET SDK: $(dotnet --version)"

if [ "$SKIP_DOCKER" = false ]; then
  if ! command -v docker &>/dev/null; then
    echo "ERROR: Docker not found. Install Docker from https://docker.com"
    exit 1
  fi
  ok "Docker: $(docker --version)"
fi

# ── 2. Docker Compose ─────────────────────────────────────────────────────────
if [ "$SKIP_DOCKER" = false ]; then
  step "Starting Docker containers (PostgreSQL + Redis)"
  docker compose up -d
  ok "Containers started"

  echo "    Waiting for PostgreSQL to be ready..."
  retries=0
  until docker exec coreforge_postgres pg_isready -U postgres &>/dev/null || [ $retries -ge 15 ]; do
    sleep 2
    retries=$((retries + 1))
  done

  if [ $retries -ge 15 ]; then
    warn "PostgreSQL may not be ready yet. Continuing anyway."
  else
    ok "PostgreSQL is ready"
  fi
fi

# ── 3. Restore packages ───────────────────────────────────────────────────────
step "Restoring NuGet packages"
dotnet restore
ok "Packages restored"

# ── 4. Local tools ────────────────────────────────────────────────────────────
step "Restoring local dotnet tools (dotnet-ef 9.x)"
dotnet tool restore
ok "Tools restored"

# ── 5. Build ──────────────────────────────────────────────────────────────────
step "Building solution"
dotnet build --no-restore -c Release
ok "Build succeeded"

# ── 6. Migrations ─────────────────────────────────────────────────────────────
if [ "$SKIP_MIGRATIONS" = false ]; then
  step "Running EF Core migrations"
  pushd src/CoreForge.WebAPI > /dev/null
  dotnet ef database update --project ../CoreForge.Infrastructure -- --environment Development
  popd > /dev/null
  ok "Migrations applied"
fi

# ── 7. Seed ───────────────────────────────────────────────────────────────────
if [ "$SEED" = true ]; then
  step "Seeding sample data"
  pushd src/CoreForge.WebAPI > /dev/null
  dotnet run --no-build -c Release -- --seed
  popd > /dev/null
  echo ""
  ok "Seed complete. Sample credentials:"
  echo "    SuperAdmin : admin@coreforge.com / Admin@1234!"
  echo "    Acme Admin : admin@acme.com      / Test@1234!"
  echo "    Acme User  : user@acme.com       / Test@1234!"
  echo "    Globex Admin: admin@globex.com   / Test@1234!"
  echo "    Globex User : user@globex.com    / Test@1234!"
fi

# ── Done ──────────────────────────────────────────────────────────────────────
echo ""
echo "Setup complete!"
echo ""
echo "Start the API:  cd src/CoreForge.WebAPI && dotnet run"
echo "Swagger UI:     https://localhost:7001/swagger"
