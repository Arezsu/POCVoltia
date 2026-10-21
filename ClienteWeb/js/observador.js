/* =====================================================================
   OBSERVADOR (patron OBSERVER)
   =====================================================================
   El problema que resuelve: cuando el usuario agrega una lavadora, la tabla,
   el panel de detalle y la version de datos deben enterarse. Si la vista
   llamara directamente al controlador despues de cada cambio, todo quedaria
   amarrado y seria dificil de mantener.

   La solucion: un "Notificador" (el sujeto observable) al que se suscriben
   los interesados (la vista, por ejemplo). Cuando el controlador termina un
   caso de uso, solo avisa:
       notificador.notificar('datos cambiaron', estado);
   y todos los suscriptores reaccionan. El controlador no sabe quien escucha.

   Eventos del proyecto:
     - "datos cambiaron" : llego una lista nueva de lavadoras.
     - "seleccion cambio": el usuario eligio otra lavadora.
     - "trabajando"      : empieza o termina una peticion de red.
     - "mensaje"         : hay un aviso para mostrar (exito o error).
     - "conexion"        : el servidor responde o dejo de responder.
   ===================================================================== */
(function (global) {
  'use strict';

  const Voltia = global.Voltia = global.Voltia || {};

  /** Registro minimo: no ensucia la consola con trazas de depuracion. */
  const registro = {
    error: function (mensaje, detalle) {
      if (global.console && typeof global.console.error === 'function') {
        global.console.error('[Voltia] ' + mensaje, detalle || '');
      }
    }
  };

  class Notificador {
    constructor() {
      // Un Map evento -> conjunto de funciones. Se usa Set para que el mismo
      // manejador no se registre dos veces por accidente.
      this.suscriptores = new Map();
    }

    /**
     * Registra una funcion para un evento y devuelve la forma de cancelar la
     * suscripcion. Devolver la funcion de baja evita tener que recordar el
     * manejador exacto mas adelante.
     */
    suscribir(evento, manejador) {
      if (typeof manejador !== 'function') {
        throw new Error('El manejador del evento "' + evento + '" debe ser una funcion');
      }
      if (!this.suscriptores.has(evento)) {
        this.suscriptores.set(evento, new Set());
      }
      this.suscriptores.get(evento).add(manejador);
      return () => this.desuscribir(evento, manejador);
    }

    desuscribir(evento, manejador) {
      const lista = this.suscriptores.get(evento);
      if (lista) {
        lista.delete(manejador);
      }
    }

    /**
     * Avisa a todos los suscriptores de un evento. Se copia la lista antes de
     * recorrerla para que un suscriptor pueda desuscribirse sin romper el ciclo.
     * Un error dentro de un suscriptor no debe tumbar a los demas.
     */
    notificar(evento, datos) {
      const lista = this.suscriptores.get(evento);
      if (!lista) {
        return;
      }
      Array.from(lista).forEach((manejador) => {
        try {
          manejador(datos);
        } catch (error) {
          registro.error('Un suscriptor del evento "' + evento + '" fallo', error);
        }
      });
    }

    contarSuscriptores(evento) {
      const lista = this.suscriptores.get(evento);
      return lista ? lista.size : 0;
    }
  }

  // Nombres de los eventos en un solo lugar: asi una letra mal escrita no
  // rompe la comunicacion entre controlador y vista de forma silenciosa.
  const EVENTOS = Object.freeze({
    DATOS: 'datos cambiaron',
    SELECCION: 'seleccion cambio',
    TRABAJANDO: 'trabajando',
    MENSAJE: 'mensaje',
    CONEXION: 'conexion'
  });

  /**
   * Se crea UNA sola instancia del notificador para toda la aplicacion.
   * Tener varios notificadores seria como tener varias lineas telefonicas:
   * el controlador hablaria por una y la vista escucharia en otra.
   */
  Voltia.notificador = new Notificador();
  Voltia.Notificador = Notificador;
  Voltia.EVENTOS = EVENTOS;
  Voltia.registro = registro;
})(window);
