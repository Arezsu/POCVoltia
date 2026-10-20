using System.Windows;
using System.Windows.Media;

namespace ClienteLavadora.gui
{
    /// <summary>
    /// Ventana base de toda la aplicacion. Es una ventana sin borde y transparente
    /// que adentro lleva el MarcoLavadora, asi la ventana completa tiene forma de lavadora.
    /// Las ventanas de cada caso de uso heredan de esta clase y solo agregan su contenido.
    /// </summary>
    public class VentanaMarco : Window
    {
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

            Marco = new MarcoLavadora(titulo, mostrarOjo, ladoOjo);
            Content = Marco;
        }

        /// <summary>El contenido de la ventana (todo lo que va dentro del cuerpo de la lavadora).</summary>
        protected UIElement Contenido
        {
            set { Marco.Contenido = value; }
        }
    }
}
