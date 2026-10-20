using System;

namespace ClienteLavadora.util
{
    /// <summary>
    /// Patron Observer (igual que ObservadorListado en el proyecto Java).
    /// Cuando una ventana adiciona, actualiza o elimina una lavadora, avisa aqui,
    /// y las ventanas de Listar que esten abiertas se recargan solas.
    /// </summary>
    public static class Notificador
    {
        // "event" = lista de ventanas suscritas que quieren enterarse del cambio
        public static event Action DatosCambiaron; // Aviso a suscriptores

        // ---- Aviso de "hay una peticion al servidor en camino" ----
        // Las ventanas lo usan para hacer girar la perilla mientras se espera la respuesta.

        public static event Action<bool> Trabajando; // Aviso a suscriptores

        private static int pendientes;

        /// <summary>true = empezo una peticion, false = termino. Si hay varias a la vez, las cuenta.</summary>
        public static void CambiarTrabajando(bool empezo)
        {
            bool antes = pendientes > 0;
            pendientes += empezo ? 1 : -1;
            if (pendientes < 0)
            {
                pendientes = 0;
            }
            bool ahora = pendientes > 0;

            Action<bool> avisar = Trabajando;
            if (antes != ahora && avisar != null)
            {
                avisar(ahora);
            }
        }

        public static void Avisar()
        {
            Action avisar = DatosCambiaron;
            if (avisar != null)
            {
                avisar();
            }
        }
    }
}
