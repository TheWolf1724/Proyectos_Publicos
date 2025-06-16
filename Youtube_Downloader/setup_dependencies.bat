@echo off

REM Verificar si Python está instalado
python --version >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo Python no está instalado. Por favor, instálalo antes de continuar.
    exit /b
)

REM Verificar si el archivo requirements.txt existe
if not exist requirements.txt (
    echo El archivo requirements.txt no existe. Saltando instalación de dependencias de Python.
) else (
    REM Instalar dependencias de Python
    pip install -r requirements.txt
)

REM Verificar si FFmpeg está instalado
ffmpeg -version >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo FFmpeg no está instalado. Descargando e instalando FFmpeg...
    set "FFmpeg_DIR=%~dp0ffmpeg"

    REM Configurar correctamente la URL de FFmpeg
    set "FFmpeg_URL=https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-essentials.zip"

    REM Validar que FFmpeg_URL esté configurado correctamente
    if "%FFmpeg_URL%"=="" (
        echo Error: FFmpeg_URL no está configurado correctamente.
        exit /b
    )

    REM Mostrar el valor de FFmpeg_URL para depuración
    echo URL configurada para FFmpeg: %FFmpeg_URL%

    REM Descargar FFmpeg
    powershell -Command "Invoke-WebRequest -Uri '%FFmpeg_URL%' -OutFile 'ffmpeg.zip'"

    REM Validar si la descarga fue exitosa
    if not exist ffmpeg.zip (
        echo Error al descargar FFmpeg. Por favor, verifica la URL y tu conexión a internet.
        exit /b
    )

    REM Extraer FFmpeg
    powershell -Command "Expand-Archive -Path 'ffmpeg.zip' -DestinationPath '%FFmpeg_DIR%'"

    REM Validar si la extracción fue exitosa
    if not exist "%FFmpeg_DIR%\bin" (
        echo Error al extraer FFmpeg. Por favor, verifica el archivo descargado.
        exit /b
    )

    REM Agregar FFmpeg al PATH
    setx PATH "%FFmpeg_DIR%\bin;%PATH%"

    echo FFmpeg instalado correctamente.
) else (
    echo FFmpeg ya está instalado.
)

REM Confirmar instalación
echo Dependencias instaladas correctamente.
