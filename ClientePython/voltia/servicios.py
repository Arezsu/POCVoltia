"""Servicios: hablan con el servidor GraphQL usando ClienteGraphQL.

ServicioCrud es una clase abstracta (el contrato). ServicioPrograma y ServicioLavadora
lo cumplen cada uno a su manera, asi el controlador y las ventanas funcionan igual
para los dos objetos (principio de inversion de dependencias).
"""
from abc import ABC, abstractmethod

from voltia.graphql import ClienteGraphQL
from voltia.modelos import Lavadora, Programa


class ServicioCrud(ABC):
    """Lo que se puede hacer con un objeto (el CRUD)."""

    @abstractmethod
    def listar(self, texto, opcion):
        """Lista con filtro (lo hace el servidor). texto y opcion pueden ser None."""

    @abstractmethod
    def buscar(self, codigo, texto):
        """Consulta UN objeto por codigo y/o texto. Devuelve None si no existe."""

    @abstractmethod
    def agregar(self, datos):
        """Crea uno nuevo. datos es un diccionario con los nombres de GraphQL."""

    @abstractmethod
    def actualizar(self, codigo, datos):
        """Cambia uno existente."""

    @abstractmethod
    def eliminar(self, codigo):
        """Elimina. Devuelve True si se pudo."""


class ServicioPrograma(ServicioCrud):
    CAMPOS = "codigo nombre minMinutos maxMinutos consumoAguaLitros fechaCreacion aguaCaliente cantidadLavadoras"

    def listar(self, texto, opcion):
        consulta = (
            "query filtrar($nombre: String, $caliente: Boolean) { "
            "programasPorFiltro(nombre: $nombre, aguaCaliente: $caliente) { %s } }" % self.CAMPOS
        )
        datos = ClienteGraphQL().ejecutar(consulta, {"nombre": texto, "caliente": opcion})
        return [Programa.desde_json(d) for d in datos["programasPorFiltro"] or []]

    def buscar(self, codigo, texto):
        consulta = (
            "query buscar($codigo: Int, $nombre: String) { "
            "programaPorCriterios(codigo: $codigo, nombre: $nombre) { %s } }" % self.CAMPOS
        )
        datos = ClienteGraphQL().ejecutar(consulta, {"codigo": codigo, "nombre": texto})
        encontrado = datos["programaPorCriterios"]
        return Programa.desde_json(encontrado) if encontrado else None

    def agregar(self, datos):
        consulta = "mutation agregar($input: ProgramaInput!) { addPrograma(input: $input) { %s } }" % self.CAMPOS
        respuesta = ClienteGraphQL().ejecutar(consulta, {"input": datos})
        return Programa.desde_json(respuesta["addPrograma"])

    def actualizar(self, codigo, datos):
        consulta = (
            "mutation actualizar($codigo: Int!, $input: ProgramaUpdateInput!) { "
            "actualizarPrograma(codigo: $codigo, input: $input) { %s } }" % self.CAMPOS
        )
        respuesta = ClienteGraphQL().ejecutar(consulta, {"codigo": codigo, "input": datos})
        return Programa.desde_json(respuesta["actualizarPrograma"])

    def eliminar(self, codigo):
        consulta = "mutation eliminar($codigo: Int!) { eliminarPrograma(codigo: $codigo) }"
        return bool(ClienteGraphQL().ejecutar(consulta, {"codigo": codigo})["eliminarPrograma"])


class ServicioLavadora(ServicioCrud):
    CAMPOS = (
        "codigo marca precioBase fechaFabricacion capacidadCargaKilos velocidadCentrifugadoRPM "
        "tecnologiaInverter minutosLavado precioFinal programaLavado { codigo nombre }"
    )

    def listar(self, texto, opcion):
        consulta = (
            "query filtrar($marca: String, $inverter: Boolean) { "
            "lavadorasPorFiltro(marca: $marca, tecnologiaInverter: $inverter) { %s } }" % self.CAMPOS
        )
        datos = ClienteGraphQL().ejecutar(consulta, {"marca": texto, "inverter": opcion})
        return [Lavadora.desde_json(d) for d in datos["lavadorasPorFiltro"] or []]

    def buscar(self, codigo, texto):
        consulta = (
            "query buscar($codigo: Int, $marca: String) { "
            "lavadoraPorCriterios(codigo: $codigo, marca: $marca) { %s } }" % self.CAMPOS
        )
        datos = ClienteGraphQL().ejecutar(consulta, {"codigo": codigo, "marca": texto})
        encontrada = datos["lavadoraPorCriterios"]
        return Lavadora.desde_json(encontrada) if encontrada else None

    def agregar(self, datos):
        consulta = "mutation agregar($input: LavadoraInput!) { addLavadora(input: $input) { %s } }" % self.CAMPOS
        respuesta = ClienteGraphQL().ejecutar(consulta, {"input": datos})
        return Lavadora.desde_json(respuesta["addLavadora"])

    def actualizar(self, codigo, datos):
        consulta = (
            "mutation actualizar($codigo: Int!, $input: LavadoraUpdateInput!) { "
            "actualizarLavadora(codigo: $codigo, input: $input) { %s } }" % self.CAMPOS
        )
        respuesta = ClienteGraphQL().ejecutar(consulta, {"codigo": codigo, "input": datos})
        return Lavadora.desde_json(respuesta["actualizarLavadora"])

    def eliminar(self, codigo):
        consulta = "mutation eliminar($codigo: Int!) { eliminarLavadora(codigo: $codigo) }"
        return bool(ClienteGraphQL().ejecutar(consulta, {"codigo": codigo})["eliminarLavadora"])


def servicio_para(tipo):
    """Fabrica: entrega el servicio que corresponde al tipo ('programa' o 'lavadora')."""
    if tipo == "lavadora":
        return ServicioLavadora()
    return ServicioPrograma()
