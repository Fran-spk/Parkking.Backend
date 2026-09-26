# Completa tarifas faltantes (tipo × categoría × todas las periodicidades).
# Uso: .\scripts\seed-tarifas-todas.ps1
# Requiere API apagada O usa SQL directo vía connection string de appsettings.

$ErrorActionPreference = "Stop"
$Root = Split-Path $PSScriptRoot -Parent
$Appsettings = Join-Path $Root "Parkking.Api\appsettings.json"

if (-not (Test-Path $Appsettings)) {
    Write-Error "No se encontró $Appsettings"
}

$json = Get-Content $Appsettings -Raw | ConvertFrom-Json
$cs = $json.ConnectionStrings.DefaultConnection
if (-not $cs) { Write-Error "ConnectionStrings:DefaultConnection vacío" }

# Parse simple Host=;Port=;Database=;Username=;Password=
function Get-CsValue([string]$conn, [string]$key) {
    if ($conn -match "(?i)(?:^|;)\s*$key\s*=\s*([^;]+)") { return $Matches[1].Trim() }
    return $null
}

$hostName = Get-CsValue $cs "Host"
$port = Get-CsValue $cs "Port"
if (-not $port) { $port = "5432" }
$db = Get-CsValue $cs "Database"
$user = Get-CsValue $cs "Username"
$pass = Get-CsValue $cs "Password"

$psqlCandidates = @(
    "C:\Program Files\PostgreSQL\17\bin\psql.exe",
    "C:\Program Files\PostgreSQL\16\bin\psql.exe",
    "C:\Program Files\PostgreSQL\15\bin\psql.exe",
    "C:\Program Files\PostgreSQL\14\bin\psql.exe"
)
$psql = $psqlCandidates | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $psql) {
    Write-Error "No se encontró psql.exe. Corré la migración EF o instalá PostgreSQL client."
}

$sql = @'
INSERT INTO "TarifasMensuales"
    ("EstacionamientoId", "TipoVehiculoId", "CategoriaCocheraId", "PeriodicidadCobro", "Precio", "FechaHoraActualizacion")
SELECT
    c."EstacionamientoId",
    c."TipoVehiculoId",
    c."CategoriaCocheraId",
    p.per,
    GREATEST(1, ROUND(c.precio_mensual * p.factor)),
    NOW() AT TIME ZONE 'UTC'
FROM (
    SELECT
        e."EstacionamientoId",
        tv."TipoVehiculoId",
        cc."CategoriaCocheraId",
        COALESCE((
            SELECT t."Precio"
            FROM "TarifasMensuales" t
            WHERE t."EstacionamientoId" = e."EstacionamientoId"
              AND t."TipoVehiculoId" = tv."TipoVehiculoId"
              AND t."CategoriaCocheraId" = cc."CategoriaCocheraId"
              AND t."PeriodicidadCobro" = 0
            ORDER BY t."FechaHoraActualizacion" DESC
            LIMIT 1
        ), 50000) AS precio_mensual
    FROM "Estacionamientos" e
    INNER JOIN "TiposVehiculo" tv
        ON tv."EstacionamientoId" = e."EstacionamientoId" AND tv."Activo" = TRUE
    INNER JOIN "CategoriasCochera" cc
        ON cc."EstacionamientoId" = e."EstacionamientoId" AND cc."Activo" = TRUE
) c
CROSS JOIN (
    VALUES
        (0, 1.0),
        (1, 0.5),
        (2, 2.0),
        (3, 3.0),
        (4, 6.0),
        (5, 12.0)
) AS p(per, factor)
WHERE NOT EXISTS (
    SELECT 1
    FROM "TarifasMensuales" x
    WHERE x."EstacionamientoId" = c."EstacionamientoId"
      AND x."TipoVehiculoId" = c."TipoVehiculoId"
      AND x."CategoriaCocheraId" = c."CategoriaCocheraId"
      AND x."PeriodicidadCobro" = p.per
);
'@

$tmp = Join-Path $env:TEMP "parkking-seed-tarifas.sql"
Set-Content -Path $tmp -Value $sql -Encoding UTF8

Write-Host "→ Sembrando tarifas en $db @ ${hostName}:$port ..." -ForegroundColor Cyan
$env:PGPASSWORD = $pass
& $psql -h $hostName -p $port -U $user -d $db -v ON_ERROR_STOP=1 -f $tmp
Remove-Item $tmp -Force -ErrorAction SilentlyContinue

Write-Host "Listo. Tarifas completadas para todas las periodicidades." -ForegroundColor Green
