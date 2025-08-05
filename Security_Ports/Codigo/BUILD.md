# Build Scripts for Port Monitor

## Desarrollo Local

### Prerrequisitos
- Visual Studio 2022 o VS Code con C# Extension
- .NET 8.0 SDK
- Windows 10/11
- PowerShell 5.1 o superior

### Configuración del Entorno de Desarrollo

1. **Clonar el repositorio:**
```powershell
git clone https://github.com/user/port-monitor.git
cd port-monitor
```

2. **Restaurar dependencias:**
```powershell
dotnet restore
```

3. **Configurar base de datos (primera vez):**
```powershell
# El servicio creará automáticamente la base de datos SQLite en la primera ejecución
# Ubicación: %APPDATA%\PortMonitor\PortMonitor.db
```

### Comandos de Compilación

#### Compilación en modo Debug
```powershell
# Compilar toda la solución
dotnet build

# Compilar proyecto específico
dotnet build PortMonitor.Core
dotnet build PortMonitor.Service  
dotnet build PortMonitor.GUI
dotnet build PortMonitor.Installer
```

#### Compilación en modo Release
```powershell
# Compilar para distribución
dotnet build --configuration Release

# Compilar con optimizaciones
dotnet build --configuration Release --verbosity minimal
```

#### Publicación para Distribución
```powershell
# Publicar GUI como ejecutable autocontenido
dotnet publish PortMonitor.GUI --configuration Release --self-contained true --runtime win-x64 --output "./dist/gui"

# Publicar Service como ejecutable autocontenido  
dotnet publish PortMonitor.Service --configuration Release --self-contained true --runtime win-x64 --output "./dist/service"

# Publicar Installer
dotnet publish PortMonitor.Installer --configuration Release --self-contained true --runtime win-x64 --output "./dist/installer"
```

### Ejecutar en Modo Desarrollo

#### Ejecutar GUI
```powershell
cd PortMonitor.GUI
dotnet run
```

#### Ejecutar Service (requiere privilegios de administrador)
```powershell
# Abrir PowerShell como administrador
cd PortMonitor.Service
dotnet run
```

#### Ejecutar Installer (requiere privilegios de administrador)
```powershell
# Abrir PowerShell como administrador
cd PortMonitor.Installer
dotnet run
```

## Compilación para Producción

### Script de Build Completo
```powershell
# build-release.ps1
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
Copy-Item "LICENSE" "$OutputDir/"

Write-Host "Build completed successfully!" -ForegroundColor Green
Write-Host "Output directory: $OutputDir" -ForegroundColor Cyan
```

### Ejecutar Build de Producción
```powershell
# Hacer el script ejecutable y correrlo
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
.\build-release.ps1 -Version "1.0.0" -OutputDir "./dist"
```

## Testing

### Ejecutar Tests Unitarios
```powershell
# Ejecutar todos los tests
dotnet test

# Ejecutar tests con cobertura
dotnet test --collect:"XPlat Code Coverage"

# Ejecutar tests específicos
dotnet test --filter "TestCategory=Unit"
dotnet test --filter "TestCategory=Integration"
```

### Tests de Integración (requieren privilegios de administrador)
```powershell
# Abrir PowerShell como administrador
dotnet test --filter "TestCategory=Integration"
```

## Distribución

### Crear Paquete de Instalación
```powershell
# Después del build de producción, crear estructura de instalación
$InstallDir = "./install-package"
New-Item -ItemType Directory -Path $InstallDir -Force

# Copiar ejecutables
Copy-Item "./dist/gui/*" "$InstallDir/Application/" -Recurse -Force
Copy-Item "./dist/service/*" "$InstallDir/Service/" -Recurse -Force
Copy-Item "./dist/installer/PortMonitor.Installer.exe" "$InstallDir/"

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
```

### Verificación Post-Build
```powershell
# Verificar que todos los ejecutables están presentes
$RequiredFiles = @(
    "./dist/gui/PortMonitor.GUI.exe",
    "./dist/service/PortMonitor.Service.exe", 
    "./dist/installer/PortMonitor.Installer.exe"
)

foreach ($file in $RequiredFiles) {
    if (Test-Path $file) {
        Write-Host "✓ $file" -ForegroundColor Green
    } else {
        Write-Host "✗ $file - MISSING" -ForegroundColor Red
    }
}

# Verificar dependencias
Write-Host "`nChecking dependencies..." -ForegroundColor Yellow
Get-ChildItem "./dist" -Recurse -Include "*.dll" | Select-Object Name, Directory | Format-Table
```

## Troubleshooting de Build

### Problemas Comunes

**Error: No se puede encontrar el SDK de .NET**
```powershell
# Verificar instalación de .NET
dotnet --version
dotnet --list-sdks

# Si no está instalado, descargar desde:
# https://dotnet.microsoft.com/download/dotnet/8.0
```

**Error: Permisos insuficientes**
```powershell
# Para componentes que requieren privilegios de administrador:
# 1. Abrir PowerShell como administrador
# 2. Navegar al directorio del proyecto
# 3. Ejecutar los comandos de build
```

**Error: Dependencias faltantes**
```powershell
# Limpiar y restaurar
dotnet clean
dotnet restore --force
dotnet build
```

**Error: SQLite no funciona**
```powershell
# Verificar que está incluido Microsoft.Data.Sqlite
dotnet list package | Select-String "Sqlite"

# Si no está, agregarlo:
dotnet add package Microsoft.Data.Sqlite
```

### Logs de Build
Los logs de compilación se guardan automáticamente en:
- `./build-logs/` (si se usa el script de build)
- Logs de MSBuild en `%TEMP%/MSBuildLogs/`

### Variables de Entorno Útiles
```powershell
# Para debugging de build
$env:DOTNET_CLI_TELEMETRY_OPTOUT = "1"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1" 
$env:MSBUILD_VERBOSITY = "normal"
```
