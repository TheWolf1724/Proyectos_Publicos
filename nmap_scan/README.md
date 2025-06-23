# 🔎 Nmap Scan Script

¡Bienvenido a **Nmap Scan Script**! Este proyecto es un sencillo script en Bash que te permite realizar diferentes tipos de escaneos de red utilizando la herramienta Nmap, de forma interactiva y automatizada.

---

## 🚀 ¿Cómo funciona?

1. **Ejecuta el script** como root:
   ```bash
   sudo ./script.sh
   ```
2. **Introduce la IP** del objetivo cuando se te solicite.
3. **Selecciona el tipo de escaneo** que deseas realizar desde el menú.
4. **¡Listo!** El script mostrará los resultados del escaneo en pantalla.

---

## ✨ Características
- Comprobación automática de la instalación de Nmap (lo instala si es necesario).
- Verificación de que la IP objetivo está activa antes de escanear.
- Menú interactivo con diferentes tipos de escaneo:
  - Escaneo rápido y ruidoso
  - Escaneo normal
  - Escaneo silencioso
  - Escaneo de servicios y versiones
  - Escaneo completo
  - Escaneo de protocolos UDP
- Salida de resultados clara y organizada.

---

## 🛠️ Instalación y uso
1. Clona este repositorio o descarga el archivo `script.sh`.
2. Da permisos de ejecución al script:
   ```bash
   chmod +x script.sh
   ```
3. Ejecuta el script como root:
   ```bash
   sudo ./script.sh
   ```

---

## ❓ Preguntas frecuentes
- **¿Necesito ser root?**
  - Sí, es necesario para ejecutar ciertos tipos de escaneo con Nmap.
- **¿Funciona en cualquier sistema?**
  - El script está pensado para sistemas Linux con Bash y gestor de paquetes apt, dnf o pacman.
- **¿Qué pasa si la IP no responde?**
  - El script te pedirá otra IP hasta que introduzcas una activa.

---

## 🤝 Contribuciones
¡Las contribuciones son bienvenidas! Si tienes ideas, mejoras o encuentras errores, abre un issue o haz un pull request.

---

## ⚖️ Licencia
Este proyecto es de código abierto bajo la licencia MIT.

---

## Futuras mejoras
- Añadir soporte para rangos de IPs o dominios.
- Exportar resultados a archivo.
- Mejorar la compatibilidad multiplataforma.
- Interfaz gráfica.

> Creado por TheWolf1724 con ❤️ para la comunidad.
