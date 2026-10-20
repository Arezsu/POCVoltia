using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using ClienteLavadora.util;

namespace ClienteLavadora.gui
{
    /// <summary>
    /// Marco con forma de LAVADORA que usan todas las ventanas: cuerpo metalico, patas,
    /// consola con perilla, pantalla digital, luces LED y botones de minimizar y cerrar.
    /// Lo que cada ventana quiere mostrar se pone en el cuerpo con la propiedad Contenido.
    ///
    /// Todo se dibuja en codigo C# (no hay XAML). El marco tambien se encarga de:
    /// mover la ventana al arrastrar la consola, cerrar con animacion, adornos de fondo
    /// (burbujas), el confeti y la perilla que gira mientras se espera al servidor.
    /// </summary>
    public class MarcoLavadora : Grid
    {
        private readonly TextBlock pantalla;
        private readonly Grid perilla;
        private readonly Ellipse[] luces = new Ellipse[3];
        private readonly Canvas particulas; // Burbujas de fondo
        private readonly Canvas confeti;    // Capa de arriba para el confeti
        private readonly Grid cuerpo;       // Aqui va el contenido de la ventana
        private bool cerrando;
        private bool yaAnimado;

        public MarcoLavadora(string titulo, bool mostrarOjo, double ladoOjo)
        {
            Margin = new Thickness(18, 12, 18, 24);

            // ---------------- Patas ----------------
            Children.Add(CrearPata(HorizontalAlignment.Left, new Thickness(60, 0, 0, -20)));
            Children.Add(CrearPata(HorizontalAlignment.Right, new Thickness(0, 0, 60, -20)));

            // ---------------- Cuerpo metalico ----------------
            Border carcasa = new Border();
            carcasa.CornerRadius = new CornerRadius(30);
            carcasa.Background = Ui.Metal();
            carcasa.BorderBrush = Ui.Pincel("#78909C");
            carcasa.BorderThickness = new Thickness(2);
            DropShadowEffect sombra = new DropShadowEffect();
            sombra.BlurRadius = 16;
            sombra.ShadowDepth = 5;
            sombra.Opacity = 0.45;
            sombra.Color = Colors.Black;
            carcasa.Effect = sombra;
            Children.Add(carcasa);

            // El interior tiene 3 filas: consola, puerta redonda y contenido
            Grid interior = new Grid();
            interior.RowDefinitions.Add(new RowDefinition { Height = new GridLength(84) });
            interior.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            interior.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            carcasa.Child = interior;

            // Adorno de fondo (burbujas)
            particulas = new Canvas();
            particulas.IsHitTestVisible = false;
            particulas.ClipToBounds = true;
            Grid.SetRowSpan(particulas, 3);
            interior.Children.Add(particulas);

            // Brillo suave en la parte de arriba del cuerpo
            Border brillo = new Border();
            brillo.CornerRadius = new CornerRadius(28);
            brillo.Height = 190;
            brillo.VerticalAlignment = VerticalAlignment.Top;
            brillo.IsHitTestVisible = false;
            brillo.Background = Ui.Degradado("#44FFFFFF", "#00FFFFFF");
            Grid.SetRowSpan(brillo, 3);
            interior.Children.Add(brillo);

            // ---------------- Consola ----------------
            pantalla = new TextBlock();
            perilla = CrearPerilla();
            Border consola = CrearConsola(titulo);
            Grid.SetRow(consola, 0);
            interior.Children.Add(consola);

            // ---------------- Puerta redonda (ojo de buey) ----------------
            if (mostrarOjo)
            {
                OjoDeBuey ojo = new OjoDeBuey(ladoOjo);
                ojo.Margin = new Thickness(0, 8, 0, 0);
                ojo.HorizontalAlignment = HorizontalAlignment.Center;
                Grid.SetRow(ojo, 1);
                interior.Children.Add(ojo);
            }

            // ---------------- Cuerpo: aqui va el contenido de cada ventana ----------------
            cuerpo = new Grid();
            cuerpo.Margin = new Thickness(28, 8, 28, 22);
            Grid.SetRow(cuerpo, 2);
            interior.Children.Add(cuerpo);

            // Capa de arriba para el confeti
            confeti = new Canvas();
            confeti.IsHitTestVisible = false;
            Grid.SetRowSpan(confeti, 3);
            interior.Children.Add(confeti);

            // Eventos de la vida del marco
            Loaded += AlCargar;
            Loaded += (s, e) => Notificador.Trabajando += AlTrabajar; // Perilla girando = cargando
            Unloaded += (s, e) => Notificador.Trabajando -= AlTrabajar;
        }

        /// <summary>Texto que se muestra en la pantalla digital.</summary>
        public string Titulo
        {
            get { return pantalla.Text; }
            set { pantalla.Text = value; }
        }

        /// <summary>El contenido de la ventana (formulario, tabla, etc.).</summary>
        public UIElement Contenido
        {
            set
            {
                cuerpo.Children.Clear();
                cuerpo.Children.Add(value);
            }
        }

        // ====================================================================
        //  Dibujo
        // ====================================================================

        private static Border CrearPata(HorizontalAlignment lado, Thickness margen)
        {
            Border pata = new Border();
            pata.Width = 90;
            pata.Height = 24;
            pata.CornerRadius = new CornerRadius(0, 0, 10, 10);
            pata.Background = Ui.Pincel("#37474F");
            pata.HorizontalAlignment = lado;
            pata.VerticalAlignment = VerticalAlignment.Bottom;
            pata.Margin = margen;
            return pata;
        }

        private Border CrearConsola(string titulo)
        {
            Border consola = new Border();
            consola.CornerRadius = new CornerRadius(28, 28, 0, 0);
            consola.Background = Ui.Consola();
            consola.BorderBrush = Ui.Pincel("#FFC107");
            consola.BorderThickness = new Thickness(0, 0, 0, 3);
            consola.Cursor = Cursors.SizeAll;

            // Arrastrar la consola mueve la ventana
            consola.MouseLeftButtonDown += (s, e) =>
            {
                Window ventana = Window.GetWindow(this);
                if (ventana != null && e.ButtonState == MouseButtonState.Pressed)
                {
                    ventana.DragMove(); // Mueve la ventana con el mouse
                }
            };

            Grid contenido = new Grid();
            contenido.Margin = new Thickness(18, 0, 18, 0);
            contenido.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            contenido.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            contenido.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Perilla
            Grid.SetColumn(perilla, 0);
            contenido.Children.Add(perilla);

            // Pantalla digital con el titulo
            pantalla.Text = titulo;
            pantalla.FontFamily = new FontFamily("Consolas");
            pantalla.FontSize = 20;
            pantalla.FontWeight = FontWeights.Bold;
            pantalla.Foreground = Ui.Pincel("#4FC3F7");
            pantalla.VerticalAlignment = VerticalAlignment.Center;
            pantalla.Margin = new Thickness(12, 0, 12, 0);
            pantalla.TextTrimming = TextTrimming.CharacterEllipsis;

            Border marcoPantalla = new Border();
            marcoPantalla.Margin = new Thickness(16, 14, 16, 14);
            marcoPantalla.CornerRadius = new CornerRadius(10);
            marcoPantalla.Background = Ui.Pincel("#08121C");
            marcoPantalla.BorderBrush = Ui.Pincel("#546E7A");
            marcoPantalla.BorderThickness = new Thickness(2);
            marcoPantalla.Child = pantalla;
            Grid.SetColumn(marcoPantalla, 1);
            contenido.Children.Add(marcoPantalla);

            // Luces LED y botones de minimizar y cerrar
            StackPanel derecha = new StackPanel();
            derecha.Orientation = Orientation.Horizontal;
            derecha.VerticalAlignment = VerticalAlignment.Center;
            string[] colores = { "#FFC107", "#4FC3F7", "#66BB6A" };
            for (int i = 0; i < luces.Length; i++)
            {
                Ellipse luz = new Ellipse();
                luz.Width = 10;
                luz.Height = 10;
                luz.Fill = Ui.Pincel(colores[i]);
                luz.Margin = new Thickness(3, 0, i == 2 ? 8 : 3, 0);
                luces[i] = luz;
                derecha.Children.Add(luz);
            }
            derecha.Children.Add(CrearBotonRedondo("M0,0 L10,0", "Minimizar", Minimizar));
            derecha.Children.Add(CrearBotonRedondo("M0,0 L10,10 M10,0 L0,10", "Cerrar", Cerrar));
            Grid.SetColumn(derecha, 2);
            contenido.Children.Add(derecha);

            consola.Child = contenido;
            return consola;
        }

        /// <summary>La perilla: al hacer click gira con resorte.</summary>
        private static Grid CrearPerilla()
        {
            Grid perilla = new Grid();
            perilla.Width = 60;
            perilla.Height = 60;
            perilla.Background = Brushes.Transparent;
            perilla.Cursor = Cursors.Hand;
            perilla.ToolTip = "Gire la perilla";

            Ellipse aro = new Ellipse();
            aro.Fill = Ui.Pincel("#CFD8DC");
            aro.Stroke = Ui.Pincel("#90A4AE");
            aro.StrokeThickness = 2;
            perilla.Children.Add(aro);

            RadialGradientBrush brillo = new RadialGradientBrush();
            brillo.GradientOrigin = new Point(0.3, 0.3);
            brillo.GradientStops.Add(new GradientStop(Colors.White, 0));
            brillo.GradientStops.Add(new GradientStop(Ui.ColorDe("#78909C"), 1));
            Ellipse centro = new Ellipse();
            centro.Margin = new Thickness(7);
            centro.Fill = brillo;
            perilla.Children.Add(centro);

            Rectangle marca = new Rectangle();
            marca.Width = 5;
            marca.Height = 18;
            marca.RadiusX = 2.5;
            marca.RadiusY = 2.5;
            marca.Fill = Ui.Pincel("#1E88E5");
            marca.VerticalAlignment = VerticalAlignment.Top;
            marca.Margin = new Thickness(0, 5, 0, 0);
            perilla.Children.Add(marca);

            perilla.MouseLeftButtonDown += (s, e) =>
            {
                e.Handled = true; // Para que no se mueva la ventana
                Anime.GirarMas(perilla, 140, 800);
            };
            return perilla;
        }

        /// <summary>Boton redondo pequeno de la consola (minimizar y cerrar).</summary>
        private static Grid CrearBotonRedondo(string trazo, string ayuda, Action alClic)
        {
            Grid boton = new Grid();
            boton.Width = 30;
            boton.Height = 30;
            boton.Margin = new Thickness(6, 0, 0, 0);
            boton.Background = Brushes.Transparent;
            boton.Cursor = Cursors.Hand;
            boton.ToolTip = ayuda;

            Ellipse fondo = new Ellipse();
            fondo.Fill = Ui.Pincel("#607D8B");
            fondo.Stroke = Ui.Pincel("#B0BEC5");
            fondo.StrokeThickness = 1.5;
            boton.Children.Add(fondo);

            Path simbolo = new Path();
            simbolo.Data = Geometry.Parse(trazo);
            simbolo.Stroke = Brushes.White;
            simbolo.StrokeThickness = 2.5;
            simbolo.HorizontalAlignment = HorizontalAlignment.Center;
            simbolo.VerticalAlignment = VerticalAlignment.Center;
            boton.Children.Add(simbolo);

            boton.MouseEnter += (s, e) => fondo.Fill = Ui.Pincel("#FFC107");
            boton.MouseLeave += (s, e) => fondo.Fill = Ui.Pincel("#607D8B");
            boton.MouseLeftButtonDown += (s, e) => e.Handled = true; // Para que no se mueva la ventana
            boton.MouseLeftButtonUp += (s, e) => alClic();
            return boton;
        }

        // ====================================================================
        //  Acciones
        // ====================================================================

        private void Minimizar()
        {
            Window ventana = Window.GetWindow(this);
            if (ventana != null)
            {
                ventana.WindowState = WindowState.Minimized;
            }
        }

        /// <summary>Cierra la ventana con la animacion de salida (se encoge y desaparece).</summary>
        public void Cerrar()
        {
            if (cerrando)
            {
                return; // Evita cerrar dos veces si hacen doble click
            }
            cerrando = true;

            Window ventana = Window.GetWindow(this);
            Anime.Salir(this, () =>
            {
                if (ventana != null)
                {
                    ventana.Close();
                }
            });
        }

        /// <summary>Mientras haya una peticion al servidor en camino, la perilla gira.</summary>
        private void AlTrabajar(bool trabajando)
        {
            if (cerrando)
            {
                return;
            }
            if (trabajando)
            {
                Anime.Girar(perilla, 0, 360, 900, true);
            }
            else
            {
                Anime.Parar(perilla);
                Anime.GirarMas(perilla, 60, 700); // Un ultimo giro con resorte
            }
        }

        /// <summary>Animaciones de entrada: la ventana "salta", el titulo se escribe y los LED parpadean.</summary>
        private void AlCargar(object sender, RoutedEventArgs e)
        {
            if (yaAnimado)
            {
                return;
            }
            yaAnimado = true;

            Anime.Aparecer(this);
            Anime.Escribir(pantalla);
            Anime.Girar(perilla, -120, 0, 1000, false, Anime.Elastico(), 250);
            for (int i = 0; i < luces.Length; i++)
            {
                Anime.Parpadeo(luces[i], i * 250); // Cada LED arranca un poco despues
            }
            CrearBurbujasDeFondo();
        }

        /// <summary>
        /// Adorno de fondo: burbujas que suben. Quedan detras del formulario,
        /// asi que solo se ven en los espacios libres.
        /// </summary>
        private void CrearBurbujasDeFondo()
        {
            if (particulas.ActualWidth < 10)
            {
                return;
            }

            Random azar = new Random();
            double alto = particulas.ActualHeight;
            for (int i = 0; i < 12; i++)
            {
                double tamano = 6 + azar.Next(0, 10);
                Ellipse burbuja = new Ellipse();
                burbuja.Width = tamano;
                burbuja.Height = tamano;
                burbuja.Stroke = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255));
                burbuja.StrokeThickness = 1.5;
                burbuja.Fill = new SolidColorBrush(Color.FromArgb(50, 79, 195, 247));
                Canvas.SetLeft(burbuja, azar.NextDouble() * Math.Max(10, particulas.ActualWidth - tamano));
                Canvas.SetTop(burbuja, alto);
                particulas.Children.Add(burbuja);

                int duracion = 5000 + azar.Next(0, 4000);
                Anime.Subir(burbuja, 0, -(alto + 40), duracion, i * 450); // Arrancan una tras otra
            }
        }

        /// <summary>
        /// Confeti: 28 pedacitos de colores salen disparados desde el centro.
        /// Se usa cuando algo sale bien (adicionar o actualizar).
        /// </summary>
        public void Celebrar()
        {
            if (confeti.ActualWidth < 10)
            {
                return;
            }

            Random azar = new Random();
            Color[] colores = { Colors.Gold, Colors.DeepSkyBlue, Colors.White, Colors.OrangeRed, Colors.LimeGreen };
            double centroX = confeti.ActualWidth / 2;
            double centroY = confeti.ActualHeight / 2;

            for (int i = 0; i < 28; i++)
            {
                Rectangle trozo = new Rectangle();
                trozo.Width = 8;
                trozo.Height = 12;
                trozo.RadiusX = 2;
                trozo.RadiusY = 2;
                trozo.Fill = new SolidColorBrush(colores[azar.Next(colores.Length)]);
                Canvas.SetLeft(trozo, centroX);
                Canvas.SetTop(trozo, centroY);
                confeti.Children.Add(trozo);

                double angulo = azar.NextDouble() * Math.PI * 2; // Cada uno sale hacia un lado distinto
                double distancia = 120 + azar.NextDouble() * 220;
                double dx = Math.Cos(angulo) * distancia;
                double dy = Math.Sin(angulo) * distancia + 80; // +80: cae un poco, como por la gravedad

                Rectangle copia = trozo; // Para quitar este pedazo cuando termine
                Anime.Explotar(trozo, dx, dy, 900 + azar.Next(0, 500), () => confeti.Children.Remove(copia));
            }
        }
    }
}
