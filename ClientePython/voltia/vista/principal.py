"""Ventana principal: menu, logo siempre visible y botones rapidos.

Desde aqui se abre cada caso de uso en SU PROPIA ventana (una funcion por ventana).
"""
import tkinter as tk
from tkinter import ttk

from voltia import sonido
from voltia.configuracion import Configuracion
from voltia.vista import tema
from voltia.vista.ventanas import (VentanaAcercaDe, VentanaActualizar, VentanaAgregar,
                                   VentanaConsultar, VentanaEliminar, VentanaListar)

# Los cinco casos de uso: (texto del boton, clase de la ventana, estilo del boton)
ACCIONES = [
    ("Adicionar", VentanaAgregar, "Azul.TButton"),
    ("Consultar", VentanaConsultar, "Azul.TButton"),
    ("Actualizar", VentanaActualizar, "Amarillo.TButton"),
    ("Eliminar", VentanaEliminar, "Rojo.TButton"),
    ("Listar", VentanaListar, "Gris.TButton"),
]

OBJETOS = [("programa", "Programa de lavado"), ("lavadora", "Lavadora")]


class VentanaPrincipal(tk.Tk):
    def __init__(self):
        super().__init__()
        tema.aplicar(self)
        self.title("Voltia - Principal")
        self.geometry("640x430+180+120")
        self.configure(bg=tema.FONDO)
        self.resizable(False, False)

        self._armar_menu()
        self._armar_logo()
        self._armar_botones()

        sonido.reproducir("click.wav")  # El sonido suena UNA sola vez, al abrir la aplicacion

    def _armar_menu(self):
        """Menu: Archivo, Programa de lavado, Lavadora y Ayuda."""
        barra = tk.Menu(self)

        archivo = tk.Menu(barra, tearoff=0)
        archivo.add_command(label="Salir", command=self.destroy)
        barra.add_cascade(label="Archivo", menu=archivo)

        for tipo, titulo in OBJETOS:
            menu = tk.Menu(barra, tearoff=0)
            for texto, clase, _estilo in ACCIONES:
                # tipo=tipo y clase=clase "congelan" los valores de este turno del ciclo
                menu.add_command(label=texto, command=lambda t=tipo, c=clase: self.abrir(c, t))
            barra.add_cascade(label=titulo, menu=menu)

        ayuda = tk.Menu(barra, tearoff=0)
        ayuda.add_command(label="Acerca de...", command=lambda: VentanaAcercaDe(self))
        barra.add_cascade(label="Ayuda", menu=ayuda)

        self.config(menu=barra)

    def _armar_logo(self):
        """El logo siempre visible, sobre una placa blanca con borde amarillo."""
        placa = tk.Frame(self, bg="white", highlightbackground=tema.AMARILLO, highlightthickness=2)
        placa.pack(pady=(16, 10), padx=40, fill="x")
        self._logo = tk.PhotoImage(file=Configuracion().ruta_asset("logo.png"))  # Se guarda para que no se borre
        tk.Label(placa, image=self._logo, bg="white").pack(pady=8)

    def _armar_botones(self):
        """Botones rapidos: una fila para el programa de lavado y otra para la lavadora."""
        for tipo, titulo in OBJETOS:
            ttk.Label(self, text=titulo.upper(), style="Rotulo.TLabel").pack(anchor="w", padx=44, pady=(8, 0))
            fila = ttk.Frame(self)
            fila.pack(fill="x", padx=40)
            for texto, clase, estilo in ACCIONES:
                ttk.Button(fila, text=texto, style=estilo,
                           command=lambda t=tipo, c=clase: self.abrir(c, t)).pack(
                    side="left", expand=True, fill="x", padx=3, pady=4)

    def abrir(self, clase, tipo):
        """Abre la ventana del caso de uso. Se pueden tener varias abiertas a la vez."""
        clase(self, tipo)
