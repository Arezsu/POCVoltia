/* =====================================================================
   SERVICIO GRAPHQL (capa de red)
   =====================================================================
   Unica capa que conoce la direccion del servidor y el lenguaje GraphQL.
   La vista NUNCA llama a este archivo: siempre pasa por el controlador.
   Asi, si manana el servicio cambia de GraphQL a REST, solo se toca aqui.

   Patron SINGLETON:
   La aplicacion necesita un solo cliente GraphQL: un solo lugar donde se
   cuentan las peticiones en curso y un solo estado de conexion. La IIFE se
   ejecuta una vez y devuelve el objeto ya creado; quien lo use recibe
   siempre la misma instancia:
       Voltia.clienteGraphQL.listarLavadoras(...)

   Manejo de errores:
   GraphQL responde 200 incluso cuando hay un error de negocio; el error
   viaja en el arreglo "errors". Ese mensaje ya viene en espanol desde el
   servidor, asi que se convierte en un Error normal y se lanza hacia arriba
   para que el controlador lo convierta en un aviso para el usuario.
   ===================================================================== */
(function (global) {
  'use strict';

  const Voltia = global.Voltia = global.Voltia || {};

  const URL_SERVIDOR = 'http://localhost:8081/graphql';

  // ---------------------------------------------------------------------
  //  Campos que se piden al servidor
  // ---------------------------------------------------------------------
  const CAMPOS_MANTENIMIENTO = `
    id
    descripcion
    tecnico
    costo
    fecha
    completado
  `;

  const CAMPOS_LAVADORA = `
    codigo
    marca
    precioBase
    fechaFabricacion
    capacidadCargaKilos
    velocidadCentrifugadoRPM
    tecnologiaInverter
    minutosLavado
    precioFinal
    cantidadMantenimientos
    mantenimientosPendientes
    costoMantenimientos
    mantenimientos {
      ${CAMPOS_MANTENIMIENTO}
    }
  `;

  // ---------------------------------------------------------------------
  //  Documentos GraphQL: queries y mutations con variables
  //  Las variables ("$marca", "$input", ...) evitan armar el texto a mano
  //  y dejan que el servidor valide los tipos.
  // ---------------------------------------------------------------------
  const DOCUMENTOS = {
    LAVADORAS_POR_FILTRO: `
      query LavadorasPorFiltro($marca: String, $tecnologiaInverter: Boolean, $conMantenimientos: Boolean) {
        lavadorasPorFiltro(
          marca: $marca
          tecnologiaInverter: $tecnologiaInverter
          conMantenimientos: $conMantenimientos
        ) {
          ${CAMPOS_LAVADORA}
        }
      }
    `,
    LAVADORA_POR_CRITERIOS: `
      query LavadoraPorCriterios($codigo: Int, $marca: String) {
        lavadoraPorCriterios(codigo: $codigo, marca: $marca) {
          ${CAMPOS_LAVADORA}
        }
      }
    `,
    VERSION_DATOS: `
      query VersionDatos {
        versionDatos
      }
    `,
    MANTENIMIENTOS_POR_FILTRO: `
      query MantenimientosPorFiltro($codigoLavadora: Int!, $tecnico: String, $completado: Boolean) {
        mantenimientosPorFiltro(
          codigoLavadora: $codigoLavadora
          tecnico: $tecnico
          completado: $completado
        ) {
          ${CAMPOS_MANTENIMIENTO}
        }
      }
    `,
    AGREGAR_LAVADORA: `
      mutation AgregarLavadora($input: LavadoraInput!) {
        addLavadora(input: $input) {
          ${CAMPOS_LAVADORA}
        }
      }
    `,
    ACTUALIZAR_LAVADORA: `
      mutation ActualizarLavadora($codigo: Int!, $input: LavadoraUpdateInput!) {
        actualizarLavadora(codigo: $codigo, input: $input) {
          ${CAMPOS_LAVADORA}
        }
      }
    `,
    ELIMINAR_LAVADORA: `
      mutation EliminarLavadora($codigo: Int!) {
        eliminarLavadora(codigo: $codigo)
      }
    `,
    AGREGAR_MANTENIMIENTO: `
      mutation AgregarMantenimiento($codigoLavadora: Int!, $input: MantenimientoInput!) {
        addMantenimiento(codigoLavadora: $codigoLavadora, input: $input) {
          ${CAMPOS_MANTENIMIENTO}
        }
      }
    `,
    ACTUALIZAR_MANTENIMIENTO: `
      mutation ActualizarMantenimiento($codigoLavadora: Int!, $id: Int!, $input: MantenimientoUpdateInput!) {
        actualizarMantenimiento(codigoLavadora: $codigoLavadora, id: $id, input: $input) {
          ${CAMPOS_MANTENIMIENTO}
        }
      }
    `,
    ELIMINAR_MANTENIMIENTO: `
      mutation EliminarMantenimiento($codigoLavadora: Int!, $id: Int!) {
        eliminarMantenimiento(codigoLavadora: $codigoLavadora, id: $id)
      }
    `
  };

  // ---------------------------------------------------------------------
  //  Instancia unica
  // ---------------------------------------------------------------------
  const cliente = (function crearCliente() {
    const notificador = Voltia.notificador;
    const registro = Voltia.registro;

    let peticionesEnCurso = 0;
    let ultimaConexion = null;

    /** Avisa a la vista si hay red en curso (la compuerta acelera). */
    function avisarTrabajo() {
      if (notificador) {
        notificador.notificar(Voltia.EVENTOS.TRABAJANDO, peticionesEnCurso > 0);
      }
    }

    /** Marca el estado de conexion solo cuando cambia. */
    function avisarConexion(conectado) {
      if (ultimaConexion === conectado) {
        return;
      }
      ultimaConexion = conectado;
      if (notificador) {
        notificador.notificar(Voltia.EVENTOS.CONEXION, { conectado: conectado });
      }
    }

    /**
     * Quita las variables nulas. GraphQL acepta null, pero enviar solo lo que
     * el usuario realmente eligio deja las peticiones mas claras y evita
     * mandar filtros vacios.
     */
    function limpiarVariables(variables) {
      const resultado = {};
      Object.keys(variables || {}).forEach((nombre) => {
        const valor = variables[nombre];
        if (valor !== null && valor !== undefined) {
          resultado[nombre] = valor;
        }
      });
      return resultado;
    }

    /** Normaliza los dos tipos de fallo: sin red y con error de negocio. */
    function normalizarError(error) {
      if (error && error.name === 'AbortError') {
        return new Error('La peticion tardo demasiado. Intente de nuevo.');
      }
      const mensaje = error && error.message ? error.message : '';
      if (error instanceof TypeError || mensaje === 'Failed to fetch' || mensaje === 'NetworkError when attempting to fetch resource.') {
        return new Error('No se pudo conectar con el servidor GraphQL (' + URL_SERVIDOR + '). Verifique que este encendido.');
      }
      return error;
    }

    /**
     * Ejecuta un documento GraphQL.
     * 1. Cuenta la peticion (para el estado LAVANDO).
     * 2. Hace el POST con fetch.
     * 3. Si el cuerpo trae "errors", lanza el mensaje tal cual.
     * 4. Devuelve solo "data" para que el controlador no navegue dentro
     *    de la estructura de la respuesta.
     */
    async function ejecutar(documento, variables) {
      peticionesEnCurso += 1;
      avisarTrabajo();
      try {
        const respuesta = await fetch(URL_SERVIDOR, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({
            query: documento,
            variables: limpiarVariables(variables)
          })
        });

        if (!respuesta.ok) {
          throw new Error('El servidor respondio con estado HTTP ' + respuesta.status + '.');
        }

        const cuerpo = await respuesta.json();

        if (Array.isArray(cuerpo.errors) && cuerpo.errors.length > 0) {
          const mensajes = cuerpo.errors
            .map((elemento) => (elemento && elemento.message ? elemento.message : 'Error desconocido'))
            .join(' | ');
          throw new Error(mensajes);
        }

        if (!cuerpo.data) {
          throw new Error('El servidor no devolvio datos.');
        }

        avisarConexion(true);
        return cuerpo.data;
      } catch (error) {
        const errorNormalizado = normalizarError(error);
        // Un error de negocio (datos invalidos) NO significa que el servidor
        // este caido: solo se marca "sin conexion" cuando falla la red.
        if (errorNormalizado instanceof TypeError || /No se pudo conectar/.test(errorNormalizado.message)) {
          avisarConexion(false);
        }
        registro.error('Peticion GraphQL fallida', errorNormalizado.message);
        throw errorNormalizado;
      } finally {
        peticionesEnCurso -= 1;
        avisarTrabajo();
      }
    }

    // -------------------------------------------------------------------
    //  API publica del cliente
    // -------------------------------------------------------------------
    return {
      url: URL_SERVIDOR,

      estaTrabajando: function () {
        return peticionesEnCurso > 0;
      },

      contarPeticiones: function () {
        return peticionesEnCurso;
      },

      /** Lista las lavadoras con los filtros que aplica el servidor. */
      listarLavadoras: async function (filtros) {
        const datos = await ejecutar(DOCUMENTOS.LAVADORAS_POR_FILTRO, filtros || {});
        return datos.lavadorasPorFiltro || [];
      },

      /** Busca UNA lavadora por codigo y/o marca. Devuelve null si no existe. */
      buscarLavadoraPorCriterios: async function (criterios) {
        const datos = await ejecutar(DOCUMENTOS.LAVADORA_POR_CRITERIOS, criterios || {});
        return datos.lavadoraPorCriterios || null;
      },

      /** Numero de version de los datos: cambia cuando alguien modifica algo. */
      obtenerVersionDatos: async function () {
        const datos = await ejecutar(DOCUMENTOS.VERSION_DATOS, {});
        return datos.versionDatos;
      },

      /** Mantenimientos de una lavadora, con filtros opcionales. */
      consultarMantenimientos: async function (codigoLavadora, filtros) {
        const variables = Object.assign({ codigoLavadora: codigoLavadora }, filtros || {});
        const datos = await ejecutar(DOCUMENTOS.MANTENIMIENTOS_POR_FILTRO, variables);
        return datos.mantenimientosPorFiltro || [];
      },

      agregarLavadora: async function (input) {
        const datos = await ejecutar(DOCUMENTOS.AGREGAR_LAVADORA, { input: input });
        return datos.addLavadora;
      },

      actualizarLavadora: async function (codigo, input) {
        const datos = await ejecutar(DOCUMENTOS.ACTUALIZAR_LAVADORA, { codigo: codigo, input: input });
        return datos.actualizarLavadora;
      },

      eliminarLavadora: async function (codigo) {
        const datos = await ejecutar(DOCUMENTOS.ELIMINAR_LAVADORA, { codigo: codigo });
        return datos.eliminarLavadora === true;
      },

      agregarMantenimiento: async function (codigoLavadora, input) {
        const datos = await ejecutar(DOCUMENTOS.AGREGAR_MANTENIMIENTO, {
          codigoLavadora: codigoLavadora,
          input: input
        });
        return datos.addMantenimiento;
      },

      actualizarMantenimiento: async function (codigoLavadora, id, input) {
        const datos = await ejecutar(DOCUMENTOS.ACTUALIZAR_MANTENIMIENTO, {
          codigoLavadora: codigoLavadora,
          id: id,
          input: input
        });
        return datos.actualizarMantenimiento;
      },

      eliminarMantenimiento: async function (codigoLavadora, id) {
        const datos = await ejecutar(DOCUMENTOS.ELIMINAR_MANTENIMIENTO, {
          codigoLavadora: codigoLavadora,
          id: id
        });
        return datos.eliminarMantenimiento === true;
      },

      /** Documentos expuestos solo para revisar el contrato en la consola. */
      documentos: DOCUMENTOS
    };
  })();

  Voltia.ClienteGraphQL = cliente;
  Voltia.clienteGraphQL = cliente;
})(window);
