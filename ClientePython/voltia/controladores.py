"""CONTROLADOR (la C de MVC).

Hace de puente entre la vista (las ventanas) y el modelo/servicios: las ventanas le piden
cosas al controlador y el controlador usa los servicios. Cuando algo cambia, avisa al
Notificador (Observer). Un mismo controlador sirve para lavadora y para programa.
"""
from voltia.observador import Notificador
from voltia.servicios import servicio_para

# Textos que cambian segun el objeto
NOMBRES = {
    "lavadora": {"titulo": "LAVADORA", "articulo": "la lavadora", "segundo": "Marca"},
    "programa": {"titulo": "PROGRAMA DE LAVADO", "articulo": "el programa de lavado", "segundo": "Nombre"},
}


class ControladorObjeto:
    def __init__(self, tipo):
        self.tipo = tipo
        self.servicio = servicio_para(tipo)
        self.nombres = NOMBRES[tipo]

    def listar(self, texto, opcion):
        return self.servicio.listar(texto, opcion)

    def buscar(self, codigo, texto):
        return self.servicio.buscar(codigo, texto)

    def agregar(self, datos):
        creado = self.servicio.agregar(datos)
        Notificador().avisar()  # Observer: las ventanas de Listar se recargan
        return creado

    def actualizar(self, codigo, datos):
        actualizado = self.servicio.actualizar(codigo, datos)
        Notificador().avisar()
        return actualizado

    def eliminar(self, codigo):
        eliminado = self.servicio.eliminar(codigo)
        if eliminado:
            Notificador().avisar()
        return eliminado
