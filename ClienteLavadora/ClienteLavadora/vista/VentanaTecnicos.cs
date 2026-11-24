using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ClienteLavadora.controlador;
using ClienteLavadora.modelo;
using ClienteLavadora.servicios;
using ClienteLavadora.util;
using ClienteLavadora.vista.controles;

namespace ClienteLavadora.vista
{
    /// <summary>Una ventana Voltia por caso de uso de Técnico (Clase C).</summary>
    public abstract class VentanaTecnicoCaso : VentanaMarco
    {
        private readonly string caso;
        private readonly ControladorTecnicos controlador = new ControladorTecnicos(new ServicioTecnicos());

        private readonly CampoTexto codigo = new CampoTexto();
        private readonly CampoTexto documento = new CampoTexto();
        private readonly CampoTexto nombre = new CampoTexto();
        private readonly CampoTexto especialidad = new CampoTexto();
        private readonly CampoTexto telefono = new CampoTexto();
        private readonly CampoFecha fecha = new CampoFecha();
        private readonly CheckBox activo = Ui.Check("Técnico activo");
        private readonly CampoTexto buscarCodigo = new CampoTexto();
        private readonly CampoTexto filtroTexto = new CampoTexto();
        private readonly CampoTexto filtroEspecialidad = new CampoTexto();
        private readonly ComboBox filtroActivo = Ui.Combo("Todos", "Activos", "Inactivos");
        private readonly DataGrid tabla = Ui.Grilla();
        private readonly TextBlock estado = new TextBlock();
        private readonly StackPanel formulario = new StackPanel();
        private TecnicoData seleccionado;

        protected VentanaTecnicoCaso(string caso, string titulo)
            : base(titulo, caso == "listar" ? 1240 : 900, caso == "listar" ? 790 : 760, true, 96)
        {
            this.caso = caso;
            MinWidth = caso == "listar" ? 980 : 760;
            MinHeight = 620;
            codigo.SinNegativos = true;
            buscarCodigo.SinNegativos = true;
            codigo.Fuente = Tema.Numeros;
            buscarCodigo.Fuente = Tema.Numeros;
            codigo.Ayuda = "Código único del técnico";
            buscarCodigo.Ayuda = "Escriba el código y presione Enter o Buscar";
            filtroTexto.Ayuda = "Buscar por nombre o documento";
            filtroEspecialidad.Ayuda = "Buscar por especialidad";

            Contenido = CrearContenido();
            Loaded += async (_, __) => { if (caso == "listar") await Listar(); };
        }

        private UIElement CrearContenido()
        {
            Grid raiz = new Grid();
            raiz.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            raiz.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            raiz.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            FrameworkElement encabezado = caso == "listar" ? CrearFiltros() : CrearBusqueda();
            Grid.SetRow(encabezado, 0);
            raiz.Children.Add(encabezado);

            FrameworkElement centro = caso == "listar" ? CrearListado() : CrearFormulario();
            Grid.SetRow(centro, 1);
            raiz.Children.Add(centro);

            FrameworkElement pie = caso == "listar" ? CrearPieListado() : CrearPieFormulario();
            Grid.SetRow(pie, 2);
            raiz.Children.Add(pie);
            return raiz;
        }

        private FrameworkElement CrearFiltros()
        {
            Grid rejilla = new Grid();
            rejilla.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.25, GridUnitType.Star) });
            rejilla.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) });
            rejilla.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            rejilla.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) });
            rejilla.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.8, GridUnitType.Star) });
            rejilla.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) });
            rejilla.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            AgregarCelda(rejilla, 0, "Nombre o documento", filtroTexto);
            AgregarCelda(rejilla, 2, "Especialidad", filtroEspecialidad);
            AgregarCelda(rejilla, 4, "Estado", filtroActivo);

            BotonVoltia filtrar = BotonVoltia.Principal("Filtrar");
            filtrar.Width = 110;
            filtrar.VerticalAlignment = VerticalAlignment.Bottom;
            filtrar.Click += async (_, __) => await Listar();
            Grid.SetColumn(filtrar, 6);
            rejilla.Children.Add(filtrar);

            Border panel = Ui.Cristal(rejilla, new Thickness(12, 10, 12, 10));
            panel.Margin = new Thickness(0, 0, 0, 10);
            return panel;
        }

        private FrameworkElement CrearListado()
        {
            ConfigurarColumna("Código", "Codigo", 0.7, 80);
            ConfigurarColumna("Documento", "Documento", 1.1, 140);
            ConfigurarColumna("Nombre", "Nombre", 1.1, 135);
            ConfigurarColumna("Especialidad", "Especialidad", 1.2, 140);
            ConfigurarColumna("Teléfono", "Telefono", 1.0, 125);
            ConfigurarColumna("Ingreso", "FechaIngresoTexto", 0.9, 115);
            ConfigurarColumna("Código lavadora", "CodigoLavadora", 0.9, 120);
            ConfigurarColumna("Marca lavadora", "MarcaLavadora", 1.0, 130);
            DataGridCheckBoxColumn estadoColumna = new DataGridCheckBoxColumn
            {
                Header = "Activo",
                Binding = new System.Windows.Data.Binding("Activo"),
                Width = new DataGridLength(0.65, DataGridLengthUnitType.Star),
                MinWidth = 78
            };
            tabla.Columns.Add(estadoColumna);
            tabla.MinWidth = 1060;
            tabla.LoadingRow += (_, e) => Anime.Entrada(e.Row, Math.Min(e.Row.GetIndex(), 12) * 35);
            return tabla;
        }

        private FrameworkElement CrearBusqueda()
        {
            Grid rejilla = new Grid();
            rejilla.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            rejilla.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) });
            rejilla.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            buscarCodigo.TeclaPresionada += async (_, e) => { if (e.Key == System.Windows.Input.Key.Enter) await Buscar(); };
            StackPanel campo = Ui.Celda("Código del técnico", buscarCodigo);
            Grid.SetColumn(campo, 0);
            rejilla.Children.Add(campo);

            BotonVoltia botonBuscar = BotonVoltia.Principal("Buscar");
            botonBuscar.Width = 110;
            botonBuscar.VerticalAlignment = VerticalAlignment.Bottom;
            botonBuscar.Click += async (_, __) => await Buscar();
            Grid.SetColumn(botonBuscar, 2);
            rejilla.Children.Add(botonBuscar);

            Border panel = Ui.Cristal(rejilla, new Thickness(12, 10, 12, 10));
            panel.Margin = new Thickness(0, 0, 0, 10);
            return panel;
        }

        private FrameworkElement CrearFormulario()
        {
            Grid campos = new Grid();
            campos.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            campos.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            campos.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            campos.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            AgregarFila(campos, 0, "Código", codigo, "Documento", documento);
            AgregarFila(campos, 1, "Nombre completo", nombre, "Especialidad", especialidad);
            AgregarFila(campos, 2, "Teléfono", telefono, "Fecha de ingreso", fecha);
            StackPanel casilla = Ui.Celda(null, activo);
            casilla.Margin = new Thickness(0, 8, 0, 0);
            Grid.SetRow(casilla, 3);
            campos.Children.Add(casilla);

            estado.TextWrapping = TextWrapping.Wrap;
            estado.Foreground = Tema.TextoSecundario;
            estado.Margin = new Thickness(2, 10, 2, 0);
            estado.MinHeight = 22;

            formulario.Children.Clear();
            formulario.Children.Add(Ui.Cristal(campos, new Thickness(16, 14, 16, 12)));
            formulario.Children.Add(estado);
            formulario.Visibility = caso == "agregar" ? Visibility.Visible : Visibility.Collapsed;
            AplicarSoloLectura(caso != "agregar");
            if (caso == "agregar")
            {
                activo.IsChecked = true;
                codigo.SoloLectura = false;
                fecha.SelectedDate = DateTime.Today;
            }
            return formulario;
        }

        private FrameworkElement CrearPieListado()
        {
            Grid pie = new Grid { Margin = new Thickness(0, 10, 0, 0) };
            pie.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            pie.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            estado.Text = "Consultando técnicos...";
            estado.Foreground = Tema.TextoSecundario;
            estado.VerticalAlignment = VerticalAlignment.Center;
            pie.Children.Add(estado);
            BotonVoltia cerrar = BotonVoltia.Secundario("Cerrar");
            cerrar.Width = 110;
            cerrar.Click += (_, __) => Cerrar();
            Grid.SetColumn(cerrar, 1);
            pie.Children.Add(cerrar);
            return pie;
        }

        private FrameworkElement CrearPieFormulario()
        {
            StackPanel acciones = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 12, 0, 0)
            };
            if (caso == "agregar") acciones.Children.Add(Boton("Registrar técnico", Guardar, true));
            if (caso == "actualizar") acciones.Children.Add(Boton("Guardar cambios", Guardar, true));
            if (caso == "eliminar") acciones.Children.Add(Boton("Eliminar técnico", Eliminar, false, true));
            acciones.Children.Add(Boton("Cerrar", (_, __) => Cerrar(), false));
            return acciones;
        }

        private async Task Buscar()
        {
            if (!int.TryParse(buscarCodigo.Texto, out int cod) || cod < 1)
            {
                estado.Text = "El código debe ser un entero mayor que cero.";
                estado.Foreground = Tema.Error;
                return;
            }
            try
            {
                estado.Text = "Buscando técnico...";
                seleccionado = await controlador.BuscarAsync(cod);
                if (seleccionado == null)
                {
                    formulario.Visibility = Visibility.Collapsed;
                    estado.Text = "No existe un técnico registrado con el código " + cod + ".";
                    estado.Foreground = Tema.Error;
                    return;
                }
                Mostrar(seleccionado);
                bool editable = caso == "actualizar";
                AplicarSoloLectura(!editable);
                codigo.SoloLectura = true;
                formulario.Visibility = Visibility.Visible;
                estado.Text = "Técnico encontrado. Revise todos sus datos antes de continuar.";
                estado.Foreground = Tema.Cian;
            }
            catch (Exception ex)
            {
                estado.Text = MensajeTecnico(ex);
                estado.Foreground = Tema.Error;
            }
        }

        private async void Guardar(object sender, RoutedEventArgs e)
        {
            try
            {
                TecnicoInput input = Leer();
                TecnicoData guardado = caso == "agregar"
                    ? await controlador.AgregarAsync(input)
                    : await controlador.ActualizarAsync(seleccionado.Codigo, input);
                if (guardado == null) throw new InvalidOperationException("El servidor no confirmó el cambio.");
                Celebrar();
                Informar(caso == "agregar" ? "Técnico registrado" : "Técnico actualizado",
                    guardado.Nombre + " (código " + guardado.Codigo + ")");
                Cerrar();
            }
            catch (Exception ex)
            {
                estado.Text = MensajeTecnico(ex);
                estado.Foreground = Tema.Error;
            }
        }

        private async void Eliminar(object sender, RoutedEventArgs e)
        {
            if (seleccionado == null)
            {
                estado.Text = "Busque primero el técnico que desea eliminar.";
                estado.Foreground = Tema.Error;
                return;
            }
            string detalle = seleccionado.Nombre + " · Documento " + seleccionado.Documento +
                " · " + seleccionado.Especialidad + " · Teléfono " + seleccionado.Telefono +
                " · Ingreso " + seleccionado.FechaIngresoTexto +
                (seleccionado.Activo ? " · Activo" : " · Inactivo");
            if (!Confirmar("Confirmar eliminación", "Se eliminará este técnico:\n\n" + detalle,
                    "Eliminar", true)) return;
            try
            {
                await controlador.EliminarAsync(seleccionado.Codigo);
                Celebrar();
                Informar("Técnico eliminado", seleccionado.Nombre + " (código " + seleccionado.Codigo + ")");
                Cerrar();
            }
            catch (Exception ex)
            {
                estado.Text = MensajeTecnico(ex);
                estado.Foreground = Tema.Error;
            }
        }

        private async Task Listar()
        {
            try
            {
                estado.Text = "Consultando técnicos...";
                estado.Foreground = Tema.TextoSecundario;
                bool? estadoActivo = filtroActivo.SelectedIndex == 1 ? true : filtroActivo.SelectedIndex == 2 ? false : (bool?)null;
                List<TecnicoData> datos = await controlador.ListarAsync(filtroTexto.Texto, filtroEspecialidad.Texto, estadoActivo);
                tabla.ItemsSource = datos;
                estado.Text = datos.Count + (datos.Count == 1 ? " técnico" : " técnicos");
                estado.Foreground = Tema.Cian;
            }
            catch (Exception ex)
            {
                estado.Text = MensajeTecnico(ex);
                estado.Foreground = Tema.Error;
            }
        }

        private TecnicoInput Leer()
        {
            if (!int.TryParse(codigo.Texto, out int cod) || cod < 1)
                throw new ArgumentException("El código del técnico debe ser un entero mayor que cero.");
            if (string.IsNullOrWhiteSpace(documento.Texto)) throw new ArgumentException("El documento es obligatorio.");
            if (string.IsNullOrWhiteSpace(nombre.Texto)) throw new ArgumentException("El nombre es obligatorio.");
            if (string.IsNullOrWhiteSpace(especialidad.Texto)) throw new ArgumentException("La especialidad es obligatoria.");
            if (string.IsNullOrWhiteSpace(telefono.Texto)) throw new ArgumentException("El teléfono es obligatorio.");
            if (fecha.SelectedDate == null) throw new ArgumentException("Seleccione una fecha de ingreso válida.");
            return new TecnicoInput
            {
                Codigo = cod,
                Documento = documento.Texto.Trim(),
                Nombre = nombre.Texto.Trim(),
                Especialidad = especialidad.Texto.Trim(),
                Telefono = telefono.Texto.Trim(),
                FechaIngreso = fecha.Texto,
                Activo = activo.IsChecked == true
            };
        }

        private void Mostrar(TecnicoData tecnico)
        {
            codigo.Texto = tecnico.Codigo.ToString();
            documento.Texto = tecnico.Documento ?? "";
            nombre.Texto = tecnico.Nombre ?? "";
            especialidad.Texto = tecnico.Especialidad ?? "";
            telefono.Texto = tecnico.Telefono ?? "";
            fecha.Texto = tecnico.FechaIngreso ?? "";
            activo.IsChecked = tecnico.Activo;
        }

        private void AplicarSoloLectura(bool soloLectura)
        {
            codigo.SoloLectura = soloLectura;
            documento.SoloLectura = soloLectura;
            nombre.SoloLectura = soloLectura;
            especialidad.SoloLectura = soloLectura;
            telefono.SoloLectura = soloLectura;
            fecha.SoloLectura = soloLectura;
            activo.IsEnabled = !soloLectura;
        }

        private void AgregarFila(Grid rejilla, int indice, string etiqueta1, UIElement control1,
            string etiqueta2, UIElement control2)
        {
            Grid fila = Ui.Fila(Ui.Celda(etiqueta1, control1), Ui.Celda(etiqueta2, control2));
            Grid.SetRow(fila, indice);
            rejilla.Children.Add(fila);
        }

        private static void AgregarCelda(Grid rejilla, int columna, string etiqueta, UIElement control)
        {
            StackPanel celda = Ui.Celda(etiqueta, control);
            Grid.SetColumn(celda, columna);
            rejilla.Children.Add(celda);
        }

        private void ConfigurarColumna(string titulo, string propiedad, double peso, double minimo)
        {
            DataGridTextColumn columna = Ui.Columna(titulo, propiedad, peso, propiedad == "Codigo");
            columna.MinWidth = minimo;
            tabla.Columns.Add(columna);
        }

        private BotonVoltia Boton(string texto, RoutedEventHandler accion, bool principal, bool peligro = false)
        {
            BotonVoltia boton = peligro ? BotonVoltia.Peligro(texto) : principal
                ? BotonVoltia.Principal(texto) : BotonVoltia.Secundario(texto);
            boton.MinWidth = 112;
            boton.Margin = new Thickness(0, 0, 8, 0);
            boton.Click += accion;
            return boton;
        }

        private static string MensajeTecnico(Exception ex)
        {
            string mensaje = ex == null ? null : ex.Message;
            return string.IsNullOrWhiteSpace(mensaje) ? "No fue posible completar la operación. Revise la conexión con el servidor." : mensaje;
        }
    }

    public sealed class VentanaListarTecnicos : VentanaTecnicoCaso { public VentanaListarTecnicos() : base("listar", "LISTAR TÉCNICOS") { } }
    public sealed class VentanaAgregarTecnico : VentanaTecnicoCaso { public VentanaAgregarTecnico() : base("agregar", "AGREGAR TÉCNICO") { } }
    public sealed class VentanaConsultarTecnico : VentanaTecnicoCaso { public VentanaConsultarTecnico() : base("consultar", "CONSULTAR TÉCNICO") { } }
    public sealed class VentanaActualizarTecnico : VentanaTecnicoCaso { public VentanaActualizarTecnico() : base("actualizar", "ACTUALIZAR TÉCNICO") { } }
    public sealed class VentanaEliminarTecnico : VentanaTecnicoCaso { public VentanaEliminarTecnico() : base("eliminar", "ELIMINAR TÉCNICO") { } }
}
