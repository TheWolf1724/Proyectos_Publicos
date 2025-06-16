from tkinter import Tk, Label, Entry, Button, StringVar, IntVar, ttk
from downloader import Downloader

class App:
    def __init__(self, master):
        self.master = master
        master.title("YouTube Video Downloader")

        self.label = Label(master, text="Ingrese el enlace del video:")
        self.label.pack()

        self.video_url = StringVar()
        self.entry = Entry(master, textvariable=self.video_url, width=50)
        self.entry.pack()

        self.download_button = Button(master, text="Descargar", command=self.start_download)
        self.download_button.pack()

        self.cancel_button = Button(master, text="Cancelar", command=self.cancel_download, bg="red", fg="white")
        # No se muestra al inicio

        self.progress = IntVar()
        self.progress_bar = ttk.Progressbar(master, maximum=100, variable=self.progress)
        self.progress_bar.pack()

        self.status_label = Label(master, text="")
        self.status_label.pack()

    def start_download(self):
        url = self.video_url.get()
        self.status_label.config(text="Descargando...")
        self.download_button.pack_forget()  # Oculta el botón de descargar
        self.cancel_button.pack(pady=10)    # Muestra el botón de cancelar
        self.downloader = Downloader(url, self.update_progress)
        self.downloader.download()

    def cancel_download(self):
        # Aquí deberías implementar la lógica para cancelar la descarga en Downloader
        self.status_label.config(text="Descarga cancelada.")
        self.cancel_button.pack_forget()    # Oculta el botón de cancelar
        self.download_button.pack()         # Muestra el botón de descargar

    def update_progress(self, percent):
        self.progress.set(percent)
        if percent >= 100:
            self.status_label.config(text="Descarga completa!")
            self.cancel_button.pack_forget()    # Oculta el botón de cancelar
            self.download_button.pack()         # Muestra el botón de descargar

if __name__ == "__main__":
    root = Tk()
    app = App(root)
    root.mainloop()