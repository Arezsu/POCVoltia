using System;
using System.Threading.Tasks;
using System.Windows.Threading;
using ClienteLavadora.util;

namespace ClienteLavadora.controlador
{
    /// <summary>
    /// VIGILANTE DEL SERVIDOR: pregunta cada 3 segundos "que version tienen los datos?"
    /// y, si la version cambio respecto a la anterior, avisa que ALGUIEN modifico los
    /// datos (puede ser este mismo cliente o, lo interesante, otro cliente abierto en
    /// otro computador).
    ///
    /// COMO FUNCIONA EL TRUCO DE LA VERSION:
    /// el servidor guarda un contador que sube en 1 con cada cambio de cualquier
    /// cliente. Si yo leo 4 y luego leo 5, se que alguien cambio algo: no necesito
    /// comparar fila por fila. Es la misma idea del patron OBSERVER, pero "sondeando"
    /// (polling) al servidor cada pocos segundos.
    ///
    /// POR QUE CON DispatcherTimer Y NO CON UN HILO PROPIO:
    /// DispatcherTimer entrega el aviso en el hilo de la interfaz, entonces las
    /// ventanas pueden actualizar sus controles sin miedo. Aun asi, cada aviso se
    /// lanza con Dispatcher.Invoke para dejar explicito que la interfaz se toca
    /// siempre en su propio hilo.
    ///
    /// POR QUE SE PROTEGE CON "revisando":
    /// si una revision tarda mas de 3 segundos (servidor lento), no queremos acumular
    /// peticiones encima de peticiones.
    /// </summary>
    public class VigilanteServidor
    {
        private readonly ControladorLavadoras controlador;
        private readonly DispatcherTimer reloj;

        private int ultimaVersion;
        private bool revisando;
        private bool primeraRevision = true;

        /// <summary>Se crea apagado; hay que llamar a Iniciar().</summary>
        public VigilanteServidor(ControladorLavadoras controlador)
        {
            this.controlador = controlador;
            reloj = new DispatcherTimer();
            reloj.Interval = TimeSpan.FromSeconds(3); // Cada 3 segundos
            reloj.Tick += AlTic;
        }

        /// <summary>Ultima version leida del servidor (util para mostrar en pantalla).</summary>
        public int UltimaVersion
        {
            get { return ultimaVersion; }
        }

        /// <summary>Empieza a vigilar. Hace una revision inmediata y despues cada 3 segundos.</summary>
        public void Iniciar()
        {
            reloj.Start();
            Revisar(); // Primera revision: solo guarda la version, todavia no avisa nada
        }

        public void Detener()
        {
            reloj.Stop();
        }

        private void AlTic(object remitente, EventArgs argumentos)
        {
            Revisar();
        }

        /// <summary>
        /// Lee la version del servidor. Si cambio desde la ultima vez, avisa a las
        /// ventanas para que se recarguen.
        /// </summary>
        private async void Revisar()
        {
            if (revisando)
            {
                return; // Todavia no termina la revision anterior
            }
            revisando = true;
            try
            {
                // Task.Run: la red nunca se corre en el hilo de la interfaz.
                int version = await Task.Run(() => controlador.VersionDatosAsync());

                AvisarConexion(true);

                bool cambio;
                if (primeraRevision)
                {
                    primeraRevision = false;
                    cambio = false; // La primera vez solo se aprende la version actual
                }
                else
                {
                    cambio = version != ultimaVersion;
                }

                ultimaVersion = version;

                if (cambio)
                {
                    // Alguien (otro cliente o nosotros) cambio los datos: recargar.
                    // Si fue este mismo cliente, la ventana se recarga una vez mas; no
                    // molesta y permite un codigo corto y facil de explicar.
                    AvisarCambio();
                }
            }
            catch (Exception)
            {
                AvisarConexion(false);
            }
            finally
            {
                revisando = false;
            }
        }

        private void AvisarCambio()
        {
            if (reloj.Dispatcher.CheckAccess())
            {
                NotificadorDatos.AvisarDatosCambiaron();
                return;
            }
            reloj.Dispatcher.Invoke(new Action(NotificadorDatos.AvisarDatosCambiaron));
        }

        private void AvisarConexion(bool conectado)
        {
            if (reloj.Dispatcher.CheckAccess())
            {
                NotificadorDatos.AvisarConexion(conectado);
                return;
            }
            reloj.Dispatcher.Invoke(new Action<bool>(NotificadorDatos.AvisarConexion), conectado);
        }
    }
}
