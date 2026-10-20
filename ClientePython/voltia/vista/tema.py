"""Colores y estilos de toda la aplicacion (metalico mate con azul, gris y amarillo)."""
import tkinter as tk
from tkinter import ttk

AZUL = "#1E88E5"
AZUL_OSCURO = "#1565C0"
AMARILLO = "#FFC107"
GRIS = "#607D8B"
GRIS_OSCURO = "#37474F"
ROJO = "#E53935"
FONDO = "#CFD8DC"
FONDO_CLARO = "#ECEFF1"
TEXTO = "#263238"


def aplicar(raiz):
    """Configura los estilos de ttk: botones de colores, tabla, titulos y pantallas digitales."""
    estilo = ttk.Style(raiz)
    try:
        estilo.theme_use("clam")  # Este tema permite cambiar los colores
    except tk.TclError:
        pass

    estilo.configure("TFrame", background=FONDO)
    estilo.configure("TLabel", background=FONDO, foreground=TEXTO, font=("Segoe UI", 10))
    estilo.configure("Etiqueta.TLabel", background=FONDO, foreground=TEXTO, font=("Segoe UI", 9, "bold"))
    estilo.configure("Rotulo.TLabel", background=FONDO, foreground=GRIS_OSCURO, font=("Segoe UI", 9, "bold"))
    estilo.configure("Titulo.TLabel", background=GRIS_OSCURO, foreground="#4FC3F7",
                     font=("Consolas", 16, "bold"), padding=(14, 12))
    estilo.configure("Pantalla.TLabel", background="#08121C", foreground="#FFD54F",
                     font=("Consolas", 12, "bold"), padding=(12, 6))
    estilo.configure("TCheckbutton", background=FONDO, foreground=TEXTO, font=("Segoe UI", 10, "bold"))

    # Un estilo de boton por color. "active" es cuando el mouse esta encima.
    colores = [
        ("Azul", AZUL, AZUL_OSCURO, "white"),
        ("Amarillo", AMARILLO, "#FFA000", TEXTO),
        ("Rojo", ROJO, "#B71C1C", "white"),
        ("Gris", GRIS, "#455A64", "white"),
    ]
    for nombre, color, color_encima, color_texto in colores:
        estilo.configure(nombre + ".TButton", background=color, foreground=color_texto,
                         font=("Segoe UI", 10, "bold"), padding=(14, 8), borderwidth=0, focusthickness=0)
        estilo.map(nombre + ".TButton",
                   background=[("disabled", "#B0BEC5"), ("active", color_encima)],
                   foreground=[("disabled", "#78909C")])

    # Tabla
    estilo.configure("Treeview", rowheight=26, font=("Segoe UI", 10), background="white", fieldbackground="white")
    estilo.configure("Treeview.Heading", background=GRIS_OSCURO, foreground="white",
                     font=("Segoe UI", 10, "bold"), padding=6)
    estilo.map("Treeview.Heading", background=[("active", "#546E7A")])
