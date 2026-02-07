# Script de compilación y empaquetado de ADAScanCenter
# Este script compila la aplicación y crea el instalador

Write-Host "=== ADAScanCenter - Build & Package ===" -ForegroundColor Cyan
Write-Host ""

# 0. Generar icono
Write-Host "[0/5] Generando icono de aplicación..." -ForegroundColor Yellow
& ".\generate-icon.ps1"

# 1. Limpiar builds anteriores
Write-Host "[1/5] Limpiando builds anteriores..." -ForegroundColor Yellow
if (Test-Path "bin") { Remove-Item -Path "bin" -Recurse -Force }
if (Test-Path "obj") { Remove-Item -Path "obj" -Recurse -Force }
if (Test-Path "installer\Output") { Remove-Item -Path "installer\Output" -Recurse -Force }

# 2. Restaurar dependencias
Write-Host "[2/5] Restaurando dependencias..." -ForegroundColor Yellow
dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error al restaurar dependencias" -ForegroundColor Red
    exit 1
}

# 3. Publicar aplicación
Write-Host "[3/5] Compilando aplicación (Release)..." -ForegroundColor Yellow
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:PublishReadyToRun=true
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error al compilar la aplicación" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Compilación exitosa!" -ForegroundColor Green
Write-Host "Ejecutable ubicado en: bin\Release\net10.0-windows\win-x64\publish\ADAScanCenter.exe" -ForegroundColor Green
Write-Host ""

# 4. Crear instalador con Inno Setup (si está instalado)
Write-Host "[4/5] Creando instalador..." -ForegroundColor Yellow

$innoSetupPath = "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
if (Test-Path $innoSetupPath) {
    & $innoSetupPath "installer\setup.iss"
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "¡Instalador creado exitosamente!" -ForegroundColor Green
        Write-Host "Ubicación: installer\Output\ADAScanCenter_Setup.exe" -ForegroundColor Green
    }
    else {
        Write-Host "Error al crear el instalador" -ForegroundColor Red
    }
}
else {
    Write-Host ""
    Write-Host "AVISO: Inno Setup no está instalado" -ForegroundColor Yellow
    Write-Host "Para crear el instalador:" -ForegroundColor Yellow
    Write-Host "1. Descarga Inno Setup desde: https://jrsoftware.org/isdl.php" -ForegroundColor Cyan
    Write-Host "2. Instálalo en la ruta por defecto" -ForegroundColor Cyan
    Write-Host "3. Ejecuta este script nuevamente" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Mientras tanto, puedes usar el ejecutable directamente desde:" -ForegroundColor Yellow
    Write-Host "bin\Release\net10.0-windows\win-x64\publish\ADAScanCenter.exe" -ForegroundColor Cyan
}

Write-Host ""
Write-Host "=== Proceso completado ===" -ForegroundColor Cyan
