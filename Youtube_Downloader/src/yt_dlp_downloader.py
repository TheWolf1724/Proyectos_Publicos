import subprocess
import os
import tempfile
import imageio_ffmpeg as ffmpeg
import re
import time

def normalize_title(title):
    # Reemplazar caracteres problemáticos con guiones bajos
    return re.sub(r'[\\/:*?"<>|]', '_', title)

def download_video(url, progress_callback, destination_path):
    try:
        # Obtener el título del video usando yt-dlp
        print("Obteniendo título del video...")
        result = subprocess.run(
            ["yt-dlp", "--get-title", url],
            capture_output=True, text=True
        )
        if result.returncode != 0:
            raise Exception(f"Error al obtener el título del video: {result.stderr}")
        video_title = result.stdout.strip()

        # Normalizar el título del video
        video_title = normalize_title(video_title)

        # Ajustar el nombre del archivo final al título original del video
        output_file = os.path.join(destination_path, f"{video_title}.mp4")

        # Verificar si el archivo combinado ya existe basado en el título del video
        if os.path.exists(output_file):
            print(f"El archivo combinado ya existe: {output_file}. Omitiendo descarga.")
            if progress_callback:
                progress_callback(100)  # Actualizar barra de progreso al 100%
            return

        # Obtener el nombre del video para el archivo .txt
        video_name = url.split('=')[-1]  # Extraer el ID del video como nombre provisional
        txt_file = os.path.join(destination_path, f"descargando_{video_name}.txt")

        # Crear el archivo .txt en la carpeta de destino
        with open(txt_file, "w") as f:
            f.write("Descargando...")

        # Ejecutar yt-dlp para descargar audio y video por separado
        print("Iniciando descarga de audio...")
        # Llama al callback para indicar que empieza la descarga de audio
        if progress_callback:
            progress_callback(0, stage='audio')
        # Ajustar el nombre del archivo combinado si ya existe
        if os.path.exists(output_file):
            base, ext = os.path.splitext(output_file)
            output_file = f"{base}_nuevo{ext}"
            print(f"El archivo combinado ya existe, se generará un nuevo archivo: {output_file}")

        # Actualizar progreso proporcional durante la descarga de audio
        yt = subprocess.Popen(
            ["yt-dlp", url, "--format", "bestaudio", "--output", os.path.join(destination_path, "audio.%(ext)s")],
            stdout=subprocess.PIPE,
            stderr=subprocess.PIPE,
            universal_newlines=True
        )
        if progress_callback:
            progress_callback(0, stage='audio', proc=yt)
        for line in yt.stdout:
            print("Audio descarga salida:", line.strip())
            match = re.search(r"(\d+\.\d+)%", line)
            if match and progress_callback:
                porcentaje_real = float(match.group(1))
                progreso_audio = (porcentaje_real / 100) * 30  # Adaptar de 0% a 30%
                progress_callback(progreso_audio)
        yt.wait()
        print("Descarga de audio completada.")

        # Actualizar progreso proporcional durante la descarga de video
        yt_video = subprocess.Popen(
            ["yt-dlp", url, "--format", "bestvideo", "--output", os.path.join(destination_path, "video.%(ext)s")],
            stdout=subprocess.PIPE,
            stderr=subprocess.PIPE,
            universal_newlines=True
        )
        if progress_callback:
            progress_callback(30, stage='video', proc=yt_video)
        for line in yt_video.stdout:
            print("Video descarga salida:", line.strip())
            match = re.search(r"(\d+\.\d+)%", line)
            if match and progress_callback:
                porcentaje_real = float(match.group(1))
                progreso_video = 30 + (porcentaje_real / 100) * 35  # Adaptar de 30% a 65%
                progress_callback(progreso_video)
        yt_video.wait()
        print("Descarga de video completada.")

        # Verificar si el archivo combinado ya existe
        if os.path.exists(output_file):
            print(f"El archivo combinado ya existe, eliminándolo: {output_file}")
            os.remove(output_file)

        # Actualizar progreso gradualmente durante la combinación de archivos
        progreso_combinacion = 65
        if progress_callback:
            for i in range(progreso_combinacion, 81):
                progress_callback(i)  # Incrementar del 65% al 80%
                time.sleep(0.1)  # Simular progreso

        # Buscar dinámicamente los archivos descargados por nombre y extensión
        audio_file = None
        video_file = None
        for file in os.listdir(destination_path):
            if "audio" in file and (file.endswith(".mp4") or file.endswith(".webm")):
                audio_file = os.path.join(destination_path, file)
            elif "video" in file and (file.endswith(".mp4") or file.endswith(".webm")):
                video_file = os.path.join(destination_path, file)

        # Imprimir los nombres de los archivos detectados para depuración
        print(f"Archivo de audio detectado: {audio_file}")
        print(f"Archivo de video detectado: {video_file}")

        # Validar que los archivos se hayan detectado correctamente
        if not audio_file or not video_file:
            raise FileNotFoundError("No se encontraron los archivos de audio o video descargados.")

        # Normalizar las rutas de los archivos
        audio_file = os.path.normpath(audio_file)
        video_file = os.path.normpath(video_file)
        output_file = os.path.normpath(output_file)

        print(f"Archivo de audio normalizado: {audio_file}")
        print(f"Archivo de video normalizado: {video_file}")

        combine_audio_video(video_file, audio_file, output_file, ffmpeg_callback=lambda proc: progress_callback(80, stage='ffmpeg', proc=proc) if progress_callback else None)

        # Verificar si el archivo combinado se generó correctamente
        if not os.path.exists(output_file):
            raise FileNotFoundError(f"El archivo combinado no se generó: {output_file}")

        print("Archivo combinado generado correctamente:", output_file)

        # Actualizar barra de progreso al 100% después de la combinación
        if progress_callback:
            progress_callback(100)

        # Eliminar archivos separados
        os.remove(video_file)
        os.remove(audio_file)

        # Eliminar el archivo .txt al finalizar la descarga
        if os.path.exists(txt_file):
            os.remove(txt_file)

    except Exception as e:
        print(f"Error al descargar el video: {e}")
        # Asegurarse de eliminar el archivo .txt en caso de error
        if os.path.exists(txt_file):
            os.remove(txt_file)

def combine_audio_video(video_path, audio_path, output_path, ffmpeg_callback=None):
    try:
        print("Iniciando combinación de audio y video...")
        ffmpeg_cmd = [
            ffmpeg.get_ffmpeg_exe(),
            "-i", video_path,
            "-i", audio_path,
            "-c:v", "copy",
            "-c:a", "aac",
            output_path
        ]
        proc = subprocess.Popen(ffmpeg_cmd, stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True)
        if ffmpeg_callback:
            ffmpeg_callback(proc)
        proc.communicate()
        if proc.returncode != 0:
            print("Error en ffmpeg")
            raise Exception("Error al combinar audio y video.")
        print("Combinación completada.")
    except Exception as e:
        print(f"Error al combinar audio y video: {e}")
        raise