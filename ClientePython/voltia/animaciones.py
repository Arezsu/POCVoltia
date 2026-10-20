"""Animaciones sencillas para las ventanas (aparecer, sacudir y contar)."""
import tkinter as tk


def _suave(t):
    """Curva de movimiento: empieza rapido y frena suave (t va de 0 a 1)."""
    return 1 - (1 - t) ** 3


def aparecer(ventana, duracion=350):
    """La ventana pasa de transparente a visible poco a poco."""
    try:
        ventana.attributes("-alpha", 0.0)
    except tk.TclError:
        return  # Algunos sistemas no permiten transparencia: no pasa nada

    pasos = 14

    def paso(i=0):
        try:
            ventana.attributes("-alpha", _suave(i / pasos))
        except tk.TclError:
            return  # La ventana ya se cerro
        if i < pasos:
            ventana.after(int(duracion / pasos), paso, i + 1)

    paso()


def sacudir(ventana):
    """La ventana se sacude de lado a lado. Se usa cuando hay un error."""
    try:
        x = ventana.winfo_x()
        y = ventana.winfo_y()
    except tk.TclError:
        return
    desplazamientos = [-12, 12, -9, 9, -5, 5, 0]

    def paso(i=0):
        if i >= len(desplazamientos):
            return
        try:
            ventana.geometry("+%d+%d" % (x + desplazamientos[i], y))
        except tk.TclError:
            return
        ventana.after(35, paso, i + 1)

    paso()


def contar(etiqueta, total, formato, duracion=700):
    """Contador: el numero sube de 0 hasta 'total'. formato es un texto como '{} encontrado(s)'."""
    pasos = 20

    def paso(i=0):
        valor = round(total * _suave(i / pasos))
        try:
            etiqueta.configure(text=formato.format(valor))
        except tk.TclError:
            return  # La ventana ya se cerro
        if i < pasos:
            etiqueta.after(int(duracion / pasos), paso, i + 1)

    paso()
