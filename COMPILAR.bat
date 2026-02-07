@echo off
echo ========================================
echo   ADAScanCenter - Build Installer
echo ========================================
echo.

cd /d "%~dp0"

echo [1/2] Compilando aplicacion...
powershell -ExecutionPolicy Bypass -File build.ps1

echo.
echo [2/2] Abriendo carpeta del instalador...
explorer "installer\installer"

echo.
echo ========================================
echo   Proceso completado
echo ========================================
echo.
echo El instalador esta en: installer\installer\ADAScanCenter_Setup.exe
echo.
pause
