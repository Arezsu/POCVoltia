/* =====================================================================
   VISTA (la V del patron MVC)
   =====================================================================
   Esta es la unica capa que toca el DOM: dibuja la tabla, el panel de
   detalle, los modales, los mensajes y la compuerta.

   Reglas de la arquitectura:
     - La vista NO llama al servicio GraphQL: solo llama al controlador.
     - La vista NO guarda datos del negocio: recibe objetos del modelo ya
       construidos por el controlador y los muestra.
     - La vista se suscribe al notificador y se redibuja cuando le avisan.

   Manejo de modales:
     - Se abren y cierran por id, con foco al primer campo.
     - Escape cierra el modal de arriba y el foco regresa al boton que lo abrio.
     - Un clic en el fondo oscuro tambien cierra.
     - El tabulador queda atrapado dentro del modal mientras esta abierto.
   ===================================================================== */
(function (global) {
  'use strict';

  const Voltia = global.Voltia = global.Voltia || {};
  const EVENTOS = Voltia.EVENTOS;

  // Relacion entre el nombre del campo en el modelo y el sufijo del id en el
  // HTML. Se usa para pintar cada mensaje de error debajo de su campo.
  const CAMPOS_LAVADORA = {
    codigo: 'codigo',
    marca: 'marca',
    precioBase: 'precio',
    fechaFabricacion: 'fecha',
    capacidadCargaKilos: 'capacidad',
    velocidadCentrifugadoRPM: 'rpm',
    tecnologiaInverter: 'inverter',
    minutosLavado: 'minutos'
  };

  class Vista {
    constructor(notificador) {
      this.notificador = notificador;
      this.controladorLavadoras = null;
      this.controladorMantenimientos = null;
      this.refs = {};
      this.estado = {
        lavadoras: [],
        filtros: { marca: '', tecnologiaInverter: null, conMantenimientos: null },
        total: 0,
        seleccionada: null,
        versionDatos: null
      };

      this.trabajando = false;
      this.estadoError = false;
      this.temporizadorError = null;
      this.ultimoFoco = null;

      this.lavadoraActualizar = null;
      this.lavadoraEliminar = null;
      this.lavadoraMantenimientos = null;
      this.mantenimientoEnEdicion = null;
      this.mantenimientoAEliminar = null;
      this.modoMantenimientos = 'listar';
      this.versionMantenimientos = 0;
    }

    // =================================================================
    //  Arranque
    // =================================================================
    iniciar(controladorLavadoras, controladorMantenimientos) {
      this.controladorLavadoras = controladorLavadoras;
      this.controladorMantenimientos = controladorMantenimientos;
      this.crearVentanasMantenimiento();
      this.capturarReferencias();
      this.suscribirEventos();
      this.conectarEventos();

      this.renderTabla();
      this.renderDetalle(null);
      this.renderVersion(null);
      this.renderConexion(false);
      this.pintarEstadoMaquina();
    }

    capturarReferencias() {
      const porId = (id) => document.getElementById(id);

      this.refs = {
        // Encabezado
        pantallaEstado: porId('pantalla-estado'),
        pantallaVersion: porId('pantalla-version'),
        indicadorConexion: porId('indicador-conexion'),
        textoConexion: porId('texto-conexion'),
        compuertaEstado: porId('compuerta-estado'),

        // Filtros y acciones
        formFiltros: porId('form-filtros'),
        filtroMarca: porId('filtro-marca'),
        filtroInverter: porId('filtro-inverter'),
        filtroMantenimientos: porId('filtro-mantenimientos'),
        botonLimpiarFiltros: porId('boton-limpiar-filtros'),
        resumenFiltros: porId('resumen-filtros'),
        botonAgregar: porId('boton-agregar'),
        botonListar: porId('boton-listar'),
        botonConsultar: porId('boton-consultar'),
        botonActualizar: porId('boton-actualizar'),
        botonEliminar: porId('boton-eliminar'),
        botonMantenimientos: porId('boton-mantenimientos'),
        botonListarMantenimientos: porId('boton-listar-mantenimientos'),
        botonAgregarMantenimientoMenu: porId('boton-agregar-mantenimiento-menu'),
        botonConsultarMantenimiento: porId('boton-consultar-mantenimiento'),
        botonActualizarMantenimiento: porId('boton-actualizar-mantenimiento'),
        botonEliminarMantenimiento: porId('boton-eliminar-mantenimiento'),
        botonAcerca: porId('boton-acerca'),
        contenidoMantenimientos: porId('contenido-mantenimientos'),
        tablaMantenimientos: document.querySelector('#modal-mantenimientos [data-parte="tabla"]'),
        filtrosMantenimientos: document.querySelector('#modal-mantenimientos [data-parte="filtros"]'),
        accionesListadoMantenimientos: document.querySelector('#modal-mantenimientos [data-parte="acciones-listado"]'),
        encabezadoAccionesMantenimiento: document.querySelector('#modal-mantenimientos th[data-col="acciones"]'),

        // Tabla
        contadorLavadoras: porId('contador-lavadoras'),
        tablaScroll: porId('tabla-scroll'),
        cuerpoTabla: porId('cuerpo-tabla'),
        tablaVacio: porId('tabla-vacio'),

        // Detalle
        detalle: porId('detalle-contenido'),

        // Modal agregar
        formAgregar: porId('form-agregar'),
        agFecha: porId('ag-fecha'),
        agPrecioFinal: porId('ag-precio-final'),
        errorAgregar: porId('error-agregar'),
        botonGuardarLavadora: porId('boton-guardar-lavadora'),

        // Modal consultar
        formConsultar: porId('form-consultar'),
        coCodigo: porId('co-codigo'),
        errorConsultar: porId('error-consultar'),
        errorCoCodigo: porId('error-co-codigo'),
        resultadoConsulta: porId('resultado-consulta'),

        // Modal actualizar
        formBuscarActualizar: porId('form-buscar-actualizar'),
        acCodigo: porId('ac-codigo'),
        botonBuscarActualizar: porId('boton-buscar-actualizar'),
        errorBuscarActualizar: porId('error-buscar-actualizar'),
        resultadoActualizar: porId('resultado-actualizar'),
        formActualizar: porId('form-actualizar'),
        acCodigoFijo: porId('ac-codigo-fijo'),
        acPrecioFinal: porId('ac-precio-final'),
        errorActualizar: porId('error-actualizar'),
        botonGuardarActualizar: porId('boton-guardar-actualizar'),

        // Modal eliminar
        formBuscarEliminar: porId('form-buscar-eliminar'),
        elCodigo: porId('el-codigo'),
        errorEliminar: porId('error-eliminar'),
        resultadoEliminar: porId('resultado-eliminar'),
        confirmacionEliminar: porId('confirmacion-eliminar'),
        textoConfirmacionEliminar: porId('texto-confirmacion-eliminar'),
        elConfirmar: porId('el-confirmar'),
        errorConfirmacionEliminar: porId('error-confirmacion-eliminar'),
        botonConfirmarEliminar: porId('boton-confirmar-eliminar'),

        // Modal mantenimientos
        modalMantenimientos: porId('modal-mantenimientos'),
        matLavadora: porId('mat-lavadora'),
        formFiltrosMantenimientos: porId('form-filtros-mantenimientos'),
        matFiltroTecnico: porId('mat-filtro-tecnico'),
        matFiltroCompletado: porId('mat-filtro-completado'),
        botonLimpiarFiltrosMantenimientos: porId('boton-limpiar-filtros-mantenimientos'),
        botonAgregarMantenimiento: porId('boton-agregar-mantenimiento'),
        contadorMantenimientos: porId('contador-mantenimientos'),
        errorMantenimientos: porId('error-mantenimientos'),
        cuerpoMantenimientos: porId('cuerpo-mantenimientos'),
        mantenimientosVacio: porId('mantenimientos-vacio'),
        formMantenimiento: porId('form-mantenimiento'),
        tituloFormMantenimiento: porId('titulo-form-mantenimiento'),
        matDescripcion: porId('mat-descripcion'),
        matTecnico: porId('mat-tecnico'),
        matCosto: porId('mat-costo'),
        matFecha: porId('mat-fecha'),
        matCompletado: porId('mat-completado'),
        errorMantenimiento: porId('error-mantenimiento'),
        botonCancelarMantenimiento: porId('boton-cancelar-mantenimiento'),
        botonGuardarMantenimiento: porId('boton-guardar-mantenimiento'),
        confirmacionMantenimiento: porId('confirmacion-mantenimiento'),
        textoConfirmacionMantenimiento: porId('texto-confirmacion-mantenimiento'),
        botonConfirmarEliminarMantenimiento: porId('boton-confirmar-eliminar-mantenimiento'),
        botonCancelarEliminarMantenimiento: porId('boton-cancelar-eliminar-mantenimiento'),

        // Avisos
        toasts: porId('toasts')
      };
    }

    /**
     * La vista es un SUSCRIPTOR del notificador. Cuando el controlador avisa,
     * la vista reacciona. Nunca hay llamadas directas del controlador al DOM.
     */
    suscribirEventos() {
      this.notificador.suscribir(EVENTOS.DATOS, (estado) => {
        this.estado = estado;
        this.renderTabla();
        this.renderDetalle(estado.seleccionada);
        this.renderVersion(estado.versionDatos);
      });

      this.notificador.suscribir(EVENTOS.SELECCION, (lavadora) => {
        this.estado.seleccionada = lavadora;
        this.marcarSeleccion(lavadora ? lavadora.codigo : null);
        this.renderDetalle(lavadora);
      });

      this.notificador.suscribir(EVENTOS.TRABAJANDO, (trabajando) => {
        this.trabajando = trabajando === true;
        this.pintarEstadoMaquina();
      });

      this.notificador.suscribir(EVENTOS.MENSAJE, (mensaje) => {
        this.mostrarMensaje(mensaje);
      });

      this.notificador.suscribir(EVENTOS.CONEXION, (datos) => {
        this.renderConexion(datos && datos.conectado === true);
      });
    }

    conectarEventos() {
      // Filtros
      this.escuchar(this.refs.formFiltros, 'submit', (evento) => this.aplicarFiltros(evento));
      this.escuchar(this.refs.botonLimpiarFiltros, 'click', () => this.limpiarFiltros());

      // Acciones sobre la lavadora
      this.escuchar(this.refs.botonAgregar, 'click', () => this.abrirAgregar());
      this.escuchar(this.refs.botonListar, 'click', () => {
        document.getElementById('panel-tabla').scrollIntoView({ behavior: 'smooth', block: 'start' });
        document.getElementById('tabla-lavadoras').focus();
      });
      this.escuchar(this.refs.botonConsultar, 'click', () => this.abrirConsultar());
      this.escuchar(this.refs.botonActualizar, 'click', () => this.abrirActualizar());
      this.escuchar(this.refs.botonEliminar, 'click', () => this.abrirEliminar());
      this.escuchar(this.refs.botonMantenimientos, 'click', () => this.abrirMantenimientos(null));
      this.escuchar(this.refs.botonListarMantenimientos, 'click', () => this.abrirMantenimientos(null, 'listar'));
      this.escuchar(this.refs.botonAgregarMantenimientoMenu, 'click', () => this.abrirCasoMantenimiento('agregar'));
      this.escuchar(this.refs.botonConsultarMantenimiento, 'click', () => this.abrirCasoMantenimiento('consultar'));
      this.escuchar(this.refs.botonActualizarMantenimiento, 'click', () => this.abrirCasoMantenimiento('actualizar'));
      this.escuchar(this.refs.botonEliminarMantenimiento, 'click', () => this.abrirCasoMantenimiento('eliminar'));
      this.escuchar(this.refs.botonAcerca, 'click', () => this.abrirModal('modal-acerca'));

      this.escuchar(document, 'submit', (evento) => {
        const formulario = evento.target.closest('[data-form-mantenimiento-caso]');
        if (formulario) this.enviarCasoMantenimiento(evento, formulario);
      });
      this.escuchar(document, 'click', (evento) => {
        const boton = evento.target.closest('[data-accion-mantenimiento-caso]');
        if (boton) this.ejecutarAccionMantenimientoCaso(boton);
      });

      document.addEventListener('click', (evento) => {
        if (evento.target.closest('.boton-caso')) this.reproducirSonido('click');
      });

      // Tabla: seleccion con clic y con teclado
      this.escuchar(this.refs.cuerpoTabla, 'click', (evento) => this.alHacerClicTabla(evento));
      this.escuchar(this.refs.cuerpoTabla, 'keydown', (evento) => this.alTeclearTabla(evento));

      // Panel de detalle (boton gestionar mantenimientos)
      this.escuchar(this.refs.detalle, 'click', (evento) => this.alHacerClicDetalle(evento));

      // Cierre de modales por boton, por fondo y por Escape
      Array.from(document.querySelectorAll('[data-cerrar]')).forEach((boton) => {
        this.escuchar(boton, 'click', () => this.cerrarModal(boton.dataset.cerrar));
      });
      Array.from(document.querySelectorAll('.modal-fondo')).forEach((fondo) => {
        this.escuchar(fondo, 'mousedown', (evento) => {
          if (evento.target === fondo) {
            this.cerrarModal(fondo.id);
          }
        });
      });
      this.escuchar(document, 'keydown', (evento) => this.alPresionarTecla(evento));

      // Formulario agregar
      this.escuchar(this.refs.formAgregar, 'submit', (evento) => this.enviarAgregar(evento));
      this.escuchar(this.refs.formAgregar, 'input', () => this.actualizarVistaPrecio('ag'));
      // La casilla de inverter avisa por 'change': se escucha tambien para que la
      // vista previa del precio final se actualice al marcarla.
      this.escuchar(this.refs.formAgregar, 'change', () => this.actualizarVistaPrecio('ag'));

      // Formulario consultar
      this.escuchar(this.refs.formConsultar, 'submit', (evento) => this.buscarConsulta(evento));

      // Formulario actualizar
      this.escuchar(this.refs.formBuscarActualizar, 'submit', (evento) => this.buscarParaActualizar(evento));
      this.escuchar(this.refs.formActualizar, 'submit', (evento) => this.enviarActualizar(evento));
      this.escuchar(this.refs.formActualizar, 'input', () => this.actualizarVistaPrecio('ac'));
      this.escuchar(this.refs.formActualizar, 'change', () => this.actualizarVistaPrecio('ac'));

      // Formulario eliminar
      this.escuchar(this.refs.formBuscarEliminar, 'submit', (evento) => this.buscarParaEliminar(evento));
      this.escuchar(this.refs.elConfirmar, 'input', () => this.revisarConfirmacionEliminar());
      this.escuchar(this.refs.botonConfirmarEliminar, 'click', () => this.confirmarEliminar());

      // Mantenimientos
      this.escuchar(this.refs.matLavadora, 'change', () => {
        this.cerrarFormularioMantenimiento();
        this.cancelarEliminarMantenimiento();
        if (this.modoMantenimientos === 'agregar') {
          const codigo = Number(this.refs.matLavadora.value);
          this.lavadoraMantenimientos = codigo ? { codigo: codigo, mantenimientos: [] } : null;
          if (codigo) this.abrirFormularioMantenimiento(null, 'agregar');
        } else {
          this.cargarMantenimientos();
        }
      });
      this.escuchar(this.refs.formFiltrosMantenimientos, 'submit', (evento) => {
        evento.preventDefault();
        this.cerrarFormularioMantenimiento();
        this.cargarMantenimientos();
      });
      this.escuchar(this.refs.botonLimpiarFiltrosMantenimientos, 'click', () => {
        this.refs.matFiltroTecnico.value = '';
        this.refs.matFiltroCompletado.value = '';
        this.cerrarFormularioMantenimiento();
        this.cargarMantenimientos();
      });
      this.escuchar(this.refs.botonAgregarMantenimiento, 'click', () => this.abrirCasoMantenimiento('agregar', this.refs.matLavadora.value));
      this.escuchar(this.refs.formMantenimiento, 'submit', (evento) => this.guardarMantenimiento(evento));
      this.escuchar(this.refs.botonCancelarMantenimiento, 'click', () => this.cerrarFormularioMantenimiento());
      this.escuchar(this.refs.cuerpoMantenimientos, 'click', (evento) => this.alHacerClicMantenimientos(evento));
      this.escuchar(this.refs.botonConfirmarEliminarMantenimiento, 'click', () => this.confirmarEliminarMantenimiento());
      this.escuchar(this.refs.botonCancelarEliminarMantenimiento, 'click', () => this.cancelarEliminarMantenimiento());
    }

    crearVentanasMantenimiento() {
      const definiciones = {
        agregar: ['Agregar mantenimiento', 'Guardar mantenimiento'],
        consultar: ['Consultar mantenimiento', 'Buscar mantenimiento'],
        actualizar: ['Actualizar mantenimiento', 'Buscar mantenimiento'],
        eliminar: ['Eliminar mantenimiento', 'Buscar mantenimiento']
      };
      const campos = (prefijo, soloLectura) => `
        <div class="rejilla-campos" data-detalle-mantenimiento hidden>
          <div class="campo"><label>Descripción</label><input name="descripcion" required maxlength="160" ${soloLectura ? 'readonly' : ''}></div>
          <div class="campo"><label>Técnico</label><input name="tecnico" required maxlength="100" ${soloLectura ? 'readonly' : ''}></div>
          <div class="campo"><label>Costo (pesos)</label><input name="costo" type="number" min="0" step="0.01" required ${soloLectura ? 'readonly' : ''}></div>
          <div class="campo"><label>Fecha</label><input name="fecha" type="datetime-local" required ${soloLectura ? 'readonly' : ''}></div>
          <label class="casilla"><input name="completado" type="checkbox" ${soloLectura ? 'disabled' : ''}><span>Completado</span></label>
        </div>`;
      Object.entries(definiciones).forEach(([caso, [titulo, textoBoton]]) => {
        const id = `modal-mantenimiento-${caso}`;
        const alta = caso === 'agregar';
        const formulario = alta ? `
          <form data-form-mantenimiento-caso="${caso}" novalidate>
            <div class="campo"><label>Lavadora</label><select name="codigoLavadora" required></select></div>
            ${campos(caso, false)}
            <p class="error-formulario" data-error-mantenimiento-caso role="alert"></p>
            <div class="modal-pie"><button type="button" class="boton secundario" data-cerrar="${id}">Cancelar</button><button class="boton primario" type="submit">${textoBoton}</button></div>
          </form>` : `
          <form data-form-mantenimiento-caso="${caso}" novalidate>
            <div class="rejilla-campos">
              <div class="campo"><label>Código de lavadora</label><input name="codigoLavadora" type="number" min="1" step="1" required></div>
              <div class="campo"><label>ID del mantenimiento</label><input name="idMantenimiento" type="number" min="1" step="1" required></div>
            </div>
            <p class="error-formulario" data-error-mantenimiento-caso role="alert"></p>
            <button class="boton primario" type="submit">${textoBoton}</button>
            ${campos(caso, caso !== 'actualizar')}
            <div class="modal-pie" data-acciones-mantenimiento-caso hidden>
              <button type="button" class="boton secundario" data-cerrar="${id}">Cerrar</button>
              ${caso === 'actualizar' ? '<button type="button" class="boton primario" data-accion-mantenimiento-caso="guardar">Guardar cambios</button>' : ''}
              ${caso === 'eliminar' ? '<button type="button" class="boton peligro" data-accion-mantenimiento-caso="eliminar">Confirmar eliminación</button>' : ''}
            </div>
          </form>`;
        const fondo = document.createElement('div');
        fondo.className = 'modal-fondo';
        fondo.id = id;
        fondo.hidden = true;
        fondo.innerHTML = `<section class="modal ancho" role="dialog" aria-modal="true" aria-labelledby="titulo-${id}">
          <header class="modal-cabecera"><h2 id="titulo-${id}">${titulo}</h2><button type="button" class="boton-cerrar" data-cerrar="${id}">Cerrar</button></header>
          <div class="modal-cuerpo">${formulario}</div></section>`;
        document.body.appendChild(fondo);
      });
    }

    abrirCasoMantenimiento(caso, codigoLavadora) {
      const fondo = document.getElementById(`modal-mantenimiento-${caso}`);
      const formulario = fondo && fondo.querySelector('[data-form-mantenimiento-caso]');
      if (!formulario) return;
      formulario.reset();
      formulario.querySelector('[data-error-mantenimiento-caso]').textContent = '';
      formulario.querySelector('[data-detalle-mantenimiento]').hidden = caso !== 'agregar';
      const acciones = formulario.querySelector('[data-acciones-mantenimiento-caso]');
      if (acciones) acciones.hidden = true;
      const selector = formulario.querySelector('[name="codigoLavadora"]');
      if (selector && selector.tagName === 'SELECT') {
        selector.replaceChildren(...(this.estado.lavadoras || []).map((lavadora) => {
          const opcion = document.createElement('option');
          opcion.value = String(lavadora.codigo);
          opcion.textContent = lavadora.titulo;
          return opcion;
        }));
        if (codigoLavadora) selector.value = String(codigoLavadora);
        selector.disabled = selector.options.length === 0;
      } else if (codigoLavadora) selector.value = String(codigoLavadora);
      this.abrirModal(fondo.id);
      if (caso === 'agregar') formulario.querySelector('[name="descripcion"]').focus();
    }

    async enviarCasoMantenimiento(evento, formulario) {
      evento.preventDefault();
      const caso = formulario.dataset.formMantenimientoCaso;
      const error = formulario.querySelector('[data-error-mantenimiento-caso]');
      error.textContent = '';
      if (caso === 'agregar') {
        const datos = this.leerDatosCasoMantenimiento(formulario);
        const errores = Voltia.Validaciones.validarMantenimiento(datos);
        if (Voltia.Validaciones.hayErrores(errores)) {
          error.textContent = Object.values(errores).filter(Boolean).join(' ');
          return;
        }
        const codigo = Number(formulario.elements.codigoLavadora.value);
        try {
          await this.controladorMantenimientos.agregar(codigo, datos);
          this.reproducirSonido('lavadora');
          this.cerrarModal('modal-mantenimiento-agregar');
          this.llenarSelectorLavadoras(this.refs.matLavadora.value);
        } catch (e) { error.textContent = e.message; }
        return;
      }
      const codigo = Number(formulario.elements.codigoLavadora.value);
      const id = Number(formulario.elements.idMantenimiento.value);
      if (!Number.isSafeInteger(codigo) || codigo < 1 || !Number.isSafeInteger(id) || id < 1) {
        error.textContent = 'Ingrese un código de lavadora y un ID de mantenimiento válidos.';
        return;
      }
      formulario.querySelector('[data-detalle-mantenimiento]').hidden = true;
      formulario.querySelector('[data-acciones-mantenimiento-caso]').hidden = true;
      this.datosMantenimientoCaso = null;
      try {
        const mantenimiento = await this.controladorMantenimientos.consultarPorId(codigo, id);
        if (!mantenimiento) { error.textContent = 'No se encontró ese mantenimiento para la lavadora indicada.'; return; }
        this.datosMantenimientoCaso = { codigo, id, mantenimiento };
        const detalle = formulario.querySelector('[data-detalle-mantenimiento]');
        detalle.hidden = false;
        detalle.querySelector('[name="descripcion"]').value = mantenimiento.descripcion;
        detalle.querySelector('[name="tecnico"]').value = mantenimiento.tecnico;
        detalle.querySelector('[name="costo"]').value = mantenimiento.costo;
        detalle.querySelector('[name="fecha"]').value = this.fechaCalendario(mantenimiento.fecha);
        detalle.querySelector('[name="completado"]').checked = mantenimiento.completado;
        formulario.querySelector('[data-acciones-mantenimiento-caso]').hidden = false;
      } catch (e) { error.textContent = e.message; }
    }

    leerDatosCasoMantenimiento(formulario) {
      return {
        descripcion: formulario.elements.descripcion.value.trim(),
        tecnico: formulario.elements.tecnico.value.trim(),
        costo: Number(formulario.elements.costo.value),
        fecha: this.fechaServidor(formulario.elements.fecha.value),
        completado: formulario.elements.completado.checked
      };
    }

    async ejecutarAccionMantenimientoCaso(boton) {
      const datos = this.datosMantenimientoCaso;
      if (!datos) return;
      const caso = boton.dataset.accionMantenimientoCaso;
      const formulario = boton.closest('form');
      const error = formulario.querySelector('[data-error-mantenimiento-caso]');
      error.textContent = '';
      try {
        if (caso === 'guardar') {
          const cambios = this.leerDatosCasoMantenimiento(formulario);
          const errores = Voltia.Validaciones.validarMantenimiento(cambios);
          if (Voltia.Validaciones.hayErrores(errores)) { error.textContent = Object.values(errores).filter(Boolean).join(' '); return; }
          await this.controladorMantenimientos.actualizar(datos.codigo, datos.id, cambios);
          this.reproducirSonido('click');
        } else {
          if (!global.confirm(`¿Eliminar el mantenimiento ${datos.id} de la lavadora ${datos.codigo}?`)) return;
          await this.controladorMantenimientos.eliminar(datos.codigo, datos.id);
          this.reproducirSonido('click');
        }
        this.datosMantenimientoCaso = null;
        this.cerrarModal(`modal-mantenimiento-${caso === 'guardar' ? 'actualizar' : 'eliminar'}`);
        this.llenarSelectorLavadoras(this.refs.matLavadora.value);
      } catch (e) { error.textContent = e.message; }
    }

    // =================================================================
    //  Ayudas pequenas
    // =================================================================
    escuchar(elemento, evento, manejador) {
      if (elemento) {
        elemento.addEventListener(evento, manejador);
      }
    }

    elemento(etiqueta, clase, texto) {
      const creado = document.createElement(etiqueta);
      if (clase) {
        creado.className = clase;
      }
      if (texto !== undefined && texto !== null) {
        creado.textContent = String(texto);
      }
      return creado;
    }

    reproducirSonido(tipo) {
      const archivo = tipo === 'lavadora' ? 'lavadora.wav' : 'click.wav';
      try {
        const audio = new Audio('assets/' + archivo);
        audio.volume = 0.72;
        const reproduccion = audio.play();
        if (reproduccion && typeof reproduccion.catch === 'function') reproduccion.catch(() => {});
      } catch (error) {
        // El sonido es decorativo; si el navegador lo bloquea la operación sigue.
      }
    }

    valorBooleano(texto) {
      if (texto === 'true') {
        return true;
      }
      if (texto === 'false') {
        return false;
      }
      return null;
    }

    valorCampo(id) {
      const campo = document.getElementById(id);
      return campo ? campo.value.trim() : '';
    }

    // =================================================================
    //  Render de la tabla principal
    // =================================================================
    renderTabla() {
      const lista = this.estado.lavadoras || [];
      const cuerpo = this.refs.cuerpoTabla;
      cuerpo.textContent = '';

      const fragmento = document.createDocumentFragment();
      lista.forEach((lavadora) => fragmento.appendChild(this.crearFilaLavadora(lavadora)));
      cuerpo.appendChild(fragmento);

      this.refs.contadorLavadoras.textContent = lista.length === 1
        ? '1 lavadora'
        : lista.length + ' lavadoras';
      this.refs.tablaVacio.hidden = lista.length > 0;
      this.refs.resumenFiltros.textContent = this.describirFiltros(this.estado.filtros);
    }

    crearFilaLavadora(lavadora) {
      const fila = document.createElement('tr');
      fila.dataset.codigo = String(lavadora.codigo);
      fila.tabIndex = 0;
      const seleccionada = this.estado.seleccionada &&
        this.estado.seleccionada.codigo === lavadora.codigo;
      fila.setAttribute('aria-selected', String(Boolean(seleccionada)));

      const celdas = [
        { texto: lavadora.codigo, clase: 'num' },
        { texto: lavadora.marca, clase: 'marca' },
        { texto: lavadora.fechaCorta, clase: 'num', titulo: 'Fabricada el ' + lavadora.fechaTexto },
        { texto: lavadora.precioBaseTexto, clase: 'num' },
        { texto: lavadora.capacidadTexto, clase: 'num' },
        { texto: lavadora.rpmTexto, clase: 'num' },
        { texto: lavadora.inverterTexto, clase: 'centro' },
        { texto: lavadora.minutosTexto, clase: 'num' },
        { texto: lavadora.cantidadMantenimientos, clase: 'num' },
        {
          texto: lavadora.mantenimientosPendientes,
          clase: 'num ' + (lavadora.mantenimientosPendientes > 0 ? 'alerta' : 'ok')
        },
        { texto: lavadora.precioFinalTexto, clase: 'num final' }
      ];

      celdas.forEach((celda) => {
        const creada = this.elemento('td', celda.clase, celda.texto);
        if (celda.titulo) {
          creada.title = celda.titulo;
        }
        fila.appendChild(creada);
      });
      return fila;
    }

    marcarSeleccion(codigo) {
      const objetivo = Number(codigo);
      Array.from(this.refs.cuerpoTabla.querySelectorAll('tr')).forEach((fila) => {
        const esLaFila = Number(fila.dataset.codigo) === objetivo;
        fila.setAttribute('aria-selected', String(esLaFila));
      });
    }

    describirFiltros(filtros) {
      const partes = [];
      if (filtros && filtros.marca) {
        partes.push('marca contiene "' + filtros.marca + '"');
      }
      if (filtros && filtros.tecnologiaInverter !== null && filtros.tecnologiaInverter !== undefined) {
        partes.push(filtros.tecnologiaInverter ? 'con inverter' : 'sin inverter');
      }
      if (filtros && filtros.conMantenimientos !== null && filtros.conMantenimientos !== undefined) {
        partes.push(filtros.conMantenimientos ? 'con mantenimientos' : 'sin mantenimientos');
      }
      return partes.length > 0
        ? 'Filtros aplicados: ' + partes.join(' | ')
        : 'Sin filtros aplicados';
    }

    // =================================================================
    //  Render del panel de detalle
    // =================================================================
    renderDetalle(lavadora) {
      const contenedor = this.refs.detalle;
      contenedor.textContent = '';

      if (!lavadora) {
        const vacio = this.elemento('div', 'detalle-vacio');
        vacio.appendChild(this.elemento('p', null,
          'Seleccione una lavadora en la tabla para ver su ficha y sus mantenimientos.'));
        contenedor.appendChild(vacio);
        return;
      }

      const cabecera = this.elemento('div', 'detalle-cabecera');
      const bloqueTitulo = this.elemento('div');
      bloqueTitulo.appendChild(this.elemento('p', 'detalle-etiqueta', 'Lavadora seleccionada'));
      bloqueTitulo.appendChild(this.elemento('h2', 'detalle-titulo', lavadora.titulo));
      cabecera.appendChild(bloqueTitulo);
      cabecera.appendChild(this.elemento('span', 'chip', 'Inverter: ' + lavadora.inverterTexto));
      contenedor.appendChild(cabecera);

      const resumen = this.elemento('div', 'detalle-resumen');
      resumen.appendChild(this.crearResumen('Precio final', lavadora.precioFinalTexto));
      resumen.appendChild(this.crearResumen('Mantenim.', lavadora.cantidadMantenimientos));
      resumen.appendChild(this.crearResumen(
        'Pendientes',
        lavadora.mantenimientosPendientes,
        lavadora.mantenimientosPendientes > 0 ? 'alerta' : null
      ));
      contenedor.appendChild(resumen);

      contenedor.appendChild(this.crearSeccionDatos(lavadora));

      const seccionDetalle = this.elemento('div', 'detalle-seccion');
      const cabeceraDetalle = this.elemento('div', 'detalle-seccion-cabecera');
      cabeceraDetalle.appendChild(this.elemento('h3', null, 'Mantenimientos'));
      const botonGestionar = this.elemento('button', 'boton primario chico boton-caso', 'Gestionar');
      botonGestionar.type = 'button';
      botonGestionar.dataset.accion = 'gestionar';
      botonGestionar.dataset.codigo = String(lavadora.codigo);
      botonGestionar.title = 'Gestionar los mantenimientos de la lavadora ' + lavadora.codigo;
      botonGestionar.setAttribute('aria-label',
        'Gestionar los mantenimientos de la lavadora ' + lavadora.codigo);
      cabeceraDetalle.appendChild(botonGestionar);
      seccionDetalle.appendChild(cabeceraDetalle);
      seccionDetalle.appendChild(this.crearListaMantenimientos(lavadora.mantenimientos));
      contenedor.appendChild(seccionDetalle);
    }

    crearResumen(etiqueta, valor, clase) {
      const bloque = this.elemento('div', 'resumen-item');
      bloque.appendChild(this.elemento('span', null, etiqueta));
      bloque.appendChild(this.elemento('strong', clase || null, valor));
      return bloque;
    }

    crearSeccionDatos(lavadora) {
      const seccion = this.elemento('div', 'detalle-seccion');
      seccion.appendChild(this.elemento('h3', null, 'Datos tecnicos'));
      const lista = this.elemento('dl', 'ficha-datos');

      const filas = [
        ['Fecha de fabricacion', lavadora.fechaTexto, null],
        ['Precio base', lavadora.precioBaseTexto, null],
        ['Capacidad', lavadora.capacidadTexto, null],
        ['Centrifugado', lavadora.rpmTexto, null],
        ['Minutos de lavado', lavadora.minutosTexto, null],
        ['Mantenimientos', String(lavadora.cantidadMantenimientos), null],
        ['Pendientes', String(lavadora.mantenimientosPendientes),
          lavadora.mantenimientosPendientes > 0 ? 'alerta' : null],
        ['Costo mantenimientos', lavadora.costoMantenimientosTexto, null],
        ['Precio final', lavadora.precioFinalTexto, 'destacado']
      ];

      filas.forEach((fila) => {
        lista.appendChild(this.elemento('dt', null, fila[0]));
        lista.appendChild(this.elemento('dd', fila[2], fila[1]));
      });

      seccion.appendChild(lista);
      return seccion;
    }

    /**
     * Lista de mantenimientos en formato de tarjetas. En una columna angosta
     * (el panel de detalle o un modal) una tabla de seis columnas no cabe, y
     * las tarjetas muestran los seis datos sin scroll horizontal.
     */
    crearListaMantenimientos(mantenimientos) {
      const lista = Array.isArray(mantenimientos) ? mantenimientos : [];
      if (lista.length === 0) {
        return this.elemento('p', 'vacio',
          'Esta lavadora no tiene mantenimientos registrados (el detalle puede estar en cero).');
      }

      const contenedor = this.elemento('ul', 'lista-mantenimientos');
      lista.forEach((mantenimiento) => {
        const item = this.elemento('li', 'mantenimiento-item');

        const linea = this.elemento('div', 'mantenimiento-linea');
        linea.appendChild(this.elemento('span', 'mantenimiento-id', '#' + mantenimiento.id));
        linea.appendChild(this.elemento('span', 'mantenimiento-descripcion', mantenimiento.descripcion));
        linea.appendChild(this.elemento('span', 'estado ' + mantenimiento.estadoClase,
          mantenimiento.estadoTexto));
        item.appendChild(linea);

        const meta = this.elemento('div', 'mantenimiento-meta');
        meta.appendChild(this.elemento('span', null, mantenimiento.tecnico));
        meta.appendChild(this.elemento('span', null, mantenimiento.costoTexto));
        meta.appendChild(this.elemento('span', null, mantenimiento.fechaTexto));
        item.appendChild(meta);

        contenedor.appendChild(item);
      });
      return contenedor;
    }

    /** Una fila de mantenimiento, con o sin botones de accion. */
    crearFilaMantenimiento(mantenimiento, conAcciones) {
      const fila = document.createElement('tr');
      fila.dataset.id = String(mantenimiento.id);
      fila.appendChild(this.elemento('td', 'num', mantenimiento.id));
      fila.appendChild(this.elemento('td', null, mantenimiento.descripcion));
      fila.appendChild(this.elemento('td', null, mantenimiento.tecnico));
      fila.appendChild(this.elemento('td', 'num', mantenimiento.costoTexto));
      fila.appendChild(this.elemento('td', 'num', mantenimiento.fechaTexto));

      const celdaEstado = document.createElement('td');
      celdaEstado.appendChild(this.elemento('span', 'estado ' + mantenimiento.estadoClase,
        mantenimiento.estadoTexto));
      fila.appendChild(celdaEstado);

      if (conAcciones) {
        const celdaAcciones = document.createElement('td');
        const grupo = this.elemento('div', 'acciones-fila');

        const accion = this.modoMantenimientos;
        const etiqueta = accion === 'consultar' ? 'Consultar' : (accion === 'actualizar' ? 'Actualizar' : 'Eliminar');
        const clase = accion === 'eliminar' ? 'boton peligro chico boton-caso' : 'boton secundario chico boton-caso';
        const botonAccion = this.elemento('button', clase, etiqueta);
        botonAccion.type = 'button';
        botonAccion.dataset.accion = accion === 'consultar' ? 'consultar' : (accion === 'actualizar' ? 'editar' : 'eliminar');
        botonAccion.dataset.id = String(mantenimiento.id);
        botonAccion.setAttribute('aria-label', etiqueta + ' el mantenimiento ' + mantenimiento.id);
        grupo.appendChild(botonAccion);
        celdaAcciones.appendChild(grupo);
        fila.appendChild(celdaAcciones);
      }
      return fila;
    }

    /** Ficha completa reutilizada en consultar, actualizar y eliminar. */
    mostrarFichaLavadora(contenedor, lavadora, opciones) {
      const configuracion = opciones || {};
      contenedor.textContent = '';

      const cabecera = this.elemento('div', 'detalle-cabecera');
      const bloqueTitulo = this.elemento('div');
      bloqueTitulo.appendChild(this.elemento('p', 'detalle-etiqueta', 'Ficha de la lavadora'));
      bloqueTitulo.appendChild(this.elemento('h2', 'detalle-titulo', lavadora.titulo));
      cabecera.appendChild(bloqueTitulo);
      cabecera.appendChild(this.elemento('span', 'chip', 'Inverter: ' + lavadora.inverterTexto));
      contenedor.appendChild(cabecera);

      if (configuracion.camposLectura) {
        const campos = this.elemento('div', 'rejilla-campos ficha-solo-lectura');
        const agregarCampo = (etiqueta, valor, tipo) => {
          const campo = this.elemento('div', 'campo');
          const id = 'ficha-' + etiqueta.toLowerCase().replace(/[^a-z0-9]+/g, '-');
          const label = this.elemento('label', null, etiqueta);
          label.htmlFor = id;
          const input = document.createElement('input');
          input.id = id;
          input.type = tipo || 'text';
          input.value = String(valor === null || valor === undefined ? '' : valor);
          input.readOnly = true;
          input.disabled = true;
          input.setAttribute('aria-readonly', 'true');
          campo.appendChild(label);
          campo.appendChild(input);
          campos.appendChild(campo);
        };
        agregarCampo('Codigo de la lavadora', lavadora.codigo, 'number');
        agregarCampo('Marca', lavadora.marca);
        agregarCampo('Fecha de fabricacion', this.fechaCalendario(lavadora.fechaFabricacion), 'datetime-local');
        agregarCampo('Precio base', lavadora.precioBase, 'number');
        agregarCampo('Capacidad de carga (kg)', lavadora.capacidadCargaKilos, 'number');
        agregarCampo('Velocidad de centrifugado (RPM)', lavadora.velocidadCentrifugadoRPM, 'number');
        agregarCampo('Minutos de lavado', lavadora.minutosLavado, 'number');
        contenedor.appendChild(campos);
      }

      const lista = this.elemento('dl', 'ficha-datos');
      const filas = [
        ['Codigo', lavadora.codigo],
        ['Marca', lavadora.marca],
        ['Fecha de fabricacion', lavadora.fechaTexto],
        ['Precio base', lavadora.precioBaseTexto],
        ['Capacidad', lavadora.capacidadTexto],
        ['Centrifugado', lavadora.rpmTexto],
        ['Minutos de lavado', lavadora.minutosTexto],
        ['Mantenimientos', lavadora.cantidadMantenimientos],
        ['Pendientes', lavadora.mantenimientosPendientes],
        ['Costo mantenimientos', lavadora.costoMantenimientosTexto],
        ['Precio final', lavadora.precioFinalTexto]
      ];
      filas.forEach((fila) => {
        lista.appendChild(this.elemento('dt', null, fila[0]));
        lista.appendChild(this.elemento('dd', fila[0] === 'Precio final' ? 'destacado' : null, fila[1]));
      });
      contenedor.appendChild(lista);

      if (configuracion.mantenimientos !== false) {
        const seccion = this.elemento('div', 'detalle-seccion');
        seccion.appendChild(this.elemento('h3', null, 'Mantenimientos'));
        seccion.appendChild(this.crearListaMantenimientos(lavadora.mantenimientos));
        contenedor.appendChild(seccion);
      }
      return contenedor;
    }

    // =================================================================
    //  Encabezado: version, conexion y estado de la maquina
    // =================================================================
    renderVersion(version) {
      this.refs.pantallaVersion.textContent = (version === null || version === undefined)
        ? 'v--'
        : 'v' + version;
      this.estado.versionDatos = version;
    }

    renderConexion(conectado) {
      const estado = conectado ? 'conectado' : 'desconectado';
      this.refs.indicadorConexion.dataset.estado = estado;
      this.refs.textoConexion.textContent = conectado ? 'Conectado' : 'Sin conexion';
    }

    pintarEstadoMaquina() {
      const estado = this.trabajando ? 'LAVANDO' : (this.estadoError ? 'ERROR' : 'LISTO');
      document.body.dataset.estado = estado;
      this.refs.pantallaEstado.textContent = estado;
      this.refs.compuertaEstado.textContent = estado === 'LAVANDO'
        ? 'lavando'
        : (estado === 'ERROR' ? 'alerta' : 'en reposo');
      this.refs.tablaScroll.setAttribute('aria-busy', String(this.trabajando));
    }

    /** Un error se muestra en pantalla digital por unos segundos y luego vuelve a LISTO. */
    marcarErrorTemporal() {
      this.estadoError = true;
      this.pintarEstadoMaquina();
      if (this.temporizadorError) {
        clearTimeout(this.temporizadorError);
      }
      this.temporizadorError = setTimeout(() => {
        this.estadoError = false;
        this.temporizadorError = null;
        this.pintarEstadoMaquina();
      }, 3200);
    }

    // =================================================================
    //  Avisos tipo toast
    // =================================================================
    mostrarMensaje(mensaje) {
      if (!mensaje || !mensaje.texto) {
        return;
      }
      const tipo = mensaje.tipo || 'info';
      const aviso = this.elemento('div', 'toast ' + tipo);
      aviso.setAttribute('role', tipo === 'error' ? 'alert' : 'status');
      aviso.appendChild(this.elemento('p', 'toast-texto', mensaje.texto));

      const cerrar = this.elemento('button', 'boton-cerrar', 'Cerrar');
      cerrar.type = 'button';
      cerrar.setAttribute('aria-label', 'Descartar aviso');
      this.escuchar(cerrar, 'click', () => this.quitarToast(aviso));
      aviso.appendChild(cerrar);

      this.refs.toasts.appendChild(aviso);
      while (this.refs.toasts.children.length > 4) {
        this.quitarToast(this.refs.toasts.children[0]);
      }
      const duracion = tipo === 'error' ? 7000 : 4500;
      aviso.dataset.temporizador = String(setTimeout(() => this.quitarToast(aviso), duracion));

      if (tipo === 'error') {
        this.marcarErrorTemporal();
      }
    }

    quitarToast(aviso) {
      if (!aviso || !aviso.parentNode) {
        return;
      }
      clearTimeout(Number(aviso.dataset.temporizador));
      aviso.remove();
    }

    // =================================================================
    //  Filtros
    // =================================================================
    aplicarFiltros(evento) {
      evento.preventDefault();
      const filtros = {
        marca: this.refs.filtroMarca.value.trim(),
        tecnologiaInverter: this.valorBooleano(this.refs.filtroInverter.value),
        conMantenimientos: this.valorBooleano(this.refs.filtroMantenimientos.value)
      };
      // El controlador ya avisa el error por el notificador; aqui solo se
      // evita que quede una promesa sin manejar.
      this.controladorLavadoras.aplicarFiltros(filtros).catch(() => {});
    }

    limpiarFiltros() {
      this.refs.filtroMarca.value = '';
      this.refs.filtroInverter.value = '';
      this.refs.filtroMantenimientos.value = '';
      this.controladorLavadoras.limpiarFiltros().catch(() => {});
    }

    // =================================================================
    //  Seleccion de la tabla
    // =================================================================
    alHacerClicTabla(evento) {
      const fila = evento.target.closest('tr[data-codigo]');
      if (!fila) {
        return;
      }
      this.controladorLavadoras.seleccionar(Number(fila.dataset.codigo));
    }

    alTeclearTabla(evento) {
      if (evento.key !== 'Enter' && evento.key !== ' ') {
        return;
      }
      const fila = evento.target.closest('tr[data-codigo]');
      if (!fila) {
        return;
      }
      evento.preventDefault();
      this.controladorLavadoras.seleccionar(Number(fila.dataset.codigo));
    }

    alHacerClicDetalle(evento) {
      const boton = evento.target.closest('button[data-accion="gestionar"]');
      if (!boton) {
        return;
      }
      this.abrirMantenimientos(Number(boton.dataset.codigo));
    }

    // =================================================================
    //  Modales: abrir, cerrar y foco
    // =================================================================
    abrirModal(id) {
      const fondo = document.getElementById(id);
      if (!fondo) {
        return;
      }
      this.ultimoFoco = document.activeElement;
      fondo.hidden = false;
      document.body.classList.add('con-modal');
      const primerCampo = fondo.querySelector('input, select, textarea, button');
      if (primerCampo) {
        primerCampo.focus();
      }
    }

    cerrarModal(id) {
      const fondo = document.getElementById(id);
      if (!fondo || fondo.hidden) {
        return;
      }
      fondo.hidden = true;
      if (id === 'modal-mantenimientos') this.versionMantenimientos += 1;
      if (!this.modalSuperior()) {
        document.body.classList.remove('con-modal');
      }
      if (this.ultimoFoco && typeof this.ultimoFoco.focus === 'function') {
        this.ultimoFoco.focus();
      }
    }

    modalSuperior() {
      const abiertos = Array.from(document.querySelectorAll('.modal-fondo'))
        .filter((fondo) => !fondo.hidden);
      return abiertos.length > 0 ? abiertos[abiertos.length - 1] : null;
    }

    alPresionarTecla(evento) {
      const abierto = this.modalSuperior();
      if (!abierto) {
        return;
      }
      if (evento.key === 'Escape') {
        evento.preventDefault();
        this.cerrarModal(abierto.id);
        return;
      }
      if (evento.key !== 'Tab') {
        return;
      }
      const enfocables = Array.from(abierto.querySelectorAll(
        'button, input, select, textarea, a[href], [tabindex]:not([tabindex="-1"])'
      )).filter((elemento) => !elemento.disabled && elemento.offsetParent !== null);
      if (enfocables.length === 0) {
        return;
      }
      const primero = enfocables[0];
      const ultimo = enfocables[enfocables.length - 1];
      if (!abierto.contains(document.activeElement)) {
        evento.preventDefault();
        primero.focus();
        return;
      }
      if (!evento.shiftKey && document.activeElement === ultimo) {
        evento.preventDefault();
        primero.focus();
      } else if (evento.shiftKey && document.activeElement === primero) {
        evento.preventDefault();
        ultimo.focus();
      }
    }

    // =================================================================
    //  Errores de formulario
    // =================================================================
    limpiarErrores(formulario) {
      if (!formulario) {
        return;
      }
      Array.from(formulario.querySelectorAll('input, select, textarea')).forEach((campo) => {
        campo.removeAttribute('aria-invalid');
        campo.classList.remove('invalido');
      });
      Array.from(formulario.querySelectorAll('.campo-error')).forEach((salida) => {
        salida.textContent = '';
      });
      const general = formulario.querySelector('.error-formulario');
      if (general) {
        general.textContent = '';
      }
    }

    mostrarErroresCampos(prefijo, errores) {
      Object.keys(errores || {}).forEach((campo) => {
        const sufijo = CAMPOS_LAVADORA[campo] || campo;
        const entrada = document.getElementById(prefijo + '-' + sufijo);
        const salida = document.getElementById('error-' + prefijo + '-' + sufijo);
        if (entrada) {
          entrada.setAttribute('aria-invalid', 'true');
          entrada.classList.add('invalido');
        }
        if (salida) {
          salida.textContent = errores[campo];
        }
      });
    }

    enfocarPrimerError(formulario) {
      if (!formulario) {
        return;
      }
      const invalido = formulario.querySelector('[aria-invalid="true"]');
      if (invalido && typeof invalido.focus === 'function') {
        invalido.focus();
      }
    }

    bloquearBoton(boton, bloqueado, textoOcupado) {
      if (!boton) {
        return;
      }
      if (!boton.dataset.textoOriginal) {
        boton.dataset.textoOriginal = boton.textContent.trim();
      }
      boton.disabled = bloqueado;
      boton.textContent = bloqueado ? (textoOcupado || 'Procesando...') : boton.dataset.textoOriginal;
    }

    fijarTextoBoton(boton, texto) {
      if (!boton) {
        return;
      }
      boton.dataset.textoOriginal = texto;
      boton.textContent = texto;
    }

    // =================================================================
    //  Formulario de lavadora (agregar y actualizar)
    // =================================================================
    leerFormularioLavadora(prefijo) {
      const leer = (sufijo) => this.valorCampo(prefijo + '-' + sufijo);
      const inverter = document.getElementById(prefijo + '-inverter');
      return {
        codigo: Number(leer('codigo')),
        marca: leer('marca'),
        precioBase: Number(leer('precio')),
        fechaFabricacion: this.fechaServidor(leer('fecha')),
        capacidadCargaKilos: Number(leer('capacidad')),
        velocidadCentrifugadoRPM: Number(leer('rpm')),
        tecnologiaInverter: Boolean(inverter && inverter.checked),
        minutosLavado: Number(leer('minutos'))
      };
    }

    fechaServidor(valor) {
      const fecha = String(valor || '').trim();
      if (!fecha) return '';
      return fecha.length === 16 ? fecha + ':00' : fecha;
    }

    fechaCalendario(valor) {
      const fecha = String(valor || '').trim();
      return fecha.length >= 16 ? fecha.slice(0, 16) : fecha;
    }

    escribirFormularioLavadora(prefijo, lavadora) {
      const escribir = (sufijo, valor) => {
        const campo = document.getElementById(prefijo + '-' + sufijo);
        if (campo) {
          campo.value = valor === null || valor === undefined ? '' : String(valor);
        }
      };
      escribir('marca', lavadora.marca);
      escribir('precio', lavadora.precioBase);
      escribir('fecha', this.fechaCalendario(lavadora.fechaFabricacion));
      escribir('capacidad', lavadora.capacidadCargaKilos);
      escribir('rpm', lavadora.velocidadCentrifugadoRPM);
      escribir('minutos', lavadora.minutosLavado);
      const inverter = document.getElementById(prefijo + '-inverter');
      if (inverter) {
        inverter.checked = lavadora.tecnologiaInverter;
      }
    }

    /** Vista previa del precio final con la misma regla del servidor. */
    actualizarVistaPrecio(prefijo) {
      const salida = document.getElementById(prefijo + '-precio-final');
      if (!salida) {
        return;
      }
      const precioFinal = Voltia.calcularPrecioFinal({
        precioBase: Number(this.valorCampo(prefijo + '-precio')),
        capacidadCargaKilos: Number(this.valorCampo(prefijo + '-capacidad')),
        tecnologiaInverter: Boolean(document.getElementById(prefijo + '-inverter').checked)
      });
      salida.textContent = Voltia.formato.formatearMonto(precioFinal);
    }

    validarFormularioLavadora(prefijo, datos) {
      const errores = Voltia.Validaciones.validarLavadora(datos);
      this.mostrarErroresCampos(prefijo, errores);
      return !Voltia.Validaciones.hayErrores(errores);
    }

    // =================================================================
    //  Agregar lavadora
    // =================================================================
    abrirAgregar() {
      this.refs.formAgregar.reset();
      this.limpiarErrores(this.refs.formAgregar);
      this.refs.agFecha.value = this.fechaCalendario(Voltia.formato.fechaActualTexto());
      this.actualizarVistaPrecio('ag');
      this.abrirModal('modal-agregar');
      document.getElementById('ag-codigo').focus();
    }

    async enviarAgregar(evento) {
      evento.preventDefault();
      this.limpiarErrores(this.refs.formAgregar);
      const datos = this.leerFormularioLavadora('ag');
      if (!this.validarFormularioLavadora('ag', datos)) {
        this.enfocarPrimerError(this.refs.formAgregar);
        return;
      }

      this.bloquearBoton(this.refs.botonGuardarLavadora, true, 'Guardando...');
      try {
        await this.controladorLavadoras.agregar(datos);
        this.reproducirSonido('lavadora');
        this.cerrarModal('modal-agregar');
      } catch (error) {
        this.refs.errorAgregar.textContent = error.message;
      } finally {
        this.bloquearBoton(this.refs.botonGuardarLavadora, false);
      }
    }

    // =================================================================
    //  Consultar lavadora (uno o dos criterios)
    // =================================================================
    abrirConsultar() {
      this.limpiarErrores(this.refs.formConsultar);
      this.refs.coCodigo.value = '';
      this.refs.errorConsultar.textContent = '';
      this.refs.resultadoConsulta.textContent = '';
      this.refs.resultadoConsulta.appendChild(this.elemento('p', 'vacio',
        'Escriba el codigo de la lavadora y pulse Buscar.'));
      this.abrirModal('modal-consultar');
      this.refs.coCodigo.focus();
    }

    async buscarConsulta(evento) {
      evento.preventDefault();
      this.limpiarErrores(this.refs.formConsultar);
      this.refs.errorConsultar.textContent = '';

      const codigoTexto = this.refs.coCodigo.value.trim();
      const codigo = Number(codigoTexto);
      if (!codigoTexto || !Number.isInteger(codigo) || codigo <= 0) {
        this.refs.errorCoCodigo.textContent = 'Escriba un codigo entero mayor que cero.';
        this.refs.coCodigo.classList.add('invalido');
        this.refs.coCodigo.setAttribute('aria-invalid', 'true');
        this.refs.coCodigo.focus();
        return;
      }

      const boton = this.refs.formConsultar.querySelector('button[type="submit"]');
      this.bloquearBoton(boton, true, 'Buscando...');
      try {
        const lavadora = await this.controladorLavadoras.consultar({ codigo: codigo });
        this.refs.resultadoConsulta.textContent = '';
        if (!lavadora) {
          this.refs.resultadoConsulta.appendChild(this.elemento('p', 'vacio',
            'No se encontro una lavadora con ese codigo.'));
          return;
        }
        this.mostrarFichaLavadora(this.refs.resultadoConsulta, lavadora, { mantenimientos: true, camposLectura: true });
      } catch (error) {
        this.refs.errorConsultar.textContent = error.message;
      } finally {
        this.bloquearBoton(boton, false);
      }
    }

    // =================================================================
    //  Actualizar lavadora (busqueda previa obligatoria)
    // =================================================================
    abrirActualizar() {
      this.lavadoraActualizar = null;
      this.limpiarErrores(this.refs.formBuscarActualizar);
      this.limpiarErrores(this.refs.formActualizar);
      this.refs.acCodigo.value = '';
      this.refs.acCodigoFijo.value = '';
      this.refs.errorBuscarActualizar.textContent = '';
      this.refs.errorActualizar.textContent = '';
      this.refs.resultadoActualizar.textContent = '';
      this.refs.resultadoActualizar.appendChild(this.elemento('p', 'vacio',
        'Primero busque la lavadora que desea modificar.'));
      this.refs.formActualizar.hidden = true;
      this.refs.botonGuardarActualizar.disabled = true;
      this.abrirModal('modal-actualizar');
      this.refs.acCodigo.focus();
    }

    async buscarParaActualizar(evento) {
      evento.preventDefault();
      this.limpiarErrores(this.refs.formBuscarActualizar);
      this.refs.errorBuscarActualizar.textContent = '';

      const codigoTexto = this.refs.acCodigo.value.trim();
      const codigo = Number(codigoTexto);
      if (!codigoTexto || !Number.isInteger(codigo) || codigo <= 0) {
        this.refs.errorBuscarActualizar.textContent = 'Escriba un codigo entero mayor que cero.';
        return;
      }

      this.bloquearBoton(this.refs.botonBuscarActualizar, true, 'Buscando...');
      try {
        const lavadora = await this.controladorLavadoras.consultar({ codigo: codigo });
        if (!lavadora) {
          this.lavadoraActualizar = null;
          this.refs.formActualizar.hidden = true;
          this.refs.botonGuardarActualizar.disabled = true;
          this.refs.resultadoActualizar.textContent = '';
          this.refs.resultadoActualizar.appendChild(this.elemento('p', 'vacio',
            'No existe una lavadora con el codigo ' + codigo + '.'));
          return;
        }
        this.lavadoraActualizar = lavadora;
        this.refs.acCodigoFijo.value = String(lavadora.codigo);
        this.mostrarFichaLavadora(this.refs.resultadoActualizar, lavadora, { mantenimientos: false });
        this.escribirFormularioLavadora('ac', lavadora);
        this.actualizarVistaPrecio('ac');
        this.limpiarErrores(this.refs.formActualizar);
        this.refs.formActualizar.hidden = false;
        this.refs.botonGuardarActualizar.disabled = false;
        document.getElementById('ac-marca').focus();
      } catch (error) {
        this.refs.errorBuscarActualizar.textContent = error.message;
      } finally {
        this.bloquearBoton(this.refs.botonBuscarActualizar, false);
      }
    }

    /** Compara el formulario con la lavadora original para enviar solo lo que cambio. */
    calcularCambios(original, datos) {
      const campos = [
        'marca', 'precioBase', 'fechaFabricacion', 'capacidadCargaKilos',
        'velocidadCentrifugadoRPM', 'tecnologiaInverter', 'minutosLavado'
      ];
      const cambios = {};
      campos.forEach((campo) => {
        if (String(datos[campo]) !== String(original[campo])) {
          cambios[campo] = datos[campo];
        }
      });
      return cambios;
    }

    async enviarActualizar(evento) {
      evento.preventDefault();
      if (!this.lavadoraActualizar) {
        this.refs.errorActualizar.textContent = 'Primero busque la lavadora que desea modificar.';
        return;
      }

      this.limpiarErrores(this.refs.formActualizar);
      const datos = this.leerFormularioLavadora('ac');
      datos.codigo = this.lavadoraActualizar.codigo;
      if (!this.validarFormularioLavadora('ac', datos)) {
        this.enfocarPrimerError(this.refs.formActualizar);
        return;
      }

      const cambios = this.calcularCambios(this.lavadoraActualizar, datos);
      if (Object.keys(cambios).length === 0) {
        this.refs.errorActualizar.textContent = 'No hay cambios para guardar.';
        return;
      }

      this.bloquearBoton(this.refs.botonGuardarActualizar, true, 'Guardando...');
      try {
        await this.controladorLavadoras.actualizar(this.lavadoraActualizar.codigo, cambios);
        this.reproducirSonido('click');
        this.cerrarModal('modal-actualizar');
      } catch (error) {
        this.refs.errorActualizar.textContent = error.message;
      } finally {
        this.bloquearBoton(this.refs.botonGuardarActualizar, false);
      }
    }

    // =================================================================
    //  Eliminar lavadora (busqueda previa y confirmacion explicita)
    // =================================================================
    abrirEliminar() {
      this.lavadoraEliminar = null;
      this.limpiarErrores(this.refs.formBuscarEliminar);
      this.refs.elCodigo.value = '';
      this.refs.errorEliminar.textContent = '';
      this.refs.errorConfirmacionEliminar.textContent = '';
      this.refs.elConfirmar.value = '';
      this.refs.confirmacionEliminar.hidden = true;
      this.refs.botonConfirmarEliminar.disabled = true;
      this.refs.resultadoEliminar.textContent = '';
      this.refs.resultadoEliminar.appendChild(this.elemento('p', 'vacio',
        'Primero busque la lavadora que desea eliminar.'));
      this.abrirModal('modal-eliminar');
      this.refs.elCodigo.focus();
    }

    async buscarParaEliminar(evento) {
      evento.preventDefault();
      this.limpiarErrores(this.refs.formBuscarEliminar);
      this.refs.errorEliminar.textContent = '';

      const codigoTexto = this.refs.elCodigo.value.trim();
      const codigo = Number(codigoTexto);
      if (!codigoTexto || !Number.isInteger(codigo) || codigo <= 0) {
        this.refs.errorEliminar.textContent = 'Escriba un codigo entero mayor que cero.';
        return;
      }

      const boton = this.refs.formBuscarEliminar.querySelector('button[type="submit"]');
      this.bloquearBoton(boton, true, 'Buscando...');
      try {
        const lavadora = await this.controladorLavadoras.consultar({ codigo: codigo });
        this.refs.resultadoEliminar.textContent = '';
        this.refs.confirmacionEliminar.hidden = true;
        this.refs.botonConfirmarEliminar.disabled = true;
        this.refs.elConfirmar.value = '';
        this.refs.errorConfirmacionEliminar.textContent = '';

        if (!lavadora) {
          this.lavadoraEliminar = null;
          this.refs.resultadoEliminar.appendChild(this.elemento('p', 'vacio',
            'No existe una lavadora con el codigo ' + codigo + '.'));
          return;
        }

        this.lavadoraEliminar = lavadora;
        this.mostrarFichaLavadora(this.refs.resultadoEliminar, lavadora, { mantenimientos: true, camposLectura: true });
        this.refs.textoConfirmacionEliminar.textContent =
          'Va a eliminar la lavadora ' + lavadora.codigo + ' (' + lavadora.marca + ') y sus ' +
          lavadora.cantidadMantenimientos + ' mantenimientos. Esta accion no se puede deshacer. ' +
          'Escriba el codigo ' + lavadora.codigo + ' para habilitar el boton.';
        this.refs.confirmacionEliminar.hidden = false;
        this.refs.elConfirmar.focus();
      } catch (error) {
        this.refs.errorEliminar.textContent = error.message;
      } finally {
        this.bloquearBoton(boton, false);
      }
    }

    revisarConfirmacionEliminar() {
      const esperado = this.lavadoraEliminar ? String(this.lavadoraEliminar.codigo) : '';
      const escrito = this.refs.elConfirmar.value.trim();
      const coincide = esperado !== '' && escrito === esperado;
      this.refs.botonConfirmarEliminar.disabled = !coincide;
      this.refs.errorConfirmacionEliminar.textContent = (escrito && !coincide)
        ? 'El codigo no coincide con la lavadora buscada.'
        : '';
    }

    async confirmarEliminar() {
      if (!this.lavadoraEliminar) {
        return;
      }
      const codigo = this.lavadoraEliminar.codigo;
      this.bloquearBoton(this.refs.botonConfirmarEliminar, true, 'Eliminando...');
      try {
        await this.controladorLavadoras.eliminar(codigo);
        this.reproducirSonido('click');
        this.lavadoraEliminar = null;
        this.cerrarModal('modal-eliminar');
      } catch (error) {
        this.refs.errorEliminar.textContent = error.message;
      } finally {
        this.bloquearBoton(this.refs.botonConfirmarEliminar, false);
      }
    }

    // =================================================================
    //  Mantenimientos (detalle de la lavadora)
    // =================================================================
    abrirMantenimientos(codigoLavadora, casoUso) {
      this.versionMantenimientos += 1;
      this.modoMantenimientos = casoUso || 'listar';
      this.refs.contenidoMantenimientos.dataset.caso = this.modoMantenimientos;
      const titulos = {
        listar: 'Listar mantenimientos',
        agregar: 'Agregar mantenimiento',
        consultar: 'Consultar mantenimiento',
        actualizar: 'Actualizar mantenimiento',
        eliminar: 'Eliminar mantenimiento'
      };
      document.getElementById('titulo-mantenimientos').textContent = titulos[this.modoMantenimientos];
      const esAlta = this.modoMantenimientos === 'agregar';
      this.refs.filtrosMantenimientos.hidden = esAlta;
      this.refs.tablaMantenimientos.hidden = esAlta;
      this.refs.accionesListadoMantenimientos.hidden = false;
      this.refs.encabezadoAccionesMantenimiento.hidden = true;
      this.llenarSelectorLavadoras(codigoLavadora);
      this.refs.matFiltroTecnico.value = '';
      this.refs.matFiltroCompletado.value = '';
      this.refs.errorMantenimientos.textContent = '';
      if (this.refs.errorMantenimiento) this.refs.errorMantenimiento.textContent = '';
      this.refs.cuerpoMantenimientos.textContent = '';
      this.refs.mantenimientosVacio.hidden = true;
      this.refs.contadorMantenimientos.textContent = 'Sin datos';
      this.lavadoraMantenimientos = null;
      this.cerrarFormularioMantenimiento();
      this.cancelarEliminarMantenimiento();
      this.abrirModal('modal-mantenimientos');

      if (codigoLavadora) {
        this.refs.matLavadora.value = String(codigoLavadora);
      }
      if (this.refs.matLavadora.value) {
        const codigo = Number(this.refs.matLavadora.value);
        if (esAlta) {
          this.lavadoraMantenimientos = { codigo: codigo, mantenimientos: [] };
          this.abrirFormularioMantenimiento(null, 'agregar');
        } else {
          this.cargarMantenimientos();
        }
      } else {
        this.lavadoraMantenimientos = null;
        this.refs.mantenimientosVacio.hidden = false;
        this.refs.mantenimientosVacio.textContent = 'No hay lavadoras registradas. Agregue una primero.';
      }
    }

    llenarSelectorLavadoras(codigoPreferido) {
      const select = this.refs.matLavadora;
      const anterior = (codigoPreferido !== null && codigoPreferido !== undefined)
        ? String(codigoPreferido)
        : select.value;
      select.textContent = '';

      const lista = this.estado.lavadoras || [];
      if (lista.length === 0) {
        const opcion = document.createElement('option');
        opcion.value = '';
        opcion.textContent = 'No hay lavadoras registradas';
        select.appendChild(opcion);
        select.disabled = true;
        return;
      }

      select.disabled = false;
      lista.forEach((lavadora) => {
        const opcion = document.createElement('option');
        opcion.value = String(lavadora.codigo);
        opcion.textContent = lavadora.titulo + ' (' + lavadora.cantidadMantenimientos + ' mantenimientos)';
        select.appendChild(opcion);
      });
      if (anterior && lista.some((lavadora) => String(lavadora.codigo) === anterior)) {
        select.value = anterior;
      }
    }

    async cargarMantenimientos() {
      const version = ++this.versionMantenimientos;
      const codigo = Number(this.refs.matLavadora.value);
      if (!codigo) {
        this.refs.errorMantenimientos.textContent = 'Elija una lavadora para ver sus mantenimientos.';
        return;
      }

      this.refs.errorMantenimientos.textContent = '';
      this.refs.contadorMantenimientos.textContent = 'Cargando...';
      const filtros = {
        tecnico: this.refs.matFiltroTecnico.value.trim(),
        completado: this.valorBooleano(this.refs.matFiltroCompletado.value)
      };

      try {
        const lista = await this.controladorMantenimientos.cargarMantenimientos(codigo, filtros);
        if (version !== this.versionMantenimientos || this.refs.modalMantenimientos.hidden) return;
        this.lavadoraMantenimientos = { codigo: codigo, mantenimientos: lista };
        this.renderMantenimientos();
      } catch (error) {
        if (version !== this.versionMantenimientos || this.refs.modalMantenimientos.hidden) return;
        this.lavadoraMantenimientos = null;
        this.refs.cuerpoMantenimientos.textContent = '';
        this.refs.mantenimientosVacio.hidden = true;
        this.refs.errorMantenimientos.textContent = error.message;
        this.refs.contadorMantenimientos.textContent = 'Sin datos';
      }
    }

    renderMantenimientos() {
      if (!this.lavadoraMantenimientos) {
        return;
      }
      const lista = this.lavadoraMantenimientos.mantenimientos;
      const cuerpo = this.refs.cuerpoMantenimientos;
      cuerpo.textContent = '';

      const conAcciones = false;
      lista.forEach((mantenimiento) => {
        cuerpo.appendChild(this.crearFilaMantenimiento(mantenimiento, conAcciones));
      });
      this.refs.mantenimientosVacio.hidden = lista.length > 0;
      this.refs.mantenimientosVacio.textContent =
        'Esta lavadora no tiene mantenimientos con los filtros indicados.';
      this.refs.contadorMantenimientos.textContent = lista.length === 1
        ? '1 mantenimiento'
        : lista.length + ' mantenimientos';
    }

    mantenimientoPorId(id) {
      if (!this.lavadoraMantenimientos) {
        return null;
      }
      const buscado = Number(id);
      return this.lavadoraMantenimientos.mantenimientos
        .find((mantenimiento) => mantenimiento.id === buscado) || null;
    }

    alHacerClicMantenimientos(evento) {
      const boton = evento.target.closest('button[data-accion]');
      if (!boton) {
        return;
      }
      const id = Number(boton.dataset.id);
      if (boton.dataset.accion === 'consultar') {
        this.abrirFormularioMantenimiento(this.mantenimientoPorId(id), 'consultar');
      } else if (boton.dataset.accion === 'editar') {
        this.abrirFormularioMantenimiento(this.mantenimientoPorId(id), 'actualizar');
      } else if (boton.dataset.accion === 'eliminar') {
        this.pedirEliminarMantenimiento(id);
      }
    }

    abrirFormularioMantenimiento(mantenimiento, casoUso) {
      if (!this.lavadoraMantenimientos) {
        this.refs.errorMantenimientos.textContent = 'Elija una lavadora antes de agregar mantenimientos.';
        return;
      }

      this.cancelarEliminarMantenimiento();
      this.limpiarErrores(this.refs.formMantenimiento);
      this.refs.errorMantenimiento.textContent = '';
      this.mantenimientoEnEdicion = mantenimiento || null;
      const modo = casoUso || this.modoMantenimientos;
      const soloLectura = modo === 'consultar';
      [this.refs.matDescripcion, this.refs.matTecnico, this.refs.matCosto, this.refs.matFecha]
        .forEach((campo) => { campo.readOnly = soloLectura; });
      this.refs.matCompletado.disabled = soloLectura;
      this.refs.botonGuardarMantenimiento.hidden = soloLectura;
      this.refs.botonCancelarMantenimiento.textContent = soloLectura ? 'Cerrar detalle' : 'Cancelar';

      if (mantenimiento) {
        this.refs.tituloFormMantenimiento.textContent = (soloLectura ? 'Consulta del mantenimiento ' : 'Actualizar mantenimiento ') + mantenimiento.id;
        this.fijarTextoBoton(this.refs.botonGuardarMantenimiento, 'Guardar cambios');
        this.refs.matDescripcion.value = mantenimiento.descripcion;
        this.refs.matTecnico.value = mantenimiento.tecnico;
        this.refs.matCosto.value = mantenimiento.costo;
        this.refs.matFecha.value = this.fechaCalendario(mantenimiento.fecha);
        this.refs.matCompletado.checked = mantenimiento.completado;
      } else {
        this.refs.tituloFormMantenimiento.textContent = 'Agregar mantenimiento';
        this.fijarTextoBoton(this.refs.botonGuardarMantenimiento, 'Guardar mantenimiento');
        this.refs.formMantenimiento.reset();
        this.refs.matFecha.value = this.fechaCalendario(Voltia.formato.fechaActualTexto());
      }

      this.refs.formMantenimiento.hidden = false;
      if (!soloLectura) this.refs.matDescripcion.focus();
    }

    cerrarFormularioMantenimiento() {
      this.mantenimientoEnEdicion = null;
      if (this.refs.formMantenimiento) {
        this.refs.formMantenimiento.hidden = true;
        this.refs.formMantenimiento.reset();
        [this.refs.matDescripcion, this.refs.matTecnico, this.refs.matCosto, this.refs.matFecha]
          .forEach((campo) => { campo.readOnly = false; });
        this.refs.matCompletado.disabled = false;
        this.refs.botonGuardarMantenimiento.hidden = false;
        this.refs.botonCancelarMantenimiento.textContent = 'Cancelar';
      }
      if (this.refs.errorMantenimiento) {
        this.refs.errorMantenimiento.textContent = '';
      }
      this.limpiarErrores(this.refs.formMantenimiento);
    }

    leerFormularioMantenimiento() {
      return {
        descripcion: this.refs.matDescripcion.value.trim(),
        tecnico: this.refs.matTecnico.value.trim(),
        costo: Number(this.refs.matCosto.value),
        fecha: this.fechaServidor(this.refs.matFecha.value),
        completado: this.refs.matCompletado.checked
      };
    }

    async guardarMantenimiento(evento) {
      evento.preventDefault();
      if (!this.lavadoraMantenimientos) {
        this.refs.errorMantenimiento.textContent = 'Elija una lavadora antes de guardar.';
        return;
      }

      this.limpiarErrores(this.refs.formMantenimiento);
      const datos = this.leerFormularioMantenimiento();
      const errores = Voltia.Validaciones.validarMantenimiento(datos);
      this.mostrarErroresCampos('mat', errores);
      if (Voltia.Validaciones.hayErrores(errores)) {
        this.enfocarPrimerError(this.refs.formMantenimiento);
        return;
      }

      const codigo = this.lavadoraMantenimientos.codigo;
      const enEdicion = this.mantenimientoEnEdicion;
      this.bloquearBoton(this.refs.botonGuardarMantenimiento, true, 'Guardando...');
      try {
        if (enEdicion) {
          await this.controladorMantenimientos.actualizar(codigo, enEdicion.id, datos);
          this.reproducirSonido('click');
        } else {
          await this.controladorMantenimientos.agregar(codigo, datos);
          this.reproducirSonido('lavadora');
        }
        this.cerrarFormularioMantenimiento();
        await this.cargarMantenimientos();
        this.llenarSelectorLavadoras(codigo);
        if (this.modoMantenimientos === 'agregar') {
          this.lavadoraMantenimientos = this.lavadoraMantenimientos || { codigo: codigo, mantenimientos: [] };
          this.abrirFormularioMantenimiento(null, 'agregar');
        }
      } catch (error) {
        this.refs.errorMantenimiento.textContent = error.message;
      } finally {
        this.bloquearBoton(this.refs.botonGuardarMantenimiento, false);
      }
    }

    pedirEliminarMantenimiento(id) {
      const mantenimiento = this.mantenimientoPorId(id);
      if (!mantenimiento || !this.lavadoraMantenimientos) {
        return;
      }
      this.cerrarFormularioMantenimiento();
      this.mantenimientoAEliminar = mantenimiento;
      this.refs.textoConfirmacionMantenimiento.textContent =
        'Mantenimiento ' + mantenimiento.id + ' · ' + mantenimiento.descripcion +
        ' · Tecnico: ' + mantenimiento.tecnico + ' · Costo: ' + mantenimiento.costoTexto +
        ' · Fecha: ' + mantenimiento.fechaTexto + ' · Estado: ' + mantenimiento.estadoTexto +
        ' · Lavadora: ' + this.lavadoraMantenimientos.codigo + '. Esta accion no se puede deshacer.';
      this.abrirFormularioMantenimiento(mantenimiento, 'consultar');
      this.refs.confirmacionMantenimiento.hidden = false;
      this.refs.botonConfirmarEliminarMantenimiento.focus();
    }

    cancelarEliminarMantenimiento() {
      this.mantenimientoAEliminar = null;
      if (this.refs.confirmacionMantenimiento) {
        this.refs.confirmacionMantenimiento.hidden = true;
      }
      if (this.refs.textoConfirmacionMantenimiento) {
        this.refs.textoConfirmacionMantenimiento.textContent = '';
      }
    }

    async confirmarEliminarMantenimiento() {
      if (!this.mantenimientoAEliminar || !this.lavadoraMantenimientos) {
        return;
      }
      const codigo = this.lavadoraMantenimientos.codigo;
      const id = this.mantenimientoAEliminar.id;
      this.bloquearBoton(this.refs.botonConfirmarEliminarMantenimiento, true, 'Eliminando...');
      try {
        await this.controladorMantenimientos.eliminar(codigo, id);
        this.reproducirSonido('click');
        this.cancelarEliminarMantenimiento();
        this.cerrarFormularioMantenimiento();
        await this.cargarMantenimientos();
        this.llenarSelectorLavadoras(codigo);
      } catch (error) {
        this.refs.errorMantenimientos.textContent = error.message;
      } finally {
        this.bloquearBoton(this.refs.botonConfirmarEliminarMantenimiento, false);
      }
    }
  }

  Voltia.Vista = Vista;
})(window);
