using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ClienteLavadora.util;

namespace ClienteLavadora.gui
{
    /// <summary>
    /// Caja de texto con esquinas redondeadas. Es un Border que lleva un TextBox adentro.
    /// El borde se pone azul cuando el usuario esta escribiendo y el fondo se pone gris
    /// cuando el campo es de solo lectura.
    /// </summary>
    public class CampoTexto : Border
    {
        private readonly TextBox caja;
        private bool soloLectura;

        public CampoTexto()
        {
            CornerRadius = new CornerRadius(8);
            BorderThickness = new Thickness(1.5);
            BorderBrush = Ui.Pincel("#90A4AE");
            Background = Ui.Pincel("#F7F9FA");
            Height = 32;

            caja = new TextBox();
            caja.BorderThickness = new Thickness(0);
            caja.Background = Brushes.Transparent;
            caja.FontSize = 14;
            caja.Foreground = Ui.Pincel("#1B2A33");
            caja.VerticalContentAlignment = VerticalAlignment.Center;
            caja.Padding = new Thickness(8, 0, 8, 0);
            Child = caja;

            // El borde cambia de color segun el foco
            caja.GotKeyboardFocus += (s, e) => BorderBrush = Ui.Pincel("#1E88E5");
            caja.LostKeyboardFocus += (s, e) => BorderBrush = Ui.Pincel("#90A4AE");
        }

        public string Texto
        {
            get { return caja.Text; }
            set { caja.Text = value; }
        }

        public bool SoloLectura
        {
            get { return soloLectura; }
            set
            {
                soloLectura = value;
                caja.IsReadOnly = value;
                Background = Ui.Pincel(value ? "#E1E6EA" : "#F7F9FA");
            }
        }

        /// <summary>Se dispara al presionar una tecla dentro de la caja (se usa para el Enter).</summary>
        public event KeyEventHandler TeclaPresionada
        {
            add { caja.KeyDown += value; }
            remove { caja.KeyDown -= value; }
        }
    }
}
