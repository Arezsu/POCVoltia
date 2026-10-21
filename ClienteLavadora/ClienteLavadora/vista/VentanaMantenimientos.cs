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
    /// VENTANA DE MANTENIMIENTOS: aqui se trabaja el objeto DETALLE de la relacion
    /// maestro-detalle (una lavadora puede tener cero, uno o muchos mantenimientos).
    ///
    /// Ventana especializada por caso de uso. El selector de lavadora se comparte,
    /// mientras que cada instancia muestra solo la operacion que le corresponde.
    /// </summary>
    public class VentanaMantenimientos : VentanaMarco
    {
        private readonly ControladorLavadoras controladorLavadoras =
            new ControladorLavadoras(new ServicioLavadoras());
        private readonly ControladorMantenimientos controlador =
            new ControladorMantenimientos(new ServicioMantenimientos());

        private readonly ComboBox cmbLavadora = Ui.Combo();
        private readonly CampoTexto txtTecnico = new CampoTexto();
        private readonly ComboBox cmbCompletado = Ui.Combo("Todos", "Pendientes", "Completados");
        private StackPanel celdaTecnico;
        private StackPanel celdaEstado;
        private StackPanel botonesSelector;
        private Grid rejillaSelector;

        private readonly DataGrid grilla = Ui.Grilla();
        private readonly CamposMantenimiento campos = new CamposMantenimiento();

        private readonly TextBlock tituloFormulario = new TextBlock();
        private readonly TextBlock resumenLavadora = new TextBlock();
        private readonly TextBlock contador = new TextBlock();
        private readonly TextBlock aviso = new TextBlock();

        private readonly BotonVoltia btnAgregar = BotonVoltia.Principal("Agregar");
        private readonly BotonVoltia btnActualizar = BotonVoltia.Acento("Actualizar");
        private readonly BotonVoltia btnEliminar = BotonVoltia.Peligro("Eliminar");
        private BotonVoltia btnNuevo;
        private FrameworkElement pistaBuilder;

        private readonly int codigoInicial;
        private readonly string modo;

        private LavadoraData lavadoraActual;
        private MantenimientoData seleccionado;
        private bool silenciarSeleccion;
        private int versionCarga;
        private bool guardando;

        public VentanaMantenimientos(int codigoLavadora) : this(codigoLavadora, "listar") { }

        public VentanaMantenimientos(int codigoLavadora, string casoUso)
            : base(Titulo(casoUso), AnchoVentana(casoUso), AltoVentana(casoUso), true, 96)
        {
            codigoInicial = codigoLavadora;
            modo = (casoUso ?? "listar").ToLowerInvariant();

            Grid raiz = new Grid();
            raiz.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            raiz.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            raiz.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            FrameworkElement selector = CrearSelector();
            FrameworkElement cuerpo = CrearCuerpo();
            FrameworkElement pie = CrearPie();
            ConfigurarCasoDeUso(cuerpo);

            Grid.SetRow(selector, 0);
            Grid.SetRow(cuerpo, 1);
            Grid.SetRow(pie, 2);
            raiz.Children.Add(selector);
            raiz.Children.Add(cuerpo);
            raiz.Children.Add(pie);
            Contenido = raiz;

            NotificadorDatos.DatosCambiaron += AlCambiarDatos;
            Closed += AlCerrar;

            Loaded += async (s, e) =>
            {
                Anime.Escalonado(new UIElement[] { selector, cuerpo, pie }, 80, 260);
                await CargarLavadorasAsync();
            };
        }

        private static string Titulo(string casoUso)
        {
            string caso = (casoUso ?? "listar").ToLowerInvariant();
            switch (caso)
            {
                case "agregar": return "AGREGAR MANTENIMIENTO";
                case "consultar": return "CONSULTAR MANTENIMIENTO";
                case "actualizar": return "ACTUALIZAR MANTENIMIENTO";
                case "eliminar": return "ELIMINAR MANTENIMIENTO";
                default: return "LISTAR MANTENIMIENTOS";
            }
        }

        private static double AnchoVentana(string casoUso)
        {
            return casoUso == "listar" ? 1240 : 940;
        }

        private static double AltoVentana(string casoUso)
        {
            return casoUso == "listar" ? 810 : 850;
        }

        private void ConfigurarCasoDeUso(FrameworkElement cuerpo)
        {
            Grid panel = cuerpo as Grid;
            FrameworkElement formulario = panel.Children.Count > 1 ? panel.Children[1] as FrameworkElement : null;
            bool agregar = modo == "agregar";
            bool listar = modo == "listar";
            bool consultar = modo == "consultar";
            bool actualizar = modo == "actualizar";
            bool eliminar = modo == "eliminar";
            bool operacionConRegistro = consultar || actualizar || eliminar;

            grilla.Visibility = agregar ? Visibility.Collapsed : Visibility.Visible;
            if (formulario != null)
            {
                formulario.Visibility = listar ? Visibility.Collapsed : Visibility.Visible;
            }
            if (agregar)
            {
                panel.ColumnDefinitions[0].Width = new GridLength(0);
                panel.ColumnDefinitions[1].Width = new GridLength(0);
                panel.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Star);
            }
            else if (listar)
            {
                panel.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
                panel.ColumnDefinitions[1].Width = new GridLength(0);
                panel.ColumnDefinitions[2].Width = new GridLength(0);
            }

            btnAgregar.Visibility = agregar ? Visibility.Visible : Visibility.Collapsed;
            btnActualizar.Visibility = actualizar ? Visibility.Visible : Visibility.Collapsed;
            btnEliminar.Visibility = eliminar ? Visibility.Visible : Visibility.Collapsed;
            btnNuevo.Visibility = agregar ? Visibility.Visible : Visibility.Collapsed;
            pistaBuilder.Visibility = agregar ? Visibility.Visible : Visibility.Collapsed;
            btnActualizar.Habilitado = actualizar && seleccionado != null;
            btnEliminar.Habilitado = eliminar && seleccionado != null;
            campos.PermitirEdicion(agregar);
            if (agregar || operacionConRegistro)
            {
                for (int i = 1; i < rejillaSelector.ColumnDefinitions.Count; i++)
                {
                    rejillaSelector.ColumnDefinitions[i].Width = new GridLength(0);
                }
                celdaTecnico.Visibility = Visibility.Collapsed;
                celdaEstado.Visibility = Visibility.Collapsed;
                botonesSelector.Visibility = Visibility.Collapsed;
            }

            if (modo == "listar" || modo == "consultar")
            {
                btnActualizar.Visibility = Visibility.Collapsed;
                btnEliminar.Visibility = Visibility.Collapsed;
                btnAgregar.Visibility = Visibility.Collapsed;
            }
            if (actualizar || eliminar || consultar)
            {
                tituloFormulario.Text = consultar ? "CONSULTA DEL MANTENIMIENTO" :
                    (actualizar ? "ACTUALIZAR MANTENIMIENTO" : "CONFIRMAR MANTENIMIENTO");
                campos.PermitirEdicion(false);
            }
        }

        // ====================================================================
        //  Armado de la pantalla
        // ====================================================================

        /// <summary>Selector de lavadora y filtros del detalle.</summary>
        private FrameworkElement CrearSelector()
        {
            Grid rejilla = new Grid();
            rejillaSelector = rejilla;
            rejilla.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.3, GridUnitType.Star) });
            rejilla.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) });
            rejilla.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            rejilla.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) });
            rejilla.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.8, GridUnitType.Star) });
            rejilla.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) });
            rejilla.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            cmbLavadora.SelectionChanged += AlCambiarLavadora;
            txtTecnico.TeclaPresionada += AlPresionarTecla;
            cmbCompletado.SelectionChanged += async (s, e) => await CargarMantenimientosAsync();

            StackPanel celdaLavadora = Ui.Celda("Lavadora (maestro)", cmbLavadora);
            Grid.SetColumn(celdaLavadora, 0);
            rejilla.Children.Add(celdaLavadora);

            celdaTecnico = Ui.Celda("Tecnico (contiene el texto)", txtTecnico);
            Grid.SetColumn(celdaTecnico, 2);
            rejilla.Children.Add(celdaTecnico);

            celdaEstado = Ui.Celda("Estado", cmbCompletado);
            Grid.SetColumn(celdaEstado, 4);
            rejilla.Children.Add(celdaEstado);

            botonesSelector = new StackPanel();
            botonesSelector.Orientation = Orientation.Horizontal;
            botonesSelector.VerticalAlignment = VerticalAlignment.Bottom;

            BotonVoltia btnFiltrar = BotonVoltia.Principal("Filtrar");
            btnFiltrar.Width = 95;
            btnFiltrar.Height = 32;
            btnFiltrar.Margin = new Thickness(0, 0, 8, 0);
            btnFiltrar.Click += async (s, e) => await CargarMantenimientosAsync();
            botonesSelector.Children.Add(btnFiltrar);

            BotonVoltia btnRecargar = BotonVoltia.Secundario("Recargar");
            btnRecargar.Width = 100;
            btnRecargar.Height = 32;
            btnRecargar.Click += async (s, e) => await CargarLavadorasAsync();
            botonesSelector.Children.Add(btnRecargar);

            Grid.SetColumn(botonesSelector, 6);
            rejilla.Children.Add(botonesSelector);

            Border panel = Ui.Cristal(rejilla, new Thickness(12, 10, 12, 10));
            panel.Margin = new Thickness(0, 0, 0, 10);
            return panel;
        }

        /// <summary>Lista y formulario se acomodan según el caso y el espacio disponible.</summary>
        private FrameworkElement CrearCuerpo()
        {
            ArmarGrilla();

            Grid cuerpo = new Grid();
            if (modo == "consultar" || modo == "actualizar" || modo == "eliminar")
            {
                cuerpo.RowDefinitions.Add(new RowDefinition { Height = new GridLength(190) });
                cuerpo.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                Grid.SetRow(grilla, 0);
            }
            else
            {
                cuerpo.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                cuerpo.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) });
                cuerpo.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(420) });
                Grid.SetColumn(grilla, 0);
            }
            cuerpo.Children.Add(grilla);

            FrameworkElement formulario = CrearFormulario();
            if (modo == "consultar" || modo == "actualizar" || modo == "eliminar")
            {
                Grid.SetRow(formulario, 1);
            }
            else
            {
                Grid.SetColumn(formulario, 2);
            }
            cuerpo.Children.Add(formulario);
            return cuerpo;
        }

        private void ArmarGrilla()
        {
            grilla.Columns.Add(Ui.Columna("Id", "Id", 0.45, true));
            grilla.Columns.Add(Ui.Columna("Descripcion", "Descripcion", 2.2, false));
            grilla.Columns.Add(Ui.Columna("Tecnico", "Tecnico", 1.4, false));
            grilla.Columns.Add(Ui.Columna("Fecha", "FechaTexto", 1.3, false));
            grilla.Columns.Add(Ui.Columna("Costo", "CostoTexto", 1.1, true));
            grilla.Columns.Add(Ui.Columna("Estado", "EstadoTexto", 0.9, false));

            grilla.SelectionChanged += AlSeleccionarMantenimiento;
            grilla.LoadingRow += (s, e) => Anime.Entrada(e.Row, Math.Min(e.Row.GetIndex(), 12) * 40);
        }

        private FrameworkElement CrearFormulario()
        {
            tituloFormulario.Text = "NUEVO MANTENIMIENTO";
            tituloFormulario.FontFamily = Tema.Numeros;
            tituloFormulario.FontSize = 12;
            tituloFormulario.FontWeight = FontWeights.Bold;
            tituloFormulario.Foreground = Tema.Cian;
            tituloFormulario.Margin = new Thickness(0, 0, 0, 10);

            resumenLavadora.Text = "Escoja una lavadora.";
            resumenLavadora.FontFamily = Tema.Numeros;
            resumenLavadora.FontSize = 10.5;
            resumenLavadora.Foreground = Tema.TextoSecundario;
            resumenLavadora.TextWrapping = TextWrapping.Wrap;
            resumenLavadora.Margin = new Thickness(0, 0, 0, 10);

            btnActualizar.Habilitado = false;
            btnEliminar.Habilitado = false;
            btnAgregar.Height = 34;
            btnActualizar.Height = 34;
            btnEliminar.Height = 34;

            btnAgregar.Click += AlAgregar;
            btnActualizar.Click += AlActualizar;
            btnEliminar.Click += AlEliminar;

            StackPanel botones = new StackPanel();
            botones.Orientation = Orientation.Horizontal;
            botones.Margin = new Thickness(0, 4, 0, 0);

            btnAgregar.MinWidth = 100;
            btnAgregar.Margin = new Thickness(0, 0, 6, 0);
            btnActualizar.MinWidth = 105;
            btnActualizar.Margin = new Thickness(0, 0, 6, 0);
            btnEliminar.MinWidth = 100;

            botones.Children.Add(btnAgregar);
            botones.Children.Add(btnActualizar);
            botones.Children.Add(btnEliminar);

            btnNuevo = BotonVoltia.Secundario("Limpiar formulario");
            btnNuevo.Height = 34;
            btnNuevo.MinWidth = 130;
            btnNuevo.Margin = new Thickness(0, 8, 0, 0);
            btnNuevo.HorizontalAlignment = HorizontalAlignment.Left;
            btnNuevo.Click += (s, e) => PrepararNuevo();

            pistaBuilder = Ui.Pista(
                "El id del mantenimiento lo calcula el servidor y solo es unico dentro de su lavadora.");

            StackPanel contenido = new StackPanel();
            contenido.Children.Add(tituloFormulario);
            contenido.Children.Add(resumenLavadora);
            contenido.Children.Add(campos);
            contenido.Children.Add(botones);
            contenido.Children.Add(btnNuevo);
            contenido.Children.Add(pistaBuilder);

            ScrollViewer desplazable = Ui.Desplazable(contenido);
            return Ui.Cristal(desplazable, new Thickness(14, 12, 14, 12));
        }

        /// <summary>Contador digital y boton de cierre.</summary>
        private FrameworkElement CrearPie()
        {
            Grid pie = new Grid();
            pie.Margin = new Thickness(0, 10, 0, 0);
            pie.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            pie.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            StackPanel izquierda = new StackPanel();
            izquierda.Orientation = Orientation.Horizontal;

            contador.Text = "0 mantenimiento(s)";
            contador.FontFamily = Tema.Numeros;
            contador.FontSize = 14;
            contador.FontWeight = FontWeights.Bold;
            contador.Foreground = Tema.Ambar;

            Border pantalla = Ui.Pantalla(contador);
            pantalla.Height = 36;
            pantalla.MinWidth = 230;
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

            BotonVoltia btnCerrar = BotonVoltia.Secundario("Cerrar");
            btnCerrar.Width = 110;
            btnCerrar.Height = 34;
            btnCerrar.Click += (s, e) => Cerrar();
            Grid.SetColumn(btnCerrar, 1);
            pie.Children.Add(btnCerrar);
            return pie;
        }

        // ====================================================================
        //  Cargar lavadoras y mantenimientos
        // ====================================================================

        private async Task CargarLavadorasAsync()
        {
            try
            {
                List<LavadoraData> lavadoras = await Task.Run(
                    () => controladorLavadoras.ListarAsync(null, null, null));

                silenciarSeleccion = true;
                cmbLavadora.ItemsSource = lavadoras;

                LavadoraData elegida = null;
                foreach (LavadoraData lavadora in lavadoras)
                {
                    if (lavadora.Codigo == codigoInicial)
                    {
                        elegida = lavadora;
                        break;
                    }
                }
                if (elegida == null && lavadoras.Count > 0)
                {
                    elegida = lavadoras[0];
                }
                cmbLavadora.SelectedItem = elegida;
                silenciarSeleccion = false;

                if (elegida == null)
                {
                    lavadoraActual = null;
                    grilla.ItemsSource = null;
                    contador.Text = "0 mantenimiento(s)";
                    resumenLavadora.Text = "No hay lavadoras registradas: primero agregue una en el caso de uso Agregar.";
                    btnAgregar.Habilitado = false;
                    return;
                }

                btnAgregar.Habilitado = true;
                await CargarMantenimientosAsync();
            }
            catch (Exception ex)
            {
                aviso.Text = "Error al consultar";
                Aviso("No se pudieron cargar las lavadoras", ex.Message);
            }
        }

        private async Task CargarMantenimientosAsync()
        {
            int solicitud = ++versionCarga;
            LavadoraData lavadora = cmbLavadora.SelectedItem as LavadoraData;
            lavadoraActual = lavadora;

            if (lavadora == null)
            {
                grilla.ItemsSource = null;
                contador.Text = "0 mantenimiento(s)";
                return;
            }

            aviso.Text = "Consultando...";

            try
            {
                int codigo = lavadora.Codigo;
                string tecnico = txtTecnico.Texto.Trim();
                if (tecnico.Length == 0)
                {
                    tecnico = null;
                }
                bool? completado = Opcion(cmbCompletado.SelectedIndex);

                List<MantenimientoData> lista = await Task.Run(
                    () => controlador.ListarAsync(codigo, tecnico, completado));

                // Si el usuario cambió lavadora/filtros durante la petición, no mostrar
                // una respuesta anterior encima del resultado más reciente.
                if (solicitud != versionCarga || !IsLoaded || !IsVisible)
                {
                    return;
                }

                grilla.ItemsSource = lista;
                Anime.Contar(contador, lista.Count, "{0} mantenimiento(s)");
                aviso.Text = "Actualizado " + DateTime.Now.ToString("HH:mm:ss");

                resumenLavadora.Text = "Lavadora " + lavadora.Codigo + " - " + lavadora.Marca +
                                       "  |  precio final " + lavadora.PrecioFinalTexto +
                                       "  |  " + lavadora.CantidadMantenimientos + " mantenimiento(s) en total, " +
                                       lavadora.MantenimientosPendientes + " pendiente(s)";

                PrepararNuevo(); // Sin fila seleccionada se empieza un mantenimiento nuevo
            }
            catch (Exception ex)
            {
                if (solicitud != versionCarga || !IsLoaded || !IsVisible)
                {
                    return;
                }
                aviso.Text = "Error al consultar";
                Aviso("No se pudieron cargar los mantenimientos", ex.Message);
            }
        }

        private static bool? Opcion(int indice)
        {
            if (indice == 1)
            {
                return false; // Pendientes
            }
            if (indice == 2)
            {
                return true; // Completados
            }
            return null;   // Todos
        }

        // ====================================================================
        //  Seleccion y formulario
        // ====================================================================

        private void AlCambiarLavadora(object remitente, SelectionChangedEventArgs argumentos)
        {
            if (silenciarSeleccion)
            {
                return;
            }
            // La carga se hace en el hilo de la interfaz, por eso se puede llamar asi.
            CargarMantenimientosDesdeEvento();
        }

        private async void CargarMantenimientosDesdeEvento()
        {
            await CargarMantenimientosAsync();
        }

        private void AlSeleccionarMantenimiento(object remitente, SelectionChangedEventArgs argumentos)
        {
            MantenimientoData mantenimiento = grilla.SelectedItem as MantenimientoData;
            seleccionado = mantenimiento;

            if (mantenimiento == null)
            {
                campos.Limpiar();
                campos.PermitirEdicion(false);
                tituloFormulario.Text = modo == "agregar" ? "NUEVO MANTENIMIENTO" : "SELECCIONE UN MANTENIMIENTO";
                btnActualizar.Habilitado = false;
                btnEliminar.Habilitado = false;
                return;
            }

            campos.Mostrar(mantenimiento);
            tituloFormulario.Text = modo == "consultar" ? "CONSULTA  id " + mantenimiento.Id :
                (modo == "eliminar" ? "ELIMINAR  id " + mantenimiento.Id :
                (modo == "actualizar" ? "ACTUALIZAR  id " + mantenimiento.Id : "MANTENIMIENTO  id " + mantenimiento.Id));
            btnActualizar.Habilitado = modo == "actualizar";
            btnEliminar.Habilitado = modo == "eliminar";
            campos.PermitirEdicion(modo == "actualizar");
        }

        private void PrepararNuevo()
        {
            seleccionado = null;
            tituloFormulario.Text = modo == "agregar" ? "NUEVO MANTENIMIENTO" : "SELECCIONE UN MANTENIMIENTO";
            btnActualizar.Habilitado = false;
            btnEliminar.Habilitado = false;
            campos.Limpiar();
        }

        // ====================================================================
        //  Agregar (con vista previa del Builder)
        // ====================================================================

        private async void AlAgregar(object remitente, RoutedEventArgs argumentos)
        {
            if (lavadoraActual == null)
            {
                Aviso("Falta la lavadora", "Escoja primero la lavadora a la que le va a agregar el mantenimiento.");
                return;
            }

            try
            {
                MantenimientoInput input = campos.LeerInput();

                // BUILDER: se arma el mantenimiento "de papel" para mostrar el resumen
                // antes de mandarlo. No cuesta ninguna peticion al servidor.
                MantenimientoData previa = controlador.VistaPrevia(input);
                string resumen = "Se agregara a la lavadora " + lavadoraActual.Codigo + ":" +
                                 Environment.NewLine + Environment.NewLine +
                                 "Descripcion: " + previa.Descripcion + Environment.NewLine +
                                 "Tecnico: " + previa.Tecnico + Environment.NewLine +
                                 "Costo: " + previa.CostoTexto + Environment.NewLine +
                                 "Fecha: " + previa.FechaTexto + Environment.NewLine +
                                 "Estado: " + previa.EstadoTexto;

                if (!Confirmar("VISTA PREVIA DEL MANTENIMIENTO", resumen, "Agregar", false))
                {
                    return;
                }

                int codigo = lavadoraActual.Codigo;
                btnAgregar.Habilitado = false;
                guardando = true;
                MantenimientoData creado = await Task.Run(() => controlador.AgregarAsync(codigo, input));
                guardando = false;

                Sonidos.Lavadora();
                Celebrar();
                Anime.Pulso(Marco);
                Informar("Mantenimiento agregado",
                    "Se agrego el mantenimiento id " + creado.Id + " a la lavadora " + codigo + ".");

                PrepararNuevo();
                await CargarMantenimientosAsync();
            }
            catch (Exception ex)
            {
                guardando = false;
                Anime.Sacudir(Marco);
                Aviso("No se pudo agregar el mantenimiento", ex.Message);
            }
            finally
            {
                btnAgregar.Habilitado = lavadoraActual != null;
            }
        }

        // ====================================================================
        //  Actualizar
        // ====================================================================

        private async void AlActualizar(object remitente, RoutedEventArgs argumentos)
        {
            if (lavadoraActual == null || seleccionado == null)
            {
                Aviso("Falta escoger un mantenimiento", "Haga clic en una fila de la grilla para actualizarla.");
                return;
            }

            try
            {
                MantenimientoUpdateInput input = campos.LeerUpdateInput();
                int codigo = lavadoraActual.Codigo;
                int id = seleccionado.Id;

                btnActualizar.Habilitado = false;
                guardando = true;
                MantenimientoData actualizado = await Task.Run(() => controlador.ActualizarAsync(codigo, id, input));
                guardando = false;

                Sonidos.Click();
                Anime.Pulso(Marco);
                Informar("Mantenimiento actualizado",
                    "Se actualizo el mantenimiento id " + actualizado.Id + " de la lavadora " + codigo + ".");

                await CargarMantenimientosAsync();
            }
            catch (Exception ex)
            {
                guardando = false;
                Anime.Sacudir(Marco);
                Aviso("No se pudo actualizar el mantenimiento", ex.Message);
            }
            finally
            {
                btnActualizar.Habilitado = seleccionado != null;
            }
        }

        // ====================================================================
        //  Eliminar (con confirmacion)
        // ====================================================================

        private async void AlEliminar(object remitente, RoutedEventArgs argumentos)
        {
            if (lavadoraActual == null || seleccionado == null)
            {
                Aviso("Falta escoger un mantenimiento", "Haga clic en una fila de la grilla para eliminarla.");
                return;
            }

            int codigo = lavadoraActual.Codigo;
            int id = seleccionado.Id;
            string mensaje = "Se eliminara el mantenimiento id " + id + " (" + seleccionado.Descripcion + ")" +
                             " de la lavadora " + codigo + "." + Environment.NewLine + Environment.NewLine +
                             "Esta seguro?";

            if (!Confirmar("CONFIRMAR ELIMINACION", mensaje, "Si, eliminar", true))
            {
                return;
            }

            try
            {
                btnEliminar.Habilitado = false;
                guardando = true;
                await Task.Run(() => controlador.EliminarAsync(codigo, id));
                guardando = false;

                Sonidos.Click();
                PrepararNuevo();
                Informar("Mantenimiento eliminado",
                    "Se elimino el mantenimiento id " + id + " de la lavadora " + codigo + ".");

                await CargarMantenimientosAsync();
            }
            catch (Exception ex)
            {
                guardando = false;
                Anime.Sacudir(Marco);
                Aviso("No se pudo eliminar el mantenimiento", ex.Message);
                btnEliminar.Habilitado = seleccionado != null;
            }
        }

        // ====================================================================
        //  Observer
        // ====================================================================

        private void AlCambiarDatos()
        {
            if (guardando)
            {
                return; // Nuestro propio cambio: ya se refresco en el metodo que guardo
            }
            Dispatcher.Invoke(new Action(RecargarPorAviso));
        }

        private async void RecargarPorAviso()
        {
            if (lavadoraActual == null)
            {
                return;
            }
            aviso.Text = "Cambio detectado: recargando...";
            await CargarMantenimientosAsync();
        }

        private void AlCerrar(object remitente, EventArgs argumentos)
        {
            NotificadorDatos.DatosCambiaron -= AlCambiarDatos;
        }

        private async void AlPresionarTecla(object remitente, KeyEventArgs argumentos)
        {
            if (argumentos.Key == Key.Enter)
            {
                await CargarMantenimientosAsync();
            }
        }
    }
}
