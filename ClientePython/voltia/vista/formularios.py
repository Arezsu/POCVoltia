"""Formularios con todos los datos de cada objeto.

Cada formulario se arma a partir de una DEFINICION (una lista de campos). Asi el mismo
codigo dibuja el formulario de la lavadora y el del programa de lavado, y las ventanas
no necesitan saber de cual se trata.
"""
import tkinter as tk
from datetime import datetime
from tkinter import ttk

from voltia.graphql import ErrorVoltia

MARCAS = ["Samsung", "LG", "Whirlpool", "Mabe", "Haceb",
          "Electrolux", "Bosch", "Haier", "Panasonic", "Frigidaire"]


class Campo:
    """Describe un campo del formulario.

    atributo: nombre en el modelo (por ejemplo precio_base)
    clave:    nombre en GraphQL (por ejemplo precioBase)
    tipo:     entero, decimal, texto, combo, check, fechahora o calculado
    """

    def __init__(self, atributo, clave, etiqueta, tipo, minimo=1, mensaje="", opciones=None):
        self.atributo = atributo
        self.clave = clave
        self.etiqueta = etiqueta
        self.tipo = tipo
        self.minimo = minimo
        self.mensaje = mensaje
        self.opciones = opciones


# Cada lista interna es una fila del formulario (maximo dos campos por fila)
DEFINICIONES = {
    "lavadora": [
        [Campo("codigo", "codigo", "Código", "entero", 1, "El código debe ser un número entero mayor que cero."),
         Campo("marca", "marca", "Marca", "combo", opciones=MARCAS)],
        [Campo("fecha_fabricacion", "fechaFabricacion", "Fecha de fabricación (AAAA-MM-DD) y hora (HH:MM)", "fechahora")],
        [Campo("precio_base", "precioBase", "Precio base ($)", "decimal",
               mensaje="El precio base debe ser un número mayor que cero (por ejemplo 1450000)."),
         Campo("capacidad_carga_kilos", "capacidadCargaKilos", "Capacidad (kg)", "decimal",
               mensaje="La capacidad debe ser un número mayor que cero (por ejemplo 15.5).")],
        [Campo("velocidad_centrifugado_rpm", "velocidadCentrifugadoRPM", "Velocidad de centrifugado (RPM)", "entero", 600,
               "La velocidad de centrifugado debe ser un número entero de 600 RPM o más."),
         Campo("minutos_lavado", "minutosLavado", "Tiempo de lavado (min)", "entero", 1,
               "El tiempo de lavado debe ser un número entero de minutos (por ejemplo 45).")],
        [Campo("tecnologia_inverter", "tecnologiaInverter", "Tecnología Inverter", "check")],
        [Campo("programa_nombre", "", "Programa de lavado (automático)", "calculado"),
         Campo("precio_final_texto", "", "Precio final (calculado)", "calculado")],
    ],
    "programa": [
        [Campo("codigo", "codigo", "Código", "entero", 1, "El código debe ser un número entero mayor que cero."),
         Campo("nombre", "nombre", "Nombre", "texto", mensaje="Escriba el nombre del programa.")],
        [Campo("min_minutos", "minMinutos", "Minutos mínimos", "entero", 1,
               "Los minutos mínimos deben ser un número entero de 1 o más."),
         Campo("max_minutos", "maxMinutos", "Minutos máximos", "entero", 1,
               "Los minutos máximos deben ser un número entero de 1 o más.")],
        [Campo("consumo_agua_litros", "consumoAguaLitros", "Consumo de agua (litros)", "decimal",
               mensaje="El consumo de agua debe ser un número mayor que cero (por ejemplo 55.5)."),
         Campo("agua_caliente", "aguaCaliente", "Usa agua caliente", "check")],
        [Campo("fecha_creacion", "fechaCreacion", "Fecha de creación (AAAA-MM-DD) y hora (HH:MM)", "fechahora")],
        [Campo("cantidad_lavadoras", "", "Lavadoras que usan este programa", "calculado")],
    ],
}


# ====================================================================
#  Entradas: cada una sabe ponerse, leerse y bloquearse
# ====================================================================

class _EntradaTexto:
    """Caja de texto normal."""

    def __init__(self, padre, campo):
        self.var = tk.StringVar()
        self.widget = ttk.Entry(padre, textvariable=self.var, font=("Segoe UI", 10))

    def poner(self, valor):
        self.var.set("" if valor is None else str(valor))

    def texto(self):
        return self.var.get().strip()

    def leer(self, campo):
        if not self.texto():
            raise ErrorVoltia(campo.mensaje or "Complete el campo " + campo.etiqueta + ".")
        return self.texto()

    def limpiar(self):
        self.var.set("")

    def habilitar(self, si):
        self.widget.configure(state="normal" if si else "readonly")


class _EntradaEntero(_EntradaTexto):
    def leer(self, campo):
        try:
            valor = int(self.texto())
        except ValueError:
            raise ErrorVoltia(campo.mensaje)
        if valor < campo.minimo:
            raise ErrorVoltia(campo.mensaje)
        return valor


class _EntradaDecimal(_EntradaTexto):
    def poner(self, valor):
        self.var.set("" if valor is None else "%g" % valor)

    def leer(self, campo):
        try:
            valor = float(self.texto().replace(",", "."))  # "15,5" tambien sirve
        except ValueError:
            raise ErrorVoltia(campo.mensaje)
        if valor <= 0:
            raise ErrorVoltia(campo.mensaje)
        return valor


class _EntradaCalculada(_EntradaTexto):
    """Dato que calcula el servidor: siempre es de solo lectura."""

    def __init__(self, padre, campo):
        super().__init__(padre, campo)
        self.widget.configure(state="readonly")

    def habilitar(self, si):
        pass  # Nunca se puede editar


class _EntradaCombo:
    def __init__(self, padre, campo):
        self.var = tk.StringVar()
        self.widget = ttk.Combobox(padre, textvariable=self.var, values=campo.opciones,
                                   state="readonly", font=("Segoe UI", 10))

    def poner(self, valor):
        self.var.set(valor or "")

    def leer(self, campo):
        if not self.var.get():
            raise ErrorVoltia("Escoja " + campo.etiqueta.lower() + ".")
        return self.var.get()

    def limpiar(self):
        self.var.set(self.widget.cget("values")[0])

    def habilitar(self, si):
        self.widget.configure(state="readonly" if si else "disabled")


class _EntradaCheck:
    def __init__(self, padre, campo):
        self.var = tk.BooleanVar(value=False)
        self.widget = ttk.Checkbutton(padre, text=campo.etiqueta, variable=self.var)

    def poner(self, valor):
        self.var.set(bool(valor))

    def leer(self, campo):
        return bool(self.var.get())

    def limpiar(self):
        self.var.set(False)

    def habilitar(self, si):
        self.widget.configure(state="normal" if si else "disabled")


class _EntradaFechaHora:
    """Dos cajas: la fecha (AAAA-MM-DD) y la hora (HH:MM). Juntas forman el LocalDateTime."""

    def __init__(self, padre, campo):
        self.widget = ttk.Frame(padre)
        self.fecha = tk.StringVar()
        self.hora = tk.StringVar()
        self.caja_fecha = ttk.Entry(self.widget, textvariable=self.fecha, width=14, font=("Segoe UI", 10))
        self.caja_hora = ttk.Entry(self.widget, textvariable=self.hora, width=8, font=("Segoe UI", 10))
        self.caja_fecha.pack(side="left")
        self.caja_hora.pack(side="left", padx=(10, 0))
        self.limpiar()

    def poner(self, valor):
        try:
            momento = datetime.strptime(valor, "%Y-%m-%dT%H:%M:%S")
            self.fecha.set(momento.strftime("%Y-%m-%d"))
            self.hora.set(momento.strftime("%H:%M"))
        except (ValueError, TypeError):
            self.limpiar()

    def leer(self, campo):
        try:
            momento = datetime.strptime(self.fecha.get().strip() + " " + self.hora.get().strip(), "%Y-%m-%d %H:%M")
        except ValueError:
            raise ErrorVoltia("Escriba la fecha así: 2025-06-18 y la hora así: 14:30")
        return momento.strftime("%Y-%m-%dT%H:%M:%S")  # Formato del servidor

    def limpiar(self):
        ahora = datetime.now()
        self.fecha.set(ahora.strftime("%Y-%m-%d"))
        self.hora.set(ahora.strftime("%H:%M"))

    def habilitar(self, si):
        estado = "normal" if si else "readonly"
        self.caja_fecha.configure(state=estado)
        self.caja_hora.configure(state=estado)


_CLASES = {
    "entero": _EntradaEntero,
    "decimal": _EntradaDecimal,
    "texto": _EntradaTexto,
    "combo": _EntradaCombo,
    "check": _EntradaCheck,
    "fechahora": _EntradaFechaHora,
    "calculado": _EntradaCalculada,
}


# ====================================================================
#  El formulario completo
# ====================================================================

class Formulario(ttk.Frame):
    def __init__(self, padre, tipo):
        super().__init__(padre)
        self.tipo = tipo
        self.definicion = DEFINICIONES[tipo]
        self._entradas = {}              # atributo -> entrada
        self._filas_calculadas = []      # filas que solo se ven al buscar
        self.columnconfigure(0, weight=1)

        for numero, campos_fila in enumerate(self.definicion):
            fila = ttk.Frame(self)
            fila.grid(row=numero, column=0, sticky="ew", pady=5)
            fila.columnconfigure(0, weight=1, uniform="columna")
            fila.columnconfigure(1, weight=1, uniform="columna")

            for columna, campo in enumerate(campos_fila):
                celda = ttk.Frame(fila)
                # Si el campo va solo en la fila, ocupa las dos columnas
                ocupa = 2 if len(campos_fila) == 1 else 1
                celda.grid(row=0, column=columna, columnspan=ocupa, sticky="ew",
                           padx=(0, 0) if columna == 0 else (12, 0))
                entrada = _CLASES[campo.tipo](celda, campo)
                if campo.tipo != "check":
                    ttk.Label(celda, text=campo.etiqueta, style="Etiqueta.TLabel").pack(anchor="w")
                entrada.widget.pack(anchor="w" if campo.tipo in ("check", "fechahora") else "center",
                                    fill="x" if campo.tipo not in ("check", "fechahora") else "none",
                                    pady=(2, 0))
                self._entradas[campo.atributo] = entrada

            if all(c.tipo == "calculado" for c in campos_fila):
                self._filas_calculadas.append(fila)

        self.filas = [f for f in self.winfo_children()]
        self.limpiar()
        self.modo_agregar()

    # ---------------- Modos de uso ----------------

    def modo_agregar(self):
        """Todo editable y sin datos calculados."""
        for fila in self._filas_calculadas:
            fila.grid_remove()
        self._entradas["codigo"].habilitar(True)
        self.permitir_edicion(True)

    def modo_buscar(self):
        """Datos calculados visibles y todo bloqueado hasta que se busque algo."""
        for fila in self._filas_calculadas:
            fila.grid()
        self._entradas["codigo"].habilitar(False)
        self.permitir_edicion(False)

    def permitir_edicion(self, si):
        """Bloquea o desbloquea los campos que se pueden cambiar (el codigo nunca cambia aqui)."""
        for campos_fila in self.definicion:
            for campo in campos_fila:
                if campo.atributo != "codigo":
                    self._entradas[campo.atributo].habilitar(si)

    # ---------------- Limpiar, mostrar y leer ----------------

    def limpiar(self):
        for entrada in self._entradas.values():
            entrada.limpiar()

    def mostrar(self, objeto):
        """Muestra TODA la informacion del objeto en el formulario."""
        for campos_fila in self.definicion:
            for campo in campos_fila:
                self._entradas[campo.atributo].poner(getattr(objeto, campo.atributo))

    def leer(self, incluir_codigo):
        """Lee y valida todos los campos. Devuelve un diccionario con los nombres de GraphQL.
        Si algo esta mal lanza ErrorVoltia con un mensaje claro."""
        datos = {}
        for campos_fila in self.definicion:
            for campo in campos_fila:
                if campo.tipo == "calculado":
                    continue
                if campo.atributo == "codigo" and not incluir_codigo:
                    continue
                datos[campo.clave] = self._entradas[campo.atributo].leer(campo)
        return datos
