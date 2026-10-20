"""Datos generales de la aplicacion.

Patron SINGLETON: solo existe UNA instancia de Configuracion. Cada vez que se
escribe Configuracion() se obtiene la misma. Asi la direccion del servidor y los
datos de "Acerca de" estan en un solo lugar.
"""
import os


class Configuracion:
    _instancia = None  # La unica instancia

    def __new__(cls):
        # __new__ se ejecuta antes de crear el objeto: si ya existe uno, lo devuelve
        if cls._instancia is None:
            cls._instancia = super().__new__(cls)
            cls._instancia._cargar()
        return cls._instancia

    def _cargar(self):
        # Para cambiar el servidor: variable de entorno VOLTIA_URL
        # (ejemplo: http://192.168.1.20:8081/graphql)
        self.url_servidor = os.environ.get("VOLTIA_URL", "http://localhost:8081/graphql")
        self.nombre_app = "Sistema de Gestión de Electrodomésticos - Voltia"
        self.version = "4.0.0 (cliente Python + servidor GraphQL)"
        self.fecha = "Octubre 2026"
        self.integrantes = [
            "Juan Andres Bermeo Alvarez (2220241053)",
            "Alejandro Sanchez Quimbayo (2220241077)",
            "David Arredondo (2220241062)",
        ]
        carpeta = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
        self.carpeta_assets = os.path.join(carpeta, "assets")

    def ruta_asset(self, nombre):
        """Ruta completa de un archivo de la carpeta assets."""
        return os.path.join(self.carpeta_assets, nombre)
