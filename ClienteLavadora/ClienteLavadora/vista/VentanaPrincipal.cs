using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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

        private readonly OjoDeBuey ojo = new OjoDeBuey(300);
        private readonly TextBlock estadoTambor = new TextBlock();

        private bool suscrito;

        public VentanaPrincipal() : base("VOLTIA  PANEL DE CONTROL", 900, 800, false, 0)
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ShowInTaskbar = true; // La ventana principal si debe aparecer en la barra de Windows
            vigilante = new VigilanteServidor(controlador);

            Grid raiz = new Grid();
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
            return marco;
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

        /// <summary>Un boton por cada caso de uso.</summary>
        private FrameworkElement CrearBotones()
        {
            StackPanel panel = new StackPanel();
            panel.Margin = new Thickness(0, 6, 0, 0);

            panel.Children.Add(Ui.Rotulo("LAVADORAS"));
            UniformGrid filaLavadoras = new UniformGrid();
            filaLavadoras.Columns = 5;
            filaLavadoras.Children.Add(Atajo("Listar", true, AbrirListar));
            filaLavadoras.Children.Add(Atajo("Agregar", false, AbrirAgregar));
            filaLavadoras.Children.Add(Atajo("Consultar", false, AbrirConsultar));
            filaLavadoras.Children.Add(Atajo("Actualizar", false, AbrirActualizar));
            filaLavadoras.Children.Add(Atajo("Eliminar", false, AbrirEliminar));
            panel.Children.Add(filaLavadoras);

            TextBlock rotuloMantenimientos = Ui.Rotulo("MANTENIMIENTOS");
            rotuloMantenimientos.Margin = new Thickness(0, 12, 0, 6);
            panel.Children.Add(rotuloMantenimientos);

            UniformGrid filaMantenimientos = new UniformGrid();
            filaMantenimientos.Columns = 5;
            filaMantenimientos.Children.Add(Atajo("Listar", true, AbrirListarMantenimientos));
            filaMantenimientos.Children.Add(Atajo("Agregar", false, AbrirAgregarMantenimiento));
            filaMantenimientos.Children.Add(Atajo("Consultar", false, AbrirConsultarMantenimiento));
            filaMantenimientos.Children.Add(Atajo("Actualizar", false, AbrirActualizarMantenimiento));
            filaMantenimientos.Children.Add(Atajo("Eliminar", false, AbrirEliminarMantenimiento));
            panel.Children.Add(filaMantenimientos);

            TextBlock rotuloAyuda = Ui.Rotulo("AYUDA");
            rotuloAyuda.Margin = new Thickness(0, 10, 0, 4);
            panel.Children.Add(rotuloAyuda);
            UniformGrid filaAyuda = new UniformGrid { Columns = 1 };
            filaAyuda.Children.Add(Atajo("Acerca de", false, AbrirAcercaDe));
            panel.Children.Add(filaAyuda);

            return panel;
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

        /// <summary>Pie: datos del curso y boton de salida.</summary>
        private FrameworkElement CrearPie()
        {
            Grid pie = new Grid();
            pie.Margin = new Thickness(0, 14, 0, 0);
            pie.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            pie.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            pie.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            BotonVoltia btnTema = BotonVoltia.Secundario("Modo claro");
            btnTema.Width = 130;
            btnTema.Margin = new Thickness(0, 0, 8, 0);
            btnTema.Click += (s, e) =>
            {
                Tema.Alternar();
                btnTema.Texto = Tema.EsClaro ? "Modo oscuro" : "Modo claro";
            };
            Grid.SetColumn(btnTema, 1);
            pie.Children.Add(btnTema);

            BotonVoltia btnSalir = BotonVoltia.Secundario("Cerrar");
            btnSalir.Width = 120;
            btnSalir.Click += (s, e) => Cerrar();
            Grid.SetColumn(btnSalir, 2);
            pie.Children.Add(btnSalir);

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
