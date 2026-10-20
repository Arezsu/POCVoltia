"""Ventanas de los casos de uso (la V de MVC). Cada ventana hace UN solo caso de uso.

Las mismas ventanas sirven para la lavadora y para el programa de lavado: reciben el tipo
('lavadora' o 'programa') y el controlador y el formulario se encargan del resto.
"""
import tkinter as tk
from tkinter import messagebox, ttk

from voltia import animaciones, sonido, tareas
from voltia.configuracion import Configuracion
from voltia.controladores import ControladorObjeto
from voltia.graphql import ErrorVoltia
from voltia.observador import Notificador
from voltia.vista import tema
from voltia.vista.formularios import Formulario

_cascada = [0]  # Para que cada ventana nueva salga un poco corrida de la anterior


class VentanaBase(tk.Toplevel):
    """Ventana comun: titulo arriba, cuerpo en el centro y animacion de entrada."""

    def __init__(self, principal, titulo, ancho, alto):
        super().__init__(principal)
        self.principal = principal
        self.title(titulo)
        self.configure(bg=tema.FONDO)
        self.resizable(False, False)

        # Posicion: cerca de la ventana principal, un poco corrida en cada ventana nueva
        _cascada[0] = (_cascada[0] + 1) % 6
        x = principal.winfo_x() + 40 + _cascada[0] * 28
        y = principal.winfo_y() + 40 + _cascada[0] * 28
        self.geometry("%dx%d+%d+%d" % (ancho, alto, x, y))

        ttk.Label(self, text=titulo, style="Titulo.TLabel", anchor="center").pack(fill="x")
        self.cuerpo = ttk.Frame(self, padding=18)
        self.cuerpo.pack(fill="both", expand=True)

        self.protocol("WM_DELETE_WINDOW", self.cerrar)
        animaciones.aparecer(self)

    def cerrar(self):
        self.destroy()

    def aviso(self, mensaje, titulo="No se pudo completar"):
        """Mensaje de error: la ventana se sacude."""
        animaciones.sacudir(self)
        messagebox.showwarning(titulo, mensaje, parent=self)

    def informacion(self, mensaje, titulo):
        messagebox.showinfo(titulo, mensaje, parent=self)


class VentanaAgregar(VentanaBase):
    """Caso de uso ADICIONAR: se llena el formulario y el objeto se guarda en el servidor."""

    def __init__(self, principal, tipo):
        controlador = ControladorObjeto(tipo)
        super().__init__(principal, "ADICIONAR " + controlador.nombres["titulo"], 600, 520)
        self.controlador = controlador

        self.formulario = Formulario(self.cuerpo, tipo)
        self.formulario.modo_agregar()
        self.formulario.pack(fill="x")

        botones = ttk.Frame(self.cuerpo)
        botones.pack(side="bottom", pady=(14, 0))
        self.boton_aceptar = ttk.Button(botones, text="Aceptar", style="Azul.TButton", command=self.aceptar)
        self.boton_aceptar.pack(side="left", padx=6)
        ttk.Button(botones, text="Limpiar", style="Gris.TButton",
                   command=self.formulario.limpiar).pack(side="left", padx=6)

    def aceptar(self):
        try:
            datos = self.formulario.leer(incluir_codigo=True)  # Lee y valida el formulario
        except ErrorVoltia as error:
            self.aviso(str(error), "Revise los datos")
            return

        self.boton_aceptar.state(["disabled"])
        # El servidor se llama en un hilo aparte para no congelar la ventana
        tareas.ejecutar(self, lambda: self.controlador.agregar(datos), self._listo, self._fallo)

    def _listo(self, creado):
        self.boton_aceptar.state(["!disabled"])
        sonido.reproducir("lavadora.wav")
        self.informacion("Se adicionó %s con el código %d." % (self.controlador.nombres["articulo"], creado.codigo),
                         "Adicionado")
        self.formulario.limpiar()

    def _fallo(self, error):
        self.boton_aceptar.state(["!disabled"])
        self.aviso(str(error), "No se pudo adicionar")


class PanelBusqueda(ttk.Frame):
    """Barra de busqueda: se puede buscar por UNO o por DOS criterios
    (el codigo y/o la marca o el nombre). Si se escriben los dos, el objeto debe cumplir los dos."""

    def __init__(self, padre, tipo, al_buscar):
        super().__init__(padre)
        segundo = ControladorObjeto(tipo).nombres["segundo"].lower()
        self.columnconfigure(0, weight=1, uniform="busqueda")
        self.columnconfigure(1, weight=1, uniform="busqueda")

        self.codigo = tk.StringVar()
        self.texto = tk.StringVar()

        celda1 = ttk.Frame(self)
        celda1.grid(row=0, column=0, sticky="ew")
        ttk.Label(celda1, text="Buscar por código", style="Etiqueta.TLabel").pack(anchor="w")
        entrada1 = ttk.Entry(celda1, textvariable=self.codigo, font=("Segoe UI", 10))
        entrada1.pack(fill="x", pady=(2, 0))

        celda2 = ttk.Frame(self)
        celda2.grid(row=0, column=1, sticky="ew", padx=(12, 0))
        ttk.Label(celda2, text="y/o por " + segundo, style="Etiqueta.TLabel").pack(anchor="w")
        entrada2 = ttk.Entry(celda2, textvariable=self.texto, font=("Segoe UI", 10))
        entrada2.pack(fill="x", pady=(2, 0))

        # Boton con la lupa
        self._icono = tk.PhotoImage(file=Configuracion().ruta_asset("buscar.png")).subsample(3, 3)
        boton = ttk.Button(self, image=self._icono, command=al_buscar)
        boton.grid(row=0, column=2, sticky="s", padx=(8, 0))

        # La tecla Enter tambien busca
        entrada1.bind("<Return>", lambda evento: al_buscar())
        entrada2.bind("<Return>", lambda evento: al_buscar())

    def leer_criterios(self):
        """Devuelve (codigo, texto). Lo que se deje vacio llega como None."""
        escrito = self.codigo.get().strip()
        texto = self.texto.get().strip() or None
        codigo = None
        if escrito:
            try:
                codigo = int(escrito)
            except ValueError:
                codigo = -1
            if codigo <= 0:
                raise ErrorVoltia("El código debe ser un número entero mayor que cero.")
        if codigo is None and texto is None:
            raise ErrorVoltia("Escriba un código, un dato de búsqueda o los dos.")
        return codigo, texto


class VentanaConBusqueda(VentanaBase):
    """Base de Consultar, Actualizar y Eliminar: los tres primero BUSCAN el objeto y
    muestran TODA su informacion en el formulario. Cada una decide que hacer despues."""

    def __init__(self, principal, tipo, accion):
        controlador = ControladorObjeto(tipo)
        super().__init__(principal, accion + " " + controlador.nombres["titulo"], 600, 620)
        self.controlador = controlador
        self.objeto = None  # El objeto que se encontro

        self.panel = PanelBusqueda(self.cuerpo, tipo, self.buscar)
        self.panel.pack(fill="x", pady=(0, 10))

        self.formulario = Formulario(self.cuerpo, tipo)
        self.formulario.modo_buscar()  # Todo bloqueado hasta que se busque
        self.formulario.pack(fill="x")

        self.botones = ttk.Frame(self.cuerpo)
        self.botones.pack(side="bottom", pady=(14, 0))

    def buscar(self):
        try:
            codigo, texto = self.panel.leer_criterios()
        except ErrorVoltia as error:
            self.aviso(str(error), "Falta un criterio")
            return
        tareas.ejecutar(self, lambda: self.controlador.buscar(codigo, texto), self._buscado, self._fallo_busqueda)

    def _buscado(self, objeto):
        if objeto is None:
            self.objeto = None
            self.formulario.limpiar()
            self.al_no_encontrar()
            self.aviso("No existe %s con esos datos." % self.controlador.nombres["articulo"], "No encontrado")
            return
        self.objeto = objeto
        self.formulario.mostrar(objeto)  # Se muestra TODA la informacion
        self.al_encontrar(objeto)

    def _fallo_busqueda(self, error):
        self.aviso(str(error), "Error")

    # Cada ventana hija cambia estos dos metodos
    def al_encontrar(self, objeto):
        pass

    def al_no_encontrar(self):
        pass


class VentanaConsultar(VentanaConBusqueda):
    """Caso de uso CONSULTAR: busca UN objeto y lo muestra en el mismo formulario de Adicionar,
    pero de solo lectura."""

    def __init__(self, principal, tipo):
        super().__init__(principal, tipo, "CONSULTAR")
        ttk.Button(self.botones, text="Cerrar", style="Gris.TButton", command=self.cerrar).pack()


class VentanaActualizar(VentanaConBusqueda):
    """Caso de uso ACTUALIZAR: primero se busca, se muestra TODA la informacion,
    el usuario cambia lo que quiera y se manda al servidor."""

    def __init__(self, principal, tipo):
        super().__init__(principal, tipo, "ACTUALIZAR")
        self.boton_actualizar = ttk.Button(self.botones, text="Actualizar", style="Amarillo.TButton",
                                           command=self.actualizar)
        self.boton_actualizar.pack(side="left", padx=6)
        self.boton_actualizar.state(["disabled"])  # Se habilita cuando se encuentra algo
        ttk.Button(self.botones, text="Cerrar", style="Gris.TButton", command=self.cerrar).pack(side="left", padx=6)

    def al_encontrar(self, objeto):
        self.formulario.permitir_edicion(True)
        self.boton_actualizar.state(["!disabled"])

    def al_no_encontrar(self):
        self.formulario.permitir_edicion(False)
        self.boton_actualizar.state(["disabled"])

    def actualizar(self):
        try:
            datos = self.formulario.leer(incluir_codigo=False)  # El codigo no cambia
        except ErrorVoltia as error:
            self.aviso(str(error), "Revise los datos")
            return

        codigo = self.objeto.codigo  # Se actualiza SIEMPRE el objeto que se encontro
        self.boton_actualizar.state(["disabled"])
        tareas.ejecutar(self, lambda: self.controlador.actualizar(codigo, datos), self._listo, self._fallo)

    def _listo(self, actualizado):
        self.boton_actualizar.state(["!disabled"])
        self.objeto = actualizado
        self.formulario.mostrar(actualizado)  # Se ven los datos calculados nuevos
        self.informacion("Se actualizó %s con éxito." % self.controlador.nombres["articulo"], "Actualizado")

    def _fallo(self, error):
        self.boton_actualizar.state(["!disabled"])
        self.aviso(str(error), "No se pudo actualizar")


class VentanaEliminar(VentanaConBusqueda):
    """Caso de uso ELIMINAR: se busca, se muestra TODA la informacion y antes de borrar
    se pide confirmacion."""

    def __init__(self, principal, tipo):
        super().__init__(principal, tipo, "ELIMINAR")
        self.boton_eliminar = ttk.Button(self.botones, text="Eliminar", style="Rojo.TButton", command=self.eliminar)
        self.boton_eliminar.pack(side="left", padx=6)
        self.boton_eliminar.state(["disabled"])  # Se habilita cuando se encuentra algo
        ttk.Button(self.botones, text="Cerrar", style="Gris.TButton", command=self.cerrar).pack(side="left", padx=6)

    def al_encontrar(self, objeto):
        self.boton_eliminar.state(["!disabled"])

    def al_no_encontrar(self):
        self.boton_eliminar.state(["disabled"])

    def eliminar(self):
        articulo = self.controlador.nombres["articulo"]
        if not messagebox.askyesno("Confirmar eliminación",
                                   "¿Seguro que desea eliminar %s con código %d?" % (articulo, self.objeto.codigo),
                                   parent=self):
            return

        codigo = self.objeto.codigo
        self.boton_eliminar.state(["disabled"])
        tareas.ejecutar(self, lambda: self.controlador.eliminar(codigo), self._listo, self._fallo)

    def _listo(self, eliminado):
        if eliminado:
            self.informacion("Se eliminó %s con éxito." % self.controlador.nombres["articulo"], "Eliminado")
            self.objeto = None
            self.formulario.limpiar()
        else:
            self.boton_eliminar.state(["!disabled"])
            self.aviso("No se pudo eliminar.", "Error")

    def _fallo(self, error):
        # Por ejemplo: un programa que todavia usan lavadoras no se puede eliminar
        self.boton_eliminar.state(["!disabled"])
        self.aviso(str(error), "No se pudo eliminar")


# Columnas de la tabla: (titulo, atributo que se muestra, ancho, atributo para ordenar)
COLUMNAS = {
    "lavadora": [
        ("Código", "codigo", 70, "codigo"),
        ("Marca", "marca", 100, "marca"),
        ("Fecha Fab.", "fecha_texto", 130, "fecha_fabricacion"),
        ("Precio Base", "precio_base_texto", 110, "precio_base"),
        ("Capacidad", "capacidad_texto", 85, "capacidad_carga_kilos"),
        ("Inverter", "inverter_texto", 70, "tecnologia_inverter"),
        ("Centrifugado", "rpm_texto", 105, "velocidad_centrifugado_rpm"),
        ("Programa", "programa_nombre", 100, "programa_nombre"),
        ("Precio Final", "precio_final_texto", 110, "precio_final"),
    ],
    "programa": [
        ("Código", "codigo", 70, "codigo"),
        ("Nombre", "nombre", 160, "nombre"),
        ("Rango", "rango_texto", 130, "min_minutos"),
        ("Agua", "agua_texto", 90, "consumo_agua_litros"),
        ("Agua caliente", "caliente_texto", 110, "agua_caliente"),
        ("Fecha creación", "fecha_texto", 150, "fecha_creacion"),
        ("Lavadoras", "cantidad_lavadoras", 90, "cantidad_lavadoras"),
    ],
}


class VentanaListar(VentanaBase):
    """Caso de uso LISTAR: todos los objetos en una tabla, con 2 filtros que hace el SERVIDOR.

    Patron OBSERVER: si en otra ventana se adiciona, actualiza o elimina algo, esta tabla se
    recarga sola. La ventana se suscribe al Notificador y se quita al cerrarse."""

    def __init__(self, principal, tipo):
        controlador = ControladorObjeto(tipo)
        super().__init__(principal, "LISTAR " + controlador.nombres["titulo"], 980, 560)
        self.controlador = controlador
        self.tipo = tipo
        self.objetos = []
        self._orden_inverso = False

        self._armar_filtros()
        self._armar_tabla()
        self._armar_pie()

        Notificador().suscribir(self.cargar)  # Observer: avisame cuando cambien los datos
        self.cargar()

    def cerrar(self):
        Notificador().desuscribir(self.cargar)  # Ya no se quiere recibir avisos
        self.destroy()

    def _armar_filtros(self):
        marco = ttk.Frame(self.cuerpo)
        marco.pack(fill="x", pady=(0, 10))
        marco.columnconfigure(0, weight=1)
        marco.columnconfigure(1, weight=1)

        self.filtro_texto = tk.StringVar()
        celda1 = ttk.Frame(marco)
        celda1.grid(row=0, column=0, sticky="ew")
        ttk.Label(celda1, text="Filtro 1: %s (contiene el texto)" % self.controlador.nombres["segundo"],
                  style="Etiqueta.TLabel").pack(anchor="w")
        ttk.Entry(celda1, textvariable=self.filtro_texto, font=("Segoe UI", 10)).pack(fill="x", pady=(2, 0))

        # El segundo filtro cambia: inverter en la lavadora y agua caliente en el programa
        if self.tipo == "lavadora":
            etiqueta2, opciones = "Filtro 2: Tecnología Inverter", ["Todos", "Con inverter", "Sin inverter"]
        else:
            etiqueta2, opciones = "Filtro 2: Agua caliente", ["Todos", "Con agua caliente", "Sin agua caliente"]
        celda2 = ttk.Frame(marco)
        celda2.grid(row=0, column=1, sticky="ew", padx=(12, 0))
        ttk.Label(celda2, text=etiqueta2, style="Etiqueta.TLabel").pack(anchor="w")
        self.filtro_opcion = ttk.Combobox(celda2, values=opciones, state="readonly", font=("Segoe UI", 10))
        self.filtro_opcion.current(0)
        self.filtro_opcion.pack(fill="x", pady=(2, 0))

        botones = ttk.Frame(marco)
        botones.grid(row=0, column=2, sticky="s", padx=(12, 0))
        ttk.Button(botones, text="Filtrar", style="Azul.TButton", command=self.cargar).pack(side="left", padx=3)
        ttk.Button(botones, text="Quitar filtros", style="Gris.TButton",
                   command=self._quitar_filtros).pack(side="left", padx=3)

    def _armar_tabla(self):
        marco = ttk.Frame(self.cuerpo)
        marco.pack(fill="both", expand=True)

        columnas = COLUMNAS[self.tipo]
        self.tabla = ttk.Treeview(marco, columns=[c[1] for c in columnas], show="headings", selectmode="browse")
        for titulo, atributo, ancho, orden in columnas:
            # Al hacer click en el titulo, la tabla se ordena por esa columna
            self.tabla.heading(atributo, text=titulo, command=lambda o=orden: self._ordenar(o))
            self.tabla.column(atributo, width=ancho, anchor="center")
        self.tabla.tag_configure("impar", background=tema.FONDO_CLARO)

        barra = ttk.Scrollbar(marco, orient="vertical", command=self.tabla.yview)
        self.tabla.configure(yscrollcommand=barra.set)
        self.tabla.pack(side="left", fill="both", expand=True)
        barra.pack(side="right", fill="y")

    def _armar_pie(self):
        pie = ttk.Frame(self.cuerpo)
        pie.pack(fill="x", pady=(10, 0))
        self.contador = ttk.Label(pie, text="0 encontrado(s)", style="Pantalla.TLabel", width=30)
        self.contador.pack(side="left")
        ttk.Button(pie, text="Cerrar", style="Gris.TButton", command=self.cerrar).pack(side="right")

    def _quitar_filtros(self):
        self.filtro_texto.set("")
        self.filtro_opcion.current(0)
        self.cargar()

    def cargar(self):
        """Pide al servidor la lista con los filtros escogidos y la muestra en la tabla."""
        texto = self.filtro_texto.get().strip() or None  # Vacio = sin filtro
        indice = self.filtro_opcion.current()
        opcion = None if indice <= 0 else (indice == 1)  # None = todos, True = con..., False = sin...
        tareas.ejecutar(self, lambda: self.controlador.listar(texto, opcion), self._mostrar, self._fallo)

    def _mostrar(self, objetos):
        self.objetos = objetos
        self._llenar()
        animaciones.contar(self.contador, len(objetos), "{} encontrado(s)")

    def _llenar(self):
        self.tabla.delete(*self.tabla.get_children())
        for numero, objeto in enumerate(self.objetos):
            valores = [getattr(objeto, c[1]) for c in COLUMNAS[self.tipo]]
            self.tabla.insert("", "end", values=valores, tags=("impar",) if numero % 2 else ())

    def _ordenar(self, atributo):
        self._orden_inverso = not self._orden_inverso
        self.objetos.sort(key=lambda o: getattr(o, atributo), reverse=self._orden_inverso)
        self._llenar()

    def _fallo(self, error):
        self.aviso(str(error), "No se pudo listar")


class VentanaAcercaDe(VentanaBase):
    """Menu Ayuda > Acerca de...: integrantes del equipo y version de la aplicacion."""

    def __init__(self, principal):
        super().__init__(principal, "ACERCA DE", 520, 380)
        info = Configuracion()

        ttk.Label(self.cuerpo, text=info.nombre_app, font=("Segoe UI", 13, "bold"),
                  foreground=tema.AZUL_OSCURO, wraplength=460, justify="center").pack(pady=(4, 10))
        ttk.Label(self.cuerpo, text="Versión: %s\n%s" % (info.version, info.fecha),
                  font=("Segoe UI", 10, "bold"), justify="center").pack(pady=(0, 14))
        ttk.Label(self.cuerpo, text="Integrantes del equipo", style="Rotulo.TLabel").pack()
        ttk.Label(self.cuerpo, text="\n".join(info.integrantes), justify="center",
                  font=("Segoe UI", 10)).pack(pady=(4, 14))
        ttk.Button(self.cuerpo, text="Cerrar", style="Azul.TButton", command=self.cerrar).pack()
