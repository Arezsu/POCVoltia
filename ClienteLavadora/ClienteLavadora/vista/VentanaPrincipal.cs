using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ClienteLavadora.controlador;
using ClienteLavadora.servicios;
using ClienteLavadora.util;
using ClienteLavadora.vista.controles;

namespace ClienteLavadora.vista
{
    /// <summary>
    /// VENTANA PRINCIPAL: el tablero desde donde se abre cada caso de uso.
    ///
    /// Muestra el logo, el nombre VOLTIA, la puerta grande de la lavadora girando, el
    /// estado de la conexion con el servidor y un boton por cada caso de uso. Cada
    /// caso se abre en SU PROPIA ventana, como pide la catedra (una ventana por caso).
    ///
    /// AQUI ARRANCA EL OBSERVER DEL SERVIDOR:
    /// se crea el VigilanteServidor, que cada 3 segundos pregunta la version de los
    /// datos. Si otro cliente modifico algo, avisa por NotificadorDatos y la ventana
    /// Listar (si esta abierta) se recarga sola.
    /// </summary>
    public class VentanaPrincipal : VentanaMarco
    {
        private readonly ControladorLavadoras controlador = new ControladorLavadoras(new ServicioLavadoras());
        private readonly VigilanteServidor vigilante;

        private readonly Ellipse puntoConexion = new Ellipse();
        private readonly TextBlock textoConexion = new TextBlock();

        private readonly OjoDeBuey ojo = new OjoDeBuey(120);
        private readonly TextBlock estadoTambor = new TextBlock();

        private bool suscrito;

        public VentanaPrincipal() : base("VOLTIA  PANEL DE CONTROL", 900, 800, false, 0)
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ShowInTaskbar = true; // La ventana principal si debe aparecer en la barra de Windows
            vigilante = new VigilanteServidor(controlador);

            Grid raiz = new Grid { Background = Tema.Fondo };
            raiz.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });                 // encabezado
            raiz.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // puerta
            raiz.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });                 // botones
            raiz.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });                 // pie

            FrameworkElement encabezado = CrearEncabezado();
            FrameworkElement puerta = CrearPuerta();
            FrameworkElement botones = CrearBotones();
            FrameworkElement pie = CrearPie();

            Grid.SetRow(encabezado, 0);
            Grid.SetRow(puerta, 1);
            Grid.SetRow(botones, 2);
            Grid.SetRow(pie, 3);

            raiz.Children.Add(encabezado);
            raiz.Children.Add(puerta);
            raiz.Children.Add(botones);
            raiz.Children.Add(pie);
            Contenido = raiz;

            Loaded += AlCargar;
            Closed += AlCerrar;
        }

        // ====================================================================
        //  Armado de la pantalla
        // ====================================================================

        /// <summary>Logo, nombre de la casa y estado de la conexion.</summary>
        private FrameworkElement CrearEncabezado()
        {
            Menu menu = new Menu
            {
                Background = Tema.FondoAlto,
                Foreground = Tema.Texto,
                FontFamily = Tema.Fuente,
                FontSize = 13,
                ItemContainerStyle = EstiloElementoMenu()
            };
            menu.Items.Add(CrearMenu("Lavadora",
                CrearAccion("Listar lavadoras", AbrirListar),
                CrearAccion("Agregar lavadora", AbrirAgregar),
                CrearAccion("Consultar", AbrirConsultar),
                CrearAccion("Actualizar", AbrirActualizar),
                CrearAccion("Eliminar", AbrirEliminar)));
            menu.Items.Add(CrearMenu("Mantenimiento",
                CrearAccion("Listar mantenimientos", AbrirListarMantenimientos),
                CrearAccion("Agregar mantenimiento", AbrirAgregarMantenimiento),
                CrearAccion("Consultar mantenimiento", AbrirConsultarMantenimiento),
                CrearAccion("Actualizar mantenimiento", AbrirActualizarMantenimiento),
                CrearAccion("Eliminar mantenimiento", AbrirEliminarMantenimiento)));
            menu.Items.Add(CrearMenu("Ayuda",
                CrearAccion("Acerca de", AbrirAcercaDe)));

            StackPanel contenedor = new StackPanel();
            contenedor.Children.Add(menu);

            Grid cabecera = new Grid();
            cabecera.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            cabecera.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            cabecera.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // ----- Logo sobre una placa oscura -----
            Border placa = new Border();
            placa.Width = 78;
            placa.Height = 78;
            placa.CornerRadius = new CornerRadius(16);
            placa.Background = Tema.Pincel("#0E1620");
            placa.BorderBrush = Tema.CristalBorde;
            placa.BorderThickness = new Thickness(1);
            placa.Margin = new Thickness(0, 0, 16, 0);
            placa.VerticalAlignment = VerticalAlignment.Center;

            Image logo = new Image();
            logo.Source = Ui.Imagen("logo.jpg");
            logo.Stretch = Stretch.Uniform;
            logo.Margin = new Thickness(8);
            placa.Child = logo;
            cabecera.Children.Add(placa);

            // ----- Nombre y descripcion -----
            StackPanel textos = new StackPanel();
            textos.VerticalAlignment = VerticalAlignment.Center;

            TextBlock nombre = new TextBlock();
            nombre.Text = "VOLTIA";
            nombre.FontFamily = Tema.Numeros;
            nombre.FontSize = 30;
            nombre.FontWeight = FontWeights.Bold;
            nombre.Foreground = Tema.Cian;
            textos.Children.Add(nombre);

            TextBlock descripcion = Ui.Texto("Sistema de gestion de lavadoras industriales", 12, Tema.TextoSecundario);
            descripcion.Margin = new Thickness(1, 1, 0, 0);
            textos.Children.Add(descripcion);

            Grid.SetColumn(textos, 1);
            cabecera.Children.Add(textos);

            // ----- Indicador de conexion -----
            Border indicador = new Border();
            indicador.CornerRadius = new CornerRadius(12);
            indicador.Background = Tema.Pincel("#0E1620");
            indicador.BorderBrush = Tema.CristalBorde;
            indicador.BorderThickness = new Thickness(1);
            indicador.Padding = new Thickness(10, 5, 12, 5);
            indicador.VerticalAlignment = VerticalAlignment.Center;

            StackPanel filaEstado = new StackPanel();
            filaEstado.Orientation = Orientation.Horizontal;

            puntoConexion.Width = 9;
            puntoConexion.Height = 9;
            puntoConexion.Fill = Tema.TextoSecundario;
            puntoConexion.Margin = new Thickness(0, 0, 8, 0);
            puntoConexion.VerticalAlignment = VerticalAlignment.Center;
            filaEstado.Children.Add(puntoConexion);

            textoConexion.Text = "CONECTANDO...";
            textoConexion.FontFamily = Tema.Numeros;
            textoConexion.FontSize = 10.5;
            textoConexion.FontWeight = FontWeights.Bold;
            textoConexion.Foreground = Tema.TextoSecundario;
            textoConexion.VerticalAlignment = VerticalAlignment.Center;
            filaEstado.Children.Add(textoConexion);

            indicador.Child = filaEstado;
            Grid.SetColumn(indicador, 2);
            cabecera.Children.Add(indicador);

            Border marco = Ui.Cristal(cabecera, new Thickness(14));
            marco.Margin = new Thickness(0, 0, 0, 12);
            contenedor.Children.Add(marco);
            return contenedor;
        }

        /// <summary>La puerta grande de la lavadora, girando siempre.</summary>
        private FrameworkElement CrearPuerta()
        {
            ojo.HorizontalAlignment = HorizontalAlignment.Center;
            ojo.VerticalAlignment = VerticalAlignment.Center;

            estadoTambor.Text = "TAMBOR EN ESPERA";
            estadoTambor.FontFamily = Tema.Numeros;
            estadoTambor.FontSize = 11;
            estadoTambor.FontWeight = FontWeights.Bold;
            estadoTambor.Foreground = Tema.TextoSecundario;
            estadoTambor.HorizontalAlignment = HorizontalAlignment.Center;
            estadoTambor.Margin = new Thickness(0, 6, 0, 0);

            StackPanel panel = new StackPanel();
            panel.VerticalAlignment = VerticalAlignment.Center;
            panel.HorizontalAlignment = HorizontalAlignment.Center;
            panel.Children.Add(ojo);
            panel.Children.Add(estadoTambor);
            return panel;
        }

        /// <summary>La pantalla de inicio se mantiene simple: las acciones se ejecutan desde el menu.</summary>
        private FrameworkElement CrearBotones()
        {
            StackPanel panel = new StackPanel();
            panel.Margin = new Thickness(0, 10, 0, 0);
            panel.HorizontalAlignment = HorizontalAlignment.Center;
            panel.MaxWidth = 640;

            panel.Children.Add(Ui.Rotulo("PANEL PRINCIPAL"));

            Border resumen = new Border();
            resumen.Background = Tema.Cristal;
            resumen.BorderBrush = Tema.CristalBorde;
            resumen.BorderThickness = new Thickness(1);
            resumen.CornerRadius = new CornerRadius(14);
            resumen.Padding = new Thickness(18, 14, 18, 14);
            resumen.Margin = new Thickness(0, 10, 0, 0);

            StackPanel contenido = new StackPanel();
            TextBlock titulo = Ui.Texto("El sistema se controla desde el menú superior.", 15, Tema.Texto);
            titulo.FontWeight = FontWeights.SemiBold;
            contenido.Children.Add(titulo);

            TextBlock detalle = Ui.Texto("Usa los menús Lavadora y Mantenimiento para abrir cada operación, y Ayuda para consultar la información del proyecto.", 12, Tema.TextoSecundario);
            detalle.Margin = new Thickness(0, 8, 0, 0);
            detalle.TextWrapping = TextWrapping.Wrap;
            contenido.Children.Add(detalle);

            resumen.Child = contenido;
            panel.Children.Add(resumen);

            return panel;
        }

        private MenuItem CrearMenu(string titulo, params MenuItem[] opciones)
        {
            MenuItem menu = new MenuItem
            {
                Header = titulo,
                ItemContainerStyle = EstiloElementoMenu()
            };
            foreach (MenuItem opcion in opciones)
            {
                menu.Items.Add(opcion);
            }
            return menu;
        }

        private static Style EstiloElementoMenu()
        {
            Style estilo = new Style(typeof(MenuItem));
            estilo.Setters.Add(new Setter(Control.BackgroundProperty, Tema.FondoAlto));
            estilo.Setters.Add(new Setter(Control.ForegroundProperty, Tema.Texto));
            estilo.Setters.Add(new Setter(Control.FontFamilyProperty, Tema.Fuente));
            estilo.Setters.Add(new Setter(Control.FontSizeProperty, 13.0));
            estilo.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(12, 7, 18, 7)));

            Trigger resaltado = new Trigger
            {
                Property = MenuItem.IsHighlightedProperty,
                Value = true
            };
            resaltado.Setters.Add(new Setter(Control.BackgroundProperty, Tema.Chasis));
            resaltado.Setters.Add(new Setter(Control.ForegroundProperty, Tema.Cian));
            estilo.Triggers.Add(resaltado);
            return estilo;
        }

        private MenuItem CrearAccion(string texto, Action accion)
        {
            MenuItem opcion = new MenuItem { Header = texto };
            opcion.Click += (s, e) =>
            {
                Sonidos.Click();
                accion();
            };
            return opcion;
        }

        private BotonVoltia Atajo(string texto, bool principal, Action accion)
        {
            BotonVoltia boton = principal
                ? BotonVoltia.Principal(texto)
                : BotonVoltia.Secundario(texto);
            boton.Margin = new Thickness(3, 3, 3, 3);
            boton.Height = 38;
            boton.Click += (s, e) =>
            {
                Sonidos.Click();
                accion();
            };
            return boton;
        }

        /// <summary>Pie informativo; las acciones están disponibles en el menú superior.</summary>
        private FrameworkElement CrearPie()
        {
            TextBlock pie = Ui.Texto("VOLTIA  |  Gestion de electrodomesticos", 11, Tema.TextoSecundario);
            pie.Margin = new Thickness(0, 14, 0, 0);
            pie.HorizontalAlignment = HorizontalAlignment.Center;
            return pie;
        }

        // ====================================================================
        //  Vida de la ventana (Observer y Vigilante)
        // ====================================================================

        private void AlCargar(object remitente, RoutedEventArgs argumentos)
        {
            Anime.Escalonado(new UIElement[] { ojo, estadoTambor }, 90, 500);
            Anime.Flotar(ojo, 5, 3400, 1600);
            Sonidos.Click();

            if (!suscrito)
            {
                // Observer: nos enteramos cuando el servidor responde o deja de responder.
                NotificadorDatos.EstadoConexion += AlCambiarConexion;
                NotificadorDatos.Trabajando += AlTrabajar;
                suscrito = true;
            }

            vigilante.Iniciar(); // Cada 3 segundos pregunta la version de los datos
        }

        private void AlCerrar(object remitente, EventArgs argumentos)
        {
            vigilante.Detener();
            if (suscrito)
            {
                NotificadorDatos.EstadoConexion -= AlCambiarConexion;
                NotificadorDatos.Trabajando -= AlTrabajar;
                suscrito = false;
            }
        }

        /// <summary>Pinta el estado de la conexion (puede llegar de otro hilo).</summary>
        private void AlCambiarConexion(bool conectado)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(new Action<bool>(AlCambiarConexion), conectado);
                return;
            }

            if (conectado)
            {
                textoConexion.Text = "SERVIDOR EN LINEA";
                textoConexion.Foreground = Tema.Exito;
                puntoConexion.Fill = Tema.Exito;
            }
            else
            {
                textoConexion.Text = "SIN CONEXION";
                textoConexion.Foreground = Tema.Error;
                puntoConexion.Fill = Tema.Error;
            }
        }

        /// <summary>Cambia el letrero del tambor cuando hay peticiones en curso.</summary>
        private void AlTrabajar(bool trabajando)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(new Action<bool>(AlTrabajar), trabajando);
                return;
            }

            estadoTambor.Text = trabajando ? "PROCESANDO..." : "TAMBOR EN ESPERA";
            estadoTambor.Foreground = trabajando ? Tema.Ambar : Tema.TextoSecundario;
        }

        // ====================================================================
        //  Abrir cada caso de uso en su propia ventana
        // ====================================================================

        private void AbrirListar()
        {
            Abrir(new VentanaListar());
        }

        private void AbrirAgregar()
        {
            Abrir(new VentanaAgregar());
        }

        private void AbrirConsultar()
        {
            Abrir(new VentanaConsultar());
        }

        private void AbrirActualizar()
        {
            Abrir(new VentanaActualizar());
        }

        private void AbrirEliminar()
        {
            Abrir(new VentanaEliminar());
        }

        private void AbrirListarMantenimientos() { Abrir(new VentanaListarMantenimientos()); }
        private void AbrirAgregarMantenimiento() { Abrir(new VentanaAgregarMantenimiento()); }
        private void AbrirConsultarMantenimiento() { Abrir(new VentanaConsultarMantenimiento()); }
        private void AbrirActualizarMantenimiento() { Abrir(new VentanaActualizarMantenimiento()); }
        private void AbrirEliminarMantenimiento() { Abrir(new VentanaEliminarMantenimiento()); }

        private void AbrirAcercaDe()
        {
            Abrir(new VentanaAcercaDe());
        }

        /// <summary>
        /// Abre una ventana independiente para que cerrar una no cierre las demas.
        /// Se usa Show (no ShowDialog) para poder tener varias abiertas a la vez.
        /// </summary>
        private void Abrir(Window ventana)
        {
            ventana.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ventana.Show();
        }
    }
}
