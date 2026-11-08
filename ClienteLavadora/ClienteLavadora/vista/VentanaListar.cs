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
    /// Muestra una grilla de lavadoras con filtros por marca e inverter. Los detalles
    /// de mantenimiento se administran desde su propio caso de uso.
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

        private readonly DataGrid grilla = Ui.Grilla();
        private readonly TextBlock contador = new TextBlock();
        private readonly TextBlock aviso = new TextBlock();

        private bool cargando;
        private bool filtrosAplicados;

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

            Loaded += (s, e) =>
            {
                Anime.Escalonado(new UIElement[] { filtros, cuerpo, pie }, 80, 300);
                grilla.ItemsSource = new List<LavadoraData>();
                contador.Text = "0 lavadora(s)";
                aviso.Text = "Lista vacia. Use Filtrar o Recargar para consultar datos.";
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
            rejilla.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            txtMarca.Ayuda = "Escriba parte de la marca";
            txtMarca.TeclaPresionada += AlPresionarTeclaEnFiltro;

            StackPanel celdaMarca = Ui.Celda("Marca (contiene el texto)", txtMarca);
            Grid.SetColumn(celdaMarca, 0);
            rejilla.Children.Add(celdaMarca);

            StackPanel celdaInverter = Ui.Celda("Tecnologia inverter", cmbInverter);
            Grid.SetColumn(celdaInverter, 2);
            rejilla.Children.Add(celdaInverter);

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
                await CargarTodasAsync();
            };
            botones.Children.Add(btnLimpiar);

            Grid.SetColumn(botones, 4);
            rejilla.Children.Add(botones);

            Border panel = Ui.Cristal(rejilla, new Thickness(12, 10, 12, 10));
            panel.Margin = new Thickness(0, 0, 0, 10);
            return panel;
        }

        /// <summary>La lista contiene únicamente atributos de lavadoras.</summary>
        private FrameworkElement CrearCuerpo()
        {
            ArmarGrilla();
            return grilla;
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
            grilla.Columns.Add(Ui.Columna("Precio final", "PrecioFinalTexto", 1.45, true));

            grilla.LoadingRow += (s, e) => Anime.Entrada(e.Row, Math.Min(e.Row.GetIndex(), 12) * 40);
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
            btnRecargar.Click += async (s, e) => await CargarTodasAsync();
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
            filtrosAplicados = true;
            await CargarListaAsync(true);
        }

        private async Task CargarTodasAsync()
        {
            filtrosAplicados = false;
            txtMarca.Texto = "";
            cmbInverter.SelectedIndex = 0;
            await CargarListaAsync(false);
        }

        private async Task CargarActualAsync()
        {
            await CargarListaAsync(filtrosAplicados);
        }

        private async Task CargarListaAsync(bool conFiltros)
        {
            if (cargando)
            {
                return; // Evita dos peticiones iguales al mismo tiempo
            }
            cargando = true;

            // Codigo de la fila que estaba seleccionada, para volver a seleccionarla.
            LavadoraData filaSeleccionada = grilla.SelectedItem as LavadoraData;
            int codigoAnterior = filaSeleccionada == null ? 0 : filaSeleccionada.Codigo;
            aviso.Text = "Consultando...";

            try
            {
                string marca = conFiltros ? txtMarca.Texto.Trim() : null;
                if (marca != null && marca.Length == 0)
                {
                    marca = null;
                }
                bool? inverter = conFiltros ? Opcion(cmbInverter.SelectedIndex) : null;

                // Task.Run: la red corre fuera del hilo de la interfaz.
                List<LavadoraData> lista = await Task.Run(
                    () => conFiltros
                        ? controlador.ListarAsync(marca, inverter, null)
                        : controlador.ListarTodasAsync());

                grilla.ItemsSource = lista;
                Anime.Contar(contador, lista.Count, "{0} lavadora(s)");
                aviso.Text = "Actualizado " + DateTime.Now.ToString("HH:mm:ss");

                if (codigoAnterior != 0)
                {
                    SeleccionarPorCodigo(lista, codigoAnterior);
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
            await CargarActualAsync();
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
