# Prompt completo para IA: Aplicación de monitorización y control de puertos en Windows

---

## Descripción general

Desarrolla una aplicación para Windows que monitorice en tiempo real los puertos abiertos por los procesos del sistema, notifique al usuario de cada nuevo puerto detectado, y permita desde la notificación o desde una interfaz gráfica (GUI) permitir o bloquear la comunicación de esos procesos, integrándose con el firewall de Windows. La app debe ser segura, fácil de usar para usuarios básicos, pero con opciones avanzadas para usuarios expertos, y con funcionalidades de inteligencia para categorizar puertos y alertar de riesgos potenciales.

---

## Requisitos detallados

### 1. Plataforma y lenguaje
- **Sistema operativo:** Windows (10/11)
- **Lenguaje de programación:** Elige el más adecuado para máxima integración con Windows, elevada seguridad, acceso a APIs nativas, notificaciones de sistema y gestión avanzada de firewall. Prioriza C/C++ o C# (.NET), pero puedes usar wrappers/bindings si lo justificas.

### 2. Instalación y permisos
- **Instalador profesional**: requiere privilegios de administrador para instalar y desinstalar.
- **Uso diario:** no exige privilegios de administrador para ejecutar ni modificar reglas estándar.

### 3. Monitorización en tiempo real
- Detecta puertos abiertos/cerrados en tiempo real, asociando cada evento a:
  - PID
  - Ruta y nombre del ejecutable
  - Usuario propietario del proceso
  - Dirección IP y puerto (local y remoto)
  - Protocolo (TCP/UDP)
- Detecta cambios y diferencia entre procesos nuevos y existentes.

### 4. Notificaciones del sistema
- Cuando un proceso abre un nuevo puerto, muestra una notificación del sistema (“toast notification”) independiente por cada puerto:
  - Muestra: nombre del proceso, puerto, IP, protocolo, ruta ejecutable.
  - Incluye 2 botones: **Permitir** y **Bloquear**.
  - Al pulsar, aplica la acción solo a ese proceso/ejecutable y ese puerto.
- Si varios puertos se abren a la vez, muestra varias notificaciones.
- Si el puerto es conocido como potencialmente peligroso o típico de malware, muestra advertencia resaltada (“bloqueo recomendado” o “potencial riesgo”).

### 5. Gestión de reglas y acciones
- **Por defecto:** las reglas afectan solo al proceso específico (PID/ruta exe) y al puerto concreto.
- **Ventana principal (GUI):**
  - Lista historial de todos los eventos detectados (aperturas/cierres de puertos, acciones realizadas).
  - Permite editar cada regla: ampliar alcance (a todo el puerto en el sistema, a toda la app, o reglas personalizadas, incluyendo IP/rango).
  - Permite eliminar reglas.
  - Opción para restaurar reglas por defecto o limpiar todas.
  - Vista sencilla y vista avanzada para usuarios expertos.
- **Persistencia:** las reglas y el historial deben guardarse entre sesiones (archivo seguro y/o registro de Windows).

### 6. Inteligencia y catálogo de puertos
- Base de datos local de puertos conocidos (IANA y malware comunes).
- Al detectar un puerto, intenta identificar su uso habitual y muestra información amigable (ejemplo: “Puerto 80 – HTTP”, “Puerto 4444 – conocido por troyanos”).
- Si se detecta un patrón típico de malware, muestra advertencia, pero la decisión final siempre es del usuario.

### 7. Seguridad y protección
- Requiere privilegios de administrador solo para instalar/desinstalar.
- La manipulación de reglas críticas (borrar todo, desactivar protección, etc.) puede requerir confirmación o autenticación Windows.
- La app debe estar protegida frente a intentos de deshabilitación por malware (ejemplo: reinicia el servicio si se para inesperadamente, logs de eventos críticos).

### 8. Experiencia de usuario
- GUI sencilla, moderna, intuitiva y responsive.
- Traducción al menos inglés/español (multilenguaje).
- Manual de ayuda accesible desde la app.
- Buen manejo de errores: mensajes claros, logs de eventos y errores.

### 9. Otros
- Soporte para auto-update seguro (opcional).
- Código modular y fácilmente ampliable.
- Documentar todos los puntos importantes y decisiones técnicas.

---

## Checklist de desarrollo y validación

- [ ] Selección de lenguaje y frameworks óptimos para Windows (notificaciones, firewall, GUI)
- [ ] Instalador profesional con privilegios de administrador
- [ ] Servicio/daemon para monitorizar puertos en tiempo real
- [ ] Asociación correcta de puerto <-> proceso (PID, exe, usuario)
- [ ] Notificaciones del sistema independientes por cada puerto
- [ ] Botones de acción en notificación: Permitir/Bloquear (acción inmediata)
- [ ] Historial de eventos y acciones accesible desde la GUI
- [ ] Edición y eliminación de reglas en la GUI
- [ ] Modo avanzado: reglas por rango de IP, protocolo, puerto global, app completa
- [ ] Persistencia de reglas e historial entre sesiones
- [ ] Catálogo inteligente de puertos, identificación y advertencias de riesgo
- [ ] Advertencia resaltada si puerto/proceso es potencialmente malicioso
- [ ] Protección frente a desactivación o manipulación por malware
- [ ] GUI moderna, sencilla, responsive, multi-idioma (al menos inglés/español)
- [ ] Mensajes de error y logs claros y accesibles
- [ ] Manual de usuario accesible desde la app
- [ ] (Opcional) Actualización automática segura
- [ ] Código modular y bien documentado
- [ ] Testeo de todas las funcionalidades principales y casos de uso

---

## Notas para el desarrollador IA

- Justifica cualquier limitación técnica o desviación del prompt original.
- Prioriza la integración nativa y la seguridad.
- Si alguna funcionalidad crítica requiere dependencias externas, indícalo y documenta bien su uso e instalación.
- La app debe funcionar bien en Windows 10 y 11, tanto en sistemas Home como Pro.
- El código debe ser entregado correctamente organizado, comentado y con instrucciones de compilación o empaquetado.

---

**¡Desarrolla la aplicación siguiendo este prompt y checklist para asegurar un producto robusto, seguro y fácil de usar!**