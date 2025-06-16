# Proyecto de Descarga de Videos de YouTube

Esta aplicación permite descargar videos de YouTube de manera sencilla y rápida. La interfaz gráfica de usuario (GUI) facilita la interacción, permitiendo a los usuarios ingresar enlaces de videos y realizar descargas sin complicaciones.

## Estructura del Proyecto

```
youtube_downloader_app
├── src
│   ├── main.py          # Punto de entrada de la aplicación
│   ├── gui.py           # Definición de la interfaz gráfica
│   ├── downloader.py     # Lógica de descarga de videos
│   └── utils
│       └── __init__.py  # Funciones auxiliares
├── requirements.txt      # Dependencias del proyecto
└── README.md             # Documentación del proyecto
```

## Requisitos

Para ejecutar esta aplicación, asegúrate de tener instaladas las siguientes dependencias:

- `pytube`: Para la descarga de videos de YouTube.
- `tkinter`: Para la creación de la interfaz gráfica.

Puedes instalar las dependencias ejecutando:

```
pip install -r requirements.txt
```

## Ejecución

Para iniciar la aplicación, ejecuta el siguiente comando en tu terminal:

```
python src/main.py
```

Esto abrirá la ventana de la aplicación donde podrás ingresar el enlace del video que deseas descargar y hacer clic en el botón de descarga. La barra de progreso se actualizará en tiempo real durante la descarga.

## Contribuciones

Las contribuciones son bienvenidas. Si deseas mejorar la aplicación, siéntete libre de hacer un fork del repositorio y enviar un pull request.

## Licencia

Este proyecto está bajo la Licencia MIT. Consulta el archivo LICENSE para más detalles.