using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using ClienteLavadora.util;

namespace ClienteLavadora.vista.controles
{
    /// <summary>
    /// MARCO de la lavadora: el chasis oscuro con esquinas redondeadas y la barra
    /// superior tipo panel de control que tienen TODAS las ventanas.
    ///
    /// Lo que dibuja:
    ///  - chasis con sombra y brillo arriba,
    ///  - panel superior con la marca VOLTIA, la PANTALLA DIGITAL (Consolas cian),
    ///    tres luces LED y los botones de minimizar y cerrar,
    ///  - opcionalmente el ojo de buey (la puerta),
    ///  - el area grande y vacia donde cada ventana pone su contenido.
    ///
    /// Tambien se encarga de tres cosas de comportamiento:
    ///  1. mover la ventana cuando se arrastra el panel (Window.DragMove),
    ///  2. mostrar "PROCESANDO" en la pantalla mientras hay peticiones (OBSERVER),
    ///  3. soltar confeti cuando una operacion sale bien (se llama a Celebrar()).
    /// </summary>
    public class MarcoLavadora : Grid
    {
        private readonly TextBlock pantalla = new TextBlock();
        private readonly Ellipse[] luces = new Ellipse[3];
        private readonly Canvas confeti;
        private readonly Grid cuerpo;

        private string tituloBase;
        private bool cerrando;
        private bool yaAnimado;
        private bool suscrito;

        public MarcoLavadora(string titulo, bool mostrarOjo, double ladoOjo)
        {
            Margin = new Thickness(16, 12, 16, 16);
            tituloBase = titulo;

            // ------------------------------------------------------------------
            //  Chasis (la "carcasa" de la lavadora)
            // ------------------------------------------------------------------
            Border chasis = new Border();
            chasis.CornerRadius = new CornerRadius(26);
            chasis.Background = Tema.ChasisMetal();
            chasis.BorderBrush = Tema.Pincel("#2E4050");
            chasis.BorderThickness = new Thickness(1.5);
            DropShadowEffect sombra = new DropShadowEffect();
            sombra.BlurRadius = 26;
            sombra.ShadowDepth = 7;
            sombra.Opacity = 0.55;
            sombra.Color = Colors.Black;
            chasis.Effect = sombra;
            Children.Add(chasis);

            // Tres filas: panel de control, ojo de buey (opcional) y contenido.
            Grid interior = new Grid();
            interior.RowDefinitions.Add(new RowDefinition { Height = new GridLength(78) });
            interior.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            interior.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            chasis.Child = interior;

            // Panel de control
            Border consola = CrearConsola();
            Grid.SetRow(consola, 0);
            interior.Children.Add(consola);

            // Brillo tenue en la parte de arriba del cuerpo (decoracion)
            Border brillo = new Border();
            brillo.CornerRadius = new CornerRadius(22);
            brillo.Height = 150;
            brillo.VerticalAlignment = VerticalAlignment.Top;
            brillo.IsHitTestVisible = false;
            brillo.Background = Tema.Degradado("#12FFFFFF", "#00FFFFFF");
            Grid.SetRow(brillo, 2);
            interior.Children.Add(brillo);

            // Ojo de buey (la puerta)
            if (mostrarOjo)
            {
                OjoDeBuey ojo = new OjoDeBuey(ladoOjo);
                ojo.Margin = new Thickness(0, 10, 0, 4);
                ojo.HorizontalAlignment = HorizontalAlignment.Center;
                Grid.SetRow(ojo, 1);
                interior.Children.Add(ojo);
            }

            // Aqui cada ventana pone lo suyo
            cuerpo = new Grid();
            cuerpo.Margin = new Thickness(26, 10, 26, 22);
            cuerpo.ClipToBounds = true;
            Grid.SetRow(cuerpo, 2);
            interior.Children.Add(cuerpo);

            // Capa superior para el confeti (no estorba al mouse)
            confeti = new Canvas();
            confeti.IsHitTestVisible = false;
            Grid.SetRow(confeti, 0);
            Grid.SetRowSpan(confeti, 3);
            interior.Children.Add(confeti);

            Loaded += AlCargar;
            Loaded += AlCargarSuscripcion;
            Unloaded += AlDescargar;
        }

        // ====================================================================
        //  Propiedades publicas
        // ====================================================================

        /// <summary>Texto de la pantalla digital. Mientras hay peticiones muestra "PROCESANDO".</summary>
        public string Titulo
        {
            get { return tituloBase; }
            set
            {
                tituloBase = value;
                if (!NotificadorDatos.Ocupado)
                {
                    pantalla.Text = value;
                }
            }
        }

        /// <summary>Todo lo que va dentro del cuerpo de la lavadora.</summary>
        public UIElement Contenido
        {
            set
            {
                cuerpo.Children.Clear();
                if (value != null)
                {
                    cuerpo.Children.Add(value);
                }
            }
        }

        // ====================================================================
        //  Panel de control
        // ====================================================================

        private Border CrearConsola()
        {
            Border consola = new Border();
            consola.CornerRadius = new CornerRadius(26, 26, 0, 0);
            consola.Background = Tema.Consola();
            consola.BorderBrush = Tema.Pincel("#2E4050");
            consola.BorderThickness = new Thickness(0, 0, 0, 1);
            consola.Cursor = Cursors.SizeAll;

            // Arrastrar el panel mueve la ventana entera.
            consola.MouseLeftButtonDown += AlArrastrarVentana;

            Grid contenido = new Grid();
            contenido.Margin = new Thickness(20, 0, 14, 0);
            contenido.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            contenido.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            contenido.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // ----- Marca -----
            StackPanel marca = new StackPanel();
            marca.VerticalAlignment = VerticalAlignment.Center;
            marca.Margin = new Thickness(0, 0, 18, 0);

            StackPanel filaMarca = new StackPanel();
            filaMarca.Orientation = Orientation.Horizontal;
            Ellipse punto = new Ellipse();
            punto.Width = 7;
            punto.Height = 7;
            punto.Margin = new Thickness(0, 0, 7, 0);
            punto.VerticalAlignment = VerticalAlignment.Center;
            punto.Fill = Tema.Cian;
            filaMarca.Children.Add(punto);

            TextBlock nombre = new TextBlock();
            nombre.Text = "VOLTIA";
            nombre.FontFamily = Tema.Numeros;
            nombre.FontSize = 16;
            nombre.FontWeight = FontWeights.Bold;
            nombre.Foreground = Tema.Texto;
            filaMarca.Children.Add(nombre);
            marca.Children.Add(filaMarca);

            TextBlock subtitulo = new TextBlock();
            subtitulo.Text = "LAVADORA INDUSTRIAL";
            subtitulo.FontFamily = Tema.Fuente;
            subtitulo.FontSize = 8.5;
            subtitulo.Foreground = Tema.TextoSecundario;
            subtitulo.Margin = new Thickness(14, 2, 0, 0);
            marca.Children.Add(subtitulo);

            Grid.SetColumn(marca, 0);
            contenido.Children.Add(marca);

            // ----- Pantalla digital -----
            pantalla.Text = tituloBase;
            pantalla.FontFamily = Tema.Numeros;
            pantalla.FontSize = 17;
            pantalla.FontWeight = FontWeights.Bold;
            pantalla.Foreground = Tema.Cian;
            pantalla.VerticalAlignment = VerticalAlignment.Center;
            pantalla.TextTrimming = TextTrimming.CharacterEllipsis;
            pantalla.Margin = new Thickness(2, 0, 0, 0);

            Border marcoPantalla = Ui.Pantalla(pantalla);
            marcoPantalla.Height = 42;
            marcoPantalla.Margin = new Thickness(0, 0, 14, 0);
            marcoPantalla.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(marcoPantalla, 1);
            contenido.Children.Add(marcoPantalla);

            // ----- Luces LED y botones de la ventana -----
            StackPanel derecha = new StackPanel();
            derecha.Orientation = Orientation.Horizontal;
            derecha.VerticalAlignment = VerticalAlignment.Center;

            string[] colores = { Tema.HexCian, Tema.HexAmbar, Tema.HexExito };
            for (int i = 0; i < luces.Length; i++)
            {
                Ellipse luz = new Ellipse();
                luz.Width = 8;
                luz.Height = 8;
                luz.Fill = Tema.Pincel(colores[i]);
                luz.Margin = new Thickness(3, 0, i == luces.Length - 1 ? 10 : 3, 0);
                luz.VerticalAlignment = VerticalAlignment.Center;
                luces[i] = luz;
                derecha.Children.Add(luz);
            }

            derecha.Children.Add(CrearBotonVentana("M3,9 L9,9", "Minimizar", Minimizar));
            derecha.Children.Add(CrearBotonVentana("M3,3 L9,9 M9,3 L3,9", "Cerrar", Cerrar));

            Grid.SetColumn(derecha, 2);
            contenido.Children.Add(derecha);

            consola.Child = contenido;
            return consola;
        }

        /// <summary>Boton pequeno de la barra: minimizar o cerrar.</summary>
        private static Border CrearBotonVentana(string trazo, string ayuda, Action alHacerClic)
        {
            Border boton = new Border();
            boton.Width = 26;
            boton.Height = 26;
            boton.Margin = new Thickness(5, 0, 0, 0);
            boton.CornerRadius = new CornerRadius(13);
            boton.Background = Tema.Pincel("#1A2731");
            boton.BorderBrush = Tema.Pincel("#33FFFFFF");
            boton.BorderThickness = new Thickness(1);
            boton.Cursor = Cursors.Hand;
            boton.ToolTip = ayuda;

            Path simbolo = new Path();
            simbolo.Data = Geometry.Parse(trazo);
            simbolo.Stroke = Tema.TextoSecundario;
            simbolo.StrokeThickness = 1.6;
            simbolo.StrokeStartLineCap = PenLineCap.Round;
            simbolo.StrokeEndLineCap = PenLineCap.Round;
            simbolo.HorizontalAlignment = HorizontalAlignment.Center;
            simbolo.VerticalAlignment = VerticalAlignment.Center;
            boton.Child = simbolo;

            boton.MouseEnter += (s, e) =>
            {
                boton.BorderBrush = Tema.Pincel(Tema.HexCian);
                simbolo.Stroke = Tema.Cian;
            };
            boton.MouseLeave += (s, e) =>
            {
                boton.BorderBrush = Tema.Pincel("#33FFFFFF");
                simbolo.Stroke = Tema.TextoSecundario;
            };
            boton.MouseLeftButtonDown += (s, e) => e.Handled = true; // Que no arrastre la ventana
            boton.MouseLeftButtonUp += (s, e) =>
            {
                e.Handled = true;
                alHacerClic();
            };
            return boton;
        }

        private void AlArrastrarVentana(object remitente, MouseButtonEventArgs argumentos)
        {
            Window ventana = Window.GetWindow(this);
            if (ventana != null && argumentos.ButtonState == MouseButtonState.Pressed)
            {
                try
                {
                    ventana.DragMove(); // Mueve la ventana con el mouse
                }
                catch (InvalidOperationException)
                {
                    // Si el mouse ya se solto, DragMove protesta; se ignora.
                }
            }
        }

        // ====================================================================
        //  Cerrar y celebrar
        // ====================================================================

        private void Minimizar()
        {
            Window ventana = Window.GetWindow(this);
            if (ventana != null)
            {
                ventana.WindowState = WindowState.Minimized;
            }
        }

        /// <summary>Cierra la ventana con una animacion (se encoge y se desvanece).</summary>
        public void Cerrar()
        {
            if (cerrando)
            {
                return; // Evita cerrar dos veces si hacen doble clic
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

        /// <summary>
        /// Confeti: 28 pedacitos de colores salen disparados desde el centro.
        /// Se usa cuando una operacion sale bien (agregar, actualizar, eliminar).
        /// </summary>
        public void Celebrar()
        {
            if (confeti.ActualWidth < 10)
            {
                return;
            }

            Random azar = new Random();
            Color[] colores =
            {
                Tema.ColorDe(Tema.HexCian), Tema.ColorDe(Tema.HexAmbar),
                Tema.ColorDe(Tema.HexExito), Colors.White, Tema.ColorDe("#7FA8C4")
            };
            double centroX = confeti.ActualWidth / 2;
            double centroY = confeti.ActualHeight / 2;

            for (int i = 0; i < 28; i++)
            {
                Rectangle trozo = new Rectangle();
                trozo.Width = 7;
                trozo.Height = 11;
                trozo.RadiusX = 2;
                trozo.RadiusY = 2;
                trozo.Fill = new SolidColorBrush(colores[azar.Next(colores.Length)]);
                Canvas.SetLeft(trozo, centroX);
                Canvas.SetTop(trozo, centroY);
                confeti.Children.Add(trozo);

                double angulo = azar.NextDouble() * Math.PI * 2;
                double distancia = 120 + azar.NextDouble() * 210;
                double dx = Math.Cos(angulo) * distancia;
                double dy = Math.Sin(angulo) * distancia + 80; // +80: cae un poco, como por gravedad

                Rectangle copia = trozo; // Para quitar este pedazo cuando termine
                Anime.Explotar(trozo, dx, dy, 900 + azar.Next(0, 500), () => confeti.Children.Remove(copia));
            }
        }

        // ====================================================================
        //  Observer: la pantalla muestra PROCESANDO
        // ====================================================================

        private void AlCargarSuscripcion(object remitente, RoutedEventArgs argumentos)
        {
            if (suscrito)
            {
                return;
            }
            NotificadorDatos.Trabajando += AlTrabajar;
            suscrito = true;
            AlTrabajar(NotificadorDatos.Ocupado); // Si ya habia peticiones, se muestra de una vez
        }

        private void AlDescargar(object remitente, RoutedEventArgs argumentos)
        {
            if (!suscrito)
            {
                return;
            }
            NotificadorDatos.Trabajando -= AlTrabajar;
            suscrito = false;
        }

        /// <summary>
        /// El aviso puede llegar de un hilo de trabajo (la respuesta del servidor),
        /// entonces se pasa al hilo de la interfaz con Dispatcher.Invoke.
        /// </summary>
        private void AlTrabajar(bool trabajando)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(new Action<bool>(AlTrabajar), trabajando);
                return;
            }
            if (cerrando)
            {
                return;
            }

            pantalla.Text = trabajando ? "PROCESANDO" : tituloBase;
            pantalla.Foreground = trabajando ? Tema.Ambar : Tema.Cian;
        }

        // ====================================================================
        //  Animacion de entrada
        // ====================================================================

        private void AlCargar(object remitente, RoutedEventArgs argumentos)
        {
            if (yaAnimado)
            {
                return;
            }
            yaAnimado = true;

            Anime.Aparecer(this);
            Anime.Escribir(pantalla);
            for (int i = 0; i < luces.Length; i++)
            {
                Anime.Parpadeo(luces[i], i * 260); // Cada LED arranca un poco despues
            }
        }
    }
}
