using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ClienteLavadora.util;
using ClienteLavadora.vista.controles;

namespace ClienteLavadora.vista
{
    /// <summary>
    /// DIALOGO de la casa: la ventana pequena que sirve para avisar algo o para pedir
    /// una confirmacion destructiva.
    ///
    /// POR QUE NO SE USA MessageBox:
    /// MessageBox dibuja una ventana gris de Windows que rompe por completo la
    /// metafora de la lavadora y ademas no se puede personalizar (el boton de eliminar
    /// quedaria igual que el de cancelar). Este dialogo hereda de VentanaMarco, asi que
    /// es una lavadora pequena con el mismo cristal y los mismos colores.
    ///
    /// Se usa con tres metodos estaticos, que se leen solos:
    ///     DialogoVoltia.Avisar(this, "Error", "El codigo 101 ya existe.");
    ///     DialogoVoltia.Informar(this, "Listo", "Se guardo la lavadora.");
    ///     bool seguro = DialogoVoltia.Confirmar(this, "Eliminar", "Se borrara...", "Eliminar", true);
    /// </summary>
    public class DialogoVoltia : VentanaMarco
    {
        private bool aceptado;

        private DialogoVoltia(string titulo, string mensaje, string textoAceptar, string textoCancelar,
                              bool peligro, string colorAcento)
            : base(titulo, 540, 330, false, 0)
        {
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            // ---------- Mensaje ----------
            TextBlock texto = Ui.Texto(mensaje, 12.5, Tema.Texto);
            texto.LineHeight = 19;

            ScrollViewer desplazable = Ui.Desplazable(texto);
            desplazable.MaxHeight = 130;
            desplazable.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

            // ---------- Botones ----------
            StackPanel botones = new StackPanel();
            botones.Orientation = Orientation.Horizontal;
            botones.HorizontalAlignment = HorizontalAlignment.Right;
            botones.Margin = new Thickness(0, 14, 0, 0);

            BotonVoltia botonAceptar = peligro
                ? BotonVoltia.Peligro(textoAceptar)
                : BotonVoltia.Principal(textoAceptar);
            botonAceptar.MinWidth = 130;
            botonAceptar.Margin = new Thickness(0, 0, 8, 0);
            botonAceptar.Click += AlAceptar;
            botones.Children.Add(botonAceptar);

            if (textoCancelar != null)
            {
                BotonVoltia botonCancelar = BotonVoltia.Secundario(textoCancelar);
                botonCancelar.MinWidth = 130;
                botonCancelar.Click += (s, e) => Close();
                botones.Children.Add(botonCancelar);
            }

            // ---------- Lampara que indica el tono del mensaje ----------
            Border lampara = new Border();
            lampara.Width = 10;
            lampara.Height = 10;
            lampara.CornerRadius = new CornerRadius(5);
            lampara.Background = Tema.Pincel(colorAcento);
            lampara.VerticalAlignment = VerticalAlignment.Top;
            lampara.Margin = new Thickness(0, 5, 10, 0);

            Grid cuerpo = new Grid();
            cuerpo.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            cuerpo.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            cuerpo.Children.Add(lampara);
            Grid.SetColumn(desplazable, 1);
            cuerpo.Children.Add(desplazable);

            Grid raiz = new Grid();
            raiz.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            raiz.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            raiz.Children.Add(cuerpo);
            Grid.SetRow(botones, 1);
            raiz.Children.Add(botones);
            raiz.Margin = new Thickness(0, 6, 0, 0);

            Contenido = Ui.Cristal(raiz, new Thickness(16));
        }

        // ================= Metodos estaticos de uso =================

        /// <summary>Muestra un error (lampara roja) con un solo boton "Entendido".</summary>
        public static void Avisar(Window dueno, string titulo, string mensaje)
        {
            DialogoVoltia dialogo = new DialogoVoltia(titulo, mensaje, "Entendido", null, false, Tema.HexError);
            Abrir(dialogo, dueno);
        }

        /// <summary>Muestra una buena noticia (lampara verde) con un solo boton.</summary>
        public static void Informar(Window dueno, string titulo, string mensaje)
        {
            DialogoVoltia dialogo = new DialogoVoltia(titulo, mensaje, "Entendido", null, false, Tema.HexExito);
            Abrir(dialogo, dueno);
        }

        /// <summary>Pide confirmacion. Devuelve true solo si el usuario presiona el boton de aceptar.</summary>
        public static bool Confirmar(Window dueno, string titulo, string mensaje, string textoAceptar, bool peligro)
        {
            string acento = peligro ? Tema.HexError : Tema.HexCian;
            DialogoVoltia dialogo = new DialogoVoltia(titulo, mensaje, textoAceptar, "Cancelar", peligro, acento);
            Abrir(dialogo, dueno);
            return dialogo.aceptado;
        }

        private static void Abrir(DialogoVoltia dialogo, Window dueno)
        {
            // Una peticion asincrona puede terminar despues de cerrar su ventana.
            // En ese caso WPF lanza InvalidOperationException al asignar un Owner cerrado.
            // El dialogo sigue siendo util centrado en pantalla y evita el error global.
            if (dueno != null && dueno.IsLoaded && dueno.IsVisible)
            {
                dialogo.Owner = dueno;
            }
            dialogo.ShowDialog(); // Modal: bloquea la ventana de atras hasta que se conteste
        }

        private void AlAceptar(object remitente, RoutedEventArgs argumentos)
        {
            aceptado = true;
            Close();
        }
    }
}
