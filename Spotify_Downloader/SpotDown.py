import os
import subprocess
import json
from pathlib import Path

# CONFIG
SPOTIFY_URL = "https://open.spotify.com/playlist/3yBGjdjMhpGQohy8vzZTPu?si=398001224ffd4566"  # This URL seems to be a placeholder, you might want to use a real Spotify URL or remove it if not used.
FOLDER_LOCAL = "Playlist_Spotify_Descargada"

def descargar_playlist(enlace):
    os.makedirs(FOLDER_LOCAL, exist_ok=True)
    print(f"🎵 Descargando con SpotDL desde: {enlace}")
    comando = [
        "spotdl",
        enlace,
        "--output", os.path.join(FOLDER_LOCAL, "{artist} - {title}.mp3")
    ]
    subprocess.run(comando)

def main():
    enlace = input("🔗 Introduce un enlace de Spotify o YouTube (deja en blanco para usar el predeterminado): ").strip()
    if not enlace:
        enlace = SPOTIFY_URL
    descargar_playlist(enlace)
    print("🎉 Descarga completada.")

if __name__ == "__main__":
    main()