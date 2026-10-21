/* =====================================================================
   ARRANQUE DE LA APLICACION (main)
   =====================================================================
   Este archivo es el unico que "cablea" las piezas del MVC:

       modelo  ->  ya esta cargado en window.Voltia
       vista   ->  escucha al notificador
       controlador -> usa el cliente GraphQL y avisa al notificador
       servicio    -> habla con el servidor

   Ademas arranca el VIGILANTE: cada 4 segundos pregunta la versionDatos y,
   si otro cliente (el de escritorio o el de Python) cambio algo, la lista
   se refresca sola sin que el usuario haga nada.

   El vigilante se detiene cuando la pestana queda oculta y se reanuda al
   volver, para no gastar peticiones de red sin necesidad.
   ===================================================================== */
(function (global) {
  'use strict';

  const Voltia = global.Voltia = global.Voltia || {};
  const INTERVALO_VIGILANCIA = 4000;

  function arrancar() {
    const botonTema = document.getElementById('boton-tema');
    const temaGuardado = global.localStorage.getItem('voltia-tema');
    document.body.dataset.tema = temaGuardado === 'claro' ? 'claro' : 'oscuro';
    if (botonTema) {
      const actualizarBotonTema = () => {
        const claro = document.body.dataset.tema === 'claro';
        botonTema.textContent = claro ? 'Modo oscuro' : 'Modo claro';
        botonTema.setAttribute('aria-pressed', String(claro));
      };
      actualizarBotonTema();
      botonTema.addEventListener('click', () => {
        const claro = document.body.dataset.tema !== 'claro';
        document.body.dataset.tema = claro ? 'claro' : 'oscuro';
        global.localStorage.setItem('voltia-tema', claro ? 'claro' : 'oscuro');
        actualizarBotonTema();
      });
    }
    const notificador = Voltia.notificador;
    const cliente = Voltia.clienteGraphQL;

    // Composicion de dependencias: se crean una sola vez y se pasan a quien
    // las necesita. Ni la vista ni el controlador crean servicios por su cuenta.
    const controladorLavadoras = new Voltia.ControladorLavadoras(cliente, notificador);
    const controladorMantenimientos = new Voltia.ControladorMantenimientos(
      cliente,
      notificador,
      controladorLavadoras
    );
    const vista = new Voltia.Vista(notificador);

    vista.iniciar(controladorLavadoras, controladorMantenimientos);

    // -----------------------------------------------------------------
    //  Vigilante de la version de datos
    // -----------------------------------------------------------------
    let vigilante = null;

    function programarVigilante() {
      if (vigilante !== null) {
        return;
      }
      vigilante = global.setInterval(() => {
        if (!document.hidden) {
          controladorLavadoras.verificarVersion();
        }
      }, INTERVALO_VIGILANCIA);
    }

    function detenerVigilante() {
      if (vigilante !== null) {
        global.clearInterval(vigilante);
        vigilante = null;
      }
    }

    document.addEventListener('visibilitychange', () => {
      if (document.hidden) {
        detenerVigilante();
      } else {
        programarVigilante();
        controladorLavadoras.verificarVersion();
      }
    });

    // Primera carga. El vigilante arranca siempre, asi se recupera solo si el
    // servidor estaba apagado al abrir la pagina.
    controladorLavadoras.iniciar().then(() => {
      programarVigilante();
    }).catch(() => {
      programarVigilante();
    });

    // Referencia util para revisar el estado desde la consola del navegador.
    global.VoltiaApp = {
      vista: vista,
      controladorLavadoras: controladorLavadoras,
      controladorMantenimientos: controladorMantenimientos
    };
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', arrancar);
  } else {
    arrancar();
  }
})(window);
