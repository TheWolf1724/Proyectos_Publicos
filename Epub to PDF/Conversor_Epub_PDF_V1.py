import tkinter as tk
from tkinter import filedialog, messagebox
import warnings
import os
from ebooklib import epub, ITEM_DOCUMENT
from reportlab.platypus import SimpleDocTemplate, Paragraph, Spacer, PageBreak
from reportlab.lib.styles import getSampleStyleSheet, ParagraphStyle
import re
from bs4 import BeautifulSoup

# Desactivar los warnings
warnings.filterwarnings("ignore")

# Funciones de procesamiento del EPUB (igual que antes)
def clean_html_tags(text):
    """Limpia etiquetas HTML del texto."""
    return re.sub(r'<[^>]+>', '', text)

def normalize_text(text):
    """Normaliza el texto eliminando espacios redundantes y caracteres no visibles."""
    if text:  # Asegurarnos de que no sea None
        # Limpia caracteres no visibles (como &#13; y saltos de línea)
        text = re.sub(r'&#13;|\r|\n', '', text)
        text = re.sub(r'\s+', ' ', text).strip()
    return text

def extract_title_from_html(content):
    """Extrae el título del capítulo desde el HTML, buscando etiquetas de título como <h1>, <h2>, etc."""
    soup = BeautifulSoup(content, 'html.parser')
    title = None
    for tag in ['h1', 'h2', 'h3']:  # Se puede ajustar según los tipos de títulos
        title = soup.find(tag)
        if title:
            return title.get_text(strip=True)
    return None

def clean_comparison_text(text):
    """Limpia el texto para la comparación (elimina espacios extra y normaliza)."""
    if not text:  # Verificamos si el texto es None o vacío
        return ""
    text = normalize_text(text)  # Normaliza el texto
    text = re.sub(r'\s+', '', text)  # Elimina todos los espacios
    return text

def epub_to_pdf(epub_file, pdf_file):
    """Convierte un archivo EPUB a PDF."""
    try:
        book = epub.read_epub(epub_file)
        doc = SimpleDocTemplate(pdf_file)
        story = []
        styles = getSampleStyleSheet()

        title_style = ParagraphStyle(
            'ChapterTitle',
            parent=styles['Title'],
            fontSize=18,
            spaceAfter=12,
            leading=22,
            alignment=1,  # Centrado
            textColor='black',
            bold=True
        )

        normal_style = styles["Normal"]
        last_title = None  # Variable para almacenar el último título agregado
        added_titles = set()  # Usamos un conjunto para evitar títulos duplicados

        for item in book.get_items():
            if item.get_type() == ITEM_DOCUMENT:
                content = item.get_body_content().decode('utf-8')
                paragraphs = re.split(r'(?:\r?\n)+', clean_html_tags(content))

                # Intentar extraer título desde el HTML (usando <h1>, <h2>, <h3>)
                chapter_title = extract_title_from_html(content)
                if chapter_title:  # Asegurarnos de que el título no sea None
                    chapter_title = normalize_text(chapter_title)  # Limpiar y normalizar el título

                    # Verificar si el título ya fue agregado
                    if clean_comparison_text(chapter_title) not in added_titles:  # Usamos la versión limpia para comparar
                        # Si encontramos un nuevo título, lo agregamos como título de capítulo
                        story.append(PageBreak())  # Salto de página antes del nuevo capítulo
                        story.append(Paragraph(chapter_title, title_style))  # Título de capítulo
                        story.append(Spacer(1, 24))  # Espacio después del título
                        added_titles.add(clean_comparison_text(chapter_title))  # Agregar el título limpio al conjunto de títulos ya procesados

                # Procesar los párrafos del contenido
                for paragraph in paragraphs:
                    paragraph = normalize_text(paragraph)  # Limpiar y normalizar el párrafo
                    if not paragraph:
                        continue

                    # Comprobar si chapter_title es None antes de procesar fragmentos
                    if chapter_title:  # Solo verificar si tenemos un título válido
                        # Verificar si el párrafo contiene una parte del título
                        # Es decir, si el párrafo contiene una fragmentación del título que ya se ha agregado
                        if any(clean_comparison_text(paragraph).startswith(clean_comparison_text(part)) for part in chapter_title.split()):
                            continue  # Si el párrafo contiene una parte del título, lo omitimos

                    # Agregar los párrafos normales
                    story.append(Paragraph(paragraph, normal_style))
                    story.append(Spacer(1, 12))  # Espacio después de cada párrafo

        doc.build(story)
        return None  # Si todo va bien, no se retorna ningún error

    except Exception as e:
        return str(e)  # Retornar el mensaje de error si ocurre alguna excepción

def browse_epub_file():
    """Abre un diálogo para seleccionar múltiples archivos .epub"""
    epub_files = filedialog.askopenfilenames(filetypes=[("Archivos EPUB", "*.epub")])
    if epub_files:
        entry_epub.delete(0, tk.END)  # Limpiar el contenido actual
        # Insertar todos los archivos seleccionados, separados por coma
        entry_epub.insert(tk.END, ', '.join(epub_files))

def browse_output_folder():
    """Abre un diálogo para seleccionar la carpeta de salida"""
    folder = filedialog.askdirectory()
    if folder:
        entry_output_folder.delete(0, tk.END)  # Limpiar el contenido actual
        entry_output_folder.insert(tk.END, folder)  # Insertar la carpeta seleccionada

def start_conversion():
    """Inicia el proceso de conversión para cada archivo seleccionado"""
    # Obtener los archivos EPUB de la caja de texto
    selected_files = entry_epub.get()
    output_folder = entry_output_folder.get()

    if not selected_files:
        messagebox.showerror("Error", "Por favor, seleccione al menos un archivo EPUB.")
        return
    if not output_folder:
        messagebox.showerror("Error", "Por favor, seleccione una carpeta de destino.")
        return

    # Separar los archivos seleccionados por coma
    selected_files = selected_files.split(', ')

    failed_files = []  # Lista para almacenar los archivos que fallaron

    # Convertir cada archivo y no mostrar un mensaje hasta que todos estén listos
    for epub_file in selected_files:
        pdf_filename = os.path.join(output_folder, os.path.basename(epub_file).replace(".epub", ".pdf"))
        error = epub_to_pdf(epub_file, pdf_filename)
        if error:  # Si ocurrió un error, lo agregamos a la lista
            failed_files.append(f"{epub_file}: {error}")

    # Mostrar el mensaje adecuado al final
    if failed_files:
        # Si hubo errores, mostrar todos los errores
        messagebox.showerror("Errores durante la conversión", "\n".join(failed_files))
    else:
        # Si todo salió bien, mostrar el mensaje de éxito
        messagebox.showinfo("Conversión Completa", "Todos los archivos han sido convertidos a PDF correctamente.")

# Crear la ventana principal
root = tk.Tk()
root.title("Conversor de EPUB a PDF")

# Configurar la ventana para que se centre en la pantalla
screen_width = root.winfo_screenwidth()
screen_height = root.winfo_screenheight()

# Establecer el tamaño de la ventana, haciéndola más ancha
root.geometry("700x300")  # Aumenté el tamaño para incluir la carpeta de destino

# Cambiar el color de fondo
root.config(bg="#f0f0f0")

# Crear un marco (frame) para organizar los widgets
frame = tk.Frame(root, bg="#f0f0f0")
frame.pack(padx=20, pady=20, expand=True)

# Etiquetas y campos de entrada
label_epub = tk.Label(frame, text="Selecciona los archivos EPUB:", bg="#f0f0f0", font=("Helvetica", 12))
label_epub.pack(anchor="w", padx=10, pady=5)

# Crear un Frame para contener la entrada de texto y el botón juntos
entry_frame = tk.Frame(frame, bg="#f0f0f0")
entry_frame.pack(padx=10, pady=5, fill="x", expand=True)

# Entrada de texto para mostrar los archivos seleccionados
entry_epub = tk.Entry(entry_frame, width=40, font=("Helvetica", 12))
entry_epub.pack(side="left", padx=5)

# Botón para buscar archivos con estilo, con un ancho reducido
button_browse_epub = tk.Button(entry_frame, text="Buscar archivos", command=browse_epub_file, relief="flat", bg="#4CAF50", fg="white", font=("Helvetica", 12), width=15)
button_browse_epub.pack(side="right", padx=5)

# Etiqueta y caja de entrada para la carpeta de salida
label_output = tk.Label(frame, text="Selecciona la carpeta de destino:", bg="#f0f0f0", font=("Helvetica", 12))
label_output.pack(anchor="w", padx=10, pady=5)

entry_output_frame = tk.Frame(frame, bg="#f0f0f0")
entry_output_frame.pack(padx=10, pady=5, fill="x", expand=True)

entry_output_folder = tk.Entry(entry_output_frame, width=40, font=("Helvetica", 12))
entry_output_folder.pack(side="left", padx=5)

button_browse_output = tk.Button(entry_output_frame, text="Buscar carpeta", command=browse_output_folder, relief="flat", bg="#4CAF50", fg="white", font=("Helvetica", 12), width=15)
button_browse_output.pack(side="right", padx=5)

# Botón para iniciar la conversión
button_convert = tk.Button(frame, text="Iniciar Conversión", command=start_conversion, relief="flat", bg="#2196F3", fg="white", font=("Helvetica", 12))
button_convert.pack(pady=20)

# Iniciar la interfaz gráfica
root.mainloop()
