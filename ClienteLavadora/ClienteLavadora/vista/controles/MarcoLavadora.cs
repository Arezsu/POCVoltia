using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using ClienteLavadora.util;

namespace ClienteLavadora.vista.controles
{
    /// <summary>
    /// Contenedor sencillo para las ventanas de cada caso de uso. El marco nativo
    /// de Windows aporta los controles de ventana; esta clase muestra el titulo,
    /// el estado de la peticion y el contenido de la vista.
    /// </summary>
    public class MarcoLavadora : Grid
    {
        private readonly TextBlock tituloVista = new TextBlock();
        private readonly TextBlock estado = new TextBlock();
        private readonly Border cuerpo = new Border();
        private string tituloBase;
        private bool suscrito;

        public MarcoLavadora(string titulo, bool mostrarOjo, double ladoOjo)
        {
            tituloBase = titulo;
            Background = Tema.Fondo;

            RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            Grid barra = new Grid
            {
                Background = Tema.FondoAlto,
                Margin = new Thickness(0),
                Height = 38
            };
            barra.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            barra.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            tituloVista.Text = tituloBase;
            tituloVista.FontFamily = Tema.Fuente;
            tituloVista.FontSize = 13;
            tituloVista.FontWeight = FontWeights.SemiBold;
            tituloVista.Foreground = Tema.Texto;
            tituloVista.VerticalAlignment = VerticalAlignment.Center;
            tituloVista.Margin = new Thickness(12, 0, 8, 0);
            barra.Children.Add(tituloVista);

            estado.Text = "Listo";
            estado.FontFamily = Tema.Fuente;
            estado.FontSize = 11;
            estado.Foreground = Tema.TextoSecundario;
            estado.VerticalAlignment = VerticalAlignment.Center;
            estado.Margin = new Thickness(8, 0, 12, 0);
            Grid.SetColumn(estado, 1);
            barra.Children.Add(estado);
            Children.Add(barra);

            cuerpo.Background = Tema.Fondo;
            cuerpo.Margin = new Thickness(0);
            cuerpo.Padding = new Thickness(12);
            cuerpo.BorderBrush = Tema.Linea;
            cuerpo.BorderThickness = new Thickness(0, 1, 0, 0);
            Grid.SetRow(cuerpo, 1);
            Children.Add(cuerpo);

            Loaded += AlCargarSuscripcion;
            Unloaded += AlDescargar;
        }

        public string Titulo
        {
            get { return tituloBase; }
            set
            {
                tituloBase = value;
                if (!NotificadorDatos.Ocupado)
                {
                    tituloVista.Text = value;
                }
            }
        }

        public UIElement Contenido
        {
            set { cuerpo.Child = value; }
        }

        public void Cerrar()
        {
            Window ventana = Window.GetWindow(this);
            if (ventana != null)
            {
                ventana.Close();
            }
        }

        public void Celebrar()
        {
            estado.Text = "Operacion completada";
            DispatcherTimer temporizador = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
            temporizador.Tick += (remitente, argumentos) =>
            {
                temporizador.Stop();
                if (!NotificadorDatos.Ocupado)
                {
                    estado.Text = "Listo";
                }
            };
            temporizador.Start();
        }

        private void AlCargarSuscripcion(object remitente, RoutedEventArgs argumentos)
        {
            if (suscrito) return;
            NotificadorDatos.Trabajando += AlTrabajar;
            suscrito = true;
            AlTrabajar(NotificadorDatos.Ocupado);
        }

        private void AlDescargar(object remitente, RoutedEventArgs argumentos)
        {
            if (!suscrito) return;
            NotificadorDatos.Trabajando -= AlTrabajar;
            suscrito = false;
        }

        private void AlTrabajar(bool trabajando)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(new Action<bool>(AlTrabajar), trabajando);
                return;
            }

            estado.Text = trabajando ? "Procesando" : "Listo";
            estado.Foreground = trabajando ? Tema.Ambar : Tema.TextoSecundario;
        }
    }
}
