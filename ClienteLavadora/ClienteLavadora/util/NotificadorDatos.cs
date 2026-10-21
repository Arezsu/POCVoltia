using System;

namespace ClienteLavadora.util
{
    /// <summary>
    /// Patron OBSERVER del lado del cliente (igual que ObservadorRepositorio en el Java).
    ///
    /// IDEA EN UNA FRASE: un objeto avisa y muchos escuchan, sin conocerse entre si.
    /// Ejemplo real de esta aplicacion:
    ///   - La ventana Listar se SUSCRIBE al evento DatosCambiaron.
    ///   - La ventana Agregar, cuando guarda una lavadora, solo dice "AvisarDatosCambiaron".
    ///   - La ventana Listar se entera y se recarga sola, sin que Agregar sepa que existe.
    ///
    /// Las ventanas que se suscriben TIENEN que desuscribirse al cerrarse
    /// (Closed += ... -= ...), porque si no, el evento se queda con referencias a
    /// ventanas ya cerradas y el programa gasta memoria sin necesidad.
    /// </summary>
    public static class NotificadorDatos
    {
        /// <summary>Se dispara cuando los datos cambiaron (local o en otro cliente).</summary>
        public static event Action DatosCambiaron;

        /// <summary>
        /// Avisa si hay peticiones al servidor en curso (true) o ya terminaron (false).
        /// La pantalla digital del marco y el tambor del ojo de buey escuchan este evento.
        /// </summary>
        public static event Action<bool> Trabajando;

        /// <summary>Avisa si el servidor responde (true) o no (false).</summary>
        public static event Action<bool> EstadoConexion;

        // Cuenta cuantas peticiones hay al mismo tiempo: se avisa solo cuando pasa de
        // cero a una, o de una a cero. Asi la pantalla no parpadea con cada respuesta.
        private static int peticionesEnCurso;

        /// <summary>Indica si en este momento hay al menos una peticion en camino.</summary>
        public static bool Ocupado
        {
            get { return peticionesEnCurso > 0; }
        }

        /// <summary>Una peticion empezo (true) o termino (false).</summary>
        public static void AvisarTrabajando(bool empezo)
        {
            bool estabaOcupado = peticionesEnCurso > 0;
            peticionesEnCurso += empezo ? 1 : -1;
            if (peticionesEnCurso < 0)
            {
                peticionesEnCurso = 0; // Proteccion por si alguna respuesta llega dos veces
            }
            bool estaOcupado = peticionesEnCurso > 0;

            if (estabaOcupado == estaOcupado)
            {
                return; // No cambio el estado: no hace falta avisar
            }

            Action<bool> manejador = Trabajando;
            if (manejador != null)
            {
                manejador(estaOcupado);
            }
        }

        /// <summary>Avisa que los datos cambiaron. Lo llaman los controladores despues de guardar.</summary>
        public static void AvisarDatosCambiaron()
        {
            Action manejador = DatosCambiaron;
            if (manejador != null)
            {
                manejador();
            }
        }

        /// <summary>Avisa si el servidor esta en linea. Lo usa el VigilanteServidor.</summary>
        public static void AvisarConexion(bool conectado)
        {
            Action<bool> manejador = EstadoConexion;
            if (manejador != null)
            {
                manejador(conectado);
            }
        }
    }
}
