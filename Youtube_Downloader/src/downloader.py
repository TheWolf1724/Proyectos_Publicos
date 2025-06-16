import pytube
from pytube.exceptions import PytubeError
from urllib.error import HTTPError
from downloader import download_video
import tkinter as tk


class Downloader:
    def __init__(self, url, progress_callback):
        self.url = url
        self.progress_callback = progress_callback

    def download(self):
        try:
            yt = pytube.YouTube(self.url, on_progress_callback=self.on_progress)
            stream = yt.streams.get_highest_resolution()
            stream.download()
        except HTTPError as e:
            print(f"Error HTTP: {e}")
        except PytubeError as e:
            print(f"Error de PyTube: {e}")
        except Exception as e:
            print(f"Error inesperado: {e}")

    def on_progress(self, stream, chunk, bytes_remaining):
        total_size = stream.filesize
        bytes_downloaded = total_size - bytes_remaining
        percentage = bytes_downloaded / total_size * 100
        self.progress_callback(percentage)


def start_download(self):
    url = self.url_entry.get()
    if not url:
        self.show_error("Por favor, ingresa un enlace válido.")
        return

    self.download_button.config(state=tk.DISABLED)
    try:
        download_video(url)
    except Exception as e:
        self.show_error(f"Error: {e}")
    finally:
        self.download_button.config(state=tk.NORMAL)