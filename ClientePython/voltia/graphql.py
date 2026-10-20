"""Conexion con el servidor GraphQL (Java).

Es el UNICO lugar donde el cliente habla con el servidor. Usa solo la libreria
estandar de Python (urllib), asi no hay que instalar nada.

Patron SINGLETON: una sola instancia de ClienteGraphQL para toda la aplicacion.
"""
import json
import urllib.error
import urllib.request

from voltia.configuracion import Configuracion


class ErrorVoltia(Exception):
    """Error con un mensaje claro para mostrarle al usuario."""


class ClienteGraphQL:
    _instancia = None  # La unica instancia

    def __new__(cls):
        if cls._instancia is None:
            cls._instancia = super().__new__(cls)
        return cls._instancia

    def ejecutar(self, consulta, variables=None):
        """Manda una consulta o mutacion y devuelve los datos ("data") de la respuesta.

        Si el servidor responde con un error (codigo repetido, dato invalido, etc.)
        lanza ErrorVoltia con el mismo mensaje del servidor.
        """
        url = Configuracion().url_servidor
        cuerpo = json.dumps({"query": consulta, "variables": variables or {}}).encode("utf-8")
        peticion = urllib.request.Request(
            url,
            data=cuerpo,
            headers={"Content-Type": "application/json", "Accept": "application/json"},
        )

        try:
            with urllib.request.urlopen(peticion, timeout=10) as respuesta:
                datos = json.loads(respuesta.read().decode("utf-8"))
        except urllib.error.HTTPError as error:
            # El servidor respondio pero con un codigo de error HTTP
            try:
                datos = json.loads(error.read().decode("utf-8"))
            except (ValueError, OSError):
                raise ErrorVoltia("El servidor respondió con el error HTTP %s." % error.code)
        except (urllib.error.URLError, OSError):
            # Pasa cuando el servidor esta apagado o la direccion esta mal
            raise ErrorVoltia(
                "No se pudo conectar con el servidor GraphQL en %s.\n"
                "Verifique que el servidor (Java) esté corriendo." % url
            )

        errores = datos.get("errors")
        if errores:
            raise ErrorVoltia(errores[0].get("message", "Error del servidor"))
        return datos.get("data")
