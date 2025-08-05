# Build Release Script for Port Monitor
param(
    [string]$Version = "1.0.0",
    [string]$OutputDir = "./dist"
)

Write-Host "Building Port Monitor v$Version..." -ForegroundColor Green

# Limpiar directorio de salida
if (Test-Path $OutputDir) {
    Remove-Item $OutputDir -Recurse -Force
}
New-Item -ItemType Directory -Path $OutputDir -Force

# Restaurar dependencias
Write-Host "Restoring dependencies..." -ForegroundColor Yellow
dotnet restore

# Compilar en Release
Write-Host "Building solution..." -ForegroundColor Yellow
dotnet build --configuration Release --no-restore

# Publicar cada proyecto
Write-Host "Publishing GUI..." -ForegroundColor Yellow
dotnet publish PortMonitor.GUI --configuration Release --self-contained true --runtime win-x64 --output "$OutputDir/gui" --no-restore

Write-Host "Publishing Service..." -ForegroundColor Yellow  
dotnet publish PortMonitor.Service --configuration Release --self-contained true --runtime win-x64 --output "$OutputDir/service" --no-restore

Write-Host "Publishing Installer..." -ForegroundColor Yellow
dotnet publish PortMonitor.Installer --configuration Release --self-contained true --runtime win-x64 --output "$OutputDir/installer" --no-restore

# Copiar archivos adicionales
Write-Host "Copying additional files..." -ForegroundColor Yellow
Copy-Item "README.md" "$OutputDir/"
Copy-Item "LICENSE" "$OutputDir/" -ErrorAction SilentlyContinue

# Crear estructura de instalación
Write-Host "Creating installation package..." -ForegroundColor Yellow
$InstallDir = "$OutputDir/install-package"
New-Item -ItemType Directory -Path $InstallDir -Force

# Copiar ejecutables a estructura de instalación
Copy-Item "$OutputDir/gui/*" "$InstallDir/Application/" -Recurse -Force
Copy-Item "$OutputDir/service/*" "$InstallDir/Service/" -Recurse -Force
Copy-Item "$OutputDir/installer/PortMonitor.Installer.exe" "$InstallDir/"

# Crear script de instalación
@'
@echo off
echo Port Monitor Security Application - Installer
echo.
echo Checking for administrator privileges...
net session >nul 2>&1
if %errorLevel% == 0 (
    echo Administrator privileges detected.
    echo Installing Port Monitor...
    PortMonitor.Installer.exe
) else (
    echo This installer requires administrator privileges.
    echo Please run as administrator.
    pause
)
'@ | Out-File "$InstallDir/Install.bat" -Encoding ASCII

# Verificar archivos generados
Write-Host "Verifying build output..." -ForegroundColor Yellow
$RequiredFiles = @(
    "$OutputDir/gui/PortMonitor.GUI.exe",
    "$OutputDir/service/PortMonitor.Service.exe", 
    "$OutputDir/installer/PortMonitor.Installer.exe"
)

$AllFilesPresent = $true
foreach ($file in $RequiredFiles) {
    if (Test-Path $file) {
        Write-Host "✓ $file" -ForegroundColor Green
    } else {
        Write-Host "✗ $file - MISSING" -ForegroundColor Red
        $AllFilesPresent = $false
    }
}

if ($AllFilesPresent) {
    Write-Host "`nBuild completed successfully!" -ForegroundColor Green
    Write-Host "Output directory: $OutputDir" -ForegroundColor Cyan
    Write-Host "Installation package: $InstallDir" -ForegroundColor Cyan
} else {
    Write-Host "`nBuild completed with errors!" -ForegroundColor Red
    exit 1
}
