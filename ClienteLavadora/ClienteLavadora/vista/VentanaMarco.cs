using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ClienteLavadora.util;
using ClienteLavadora.vista.controles;

namespace ClienteLavadora.vista
{
    /// <summary>
    /// VENTANA BASE de toda la aplicacion. Es una ventana SIN BORDE y TRANSPARENTE
    /// (WindowStyle.None + AllowsTransparency) que por dentro lleva el MarcoLavadora:
    /// asi la ventana completa tiene forma de lavadora y no de ventana de Windows.
    ///
    /// Cada caso de uso (listar, agregar, consultar...) hereda de esta clase y solo
    /// escribe su contenido con la propiedad Contenido. Aqui quedan resueltos de una
    /// vez: el tamano, el color de fondo, la tecla ESC para cerrar y los avisos.
    ///
    /// Esa es la "V" de MVC: esta capa NO hace peticiones de red ni sabe que existe
    /// GraphQL; para eso estan los controladores.
    /// </summary>
    public class VentanaMarco : Window
    {
        /// <summary>El marco (chasis + panel) que dibuja la lavadora.</summary>
        protected MarcoLavadora Marco { get; private set; }

        public VentanaMarco(string titulo, double ancho, double alto, bool mostrarOjo, double ladoOjo)
        {
            Title = titulo;
            Width = ancho;
            Height = alto;
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            ResizeMode = ResizeMode.NoResize;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ShowInTaskbar = false;
            FontFamily = Tema.Fuente;

            Marco = new MarcoLavadora(titulo, mostrarOjo, ladoOjo);
            Content = Marco;

            KeyDown += AlPresionarTecla;
        }

        /// <summary>Lo que va dentro del cuerpo de la lavadora.</summary>
        protected UIElement Contenido
        {
            set { Marco.Contenido = value; }
        }

        // ====================================================================
        //  Apoyos que usan todas las ventanas hijas
        // ====================================================================

        /// <summary>Cierra la ventana con la animacion de salida del marco.</summary>
        protected void Cerrar()
        {
            Marco.Cerrar();
        }

        /// <summary>Lanza el confeti (operacion exitosa).</summary>
        protected void Celebrar()
        {
            Marco.Celebrar();
        }

        /// <summary>Muestra un mensaje de error con el estilo de la casa.</summary>
        protected void Aviso(string titulo, string mensaje)
        {
            DialogoVoltia.Avisar(this, titulo, mensaje);
        }

        /// <summary>Muestra un mensaje informativo (algo salio bien).</summary>
        protected void Informar(string titulo, string mensaje)
        {
            DialogoVoltia.Informar(this, titulo, mensaje);
        }

        /// <summary>Pregunta antes de una accion destructiva. Devuelve true si el usuario acepta.</summary>
        protected bool Confirmar(string titulo, string mensaje, string textoAceptar, bool peligro)
        {
            return DialogoVoltia.Confirmar(this, titulo, mensaje, textoAceptar, peligro);
        }

        // ====================================================================
        //  Teclado
        // ====================================================================

        private void AlPresionarTecla(object remitente, KeyEventArgs argumentos)
        {
            if (argumentos.Key == Key.Escape)
            {
                argumentos.Handled = true;
                Cerrar();
            }
        }
    }
}
