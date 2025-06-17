import tkinter as tk
from tkinter import ttk
from yt_dlp_downloader import download_video
import os
from tkinter import filedialog
import threading
import tkinter.font as tkFont
from tkinter import messagebox


class App:
    def __init__(self, root):
        self.root = root
        self.root.title("YouTube Downloader")
        self.root.geometry("560x420")
        self.root.resizable(False, False)
        # Paleta neutra y adaptable (oscura y clara)
        BG_MAIN = "#23272f"      # Fondo principal oscuro
        BG_FRAME = "#2d333b"     # Marco oscuro
        ACCENT = "#fca311"       # Amarillo acento
        PRIMARY = "#3a86ff"      # Azul vibrante
        SECONDARY = "#adb5bd"    # Gris claro
        TEXT = "#f8f9fa"         # Texto claro
        DANGER = "#ef233c"       # Rojo

        self.root.configure(bg=BG_MAIN)

        # Fuente personalizada
        font_title = tkFont.Font(family="Segoe UI", size=18, weight="bold")
        font_label = tkFont.Font(family="Segoe UI", size=12)
        font_button = tkFont.Font(family="Segoe UI", size=11, weight="bold")

        # Marco principal con sombra simulada
        self.shadow = tk.Frame(root, bg="#1a1d23")
        self.shadow.place(relx=0.5, rely=0.5, anchor="center", width=510, height=370, x=6, y=6)
        self.main_frame = tk.Frame(root, bg=BG_FRAME, bd=0, relief="flat")
        self.main_frame.place(relx=0.5, rely=0.5, anchor="center", width=510, height=370)

        # Título
        tk.Label(self.main_frame, text="YouTube Downloader", font=font_title, bg=BG_FRAME, fg=ACCENT).pack(pady=(22, 10))

        # Ruta de descarga
        tk.Label(self.main_frame, text="Ruta de descarga:", font=font_label, bg=BG_FRAME, fg=SECONDARY).pack(pady=(0, 2))
        self.path_label = tk.Label(self.main_frame, text=os.path.expanduser("~/Descargas"), font=font_label, fg=PRIMARY, bg=BG_FRAME)
        self.path_label.pack(pady=(0, 2))
        self.path_button = tk.Button(self.main_frame, text="Cambiar ruta", command=self.select_destination, font=font_button, bg=PRIMARY, fg=BG_FRAME, activebackground=ACCENT, activeforeground=BG_FRAME, relief="flat", bd=0, cursor="hand2")
        self.path_button.pack(pady=(0, 12))

        # Campo para ingresar la URL
        tk.Label(self.main_frame, text="Enlace del video:", font=font_label, bg=BG_FRAME, fg=SECONDARY).pack(pady=(0, 2))
        self.url_entry = tk.Entry(self.main_frame, width=48, font=font_label, relief="solid", bd=1, bg=BG_MAIN, fg=TEXT, insertbackground=TEXT)
        self.url_entry.pack(pady=(0, 12))

        # Botón para iniciar la descarga
        self.download_button = tk.Button(self.main_frame, text="Descargar", command=self.start_download, font=font_button, bg=ACCENT, fg=BG_FRAME, activebackground=PRIMARY, activeforeground=BG_FRAME, relief="flat", bd=0, cursor="hand2")
        self.download_button.pack(pady=(0, 10))

        # Botón para cancelar la descarga
        self.cancel_button = tk.Button(self.main_frame, text="Cancelar", command=self.cancel_download, font=font_button, bg=DANGER, fg=BG_FRAME, activebackground=SECONDARY, activeforeground=BG_FRAME, relief="flat", bd=0, cursor="hand2")
        self.cancel_button.pack_forget()  # No mostrar al inicio
        self.cancel_button.config(state=tk.DISABLED)

        # Barra de progreso
        self.progress = ttk.Progressbar(self.main_frame, orient="horizontal", length=400, mode="determinate", style="TProgressbar")
        self.progress_label = tk.Label(self.main_frame, text="Progreso: 0%", font=font_label, bg=BG_FRAME, fg=SECONDARY)

        # Estilo para la barra de progreso
        style = ttk.Style()
        style.theme_use("clam")
        style.configure("TProgressbar", thickness=16, troughcolor="#343a40", background=PRIMARY, bordercolor="#343a40")

        # Variable para controlar la cancelación
        self.cancel_requested = False
        self.yt_proc_audio = None
        self.yt_proc_video = None
        self.ffmpeg_proc = None
        self.destination_path = os.path.expanduser("~/Descargas")

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
        self.cancel_button.pack_forget()
        self.cancel_button.config(state=tk.DISABLED)
        self.cancel_requested = False
        self.progress.pack(pady=(8, 2))
        self.progress_label.pack(pady=(0, 8))
        threading.Thread(target=self.download_thread, args=(url,)).start()

    def on_audio_download_start(self):
        self.cancel_button.config(state=tk.NORMAL)
        self.cancel_button.pack(pady=(0, 10))

    def cancel_download(self):
        self.cancel_requested = True
        self.cancel_button.config(state=tk.DISABLED)
        self.download_button.config(state=tk.NORMAL)
        self.progress_label.config(text="Descarga cancelada", fg="#ef233c")  # DANGER
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
            self.progress_label.config(text="Descarga cancelada", fg="#ef233c")  # DANGER
            self._cleanup_temp_files()
        finally:
            self.download_button.config(state=tk.NORMAL)
            self.cancel_button.config(state=tk.DISABLED)
            self.cancel_button.pack_forget()

    def _cleanup_temp_files(self):
        try:
            for file in os.listdir(self.destination_path):
                file_path = os.path.join(self.destination_path, file)
                if (
                    file.startswith('audio.') or file.startswith('video.') or file.startswith('descargando_') or
                    file.endswith('.mp3') or file.endswith('.m4a') or file.endswith('.webm') or file.endswith('.part') or file.endswith('.tmp') or file.endswith('.ytdl') or file.endswith('.f*')
                ):
                    try:
                        os.remove(file_path)
                    except Exception as e:
                        print(f"No se pudo borrar {file}: {e}")
                elif file.endswith('.mp4'):
                    try:
                        if os.path.getsize(file_path) < 5 * 1024 * 1024:
                            os.remove(file_path)
                    except Exception as e:
                        print(f"No se pudo borrar {file}: {e}")
        except Exception as e:
            print(f"Error al limpiar archivos temporales: {e}")

    def update_progress(self, percentage):
        self.progress["value"] = percentage
        if percentage == 100:
            self.progress_label.config(text="Descarga completada", fg="#3a86ff")  # PRIMARY
        else:
            self.progress_label.config(text=f"Progreso: {percentage:.2f}%", fg="#adb5bd")  # SECONDARY
        self.root.update_idletasks()

    def show_error(self, message):
        messagebox.showerror("Error", message)


if __name__ == "__main__":
    root = tk.Tk()
    app = App(root)
    root.mainloop()