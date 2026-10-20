"""Patron OBSERVER.

Cuando una ventana adiciona, actualiza o elimina algo, avisa aqui, y las ventanas de
Listar que esten abiertas se recargan solas.

El Notificador es el "sujeto": guarda la lista de observadores (las ventanas que quieren
enterarse) y les avisa. Tambien es un Singleton: hay uno solo para toda la aplicacion.
"""


class Notificador:
    _instancia = None  # La unica instancia

    def __new__(cls):
        if cls._instancia is None:
            cls._instancia = super().__new__(cls)
            cls._instancia._observadores = []
        return cls._instancia

    def suscribir(self, funcion):
        """Una ventana dice: avisame cuando cambien los datos."""
        if funcion not in self._observadores:
            self._observadores.append(funcion)

    def desuscribir(self, funcion):
        """La ventana se quita de la lista (por ejemplo al cerrarse)."""
        if funcion in self._observadores:
            self._observadores.remove(funcion)

    def avisar(self):
        """Avisa a todos los observadores que los datos cambiaron."""
        for funcion in list(self._observadores):  # Copia de la lista, por si alguno se quita
            funcion()
