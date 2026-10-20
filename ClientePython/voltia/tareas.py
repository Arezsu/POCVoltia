"""Trabajo en segundo plano.

Las llamadas al servidor tardan un poco. Si se hicieran directamente, la ventana se
quedaria congelada esperando. Por eso se ejecutan en un hilo aparte y, cuando terminan,
el resultado vuelve a la ventana (el equivalente al "async/await" del cliente en C#).
"""
import queue
import threading
import tkinter as tk


def ejecutar(ventana, funcion, al_terminar, al_error):
    """Ejecuta funcion() en un hilo. Despues llama a al_terminar(resultado) o a al_error(error)."""
    cola = queue.Queue()  # Aqui el hilo deja su resultado

    def trabajo():
        try:
            cola.put(("ok", funcion()))
        except Exception as error:  # Cualquier error se devuelve a la ventana
            cola.put(("error", error))

    threading.Thread(target=trabajo, daemon=True).start()
    _poner_cursor(ventana, "watch")  # Cursor de espera mientras tanto

    def revisar():
        try:
            estado, valor = cola.get_nowait()
        except queue.Empty:
            try:
                ventana.after(40, revisar)  # Todavia no termina: se vuelve a revisar
            except tk.TclError:
                pass  # La ventana se cerro
            return

        _poner_cursor(ventana, "")
        try:
            if estado == "ok":
                al_terminar(valor)
            else:
                al_error(valor)
        except tk.TclError:
            pass  # La ventana se cerro mientras se esperaba

    ventana.after(40, revisar)


def _poner_cursor(ventana, cursor):
    try:
        ventana.configure(cursor=cursor)
    except tk.TclError:
        pass
