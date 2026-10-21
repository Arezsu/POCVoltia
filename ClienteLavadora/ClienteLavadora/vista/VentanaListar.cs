using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ClienteLavadora.controlador;
using ClienteLavadora.modelo;
using ClienteLavadora.servicios;
using ClienteLavadora.util;
using ClienteLavadora.vista.controles;

namespace ClienteLavadora.vista
{
    /// <summary>
    /// CASO DE USO LISTAR (ventana independiente).
    ///
    /// Muestra la grilla de lavadoras con tres filtros que aplica el SERVIDOR
    /// (marca que contiene el texto, inverter y con/sin mantenimientos), un contador
    /// digital, y a la derecha el DETALLE de la fila seleccionada con sus mantenimientos.
    ///
    /// PATRON OBSERVER (lo mas importante de esta ventana):
    ///  - se SUSCRIBE a NotificadorDatos.DatosCambiaron al construirse;
    ///  - si otra ventana agrega, actualiza o elimina algo, o si el VigilanteServidor
    ///    detecta que OTRO cliente cambio los datos, esta grilla se recarga sola;
    ///  - se DESUSCRIBE al cerrarse para no dejar referencias colgando.
    ///
    /// CONCURRENCIA: la peticion se lanza con Task.Run y se espera con await, entonces
    /// la ventana sigue respondiendo y el tambor sigue girando mientras llega la respuesta.
    /// </summary>
    public class VentanaListar : VentanaMarco
    {
        private readonly ControladorLavadoras controlador = new ControladorLavadoras(new ServicioLavadoras());

        private readonly CampoTexto txtMarca = new CampoTexto();
        private readonly ComboBox cmbInverter = Ui.Combo("Todos", "Con inverter", "Sin inverter");
        private readonly ComboBox cmbMantenimientos = Ui.Combo("Todos", "Con mantenimientos", "Sin mantenimientos");

        private readonly DataGrid grilla = Ui.Grilla();
        private readonly TextBlock contador = new TextBlock();
        private readonly TextBlock aviso = new TextBlock();

        private readonly FichaLavadora ficha = new FichaLavadora();
        private readonly ListaMantenimientos listaMantenimientos = new ListaMantenimientos();
        private readonly TextBlock tituloDetalle = new TextBlock();
        private readonly BotonVoltia btnMantenimientos = BotonVoltia.Acento("Abrir Mantenimientos");

        private LavadoraData seleccionada;
        private bool cargando;

        public VentanaListar() : base("LISTAR LAVADORAS", 1220, 810, true, 96)
        {
            Grid raiz = new Grid();
            raiz.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            raiz.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            raiz.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            FrameworkElement filtros = CrearFiltros();
            FrameworkElement cuerpo = CrearCuerpo();
            FrameworkElement pie = CrearPie();

            Grid.SetRow(filtros, 0);
            Grid.SetRow(cuerpo, 1);
            Grid.SetRow(pie, 2);
            raiz.Children.Add(filtros);
            raiz.Children.Add(cuerpo);
            raiz.Children.Add(pie);
            Contenido = raiz;

            // Observer: recarga automatica. Nos quitamos al cerrar.
            NotificadorDatos.DatosCambiaron += AlCambiarDatos;
            Closed += AlCerrar;

            Loaded += async (s, e) =>
            {
                Anime.Escalonado(new UIElement[] { filtros, cuerpo, pie }, 80, 300);
                await CargarAsync();
            };
        }

        // ====================================================================
        //  Armado de la pantalla
        // ====================================================================

        /// <summary>Los tres filtros y los botones.</summary>
        private FrameworkElement CrearFiltros()
        {
            Grid rejilla = new Grid();
            rejilla.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.4, GridUnitType.Star) });
            rejilla.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) });
            rejilla.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            rejilla.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) });
            rejilla.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            rejilla.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) });
            rejilla.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            txtMarca.Ayuda = "Escriba parte de la marca";
            txtMarca.TeclaPresionada += AlPresionarTeclaEnFiltro;

            StackPanel celdaMarca = Ui.Celda("Marca (contiene el texto)", txtMarca);
            Grid.SetColumn(celdaMarca, 0);
            rejilla.Children.Add(celdaMarca);

            StackPanel celdaInverter = Ui.Celda("Tecnologia inverter", cmbInverter);
            Grid.SetColumn(celdaInverter, 2);
            rejilla.Children.Add(celdaInverter);

            StackPanel celdaMantenimientos = Ui.Celda("Mantenimientos", cmbMantenimientos);
            Grid.SetColumn(celdaMantenimientos, 4);
            rejilla.Children.Add(celdaMantenimientos);

            StackPanel botones = new StackPanel();
            botones.Orientation = Orientation.Horizontal;
            botones.VerticalAlignment = VerticalAlignment.Bottom;

            BotonVoltia btnFiltrar = BotonVoltia.Principal("Filtrar");
            btnFiltrar.Width = 105;
            btnFiltrar.Height = 32;
            btnFiltrar.Margin = new Thickness(0, 0, 8, 0);
            btnFiltrar.Click += async (s, e) => await CargarAsync();
            botones.Children.Add(btnFiltrar);

            BotonVoltia btnLimpiar = BotonVoltia.Secundario("Quitar filtros");
            btnLimpiar.Width = 130;
            btnLimpiar.Height = 32;
            btnLimpiar.Click += async (s, e) =>
            {
                txtMarca.Texto = "";
                cmbInverter.SelectedIndex = 0;
                cmbMantenimientos.SelectedIndex = 0;
                await CargarAsync();
            };
            botones.Children.Add(btnLimpiar);

            Grid.SetColumn(botones, 6);
            rejilla.Children.Add(botones);

            Border panel = Ui.Cristal(rejilla, new Thickness(12, 10, 12, 10));
            panel.Margin = new Thickness(0, 0, 0, 10);
            return panel;
        }

        /// <summary>Grilla a la izquierda y panel de detalle a la derecha.</summary>
        private FrameworkElement CrearCuerpo()
        {
            ArmarGrilla();

            Grid cuerpo = new Grid();
            cuerpo.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            cuerpo.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) });
            cuerpo.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(390) });

            Grid.SetColumn(grilla, 0);
            cuerpo.Children.Add(grilla);

            FrameworkElement detalle = CrearDetalle();
            Grid.SetColumn(detalle, 2);
            cuerpo.Children.Add(detalle);
            return cuerpo;
        }

        /// <summary>Columnas de la grilla: las mismas del schema, con grilla oscura.</summary>
        private void ArmarGrilla()
        {
            // Columnas de la grilla: las clave para comparar lavadoras. Los datos
            // completos (RPM, minutos, costos) se ven en el panel de detalle.
            grilla.Columns.Add(Ui.Columna("Codigo", "Codigo", 0.7, true));
            grilla.Columns.Add(Ui.Columna("Marca", "Marca", 1.15, false));
            grilla.Columns.Add(Ui.Columna("Fecha fab.", "FechaTexto", 1.5, false));
            grilla.Columns.Add(Ui.Columna("Precio base", "PrecioBaseTexto", 1.35, true));
            grilla.Columns.Add(Ui.Columna("Capacidad", "CapacidadTexto", 1.0, true));
            grilla.Columns.Add(Ui.Columna("Inverter", "InverterTexto", 0.9, false));
            grilla.Columns.Add(Ui.Columna("Mant.", "CantidadMantenimientos", 0.75, true));
            grilla.Columns.Add(Ui.Columna("Precio final", "PrecioFinalTexto", 1.45, true));

            grilla.SelectionChanged += AlSeleccionar;
            grilla.MouseDoubleClick += AlDobleClic;
            grilla.LoadingRow += (s, e) => Anime.Entrada(e.Row, Math.Min(e.Row.GetIndex(), 12) * 40);
        }

        /// <summary>Panel de cristal con la ficha y los mantenimientos de la fila escogida.</summary>
        private FrameworkElement CrearDetalle()
        {
            tituloDetalle.Text = "DETALLE";
            tituloDetalle.FontFamily = Tema.Numeros;
            tituloDetalle.FontSize = 12;
            tituloDetalle.FontWeight = FontWeights.Bold;
            tituloDetalle.Foreground = Tema.Cian;
            tituloDetalle.Margin = new Thickness(0, 0, 0, 8);

            ScrollViewer desplazable = Ui.Desplazable(ficha);
            desplazable.MaxHeight = 205;
            desplazable.Margin = new Thickness(0, 0, 0, 6);

            TextBlock rotulo = Ui.Rotulo("MANTENIMIENTOS DE LA LAVADORA");
            rotulo.Margin = new Thickness(0, 2, 0, 6);

            btnMantenimientos.Height = 32;
            btnMantenimientos.Habilitado = false;
            btnMantenimientos.Margin = new Thickness(0, 8, 0, 0);
            btnMantenimientos.Click += (s, e) => AbrirMantenimientos();

            Grid panel = new Grid();
            panel.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            panel.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            panel.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            panel.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            panel.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            panel.Children.Add(tituloDetalle);
            Grid.SetRow(desplazable, 1);
            panel.Children.Add(desplazable);
            Grid.SetRow(rotulo, 2);
            panel.Children.Add(rotulo);
            Grid.SetRow(listaMantenimientos, 3);
            panel.Children.Add(listaMantenimientos);
            Grid.SetRow(btnMantenimientos, 4);
            panel.Children.Add(btnMantenimientos);

            Border cristal = Ui.Cristal(panel, new Thickness(13));
            return cristal;
        }

        /// <summary>Contador digital y botones de accion.</summary>
        private FrameworkElement CrearPie()
        {
            Grid pie = new Grid();
            pie.Margin = new Thickness(0, 10, 0, 0);
            pie.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            pie.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            StackPanel izquierda = new StackPanel();
            izquierda.Orientation = Orientation.Horizontal;

            contador.Text = "0 lavadora(s)";
            contador.FontFamily = Tema.Numeros;
            contador.FontSize = 14;
            contador.FontWeight = FontWeights.Bold;
            contador.Foreground = Tema.Ambar;

            Border pantalla = Ui.Pantalla(contador);
            pantalla.Height = 36;
            pantalla.MinWidth = 190;
            pantalla.VerticalAlignment = VerticalAlignment.Center;
            izquierda.Children.Add(pantalla);

            aviso.Text = "";
            aviso.FontFamily = Tema.Numeros;
            aviso.FontSize = 10.5;
            aviso.Foreground = Tema.TextoSecundario;
            aviso.VerticalAlignment = VerticalAlignment.Center;
            aviso.Margin = new Thickness(12, 0, 0, 0);
            izquierda.Children.Add(aviso);

            pie.Children.Add(izquierda);

            StackPanel derecha = new StackPanel();
            derecha.Orientation = Orientation.Horizontal;

            BotonVoltia btnRecargar = BotonVoltia.Acento("Recargar");
            btnRecargar.Width = 110;
            btnRecargar.Height = 34;
            btnRecargar.Margin = new Thickness(0, 0, 8, 0);
            btnRecargar.Click += async (s, e) => await CargarAsync();
            derecha.Children.Add(btnRecargar);

            BotonVoltia btnCerrar = BotonVoltia.Secundario("Cerrar");
            btnCerrar.Width = 110;
            btnCerrar.Height = 34;
            btnCerrar.Click += (s, e) => Cerrar();
            derecha.Children.Add(btnCerrar);

            Grid.SetColumn(derecha, 1);
            pie.Children.Add(derecha);
            return pie;
        }

        // ====================================================================
        //  Cargar datos
        // ====================================================================

        /// <summary>Pide al servidor la lista con los filtros escogidos.</summary>
        private async Task CargarAsync()
        {
            if (cargando)
            {
                return; // Evita dos peticiones iguales al mismo tiempo
            }
            cargando = true;

            // Codigo de la fila que estaba seleccionada, para volver a seleccionarla.
            int codigoAnterior = seleccionada == null ? 0 : seleccionada.Codigo;
            aviso.Text = "Consultando...";

            try
            {
                string marca = txtMarca.Texto.Trim();
                if (marca.Length == 0)
                {
                    marca = null;
                }
                bool? inverter = Opcion(cmbInverter.SelectedIndex);
                bool? conMantenimientos = Opcion(cmbMantenimientos.SelectedIndex);

                // Task.Run: la red corre fuera del hilo de la interfaz.
                List<LavadoraData> lista = await Task.Run(
                    () => controlador.ListarAsync(marca, inverter, conMantenimientos));

                grilla.ItemsSource = lista;
                Anime.Contar(contador, lista.Count, "{0} lavadora(s)");
                aviso.Text = "Actualizado " + DateTime.Now.ToString("HH:mm:ss");

                if (codigoAnterior != 0)
                {
                    SeleccionarPorCodigo(lista, codigoAnterior);
                }
                else
                {
                    MostrarDetalle(null);
                }
            }
            catch (Exception ex)
            {
                aviso.Text = "Error al consultar";
                Aviso("No se pudo listar", ex.Message);
            }
            finally
            {
                cargando = false;
            }
        }

        /// <summary>Convierte el indice del combo en el filtro del servidor: 0 = todos.</summary>
        private static bool? Opcion(int indice)
        {
            if (indice == 1)
            {
                return true;
            }
            if (indice == 2)
            {
                return false;
            }
            return null;
        }

        private void SeleccionarPorCodigo(List<LavadoraData> lista, int codigo)
        {
            foreach (LavadoraData lavadora in lista)
            {
                if (lavadora.Codigo == codigo)
                {
                    grilla.SelectedItem = lavadora;
                    grilla.ScrollIntoView(lavadora);
                    return;
                }
            }
            MostrarDetalle(null); // Ya no existe (otro cliente lo elimino)
        }

        // ====================================================================
        //  Seleccion y detalle
        // ====================================================================

        private void AlSeleccionar(object remitente, SelectionChangedEventArgs argumentos)
        {
            MostrarDetalle(grilla.SelectedItem as LavadoraData);
        }

        private void MostrarDetalle(LavadoraData lavadora)
        {
            seleccionada = lavadora;

            if (lavadora == null)
            {
                tituloDetalle.Text = "DETALLE";
                ficha.MostrarVacio("Escoja una lavadora en la grilla para ver su ficha y sus mantenimientos.");
                listaMantenimientos.Mostrar(null, "Sin mantenimientos para mostrar.");
                btnMantenimientos.Habilitado = false;
                return;
            }

            tituloDetalle.Text = "DETALLE  " + lavadora.Codigo + "  -  " + lavadora.Marca;
            ficha.Mostrar(lavadora);
            listaMantenimientos.Mostrar(lavadora.ListaMantenimientos,
                "Esta lavadora no tiene mantenimientos registrados.");
            btnMantenimientos.Habilitado = true;
        }

        private void AlDobleClic(object remitente, MouseButtonEventArgs argumentos)
        {
            if (seleccionada != null)
            {
                AbrirMantenimientos();
            }
        }

        private void AbrirMantenimientos()
        {
            if (seleccionada == null)
            {
                return;
            }
            VentanaMantenimientos ventana = new VentanaListarMantenimientos(seleccionada.Codigo);
            // Es una ventana de caso de uso independiente; no debe cerrarse al cerrar
            // la ventana de listado ni cerrar otras ventanas abiertas.
            ventana.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ventana.Show();
        }

        // ====================================================================
        //  Observer
        // ====================================================================

        private void AlCambiarDatos()
        {
            // El aviso puede llegar desde el hilo del vigilante: se pasa al hilo de la
            // interfaz con Dispatcher.Invoke antes de tocar cualquier control.
            Dispatcher.Invoke(new Action(RecargarPorAviso));
        }

        private async void RecargarPorAviso()
        {
            aviso.Text = "Cambio detectado: recargando...";
            await CargarAsync();
        }

        private void AlCerrar(object remitente, EventArgs argumentos)
        {
            // Desuscripcion obligatoria del Observer.
            NotificadorDatos.DatosCambiaron -= AlCambiarDatos;
        }

        // ====================================================================
        //  Teclado
        // ====================================================================

        private async void AlPresionarTeclaEnFiltro(object remitente, KeyEventArgs argumentos)
        {
            if (argumentos.Key == Key.Enter)
            {
                await CargarAsync();
            }
        }
    }
}
