"""MODELO (la M de MVC): los objetos tal como los devuelve el servidor.

Programa de lavado = objeto MAESTRO. Lavadora = objeto DETALLE.
Un programa lo usan muchas lavadoras (0..*) y cada lavadora tiene un solo programa (1..1).
Los dos heredan de ObjetoBase, asi las ventanas los tratan igual.
"""
from dataclasses import dataclass
from datetime import datetime


def formatear_fecha(texto):
    """Convierte 2025-06-18T10:45:00 en 18/06/2025 10:45."""
    try:
        return datetime.strptime(texto, "%Y-%m-%dT%H:%M:%S").strftime("%d/%m/%Y %H:%M")
    except (ValueError, TypeError):
        return str(texto)


def formatear_dinero(valor):
    """Convierte 1450000 en $ 1.450.000."""
    return "$ " + "{:,.0f}".format(valor).replace(",", ".")


def numero_corto(valor):
    """Quita el .0 de los numeros enteros (18.0 pasa a 18)."""
    return ("%g" % valor)


@dataclass
class ObjetoBase:
    """Clase padre: lo que tienen en comun los dos objetos."""
    codigo: int


@dataclass
class Programa(ObjetoBase):
    nombre: str
    min_minutos: int
    max_minutos: int
    consumo_agua_litros: float
    fecha_creacion: str
    agua_caliente: bool
    cantidad_lavadoras: int = 0  # Cuantas lavadoras lo usan (la parte 0..*)

    @staticmethod
    def desde_json(d):
        return Programa(
            codigo=d["codigo"],
            nombre=d["nombre"],
            min_minutos=d["minMinutos"],
            max_minutos=d["maxMinutos"],
            consumo_agua_litros=d["consumoAguaLitros"],
            fecha_creacion=d["fechaCreacion"],
            agua_caliente=d["aguaCaliente"],
            cantidad_lavadoras=d.get("cantidadLavadoras") or 0,
        )

    # Textos listos para mostrar en la tabla
    @property
    def rango_texto(self):
        return "%d a %d min" % (self.min_minutos, self.max_minutos)

    @property
    def agua_texto(self):
        return numero_corto(self.consumo_agua_litros) + " L"

    @property
    def caliente_texto(self):
        return "Sí" if self.agua_caliente else "No"

    @property
    def fecha_texto(self):
        return formatear_fecha(self.fecha_creacion)


@dataclass
class Lavadora(ObjetoBase):
    marca: str
    precio_base: float
    fecha_fabricacion: str
    capacidad_carga_kilos: float
    velocidad_centrifugado_rpm: int
    tecnologia_inverter: bool
    minutos_lavado: int
    programa_nombre: str  # Nombre del programa (maestro) que usa esta lavadora
    precio_final: float

    @staticmethod
    def desde_json(d):
        programa = d.get("programaLavado") or {}
        return Lavadora(
            codigo=d["codigo"],
            marca=d["marca"],
            precio_base=d["precioBase"],
            fecha_fabricacion=d["fechaFabricacion"],
            capacidad_carga_kilos=d["capacidadCargaKilos"],
            velocidad_centrifugado_rpm=d["velocidadCentrifugadoRPM"],
            tecnologia_inverter=d["tecnologiaInverter"],
            minutos_lavado=d["minutosLavado"],
            programa_nombre=programa.get("nombre", ""),
            precio_final=d["precioFinal"],
        )

    # Textos listos para mostrar en la tabla
    @property
    def fecha_texto(self):
        return formatear_fecha(self.fecha_fabricacion)

    @property
    def precio_base_texto(self):
        return formatear_dinero(self.precio_base)

    @property
    def precio_final_texto(self):
        return formatear_dinero(self.precio_final)

    @property
    def capacidad_texto(self):
        return numero_corto(self.capacidad_carga_kilos) + " kg"

    @property
    def rpm_texto(self):
        return "%d RPM" % self.velocidad_centrifugado_rpm

    @property
    def inverter_texto(self):
        return "Sí" if self.tecnologia_inverter else "No"
