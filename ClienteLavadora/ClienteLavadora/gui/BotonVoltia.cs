using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ClienteLavadora.util;

namespace ClienteLavadora.gui
{
    /// <summary>
    /// Boton redondeado con brillo. Se dibuja a mano (un Border con un texto adentro)
    /// y avisa con el evento Click cuando se suelta el mouse sobre el.
    /// Al pasar el mouse crece un poco con resorte y el brillo se intensifica.
    /// </summary>
    public class BotonVoltia : Border
    {
        public event RoutedEventHandler Click; // Aviso a suscriptores

        private readonly TextBlock texto;
        private readonly Border brillo;
        private bool habilitado = true;

        public BotonVoltia(string titulo, Brush fondo) : this(titulo, fondo, Brushes.White)
        {
        }

        public BotonVoltia(string titulo, Brush fondo, Brush colorTexto)
        {
            CornerRadius = new CornerRadius(12);
            Background = fondo;
            BorderBrush = Ui.Pincel("#55FFFFFF");
            BorderThickness = new Thickness(1);
            Height = 40;
            MinWidth = 90;
            Cursor = Cursors.Hand;

            // Brillo de la mitad de arriba
            brillo = new Border();
            brillo.CornerRadius = new CornerRadius(12);
            brillo.IsHitTestVisible = false;
            brillo.Opacity = 0.5;
            brillo.Background = Ui.Degradado("#99FFFFFF", "#00FFFFFF", "#00FFFFFF");

            texto = new TextBlock();
            texto.Text = titulo;
            texto.Foreground = colorTexto;
            texto.FontSize = 14;
            texto.FontWeight = FontWeights.SemiBold;
            texto.HorizontalAlignment = HorizontalAlignment.Center;
            texto.VerticalAlignment = VerticalAlignment.Center;
            texto.Margin = new Thickness(12, 0, 12, 0);

            Grid raiz = new Grid();
            raiz.Children.Add(brillo);
            raiz.Children.Add(texto);
            Child = raiz;

            // Efectos del mouse
            MouseEnter += (s, e) =>
            {
                if (habilitado)
                {
                    brillo.Opacity = 1;
                    Anime.Hover(this, true);
                }
            };
            MouseLeave += (s, e) =>
            {
                brillo.Opacity = 0.5;
                Anime.Hover(this, false);
            };
            MouseLeftButtonDown += (s, e) =>
            {
                if (habilitado)
                {
                    CaptureMouse(); // Para saber si se suelta dentro del boton
                    texto.Opacity = 0.7;
                }
            };
            MouseLeftButtonUp += (s, e) =>
            {
                texto.Opacity = 1;
                if (!IsMouseCaptured)
                {
                    return;
                }
                ReleaseMouseCapture();
                Point posicion = e.GetPosition(this);
                bool dentro = posicion.X >= 0 && posicion.Y >= 0 && posicion.X <= ActualWidth && posicion.Y <= ActualHeight;
                if (habilitado && dentro && Click != null)
                {
                    Click(this, new RoutedEventArgs());
                }
            };
        }

        /// <summary>El texto del boton.</summary>
        public string Texto
        {
            get { return texto.Text; }
            set { texto.Text = value; }
        }

        /// <summary>Si es false, el boton se ve apagado y no responde.</summary>
        public bool Habilitado
        {
            get { return habilitado; }
            set
            {
                habilitado = value;
                texto.Opacity = value ? 1 : 0.45;
                brillo.Opacity = value ? 0.5 : 0.1;
                Cursor = value ? Cursors.Hand : Cursors.Arrow;
            }
        }
    }
}
