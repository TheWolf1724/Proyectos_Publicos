# Port Monitor - Instrucciones de Uso

## 📋 Descripción General

Port Monitor es una aplicación profesional de monitorización y control de puertos en Windows que te permite:
- **Monitorear en tiempo real** todos los puertos que abren los procesos del sistema
- **Recibir notificaciones** cada vez que se abre un nuevo puerto
- **Permitir o bloquear** comunicaciones desde las notificaciones o la interfaz
- **Gestionar reglas** de firewall de forma intuitiva
- **Identificar amenazas** con inteligencia integrada de seguridad

---

## 🚀 Instalación

### Prerrequisitos
- **Sistema operativo:** Windows 10/11
- **Framework:** .NET 8.0 Runtime
- **Permisos:** Administrador (solo para instalación)

### Proceso de Instalación

1. **Compilar la aplicación:**
   ```powershell
   cd "c:\Users\master\Documents\GitHub\Proyectos_Publicos\Security_Ports\Codigo"
   dotnet build --configuration Release
   ```

2. **Ejecutar como administrador** (primera vez):
   ```powershell
   # Instalar el servicio de Windows
   cd PortMonitor.Installer\bin\Release\net8.0
   .\PortMonitor.Installer.exe install
   ```

3. **Iniciar la aplicación:**
   ```powershell
   # Ejecutar la interfaz gráfica
   cd PortMonitor.GUI\bin\Release\net8.0-windows
   .\PortMonitor.GUI.exe
   ```

---

## 🎯 Uso Básico

### Primera Ejecución

1. **Ejecuta la GUI** desde el acceso directo o `PortMonitor.GUI.exe`
2. La aplicación iniciará automáticamente el **servicio de monitorización**
3. Verás la **interfaz principal** con las siguientes secciones:
   - **Eventos de Puertos:** Historial de puertos detectados
   - **Reglas de Firewall:** Reglas actuales configuradas
   - **Configuración:** Ajustes de la aplicación

### Monitorización en Tiempo Real

1. **El servicio detecta automáticamente** cuando cualquier proceso abre un puerto
2. **Recibirás notificaciones** del sistema (toast notifications) para cada nuevo puerto:
   ```
   🔔 Nuevo Puerto Detectado
   Proceso: chrome.exe (PID: 1234)
   Puerto: 443 (HTTPS)
   IP: 192.168.1.100 → 216.58.194.174
   
   [Permitir]  [Bloquear]
   ```

3. **Haz clic en los botones** de la notificación para tomar acción inmediata

### Gestión desde la Interfaz

#### Pestaña "Eventos de Puertos"
- **Ve el historial completo** de todos los puertos detectados
- **Filtra por proceso, puerto, o IP**
- **Haz clic derecho** en cualquier evento para:
  - Crear regla de bloqueo
  - Crear regla de permiso
  - Ver detalles del proceso
  - Buscar información del puerto

#### Pestaña "Reglas de Firewall"
- **Administra todas tus reglas** de firewall
- **Activa/desactiva reglas** con un clic
- **Edita reglas existentes** haciendo doble clic
- **Elimina reglas** que ya no necesites

#### Pestaña "Configuración"
- **Configura notificaciones:** Activa/desactiva, duración, sonidos
- **Nivel de monitorización:** Básico, Avanzado, Experto
- **Actualizaciones automáticas:** Base de datos de puertos
- **Idioma:** Español/Inglés

---

## 🛡️ Funciones de Seguridad

### Identificación Inteligente de Amenazas

La aplicación incluye una **base de datos integrada** que identifica:

- **Puertos legítimos:** 
  - Puerto 80/443 (HTTP/HTTPS) ✅
  - Puerto 22 (SSH) ✅
  - Puerto 25 (SMTP) ✅

- **Puertos sospechosos:**
  - Puerto 4444 (Troyanos comunes) ⚠️
  - Puerto 1337 (Elite hacker port) ⚠️
  - Puertos altos aleatorios ⚠️

- **Procesos de riesgo:**
  - Ejecutables sin firmar digital ⚠️
  - Procesos desde carpetas temporales ⚠️
  - Conexiones a IPs conocidas como maliciosas ⚠️

### Notificaciones de Riesgo

Cuando se detecta algo sospechoso, verás notificaciones **resaltadas en rojo**:

```
🚨 ¡PUERTO DE ALTO RIESGO DETECTADO!
Proceso: suspicious.exe
Puerto: 4444 (Conocido por troyanos)
RECOMENDACIÓN: BLOQUEAR INMEDIATAMENTE
[BLOQUEAR] [Investigar] [Permitir]
```

---

## ⚙️ Configuración Avanzada

### Modos de Usuario

1. **Modo Básico** (Recomendado para usuarios normales):
   - Solo notificaciones para puertos de riesgo
   - Reglas automáticas para aplicaciones conocidas
   - Interfaz simplificada

2. **Modo Avanzado** (Para usuarios con experiencia):
   - Notificaciones para todos los puertos nuevos
   - Control granular de reglas
   - Información técnica detallada

3. **Modo Experto** (Para administradores de sistemas):
   - Notificaciones de todos los eventos
   - Acceso a logs detallados
   - Configuración de políticas avanzadas

### Personalización de Reglas

Puedes crear reglas personalizadas con diferentes alcances:

- **Por proceso específico:** Solo afecta a un PID particular
- **Por aplicación:** Afecta a todas las instancias de un ejecutable
- **Por puerto:** Afecta a todas las conexiones en un puerto específico
- **Por IP/Rango:** Afecta a conexiones desde/hacia IPs específicas
- **Global:** Afecta a todo el tráfico del sistema

---

## 🔧 Administración del Sistema

### Gestión del Servicio

```powershell
# Verificar estado del servicio
sc query "PortMonitor Service"

# Detener el servicio
sc stop "PortMonitor Service"

# Iniciar el servicio
sc start "PortMonitor Service"

# Reiniciar el servicio
sc stop "PortMonitor Service" && sc start "PortMonitor Service"
```

### Ubicaciones de Archivos

- **Ejecutables:** `%PROGRAMFILES%\PortMonitor\`
- **Base de datos:** `%APPDATA%\PortMonitor\portmonitor.db`
- **Configuración:** `%APPDATA%\PortMonitor\settings.json`
- **Logs:** `%APPDATA%\PortMonitor\Logs\`

### Desinstalación


```powershell
# Ejecutar como administrador
cd "$Env:PROGRAMFILES\PortMonitor"
./PortMonitor.Installer.exe uninstall
```

---

## 🔍 Ejemplos de Uso Típicos


### Escenario 1: Navegación Web Segura
```
1. Abres Chrome → Notificación: "Puerto 443 HTTPS - Permitir"
2. Haces clic en "Permitir" → Se crea regla automática para Chrome
3. Futuras conexiones HTTPS de Chrome se permiten automáticamente
```


### Escenario 2: Detección de Malware
```
1. Un proceso sospechoso abre puerto 4444
2. Notificación ROJA: "¡RIESGO ALTO! Puerto 4444 - Conocido por troyanos"
3. Haces clic en "BLOQUEAR" → Se bloquea inmediatamente y se crea regla
4. El proceso queda aislado de la red
```


### Escenario 3: Desarrollo de Software
```
1. Visual Studio abre puerto 8080 para debug
2. Notificación: "Puerto 8080 HTTP - Desarrollo local detectado"
3. Seleccionas "Permitir temporalmente" → Solo para esta sesión
```


### Escenario 4: Servidor Personal
```
1. Configuras un servidor web en puerto 8080
2. Vas a Reglas → "Nueva Regla"
3. Configuras: Puerto 8080, Acción: Permitir, Scope: Global
4. Todas las conexiones al puerto 8080 se permiten automáticamente
```

---

## 🆘 Solución de Problemas

### Problemas Comunes

#### 1. **Las notificaciones no aparecen**
- Verifica que el servicio esté ejecutándose: `sc query "PortMonitor Service"`
- Revisa la configuración de notificaciones en Windows
- Ejecuta la GUI como administrador una vez

#### 2. **Error de permisos al crear reglas**
- La primera ejecución requiere permisos de administrador
- Algunas reglas avanzadas requieren elevación
- Ejecuta como administrador: `Clic derecho → Ejecutar como administrador`

#### 3. **Base de datos corrupta**
- Detén el servicio: `sc stop "PortMonitor Service"`
- Elimina: `%APPDATA%\PortMonitor\portmonitor.db`
- Reinicia el servicio: la base se recrea automáticamente

#### 4. **Alto uso de CPU**
- Reduce el nivel de monitorización en Configuración
- Activa el filtrado inteligente para reducir eventos
- Considera usar "Modo Básico"

### Logs y Diagnóstico

Los logs se encuentran en `%APPDATA%\PortMonitor\Logs\`:
- `service.log`: Eventos del servicio de monitorización
- `gui.log`: Eventos de la interfaz gráfica  
- `firewall.log`: Cambios en las reglas de firewall
- `errors.log`: Errores y excepciones

### Contacto y Soporte

- **Logs de diagnóstico:** Adjunta los archivos de `%APPDATA%\PortMonitor\Logs\`
- **Información del sistema:** Incluye versión de Windows y .NET
- **Descripción detallada:** Pasos para reproducir el problema

---

## 📚 Documentación Adicional

- **BUILD.md:** Instrucciones de compilación detalladas
- **ARCHITECTURE.md:** Documentación técnica de la arquitectura
- **SECURITY.md:** Detalles sobre la implementación de seguridad
- **API.md:** Documentación de APIs internas (para desarrolladores)

---

## ⚡ Consejos Pro

1. **Usa listas blancas para aplicaciones conocidas:** Crea reglas permisivas para aplicaciones que usas regularmente
2. **Revisa el historial semanalmente:** Busca patrones inusuales en la pestaña de eventos
3. **Mantén actualizada la base de datos:** Activa las actualizaciones automáticas
4. **Usa el modo experto para análisis forense:** Cuando investigues posibles amenazas
5. **Exporta tus reglas:** Guarda backups de tu configuración personalizada

---

¡Protege tu sistema de forma inteligente con Port Monitor! 🛡️
