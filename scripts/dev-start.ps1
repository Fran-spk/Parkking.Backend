# Levanta API (.NET) + Front (Vite) en ventanas separadas.
# Uso:  .\scripts\dev-start.ps1

param(
    [string]$FrontPath = "C:\Users\judok\OneDrive\Escritorio\Desarrollo\Parkking Frontend\parkking",
    [string]$ApiProfile = "http"
)

$ErrorActionPreference = "Stop"
$Root = Split-Path $PSScriptRoot -Parent
$ApiDir = Join-Path $Root "Parkking.Api"
$ApiCsproj = Join-Path $ApiDir "Parkking.Api.csproj"
$PidFile = Join-Path $PSScriptRoot ".dev-pids.json"

if (-not (Test-Path $ApiCsproj)) {
    Write-Error "No se encontró $ApiCsproj"
}
if (-not (Test-Path $FrontPath)) {
    Write-Error "No se encontró el front en $FrontPath"
}

if (Test-Path $PidFile) {
    Write-Host "Ya hay una sesión en $PidFile. Corré primero .\scripts\dev-stop.ps1" -ForegroundColor Yellow
    exit 1
}

# cmd /k deja la ventana abierta si falla (así ves el error).
# Rutas con espacios van entre comillas dentro del comando.

Write-Host "→ API:  $ApiDir  (profile $ApiProfile)" -ForegroundColor Cyan
$apiCmd = "title Parkking API && dotnet run --project `"$ApiCsproj`" --launch-profile $ApiProfile"
$api = Start-Process -FilePath "cmd.exe" `
    -ArgumentList @("/k", $apiCmd) `
    -WorkingDirectory $ApiDir `
    -PassThru `
    -WindowStyle Normal

Start-Sleep -Seconds 1
if ($api.HasExited) {
    Write-Host "La ventana de la API se cerró al toque. Revisá el error en la consola." -ForegroundColor Red
    exit 1
}

Write-Host "→ Front: $FrontPath" -ForegroundColor Cyan
$frontCmd = "title Parkking Front && npm run dev"
$front = Start-Process -FilePath "cmd.exe" `
    -ArgumentList @("/k", $frontCmd) `
    -WorkingDirectory $FrontPath `
    -PassThru `
    -WindowStyle Normal

@{
    apiPid   = $api.Id
    frontPid = $front.Id
    started  = (Get-Date).ToString("o")
} | ConvertTo-Json | Set-Content -Path $PidFile -Encoding UTF8

Write-Host ""
Write-Host "Listo. Dos ventanas abiertas (no las cierres)." -ForegroundColor Green
Write-Host "  API   → http://localhost:5105"
Write-Host "  Front → http://localhost:5173"
Write-Host ""
Write-Host "La API puede tardar 20-40s en compilar la primera vez."
Write-Host "Para apagar:  .\scripts\dev-stop.ps1"
