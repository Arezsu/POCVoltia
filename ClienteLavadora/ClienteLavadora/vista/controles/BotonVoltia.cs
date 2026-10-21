using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ClienteLavadora.util;

namespace ClienteLavadora.vista.controles
{
    /// <summary>
    /// Boton de la casa VOLTIA. Se dibuja a mano (un Border redondeado con un texto
    /// adentro) porque en este proyecto NO se usa XAML.
    ///
    /// TIENE DOS ESTILOS:
    ///  - relleno = true  ("Principal" y "Peligro"): fondo del color del acento y letra oscura.
    ///  - relleno = false ("Secundario" y "Acento"): fondo casi transparente y letra del acento.
    ///
    /// Usa un evento propio llamado Click (RoutedEventHandler) para que las ventanas
    /// escriban boton.Click += ... igual que con un Button normal de WPF.
    /// </summary>
    public class BotonVoltia : Border
    {
        /// <summary>Aviso que se dispara cuando el usuario suelta el mouse dentro del boton.</summary>
        public event RoutedEventHandler Click;

        private readonly TextBlock texto;
        private readonly Color acento;
        private readonly bool relleno;

        private bool habilitado = true;
        private bool ratonEncima;

        public BotonVoltia(string titulo) : this(titulo, Tema.HexAceroClaro, false)
        {
        }

        public BotonVoltia(string titulo, string acentoHex, bool relleno)
        {
            this.acento = Tema.ColorDe(acentoHex);
            this.relleno = relleno;

            CornerRadius = new CornerRadius(10);
            BorderThickness = new Thickness(1);
            Height = 36;
            MinWidth = 92;
            Padding = new Thickness(12, 0, 12, 0);

            texto = new TextBlock();
            texto.Text = titulo;
            texto.FontFamily = Tema.Fuente;
            texto.FontSize = 13;
            texto.FontWeight = FontWeights.SemiBold;
            texto.HorizontalAlignment = HorizontalAlignment.Center;
            texto.VerticalAlignment = VerticalAlignment.Center;
            Child = texto;

            AplicarColores();

            MouseEnter += AlEntrarElRaton;
            MouseLeave += AlSalirElRaton;
            MouseLeftButtonDown += AlPresionar;
            MouseLeftButtonUp += AlSoltar;
        }

        // ================= Estilos listos para usar =================

        /// <summary>Boton principal: relleno cian, letra oscura. Para "Guardar", "Buscar".</summary>
        public static BotonVoltia Principal(string titulo)
        {
            return new BotonVoltia(titulo, Tema.HexCian, true);
        }

        /// <summary>Boton secundario: solo borde de acero. Para "Limpiar", "Cerrar".</summary>
        public static BotonVoltia Secundario(string titulo)
        {
            return new BotonVoltia(titulo, Tema.HexAceroClaro, false);
        }

        /// <summary>Boton de advertencia suave (ambar). Para "Recargar", "Filtrar".</summary>
        public static BotonVoltia Acento(string titulo)
        {
            return new BotonVoltia(titulo, Tema.HexAmbar, false);
        }

        /// <summary>Boton destructivo (rojo). Solo para eliminar.</summary>
        public static BotonVoltia Peligro(string titulo)
        {
            return new BotonVoltia(titulo, Tema.HexError, true);
        }

        // ================= Propiedades =================

        public string Texto
        {
            get { return texto.Text; }
            set { texto.Text = value; }
        }

        /// <summary>Si es false, el boton se ve apagado y no responde al mouse.</summary>
        public bool Habilitado
        {
            get { return habilitado; }
            set
            {
                habilitado = value;
                AplicarColores();
            }
        }

        // ================= Pintura =================

        private void AplicarColores()
        {
            if (!habilitado)
            {
                Background = Tema.Pincel("#10FFFFFF");
                BorderBrush = Tema.Pincel("#18FFFFFF");
                texto.Foreground = Tema.TextoSecundario;
                Cursor = Cursors.Arrow;
                return;
            }

            Cursor = Cursors.Hand;

            if (relleno)
            {
                byte alfa = (byte)(ratonEncima ? 255 : 225);
                Background = new SolidColorBrush(Color.FromArgb(alfa, acento.R, acento.G, acento.B));
                BorderBrush = new SolidColorBrush(Color.FromArgb(255, acento.R, acento.G, acento.B));
                texto.Foreground = Tema.Pincel(Tema.HexFondo);
                return;
            }

            byte fondo = (byte)(ratonEncima ? 0x3A : 0x1C);
            byte borde = (byte)(ratonEncima ? 0xEE : 0x77);
            Background = new SolidColorBrush(Color.FromArgb(fondo, acento.R, acento.G, acento.B));
            BorderBrush = new SolidColorBrush(Color.FromArgb(borde, acento.R, acento.G, acento.B));
            texto.Foreground = new SolidColorBrush(acento);
        }

        // ================= Eventos del mouse =================

        private void AlEntrarElRaton(object remitente, MouseEventArgs argumentos)
        {
            if (!habilitado)
            {
                return;
            }
            ratonEncima = true;
            AplicarColores();
        }

        private void AlSalirElRaton(object remitente, MouseEventArgs argumentos)
        {
            ratonEncima = false;
            AplicarColores();
        }

        private void AlPresionar(object remitente, MouseButtonEventArgs argumentos)
        {
            if (!habilitado)
            {
                return;
            }
            CaptureMouse(); // Para saber si el usuario suelta el boton dentro o fuera
            texto.Opacity = 0.65;
        }

        private void AlSoltar(object remitente, MouseButtonEventArgs argumentos)
        {
            texto.Opacity = 1;

            if (!IsMouseCaptured)
            {
                return;
            }
            ReleaseMouseCapture();

            // Solo cuenta como clic si el mouse se solto dentro del boton.
            Point posicion = argumentos.GetPosition(this);
            bool dentro = posicion.X >= 0 && posicion.Y >= 0
                          && posicion.X <= ActualWidth && posicion.Y <= ActualHeight;

            if (habilitado && dentro && Click != null)
            {
                Click(this, new RoutedEventArgs());
            }
        }
    }
}
