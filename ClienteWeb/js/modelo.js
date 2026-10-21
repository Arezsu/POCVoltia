/* =====================================================================
   MODELO DEL DOMINIO (la M del patron MVC)
   =====================================================================
   Esta capa guarda el conocimiento del negocio: que es una lavadora, que
   es un mantenimiento, como se calcula el precio final y como se escriben
   las fechas y los montos.

   Reglas de la arquitectura:
     - Aqui NO se toca el DOM (nada de document, nada de querySelector).
     - Aqui NO se hace red (nada de fetch). El modelo no sabe de GraphQL.
     - Si manana cambia una regla de negocio, se cambia solo en este archivo.

   Patron BUILDER:
     Lavadora y Mantenimiento tienen muchos atributos. Un constructor con
     ocho parametros obliga a recordar el orden exacto y es facil equivocarse.
     El Builder arma el objeto paso a paso, con metodos encadenados, y al
     final .construir() revisa que no falte nada. El resultado es un objeto
     Lavadora ya completo.
   ===================================================================== */
(function (global) {
  'use strict';

  const Voltia = global.Voltia = global.Voltia || {};

  // ---------------------------------------------------------------------
  //  Constantes de negocio (deben coincidir con el servidor Java)
  // ---------------------------------------------------------------------
  const FORMATO_FECHA = 'yyyy-MM-ddTHH:mm:ss';
  const RPM_MINIMO = 600;
  const MINUTOS_MINIMOS = 1;
  const MINUTOS_MAXIMOS = 300;
  const BONO_CAPACIDAD = 120000;
  const BONO_INVERTER = 250000;
  const KILOS_QUE_ACTIVAN_EL_BONO = 15;
  const EXPRESION_FECHA = /^(\d{4})-(\d{2})-(\d{2})T(\d{2}):(\d{2}):(\d{2})$/;

  // ---------------------------------------------------------------------
  //  Utilidades de formato (solo texto, sin DOM y sin red)
  // ---------------------------------------------------------------------

  /** Convierte cualquier valor a numero seguro: si no es un numero, devuelve 0. */
  function aNumero(valor) {
    const numero = Number(valor);
    return Number.isFinite(numero) ? numero : 0;
  }

  /** Pone puntos cada tres digitos: 1820000 -> "1.820.000". */
  function agruparMiles(entero) {
    const digitos = String(Math.abs(Math.trunc(entero)));
    let resultado = '';
    for (let i = 0; i < digitos.length; i++) {
      if (i > 0 && (digitos.length - i) % 3 === 0) {
        resultado += '.';
      }
      resultado += digitos[i];
    }
    return resultado;
  }

  /** Formatea dinero: 1820000 -> "$ 1.820.000" y 250000.5 -> "$ 250.000,50". */
  function formatearMonto(valor) {
    const numero = aNumero(valor);
    const signo = numero < 0 ? '-$ ' : '$ ';
    const absoluto = Math.abs(numero);
    const entero = Math.floor(absoluto);
    const centavos = Math.round((absoluto - entero) * 100);
    let texto = agruparMiles(entero);
    if (centavos > 0) {
      texto += ',' + String(centavos).padStart(2, '0');
    }
    return signo + texto;
  }

  /** Formatea cantidades: 15 -> "15", 15.5 -> "15,5". */
  function formatearCantidad(valor) {
    const numero = aNumero(valor);
    if (Number.isInteger(numero)) {
      return String(numero);
    }
    return numero.toFixed(2).replace('.', ',');
  }

  /**
   * Separa un texto yyyy-MM-ddTHH:mm:ss en sus partes.
   * Se trabaja con texto y no con new Date() a proposito: asi la fecha que
   * se ve en pantalla es exactamente la que manda el servidor, sin que la
   * zona horaria del navegador la corra un dia.
   */
  function partirFecha(texto) {
    if (typeof texto !== 'string') {
      return null;
    }
    const partes = EXPRESION_FECHA.exec(texto.trim());
    if (!partes) {
      return null;
    }
    return {
      anio: partes[1],
      mes: partes[2],
      dia: partes[3],
      hora: partes[4],
      minuto: partes[5],
      segundo: partes[6]
    };
  }

  /** Texto de fecha para la pantalla: "2025-06-18T10:45:00" -> "18/06/2025 10:45". */
  function formatearFecha(texto) {
    const partes = partirFecha(texto);
    if (!partes) {
      return texto ? String(texto) : '--';
    }
    return partes.dia + '/' + partes.mes + '/' + partes.anio +
      ' ' + partes.hora + ':' + partes.minuto;
  }

  /** Version corta para la tabla: "2025-06-18T10:45:00" -> "18/06/2025". */
  function formatearFechaCorta(texto) {
    const partes = partirFecha(texto);
    if (!partes) {
      return texto ? String(texto) : '--';
    }
    return partes.dia + '/' + partes.mes + '/' + partes.anio;
  }

  /** Revisa el formato Y que la fecha exista (por ejemplo 2025-02-31 no existe). */
  function esFechaValida(texto) {
    const partes = partirFecha(texto);
    if (!partes) {
      return false;
    }
    const anio = Number(partes.anio);
    const mes = Number(partes.mes);
    const dia = Number(partes.dia);
    const hora = Number(partes.hora);
    const minuto = Number(partes.minuto);
    const segundo = Number(partes.segundo);
    if (mes < 1 || mes > 12 || dia < 1 || hora > 23 || minuto > 59 || segundo > 59) {
      return false;
    }
    const diasDelMes = new Date(anio, mes, 0).getDate();
    return dia <= diasDelMes;
  }

  /** Fecha y hora actuales en el formato del contrato (para precargar formularios). */
  function fechaActualTexto() {
    const ahora = new Date();
    const dosDigitos = (numero) => String(numero).padStart(2, '0');
    return ahora.getFullYear() + '-' + dosDigitos(ahora.getMonth() + 1) + '-' +
      dosDigitos(ahora.getDate()) + 'T' + dosDigitos(ahora.getHours()) + ':' +
      dosDigitos(ahora.getMinutes()) + ':' + dosDigitos(ahora.getSeconds());
  }

  /**
   * Regla del precio final (igual que Lavadora.calcularPrecioFinal del servidor):
   * precio base + 120.000 si carga mas de 15 kilos + 250.000 si es inverter.
   * Se usa para la vista previa del formulario, antes de guardar.
   */
  function calcularPrecioFinal(datos) {
    const precioBase = aNumero(datos && datos.precioBase);
    const capacidad = aNumero(datos && datos.capacidadCargaKilos);
    const inverter = Boolean(datos && datos.tecnologiaInverter);
    let precio = precioBase;
    if (capacidad > KILOS_QUE_ACTIVAN_EL_BONO) {
      precio += BONO_CAPACIDAD;
    }
    if (inverter) {
      precio += BONO_INVERTER;
    }
    return precio;
  }

  // ---------------------------------------------------------------------
  //  Modelo: Mantenimiento (el DETALLE de la relacion maestro - detalle)
  // ---------------------------------------------------------------------
  class Mantenimiento {
    constructor(datos) {
      const origen = datos || {};
      this.id = aNumero(origen.id);
      this.descripcion = String(origen.descripcion || '').trim();
      this.tecnico = String(origen.tecnico || '').trim();
      this.costo = aNumero(origen.costo);
      this.fecha = String(origen.fecha || '');
      this.completado = origen.completado === true;
    }

    get estaPendiente() {
      return this.completado === false;
    }

    get estadoTexto() {
      return this.completado ? 'Completado' : 'Pendiente';
    }

    get estadoClase() {
      return this.completado ? 'completado' : 'pendiente';
    }

    get fechaTexto() {
      return formatearFecha(this.fecha);
    }

    get costoTexto() {
      return formatearMonto(this.costo);
    }

    /** Copia plana, util para comparar y para armar entradas GraphQL. */
    aObjetoPlano() {
      return {
        id: this.id,
        descripcion: this.descripcion,
        tecnico: this.tecnico,
        costo: this.costo,
        fecha: this.fecha,
        completado: this.completado
      };
    }
  }

  // ---------------------------------------------------------------------
  //  Modelo: Lavadora (el MAESTRO de la relacion)
  // ---------------------------------------------------------------------
  class Lavadora {
    constructor(datos) {
      const origen = datos || {};
      this.codigo = aNumero(origen.codigo);
      this.marca = String(origen.marca || '').trim();
      this.precioBase = aNumero(origen.precioBase);
      this.fechaFabricacion = String(origen.fechaFabricacion || '');
      this.capacidadCargaKilos = aNumero(origen.capacidadCargaKilos);
      this.velocidadCentrifugadoRPM = aNumero(origen.velocidadCentrifugadoRPM);
      this.tecnologiaInverter = origen.tecnologiaInverter === true;
      this.minutosLavado = aNumero(origen.minutosLavado);

      // La coleccion de detalle puede llegar vacia, con uno o con muchos
      // mantenimientos. Se normaliza a objetos Mantenimiento del modelo.
      const detalle = Array.isArray(origen.mantenimientos) ? origen.mantenimientos : [];
      this.mantenimientos = detalle.map((elemento) => (
        elemento instanceof Mantenimiento ? elemento : new Mantenimiento(elemento)
      ));

      // El servidor manda un precioFinal calculado; si no llega, se calcula local.
      this.precioFinalServidor = origen.precioFinal === undefined || origen.precioFinal === null
        ? null
        : aNumero(origen.precioFinal);
    }

    get precioFinal() {
      if (this.precioFinalServidor !== null) {
        return this.precioFinalServidor;
      }
      return calcularPrecioFinal(this);
    }

    get cantidadMantenimientos() {
      return this.mantenimientos.length;
    }

    get mantenimientosPendientes() {
      return this.mantenimientos.filter((mantenimiento) => !mantenimiento.completado).length;
    }

    get costoMantenimientos() {
      return this.mantenimientos.reduce(
        (total, mantenimiento) => total + aNumero(mantenimiento.costo),
        0
      );
    }

    get tieneMantenimientos() {
      return this.mantenimientos.length > 0;
    }

    get titulo() {
      return this.codigo + ' - ' + this.marca;
    }

    get inverterTexto() {
      return this.tecnologiaInverter ? 'Si' : 'No';
    }

    get fechaTexto() {
      return formatearFecha(this.fechaFabricacion);
    }

    get fechaCorta() {
      return formatearFechaCorta(this.fechaFabricacion);
    }

    get precioBaseTexto() {
      return formatearMonto(this.precioBase);
    }

    get precioFinalTexto() {
      return formatearMonto(this.precioFinal);
    }

    get costoMantenimientosTexto() {
      return formatearMonto(this.costoMantenimientos);
    }

    get capacidadTexto() {
      return formatearCantidad(this.capacidadCargaKilos) + ' kg';
    }

    get minutosTexto() {
      return this.minutosLavado + ' min';
    }

    get rpmTexto() {
      return this.velocidadCentrifugadoRPM + ' RPM';
    }

    buscarMantenimiento(id) {
      const buscado = aNumero(id);
      return this.mantenimientos.find((mantenimiento) => mantenimiento.id === buscado) || null;
    }

    /** Copia plana con los nombres exactos del contrato GraphQL. */
    aObjetoPlano() {
      return {
        codigo: this.codigo,
        marca: this.marca,
        precioBase: this.precioBase,
        fechaFabricacion: this.fechaFabricacion,
        capacidadCargaKilos: this.capacidadCargaKilos,
        velocidadCentrifugadoRPM: this.velocidadCentrifugadoRPM,
        tecnologiaInverter: this.tecnologiaInverter,
        minutosLavado: this.minutosLavado,
        mantenimientos: this.mantenimientos.map((mantenimiento) => mantenimiento.aObjetoPlano())
      };
    }
  }

  // ---------------------------------------------------------------------
  //  Patron BUILDER: ConstructorMantenimiento
  // ---------------------------------------------------------------------
  class ConstructorMantenimiento {
    constructor() {
      this.datos = { completado: false };
    }

    conId(id) {
      this.datos.id = id;
      return this;
    }

    conDescripcion(descripcion) {
      this.datos.descripcion = descripcion;
      return this;
    }

    conTecnico(tecnico) {
      this.datos.tecnico = tecnico;
      return this;
    }

    conCosto(costo) {
      this.datos.costo = costo;
      return this;
    }

    conFecha(fecha) {
      this.datos.fecha = fecha;
      return this;
    }

    conCompletado(completado) {
      this.datos.completado = completado === true;
      return this;
    }

    /** Valida lo minimo y entrega el objeto Mantenimiento terminado. */
    construir() {
      const faltantes = [];
      if (!String(this.datos.descripcion || '').trim()) {
        faltantes.push('descripcion');
      }
      if (!String(this.datos.tecnico || '').trim()) {
        faltantes.push('tecnico');
      }
      if (!esFechaValida(this.datos.fecha)) {
        faltantes.push('fecha (' + FORMATO_FECHA + ')');
      }
      if (faltantes.length > 0) {
        throw new Error('Faltan datos obligatorios del mantenimiento: ' + faltantes.join(', '));
      }
      return new Mantenimiento(this.datos);
    }
  }

  // ---------------------------------------------------------------------
  //  Patron BUILDER: ConstructorLavadora
  // ---------------------------------------------------------------------
  class ConstructorLavadora {
    constructor() {
      this.datos = { tecnologiaInverter: false, mantenimientos: [] };
    }

    conCodigo(codigo) {
      this.datos.codigo = codigo;
      return this;
    }

    conMarca(marca) {
      this.datos.marca = marca;
      return this;
    }

    conPrecioBase(precioBase) {
      this.datos.precioBase = precioBase;
      return this;
    }

    conFechaFabricacion(fechaFabricacion) {
      this.datos.fechaFabricacion = fechaFabricacion;
      return this;
    }

    conCapacidadCargaKilos(capacidadCargaKilos) {
      this.datos.capacidadCargaKilos = capacidadCargaKilos;
      return this;
    }

    conVelocidadCentrifugadoRPM(velocidadCentrifugadoRPM) {
      this.datos.velocidadCentrifugadoRPM = velocidadCentrifugadoRPM;
      return this;
    }

    conTecnologiaInverter(tecnologiaInverter) {
      this.datos.tecnologiaInverter = tecnologiaInverter === true;
      return this;
    }

    conMinutosLavado(minutosLavado) {
      this.datos.minutosLavado = minutosLavado;
      return this;
    }

    /** Agrega un mantenimiento al detalle mientras se arma la lavadora. */
    conMantenimiento(mantenimiento) {
      this.datos.mantenimientos.push(mantenimiento);
      return this;
    }

    /**
     * Revisa que esten los ocho datos obligatorios. Si falta alguno lanza un
     * error claro en vez de crear una lavadora incompleta.
     */
    construir() {
      const obligatorios = [
        'codigo', 'marca', 'precioBase', 'fechaFabricacion',
        'capacidadCargaKilos', 'velocidadCentrifugadoRPM', 'minutosLavado'
      ];
      const faltantes = obligatorios.filter((campo) => {
        const valor = this.datos[campo];
        return valor === undefined || valor === null || String(valor).trim() === '';
      });
      if (faltantes.length > 0) {
        throw new Error('Faltan datos obligatorios de la lavadora: ' + faltantes.join(', '));
      }
      return new Lavadora(this.datos);
    }
  }

  // ---------------------------------------------------------------------
  //  Validaciones locales (las mismas reglas del servidor, para avisar
  //  rapido sin gastar una peticion de red)
  // ---------------------------------------------------------------------
  const Validaciones = {
    FORMATO_FECHA: FORMATO_FECHA,
    RPM_MINIMO: RPM_MINIMO,
    MINUTOS_MINIMOS: MINUTOS_MINIMOS,
    MINUTOS_MAXIMOS: MINUTOS_MAXIMOS,

    /**
     * Devuelve un objeto { campo: mensaje }. Si esta vacio, todo esta bien.
     * La vista pinta cada mensaje debajo de su campo.
     */
    validarLavadora(datos) {
      const errores = {};
      const codigo = Number(datos.codigo);
      const precioBase = Number(datos.precioBase);
      const capacidad = Number(datos.capacidadCargaKilos);
      const rpm = Number(datos.velocidadCentrifugadoRPM);
      const minutos = Number(datos.minutosLavado);

      if (!Number.isInteger(codigo) || codigo <= 0) {
        errores.codigo = 'El codigo debe ser un numero entero mayor que cero.';
      }
      if (!String(datos.marca || '').trim()) {
        errores.marca = 'La marca es obligatoria.';
      }
      if (!Number.isFinite(precioBase) || precioBase <= 0) {
        errores.precioBase = 'El precio base debe ser un numero mayor que cero.';
      }
      if (!esFechaValida(datos.fechaFabricacion)) {
        errores.fechaFabricacion = 'Use el formato ' + FORMATO_FECHA +
          ' (ejemplo 2025-06-18T10:45:00).';
      }
      if (!Number.isFinite(capacidad) || capacidad <= 0) {
        errores.capacidadCargaKilos = 'La capacidad debe ser mayor que cero.';
      }
      if (!Number.isInteger(rpm) || rpm < RPM_MINIMO) {
        errores.velocidadCentrifugadoRPM = 'La velocidad minima es ' + RPM_MINIMO + ' RPM.';
      }
      if (!Number.isInteger(minutos) || minutos < MINUTOS_MINIMOS || minutos > MINUTOS_MAXIMOS) {
        errores.minutosLavado = 'Los minutos deben estar entre ' + MINUTOS_MINIMOS +
          ' y ' + MINUTOS_MAXIMOS + '.';
      }
      return errores;
    },

    validarMantenimiento(datos) {
      const errores = {};
      const costo = Number(datos.costo);
      if (!String(datos.descripcion || '').trim()) {
        errores.descripcion = 'La descripcion es obligatoria.';
      }
      if (!String(datos.tecnico || '').trim()) {
        errores.tecnico = 'El nombre del tecnico es obligatorio.';
      }
      if (!Number.isFinite(costo) || costo < 0) {
        errores.costo = 'El costo debe ser un numero mayor o igual a cero.';
      }
      if (!esFechaValida(datos.fecha)) {
        errores.fecha = 'Use el formato ' + FORMATO_FECHA +
          ' (ejemplo 2025-06-18T10:45:00).';
      }
      return errores;
    },

    /** Valida la busqueda por uno o dos criterios. */
    validarCriterios(criterios) {
      const errores = {};
      const codigoTexto = String(criterios.codigoTexto || '').trim();
      const marca = String(criterios.marca || '').trim();
      if (!codigoTexto && !marca) {
        errores.general = 'Escriba al menos un criterio de busqueda (codigo o marca).';
      }
      if (codigoTexto) {
        const codigo = Number(codigoTexto);
        if (!Number.isInteger(codigo) || codigo <= 0) {
          errores.codigo = 'El codigo debe ser un numero entero mayor que cero.';
        }
      }
      return errores;
    },

    hayErrores(errores) {
      return Object.keys(errores || {}).length > 0;
    },

    esFechaValida: esFechaValida
  };

  // ---------------------------------------------------------------------
  //  Exportacion al espacio de nombres global (sin modulos ES6)
  // ---------------------------------------------------------------------
  Voltia.constantes = {
    FORMATO_FECHA: FORMATO_FECHA,
    RPM_MINIMO: RPM_MINIMO,
    MINUTOS_MINIMOS: MINUTOS_MINIMOS,
    MINUTOS_MAXIMOS: MINUTOS_MAXIMOS,
    BONO_CAPACIDAD: BONO_CAPACIDAD,
    BONO_INVERTER: BONO_INVERTER
  };
  Voltia.formato = {
    formatearMonto: formatearMonto,
    formatearCantidad: formatearCantidad,
    formatearFecha: formatearFecha,
    formatearFechaCorta: formatearFechaCorta,
    esFechaValida: esFechaValida,
    fechaActualTexto: fechaActualTexto
  };
  Voltia.calcularPrecioFinal = calcularPrecioFinal;
  Voltia.Validaciones = Validaciones;
  Voltia.Mantenimiento = Mantenimiento;
  Voltia.Lavadora = Lavadora;
  Voltia.ConstructorLavadora = ConstructorLavadora;
  Voltia.ConstructorMantenimiento = ConstructorMantenimiento;
})(window);
