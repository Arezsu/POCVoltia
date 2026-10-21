/* =====================================================================
   CONTROLADOR (la C del patron MVC)
   =====================================================================
   El controlador es el director de orquesta: recibe la intencion del usuario
   (que llega desde la vista), decide que caso de uso ejecutar, llama al
   servicio GraphQL y finalmente avisa por el notificador.

   Reglas de la arquitectura:
     - La vista NO conoce el servicio GraphQL: solo habla con el controlador.
     - El controlador NO toca el DOM: no sabe que existe una tabla o un modal.
     - El controlador guarda el estado de la pantalla (filtros, seleccion,
       version de datos) para que la vista solo dibuje lo que le entregan.

   Casos de uso de las lavadoras (maestro):
     listar, aplicar filtros, seleccionar, consultar, agregar, actualizar,
     eliminar y vigilar la version de los datos.

   Casos de uso de los mantenimientos (detalle):
     listar con filtros, agregar, actualizar y eliminar. Despues de cada
     cambio del detalle se recarga el maestro porque los campos calculados
     (cantidad, pendientes, costo) viven en la lavadora.
   ===================================================================== */
(function (global) {
  'use strict';

  const Voltia = global.Voltia = global.Voltia || {};
  const EVENTOS = Voltia.EVENTOS;

  class ControladorLavadoras {
    constructor(cliente, notificador) {
      this.cliente = cliente;
      this.notificador = notificador;
      this.lavadoras = [];
      this.filtros = { marca: '', tecnologiaInverter: null, conMantenimientos: null };
      this.codigoSeleccionado = null;
      this.versionDatos = null;
      this.cargado = false;
    }

    // -------------------------------------------------------------------
    //  Estado que se publica a la vista
    // -------------------------------------------------------------------
    estado() {
      return {
        lavadoras: this.lavadoras,
        filtros: Object.assign({}, this.filtros),
        total: this.lavadoras.length,
        seleccionada: this.lavadoraSeleccionada(),
        versionDatos: this.versionDatos
      };
    }

    lavadoraSeleccionada() {
      if (this.codigoSeleccionado === null) {
        return null;
      }
      return this.lavadoras.find((lavadora) => lavadora.codigo === this.codigoSeleccionado) || null;
    }

    // -------------------------------------------------------------------
    //  Arranque y recarga
    // -------------------------------------------------------------------
    async iniciar() {
      try {
        await this.recargar({ silencioso: true });
      } catch (error) {
        this.notificador.notificar(EVENTOS.MENSAJE, {
          tipo: 'error',
          texto: 'No se pudo cargar la lista de lavadoras: ' + error.message
        });
      }
    }

    /**
     * Trae la lista y la version de datos en paralelo. Publica el evento
     * "datos cambiaron" para que la vista redibuje la tabla y el detalle.
     *
     * @param {Object} opciones - {silencioso: true} evita el aviso de error
     *        cuando la recarga la hace el vigilante (no molesta al usuario).
     */
    async recargar(opciones) {
      const silencioso = Boolean(opciones && opciones.silencioso);
      try {
        const resultados = await Promise.all([
          this.cliente.listarLavadoras(this.filtros),
          this.cliente.obtenerVersionDatos()
        ]);
        this.lavadoras = (resultados[0] || []).map((datos) => new Voltia.Lavadora(datos));
        this.versionDatos = resultados[1];
        this.cargado = true;
        this.ajustarSeleccion();
        this.notificador.notificar(EVENTOS.DATOS, this.estado());
      } catch (error) {
        if (!silencioso) {
          this.notificador.notificar(EVENTOS.MENSAJE, { tipo: 'error', texto: error.message });
        }
        throw error;
      }
    }

    /**
     * Mantiene coherente la seleccion:
     *  - si la lavadora seleccionada ya no existe, se limpia;
     *  - si no hay nada seleccionado y hay filas, se elige la primera para que
     *    el panel de detalle no quede vacio sin motivo.
     */
    ajustarSeleccion() {
      if (this.codigoSeleccionado !== null &&
          !this.lavadoras.some((lavadora) => lavadora.codigo === this.codigoSeleccionado)) {
        this.codigoSeleccionado = null;
      }
      if (this.codigoSeleccionado === null && this.lavadoras.length > 0) {
        this.codigoSeleccionado = this.lavadoras[0].codigo;
      }
    }

    // -------------------------------------------------------------------
    //  Filtros
    // -------------------------------------------------------------------
    async aplicarFiltros(filtros) {
      this.filtros = {
        marca: String(filtros.marca || '').trim(),
        tecnologiaInverter: this.normalizarBooleano(filtros.tecnologiaInverter),
        conMantenimientos: this.normalizarBooleano(filtros.conMantenimientos)
      };
      await this.recargar();
      if (this.lavadoras.length === 0) {
        this.notificador.notificar(EVENTOS.MENSAJE, {
          tipo: 'info',
          texto: 'Ninguna lavadora cumple los filtros indicados.'
        });
      }
    }

    async limpiarFiltros() {
      this.filtros = { marca: '', tecnologiaInverter: null, conMantenimientos: null };
      await this.recargar();
    }

    normalizarBooleano(valor) {
      if (valor === true || valor === 'true') {
        return true;
      }
      if (valor === false || valor === 'false') {
        return false;
      }
      return null;
    }

    // -------------------------------------------------------------------
    //  Seleccion
    // -------------------------------------------------------------------
    seleccionar(codigo) {
      const buscado = Number(codigo);
      const lavadora = this.lavadoras.find((elemento) => elemento.codigo === buscado) || null;
      this.codigoSeleccionado = lavadora ? lavadora.codigo : null;
      this.notificador.notificar(EVENTOS.SELECCION, lavadora);
    }

    // -------------------------------------------------------------------
    //  Consultar por criterios (codigo y/o marca)
    // -------------------------------------------------------------------
    async consultar(criterios) {
      try {
        const datos = await this.cliente.buscarLavadoraPorCriterios(criterios);
        return datos ? new Voltia.Lavadora(datos) : null;
      } catch (error) {
        this.avisarError(error);
        throw error;
      }
    }

    // -------------------------------------------------------------------
    //  Agregar
    // -------------------------------------------------------------------
    async agregar(datos) {
      try {
        const creada = await this.cliente.agregarLavadora(this.entradaLavadora(datos));
        this.codigoSeleccionado = creada ? creada.codigo : this.codigoSeleccionado;
        await this.recargar({ silencioso: true });
        const texto = creada
          ? 'Lavadora ' + creada.codigo + ' (' + creada.marca + ') agregada correctamente.'
          : 'Lavadora agregada correctamente.';
        this.notificador.notificar(EVENTOS.MENSAJE, { tipo: 'exito', texto: texto });
        return creada;
      } catch (error) {
        this.avisarError(error);
        throw error;
      }
    }

    /** Arma EXACTAMENTE el tipo LavadoraInput del contrato. */
    entradaLavadora(datos) {
      return {
        codigo: Number(datos.codigo),
        marca: String(datos.marca || '').trim(),
        precioBase: Number(datos.precioBase),
        fechaFabricacion: String(datos.fechaFabricacion || ''),
        capacidadCargaKilos: Number(datos.capacidadCargaKilos),
        velocidadCentrifugadoRPM: Number(datos.velocidadCentrifugadoRPM),
        tecnologiaInverter: datos.tecnologiaInverter === true,
        minutosLavado: Number(datos.minutosLavado)
      };
    }

    // -------------------------------------------------------------------
    //  Actualizar (solo los campos que cambiaron)
    // -------------------------------------------------------------------
    async actualizar(codigo, cambios) {
      try {
        const actualizada = await this.cliente.actualizarLavadora(Number(codigo), cambios);
        this.codigoSeleccionado = Number(codigo);
        await this.recargar({ silencioso: true });
        const texto = actualizada
          ? 'Lavadora ' + actualizada.codigo + ' actualizada correctamente.'
          : 'Lavadora actualizada correctamente.';
        this.notificador.notificar(EVENTOS.MENSAJE, { tipo: 'exito', texto: texto });
        return actualizada;
      } catch (error) {
        this.avisarError(error);
        throw error;
      }
    }

    // -------------------------------------------------------------------
    //  Eliminar
    // -------------------------------------------------------------------
    async eliminar(codigo) {
      try {
        const numero = Number(codigo);
        await this.cliente.eliminarLavadora(numero);
        if (this.codigoSeleccionado === numero) {
          this.codigoSeleccionado = null;
        }
        await this.recargar({ silencioso: true });
        this.notificador.notificar(EVENTOS.MENSAJE, {
          tipo: 'exito',
          texto: 'Lavadora ' + numero + ' eliminada con sus mantenimientos.'
        });
      } catch (error) {
        this.avisarError(error);
        throw error;
      }
    }

    // -------------------------------------------------------------------
    //  Vigilante de la version de datos (actualizacion automatica)
    // -------------------------------------------------------------------
    /**
     * Consulta versionDatos cada pocos segundos. Si cambio, significa que
     * otro cliente (el de escritorio o el de Python) modifico los datos, y
     * se recarga sin que el usuario tenga que hacer nada.
     * Devuelve true cuando hubo que refrescar.
     */
    async verificarVersion() {
      try {
        const version = await this.cliente.obtenerVersionDatos();
        if (!this.cargado) {
          // La primera carga fallo (por ejemplo el servidor estaba apagado).
          // El vigilante aprovecha para intentar la lista completa otra vez.
          await this.recargar({ silencioso: true });
          return true;
        }
        if (this.versionDatos === null) {
          this.versionDatos = version;
          return false;
        }
        if (version === this.versionDatos) {
          return false;
        }
        this.versionDatos = version;
        await this.recargar({ silencioso: true });
        this.notificador.notificar(EVENTOS.MENSAJE, {
          tipo: 'info',
          texto: 'Otro cliente modifico los datos. La lista se actualizo sola.'
        });
        return true;
      } catch (error) {
        // Si el servidor no responde, el cliente GraphQL ya marco "sin conexion".
        return false;
      }
    }

    /** Avisa al usuario que una operacion fallo, sin borrar el mensaje del servidor. */
    avisarError(error) {
      const texto = error && error.message ? error.message : 'Ocurrio un error inesperado.';
      this.notificador.notificar(EVENTOS.MENSAJE, { tipo: 'error', texto: texto });
    }
  }

  class ControladorMantenimientos {
    constructor(cliente, notificador, controladorLavadoras) {
      this.cliente = cliente;
      this.notificador = notificador;
      // Se recibe el controlador del maestro para refrescar sus campos
      // calculados cuando cambia el detalle.
      this.controladorLavadoras = controladorLavadoras;
    }

    async cargarMantenimientos(codigoLavadora, filtros) {
      try {
        const datos = await this.cliente.consultarMantenimientos(
          Number(codigoLavadora),
          filtros || {}
        );
        return (datos || []).map((elemento) => new Voltia.Mantenimiento(elemento));
      } catch (error) {
        this.avisarError(error);
        throw error;
      }
    }

    async agregar(codigoLavadora, datos) {
      try {
        const creado = await this.cliente.agregarMantenimiento(
          Number(codigoLavadora),
          this.entradaMantenimiento(datos)
        );
        await this.refrescarMaestro();
        const texto = creado
          ? 'Mantenimiento ' + creado.id + ' agregado a la lavadora ' + codigoLavadora + '.'
          : 'Mantenimiento agregado correctamente.';
        this.notificador.notificar(EVENTOS.MENSAJE, { tipo: 'exito', texto: texto });
        return creado;
      } catch (error) {
        this.avisarError(error);
        throw error;
      }
    }

    async actualizar(codigoLavadora, id, datos) {
      try {
        const actualizado = await this.cliente.actualizarMantenimiento(
          Number(codigoLavadora),
          Number(id),
          this.entradaMantenimiento(datos)
        );
        await this.refrescarMaestro();
        const texto = actualizado
          ? 'Mantenimiento ' + actualizado.id + ' actualizado correctamente.'
          : 'Mantenimiento actualizado correctamente.';
        this.notificador.notificar(EVENTOS.MENSAJE, { tipo: 'exito', texto: texto });
        return actualizado;
      } catch (error) {
        this.avisarError(error);
        throw error;
      }
    }

    async eliminar(codigoLavadora, id) {
      try {
        await this.cliente.eliminarMantenimiento(Number(codigoLavadora), Number(id));
        await this.refrescarMaestro();
        this.notificador.notificar(EVENTOS.MENSAJE, {
          tipo: 'exito',
          texto: 'Mantenimiento ' + id + ' eliminado de la lavadora ' + codigoLavadora + '.'
        });
      } catch (error) {
        this.avisarError(error);
        throw error;
      }
    }

    /** Entrada MantenimientoInput: el id NO se manda, lo calcula el servidor. */
    entradaMantenimiento(datos) {
      return {
        descripcion: String(datos.descripcion || '').trim(),
        tecnico: String(datos.tecnico || '').trim(),
        costo: Number(datos.costo),
        fecha: String(datos.fecha || ''),
        completado: datos.completado === true
      };
    }

    /**
     * Al cambiar el detalle, la lavadora cambia en sus campos calculados.
     * Se recarga el maestro para que la tabla y el panel muestren los valores
     * nuevos (cantidad de mantenimientos, pendientes y costo).
     */
    async refrescarMaestro() {
      try {
        await this.controladorLavadoras.recargar({ silencioso: true });
      } catch (error) {
        // El controlador del maestro ya avisa; aqui solo se evita cortar el flujo.
      }
    }

    avisarError(error) {
      const texto = error && error.message ? error.message : 'Ocurrio un error inesperado.';
      this.notificador.notificar(EVENTOS.MENSAJE, { tipo: 'error', texto: texto });
    }
  }

  Voltia.ControladorLavadoras = ControladorLavadoras;
  Voltia.ControladorMantenimientos = ControladorMantenimientos;
})(window);
