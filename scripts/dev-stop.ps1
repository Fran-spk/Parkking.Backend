# Apaga API + Front levantados con dev-start.ps1 (y limpia puertos típicos).
# Uso:  .\scripts\dev-stop.ps1 .\scripts\dev-start.ps1

$ErrorActionPreference = "Continue"
$PidFile = Join-Path $PSScriptRoot ".dev-pids.json"
$Ports = @(5105, 7069, 5173)

function Stop-Tree([int]$ProcessId) {
    if ($ProcessId -le 0) { return }
    try {
        # Mata el proceso y sus hijos (dotnet/npm suelen spawnear procesos)
        & taskkill /PID $ProcessId /T /F 2>$null | Out-Null
        Write-Host "  Detenido PID $ProcessId" -ForegroundColor Green
    } catch {
        Write-Host "  PID $ProcessId ya no estaba corriendo" -ForegroundColor DarkGray
    }
}

function Stop-Port([int]$Port) {
    $conns = Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue
    foreach ($c in $conns) {
        $owner = $c.OwningProcess
        if ($owner) {
            Write-Host "  Puerto $Port → PID $owner" -ForegroundColor Yellow
            Stop-Tree $owner
        }
    }
}

Write-Host "Apagando Parkking..." -ForegroundColor Cyan

if (Test-Path $PidFile) {
    $info = Get-Content $PidFile -Raw | ConvertFrom-Json
    if ($info.apiPid)   { Stop-Tree ([int]$info.apiPid) }
    if ($info.frontPid) { Stop-Tree ([int]$info.frontPid) }
    Remove-Item $PidFile -Force -ErrorAction SilentlyContinue
} else {
    Write-Host "  No hay .dev-pids.json; limpio por puertos..." -ForegroundColor DarkGray
}

foreach ($p in $Ports) { Stop-Port $p }

Write-Host "Listo." -ForegroundColor Green
