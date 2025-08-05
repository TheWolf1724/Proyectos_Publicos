# Port Monitor - Aplicación de Monitoreo de Puertos de Seguridad

## Descripción

Port Monitor es una aplicación de seguridad para Windows que monitorea en tiempo real los puertos abiertos por los procesos del sistema, proporciona notificaciones inteligentes y permite controlar el acceso a través del firewall de Windows.

## Características Principales

### 🔍 Monitoreo en Tiempo Real
- Detecta automáticamente puertos TCP/UDP abiertos y cerrados
- Asocia cada puerto con su proceso correspondiente (PID, ejecutable, usuario)
- Monitoreo continuo con intervalo configurable
- Detección de cambios y eventos de red

### 🔔 Notificaciones Inteligentes
- Notificaciones toast nativas de Windows 10/11
- Botones de acción directa: Permitir/Bloquear desde la notificación
- Categorización automática de riesgo (Bajo, Medio, Alto, Crítico)
- Alertas especiales para puertos asociados con malware conocido

### 🛡️ Integración con Firewall
- Control granular de reglas de firewall de Windows
- Reglas específicas por proceso y puerto
- Reglas avanzadas por rango de IP, protocolo, aplicación completa
- Backup automático y restauración de reglas

### 🎯 Inteligencia de Seguridad
- Base de datos de puertos conocidos (IANA + malware)
- Detección automática de patrones sospechosos
- Análisis de riesgo basado en múltiples factores
- Información contextual para cada puerto detectado

### 🖥️ Interfaz de Usuario
- GUI moderna basada en Material Design
- Modo básico para usuarios novatos y modo avanzado para expertos
- Soporte multiidioma (español/inglés)
- Icono en bandeja del sistema con controles rápidos

## Arquitectura del Sistema

```
PortMonitor/
├── PortMonitor.Core/          # Lógica de negocio y modelos
├── PortMonitor.Service/       # Servicio de Windows
├── PortMonitor.GUI/          # Interfaz gráfica WPF
└── PortMonitor.Installer/    # Instalador de la aplicación
```

### Componentes Principales

1. **PortMonitor.Core**: Contiene toda la lógica de negocio
   - `IPortMonitor`: Interfaz para monitoreo de puertos
   - `IFirewallManager`: Gestión del firewall de Windows
   - `INotificationService`: Sistema de notificaciones
   - `IPortCatalog`: Base de datos de información de puertos
   - `IDataRepository`: Persistencia de datos con SQLite

2. **PortMonitor.Service**: Servicio de Windows que ejecuta en segundo plano
   - Monitoreo continuo de puertos
   - Procesamiento de eventos y notificaciones
   - Aplicación automática de reglas de seguridad

3. **PortMonitor.GUI**: Interfaz gráfica de usuario
   - Dashboard con estadísticas en tiempo real
   - Gestión de eventos de puertos
   - Configuración de reglas de firewall
   - Configuración avanzada de la aplicación

4. **PortMonitor.Installer**: Instalador profesional
   - Instalación con privilegios de administrador
   - Configuración automática del servicio
   - Integración con el firewall de Windows

## Requisitos del Sistema

- **Sistema Operativo**: Windows 10/11 (Home, Pro, Enterprise)
- **Framework**: .NET 8.0 Runtime
- **Privilegios**: Administrador para instalación/desinstalación solamente
- **Firewall**: Windows Defender Firewall habilitado

## Instalación

### Instalación Automática (Recomendada)

1. Ejecutar `PortMonitor.Installer.exe` como administrador
2. Seguir el asistente de instalación
3. La aplicación se configurará automáticamente como servicio de Windows

### Instalación Manual

1. Compilar la solución en modo Release
2. Copiar los archivos a `C:\Program Files\PortMonitor\`
3. Instalar el servicio manualmente:
   ```cmd
   sc create "PortMonitor Security Service" binPath="C:\Program Files\PortMonitor\PortMonitor.Service.exe" start=auto
   ```

## Configuración

### Archivo de Configuración Principal
El archivo `appsettings.json` contiene toda la configuración de la aplicación:

```json
{
  "AppConfiguration": {
    "EnableRealTimeMonitoring": true,
    "MonitoringIntervalMs": 1000,
    "EnableToastNotifications": true,
    "RequireConfirmationForCriticalActions": true,
    "Language": "es-ES"
  }
}
```

### Configuración a través de la GUI
- Acceder a Configuración desde la aplicación principal
- Modo básico: configuraciones esenciales
- Modo avanzado: configuraciones detalladas para expertos

## Uso de la Aplicación

### Primera Ejecución
1. La aplicación se inicia automáticamente con Windows
2. Icono aparece en la bandeja del sistema
3. El servicio comienza a monitorear puertos inmediatamente

### Gestión de Notificaciones
- **Permitir**: Crea una regla que permite el tráfico del proceso específico
- **Bloquear**: Crea una regla que bloquea el tráfico del proceso específico
- **Detalles**: Abre la ventana principal con información detallada

### Panel Principal
- **Dashboard**: Resumen de actividad y estadísticas
- **Eventos de Puertos**: Historial completo de eventos detectados
- **Reglas de Firewall**: Gestión de reglas creadas por la aplicación
- **Configuración**: Ajustes de la aplicación

### Filtros y Búsqueda
- Filtrar por nivel de riesgo (Bajo, Medio, Alto, Crítico)
- Filtrar por protocolo (TCP, UDP)
- Búsqueda por nombre de proceso o puerto
- Exportación de datos a CSV/JSON

## Seguridad

### Protección de la Aplicación
- El servicio se reinicia automáticamente si se detiene inesperadamente
- Logs de eventos críticos para auditoría
- Validación de integridad de reglas de firewall

### Niveles de Riesgo
- **Bajo**: Puertos de servicios conocidos y seguros
- **Medio**: Puertos no estándar pero probablemente legítimos
- **Alto**: Puertos asociados con herramientas de hacking o acceso remoto
- **Crítico**: Puertos conocidos por malware o actividad maliciosa

### Base de Datos de Amenazas
- Puertos IANA estándar
- Puertos comúnmente usados por malware
- Troyanos y backdoors conocidos
- Actualización automática de definiciones (opcional)

## Troubleshooting

### Problemas Comunes

**El servicio no se inicia**
- Verificar que se tiene privilegios de administrador
- Comprobar que el firewall de Windows está habilitado
- Revisar los logs en `%APPDATA%\PortMonitor\Logs\`

**Las notificaciones no aparecen**
- Verificar configuración de notificaciones de Windows
- Comprobar que la aplicación está en la lista de aplicaciones permitidas para notificaciones

**El monitoreo no detecta puertos**
- Verificar que no hay conflictos con otros software de seguridad
- Comprobar que el servicio tiene los permisos necesarios

### Logs y Diagnóstico
Los logs se almacenan en:
- Servicio: `%APPDATA%\PortMonitor\Logs\service.log`
- GUI: `%APPDATA%\PortMonitor\Logs\gui.log`
- Base de datos: `%APPDATA%\PortMonitor\PortMonitor.db`

## Desinstalación

### Desinstalación Automática
1. Ir a "Agregar o quitar programas" en Windows
2. Buscar "Port Monitor Security Application"
3. Hacer clic en "Desinstalar"

### Desinstalación Manual
1. Detener el servicio: `sc stop "PortMonitor Security Service"`
2. Eliminar el servicio: `sc delete "PortMonitor Security Service"`
3. Eliminar archivos de `C:\Program Files\PortMonitor\`
4. Limpiar entradas del registro (opcional)

## Desarrollo y Contribución

### Compilación desde Código Fuente
```bash
git clone https://github.com/user/port-monitor.git
cd port-monitor
dotnet restore
dotnet build --configuration Release
```

### Estructura del Proyecto
- Usa .NET 8.0 y WPF para la interfaz
- SQLite para persistencia de datos
- Material Design para el tema visual
- Patrón MVVM con CommunityToolkit.Mvvm

### Testing
```bash
dotnet test
```

## Licencia y Soporte

Esta aplicación está desarrollada para uso educativo y profesional. Para soporte técnico o reportar bugs, contactar al desarrollador.

**Versión**: 1.0.0  
**Última actualización**: Agosto 2025  
**Compatibilidad**: Windows 10/11
