import tkinter as tk
from tkinter import ttk
from yt_dlp_downloader import download_video
import os
from tkinter import filedialog
import threading
import tkinter.font as tkFont


class App:
    def __init__(self, root):
        self.root = root
        self.root.title("YouTube Downloader")
        self.root.geometry("500x350")
        self.root.resizable(False, False)
        self.root.configure(bg="#f0f0f0")

        # Fuente personalizada
        font_title = tkFont.Font(family="Arial", size=14, weight="bold")
        font_label = tkFont.Font(family="Arial", size=12)
        font_button = tkFont.Font(family="Arial", size=10, weight="bold")

        # Ruta de descarga por defecto
        self.destination_path = os.path.expanduser("~/Descargas")

        # Selección de ruta de descarga
        tk.Label(root, text="Ruta de descarga:", font=font_label, bg="#f0f0f0").pack(pady=5)
        self.path_label = tk.Label(root, text=self.destination_path, font=font_label, fg="blue", bg="#f0f0f0")
        self.path_label.pack(pady=5)
        self.path_button = tk.Button(root, text="Cambiar ruta", command=self.select_destination, font=font_button, bg="#0078D7", fg="white")
        self.path_button.pack(pady=5)

        # Campo para ingresar la URL
        tk.Label(root, text="Enlace del video:", font=font_label, bg="#f0f0f0").pack(pady=5)
        self.url_entry = tk.Entry(root, width=50, font=font_label)
        self.url_entry.pack(pady=5)

        # Botón para iniciar la descarga
        self.download_button = tk.Button(root, text="Descargar", command=self.start_download, font=font_button, bg="#0078D7", fg="white")
        self.download_button.pack(pady=10)

        # Botón para cancelar la descarga
        self.cancel_button = tk.Button(root, text="Cancelar", command=self.cancel_download, font=font_button, bg="red", fg="white")
        self.cancel_button.pack_forget()  # No mostrar al inicio
        self.cancel_button.config(state=tk.DISABLED)

        # Barra de progreso (oculta inicialmente)
        self.progress = ttk.Progressbar(root, orient="horizontal", length=400, mode="determinate")
        self.progress_label = tk.Label(root, text="Progreso: 0%", font=font_label, bg="#f0f0f0")

        # Variable para controlar la cancelación
        self.cancel_requested = False
        self.yt_proc_audio = None
        self.yt_proc_video = None
        self.ffmpeg_proc = None

    def select_destination(self):
        path = filedialog.askdirectory()
        if path:
            self.destination_path = path
            self.path_label.config(text=self.destination_path)

    def start_download(self):
        url = self.url_entry.get()
        if not url:
            self.show_error("Por favor, ingresa un enlace válido.")
            return

        if not self.destination_path:
            self.show_error("Por favor, selecciona una carpeta de destino.")
            return

        self.download_button.config(state=tk.DISABLED)
        self.cancel_button.pack_forget()  # Asegura que no se muestre
        self.cancel_button.config(state=tk.DISABLED)
        self.cancel_requested = False
        self.progress.pack(pady=10)
        self.progress_label.pack(pady=5)

        # Ejecutar la descarga en un hilo separado
        threading.Thread(target=self.download_thread, args=(url,)).start()

    def on_audio_download_start(self):
        # Llama a esto desde tu función download_video cuando comience la descarga del audio
        self.cancel_button.config(state=tk.NORMAL)
        self.cancel_button.pack(pady=5)

    def cancel_download(self):
        self.cancel_requested = True
        self.cancel_button.config(state=tk.DISABLED)
        self.download_button.config(state=tk.NORMAL)
        self.progress_label.config(text="Descarga cancelada", fg="red")
        # Terminar procesos yt-dlp y ffmpeg si existen
        if self.yt_proc_audio:
            try:
                self.yt_proc_audio.terminate()
            except Exception:
                pass
        if self.yt_proc_video:
            try:
                self.yt_proc_video.terminate()
            except Exception:
                pass
        if self.ffmpeg_proc:
            try:
                self.ffmpeg_proc.terminate()
            except Exception:
                pass
        self._terminate_download_processes()
        self._cleanup_temp_files()

    def _terminate_download_processes(self):
        # Intenta terminar procesos yt-dlp activos en la carpeta destino
        import psutil
        try:
            for proc in psutil.process_iter(['pid', 'name', 'cmdline']):
                try:
                    if 'yt-dlp' in ' '.join(proc.info['cmdline']):
                        proc.terminate()
                except Exception:
                    continue
        except Exception as e:
            print(f"Error al terminar procesos yt-dlp: {e}")

    def download_thread(self, url):
        try:
            def progress_callback(percentage, stage=None, proc=None):
                if stage == 'audio' and proc:
                    self.yt_proc_audio = proc
                    self.root.after(0, self.on_audio_download_start)
                if stage == 'video' and proc:
                    self.yt_proc_video = proc
                if stage == 'ffmpeg' and proc:
                    self.ffmpeg_proc = proc
                if self.cancel_requested:
                    raise Exception("Descarga cancelada por el usuario")
                self.update_progress(percentage)
            download_video(url, progress_callback, self.destination_path)
        except Exception as e:
            self.show_error(f"Error: {e}")
            self.progress_label.config(text="Descarga cancelada", fg="red")
            self._cleanup_temp_files()
        finally:
            self.download_button.config(state=tk.NORMAL)
            self.cancel_button.config(state=tk.DISABLED)
            self.cancel_button.pack_forget()

    def _cleanup_temp_files(self):
        # Borra archivos temporales de audio, video, txt y fragmentos en la carpeta destino
        try:
            for file in os.listdir(self.destination_path):
                file_path = os.path.join(self.destination_path, file)
                # Borrar archivos temporales y fragmentos
                if (
                    file.startswith('audio.') or file.startswith('video.') or file.startswith('descargando_') or
                    file.endswith('.mp3') or file.endswith('.m4a') or file.endswith('.webm') or file.endswith('.part') or file.endswith('.tmp') or file.endswith('.ytdl') or file.endswith('.f*')
                ):
                    try:
                        os.remove(file_path)
                    except Exception as e:
                        print(f"No se pudo borrar {file}: {e}")
                # Borrar cualquier .mp4 menor a 5MB (incompleto)
                elif file.endswith('.mp4'):
                    try:
                        if os.path.getsize(file_path) < 5 * 1024 * 1024:  # 5MB
                            os.remove(file_path)
                    except Exception as e:
                        print(f"No se pudo borrar {file}: {e}")
        except Exception as e:
            print(f"Error al limpiar archivos temporales: {e}")

    def update_progress(self, percentage):
        self.progress["value"] = percentage
        if percentage == 100:
            self.progress_label.config(text="Descarga completada", fg="green")
        else:
            self.progress_label.config(text=f"Progreso: {percentage:.2f}%", fg="black")
        self.root.update_idletasks()

    def show_error(self, message):
        tk.messagebox.showerror("Error", message)


if __name__ == "__main__":
    root = tk.Tk()
    app = App(root)
    root.mainloop()