using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ClienteLavadora.util;

namespace ClienteLavadora.vista.controles
{
    /// <summary>
    /// Caja de texto con el estilo de la casa: esquinas redondeadas, fondo casi negro
    /// y borde cian cuando el usuario esta escribiendo.
    ///
    /// Es un Border que lleva un TextBox adentro. Se hace asi (y no se usa el TextBox
    /// pelado) porque el TextBox de WPF no tiene esquinas redondeadas.
    ///
    /// Los formularios de la aplicacion nunca tocan el TextBox directamente: usan la
    /// propiedad Texto, y asi manana se puede cambiar la apariencia sin tocar las ventanas.
    /// </summary>
    public class CampoTexto : Border
    {
        private readonly TextBox caja;
        private bool soloLectura;

        public CampoTexto()
        {
            CornerRadius = new CornerRadius(8);
            BorderThickness = new Thickness(1);
            BorderBrush = Tema.Pincel("#2A3A47");
            Background = Tema.Pincel("#0E1620");
            Height = 32;

            caja = new TextBox();
            caja.BorderThickness = new Thickness(0);
            caja.Background = Brushes.Transparent;
            caja.Foreground = Tema.Texto;
            caja.CaretBrush = Tema.Cian;
            caja.FontFamily = Tema.Fuente;
            caja.FontSize = 13;
            caja.VerticalContentAlignment = VerticalAlignment.Center;
            caja.Padding = new Thickness(8, 0, 8, 0);
            Child = caja;

            caja.GotKeyboardFocus += AlTomarFoco;
            caja.LostKeyboardFocus += AlPerderFoco;
        }

        // ================= Propiedades =================

        public string Texto
        {
            get { return caja.Text; }
            set { caja.Text = value; }
        }

        /// <summary>Cuando es true el usuario puede leer pero no escribir (datos de consulta).</summary>
        public bool SoloLectura
        {
            get { return soloLectura; }
            set
            {
                soloLectura = value;
                caja.IsReadOnly = value;
                Background = Tema.Pincel(value ? "#0A1219" : "#0E1620");
                caja.Foreground = value ? Tema.TextoSecundario : Tema.Texto;
            }
        }

        /// <summary>Permite poner los numeros en Consolas (ancho fijo) para que se alineen.</summary>
        public FontFamily Fuente
        {
            set { caja.FontFamily = value; }
        }

        /// <summary>Texto de ayuda que aparece al dejar el mouse encima.</summary>
        public string Ayuda
        {
            set { caja.ToolTip = value; }
        }

        /// <summary>Se dispara al presionar una tecla dentro de la caja (se usa para Enter).</summary>
        public event KeyEventHandler TeclaPresionada
        {
            add { caja.KeyDown += value; }
            remove { caja.KeyDown -= value; }
        }

        /// <summary>Se dispara cada vez que cambia el texto (para la vista previa).</summary>
        public event TextChangedEventHandler TextoCambiado
        {
            add { caja.TextChanged += value; }
            remove { caja.TextChanged -= value; }
        }

        /// <summary>Lleva el cursor a la caja como si el usuario hubiera hecho clic.</summary>
        public void Enfocar()
        {
            caja.Focus();
            caja.SelectAll();
        }

        // ================= Apariencia =================

        private void AlTomarFoco(object remitente, KeyboardFocusChangedEventArgs argumentos)
        {
            if (!soloLectura)
            {
                BorderBrush = Tema.Cian;
            }
        }

        private void AlPerderFoco(object remitente, KeyboardFocusChangedEventArgs argumentos)
        {
            BorderBrush = Tema.Pincel("#2A3A47");
        }
    }
}
