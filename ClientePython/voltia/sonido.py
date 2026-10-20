"""Sonidos. Usa winsound, que viene con Python en Windows. En otros sistemas no suena nada."""
import os

try:
    import winsound
except ImportError:  # No es Windows
    winsound = None

from voltia.configuracion import Configuracion


def reproducir(nombre):
    """Reproduce un archivo de la carpeta assets sin bloquear la ventana."""
    if winsound is None:
        return
    ruta = Configuracion().ruta_asset(nombre)
    if os.path.exists(ruta):
        try:
            winsound.PlaySound(ruta, winsound.SND_FILENAME | winsound.SND_ASYNC)
        except RuntimeError:
            pass  # El sonido es solo decoracion: nunca debe tumbar la aplicacion
