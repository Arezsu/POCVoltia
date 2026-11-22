using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ClienteLavadora.controlador;
using ClienteLavadora.modelo;
using ClienteLavadora.servicios;
using ClienteLavadora.util;

namespace ClienteLavadora.vista
{
    /// <summary>Una ventana por caso de uso de Técnico (Clase C).</summary>
    public abstract class VentanaTecnicoCaso : VentanaMarco
    {
        private readonly string caso;
        private readonly ControladorTecnicos controlador = new ControladorTecnicos(new ServicioTecnicos());
        private readonly TextBox codigo = Caja(), documento = Caja(), nombre = Caja(), especialidad = Caja(), telefono = Caja();
        private readonly DatePicker fecha = new DatePicker { SelectedDate = DateTime.Today };
        private readonly CheckBox activo = new CheckBox { Content = "Técnico activo", IsChecked = true, Foreground = Tema.Texto };
        private readonly TextBox buscarCodigo = Caja(), filtroTexto = Caja(), filtroEspecialidad = Caja();
        private readonly ComboBox filtroActivo = new ComboBox { Width = 130, Margin = new Thickness(4) };
        private readonly DataGrid tabla = new DataGrid { AutoGenerateColumns = false, IsReadOnly = true, CanUserAddRows = false, Height = 300 };
        private readonly StackPanel formulario = new StackPanel { Margin = new Thickness(10) };
        private readonly TextBlock estado = new TextBlock { Margin = new Thickness(8), Foreground = Tema.TextoSecundario, TextWrapping = TextWrapping.Wrap };
        private TecnicoData seleccionado;

        protected VentanaTecnicoCaso(string caso, string titulo) : base(titulo, 1000, 680, false, 0)
        {
            this.caso = caso;
            Contenido = CrearContenido();
            Loaded += async (_, __) => { if (caso == "listar") await Listar(); };
        }

        private FrameworkElement CrearContenido()
        {
            StackPanel raiz = new StackPanel { Margin = new Thickness(16) };
            if (caso == "listar")
            {
                StackPanel filtros = new StackPanel { Orientation = Orientation.Horizontal };
                filtros.Children.Add(Etiquetado("Nombre o documento", filtroTexto));
                filtros.Children.Add(Etiquetado("Especialidad", filtroEspecialidad));
                filtroActivo.Items.Add("Todos"); filtroActivo.Items.Add("Activos"); filtroActivo.Items.Add("Inactivos"); filtroActivo.SelectedIndex = 0;
                filtros.Children.Add(Etiquetado("Estado", filtroActivo));
                filtros.Children.Add(Boton("Filtrar", async (_, __) => await Listar())); raiz.Children.Add(filtros);
                tabla.Columns.Add(Columna("Código", "Codigo", 0.6)); tabla.Columns.Add(Columna("Documento", "Documento", 1));
                tabla.Columns.Add(Columna("Nombre", "Nombre", 1.3)); tabla.Columns.Add(Columna("Especialidad", "Especialidad", 1.2));
                tabla.Columns.Add(Columna("Teléfono", "Telefono", 1)); tabla.Columns.Add(Columna("Ingreso", "FechaIngreso", 1));
                tabla.Columns.Add(Columna("Código lavadora (A)", "CodigoLavadora", 0.9)); tabla.Columns.Add(Columna("Marca lavadora (A)", "MarcaLavadora", 1));
                tabla.Columns.Add(new DataGridCheckBoxColumn { Header = "Activo", Binding = new System.Windows.Data.Binding("Activo") });
                raiz.Children.Add(tabla);
            }
            else
            {
                if (caso != "agregar")
                {
                    StackPanel busqueda = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 12) };
                    busqueda.Children.Add(Etiquetado("Código del técnico", buscarCodigo));
                    busqueda.Children.Add(Boton("Buscar", Buscar)); raiz.Children.Add(busqueda);
                }
                formulario.Children.Add(Etiquetado("Código", codigo)); formulario.Children.Add(Etiquetado("Documento", documento));
                formulario.Children.Add(Etiquetado("Nombre", nombre)); formulario.Children.Add(Etiquetado("Especialidad", especialidad));
                formulario.Children.Add(Etiquetado("Teléfono", telefono)); formulario.Children.Add(Etiquetado("Fecha de ingreso", fecha)); formulario.Children.Add(activo);
                raiz.Children.Add(formulario);
                formulario.Visibility = caso == "agregar" ? Visibility.Visible : Visibility.Collapsed;
                if (caso == "agregar") codigo.IsReadOnly = false;
            }
            raiz.Children.Add(estado);
            StackPanel acciones = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            if (caso == "agregar") acciones.Children.Add(Boton("Registrar", Guardar));
            if (caso == "actualizar") acciones.Children.Add(Boton("Guardar cambios", Guardar));
            if (caso == "eliminar") acciones.Children.Add(Boton("Eliminar técnico", Eliminar));
            acciones.Children.Add(Boton("Cerrar", (_, __) => Cerrar())); raiz.Children.Add(acciones);
            return raiz;
        }

        private async void Buscar(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(buscarCodigo.Text, out int cod) || cod < 1) { Aviso("Código inválido", "Escriba un código positivo."); return; }
            try
            {
                seleccionado = await controlador.BuscarAsync(cod);
                if (seleccionado == null) { formulario.Visibility = Visibility.Collapsed; estado.Text = "No existe un técnico con ese código."; return; }
                Mostrar(seleccionado); estado.Text = "Técnico encontrado. Revise sus datos antes de continuar.";
                formulario.Visibility = Visibility.Visible;
                bool editable = caso == "actualizar";
                codigo.IsReadOnly = true; documento.IsReadOnly = !editable; nombre.IsReadOnly = !editable;
                especialidad.IsReadOnly = !editable; telefono.IsReadOnly = !editable; fecha.IsEnabled = editable; activo.IsEnabled = editable;
            }
            catch (Exception ex) { Aviso("No se pudo consultar el técnico", ex.Message); }
        }

        private async void Guardar(object sender, RoutedEventArgs e)
        {
            try
            {
                TecnicoInput input = Leer();
                TecnicoData guardado = caso == "agregar" ? await controlador.AgregarAsync(input) : await controlador.ActualizarAsync(seleccionado.Codigo, input);
                if (guardado == null) throw new Exception("El servidor no confirmó el cambio.");
                Celebrar(); Informar(caso == "agregar" ? "Técnico registrado" : "Técnico actualizado", guardado.Nombre + " (código " + guardado.Codigo + ")");
                Cerrar();
            }
            catch (Exception ex) { Aviso("No se pudo guardar el técnico", ex.Message); }
        }

        private async void Eliminar(object sender, RoutedEventArgs e)
        {
            if (seleccionado == null) { Aviso("Busque primero", "Consulte el técnico antes de eliminarlo."); return; }
            if (!Confirmar("Confirmar eliminación", "Se eliminará " + seleccionado.Nombre + " (" + seleccionado.Codigo + ").", "Eliminar", true)) return;
            try { await controlador.EliminarAsync(seleccionado.Codigo); Celebrar(); Informar("Técnico eliminado", seleccionado.Nombre); Cerrar(); }
            catch (Exception ex) { Aviso("No se pudo eliminar el técnico", ex.Message); }
        }

        private async Task Listar()
        {
            try
            {
                bool? estadoActivo = filtroActivo.SelectedIndex == 1 ? true : filtroActivo.SelectedIndex == 2 ? false : (bool?)null;
                var datos = await controlador.ListarAsync(filtroTexto.Text, filtroEspecialidad.Text, estadoActivo);
                tabla.ItemsSource = datos; estado.Text = datos.Count + " técnico(s)";
            }
            catch (Exception ex) { Aviso("No se pudieron listar técnicos", ex.Message); }
        }

        private TecnicoInput Leer()
        {
            if (!int.TryParse(codigo.Text, out int cod) || cod < 1) throw new ArgumentException("El código debe ser positivo.");
            if (string.IsNullOrWhiteSpace(documento.Text) || string.IsNullOrWhiteSpace(nombre.Text) || string.IsNullOrWhiteSpace(especialidad.Text) || string.IsNullOrWhiteSpace(telefono.Text) || fecha.SelectedDate == null)
                throw new ArgumentException("Complete documento, nombre, especialidad, teléfono y fecha de ingreso.");
            return new TecnicoInput { Codigo = cod, Documento = documento.Text.Trim(), Nombre = nombre.Text.Trim(), Especialidad = especialidad.Text.Trim(), Telefono = telefono.Text.Trim(), FechaIngreso = fecha.SelectedDate.Value.ToString("yyyy-MM-dd'T'HH:mm:ss"), Activo = activo.IsChecked == true };
        }
        private void Mostrar(TecnicoData t)
        {
            codigo.Text = t.Codigo.ToString(); documento.Text = t.Documento; nombre.Text = t.Nombre; especialidad.Text = t.Especialidad;
            telefono.Text = t.Telefono; if (DateTime.TryParse(t.FechaIngreso, out DateTime d)) fecha.SelectedDate = d; activo.IsChecked = t.Activo;
        }
        private static TextBox Caja() => new TextBox { MinWidth = 190, Margin = new Thickness(4), Padding = new Thickness(6) };
        private static FrameworkElement Etiquetado(string etiqueta, Control control)
        {
            StackPanel fila = new StackPanel { Margin = new Thickness(4, 6, 10, 6) }; fila.Children.Add(new TextBlock { Text = etiqueta, Foreground = Tema.Texto }); fila.Children.Add(control); return fila;
        }
        private static Button Boton(string texto, RoutedEventHandler accion)
        {
            Button boton = new Button { Content = texto, Margin = new Thickness(5), Padding = new Thickness(12, 7, 12, 7), MinWidth = 95 }; boton.Click += accion; return boton;
        }
        private static DataGridTextColumn Columna(string titulo, string campo, double ancho) => new DataGridTextColumn { Header = titulo, Binding = new System.Windows.Data.Binding(campo), Width = new DataGridLength(ancho, DataGridLengthUnitType.Star) };
    }

    public sealed class VentanaListarTecnicos : VentanaTecnicoCaso { public VentanaListarTecnicos() : base("listar", "LISTAR TÉCNICOS") { } }
    public sealed class VentanaAgregarTecnico : VentanaTecnicoCaso { public VentanaAgregarTecnico() : base("agregar", "AGREGAR TÉCNICO") { } }
    public sealed class VentanaConsultarTecnico : VentanaTecnicoCaso { public VentanaConsultarTecnico() : base("consultar", "CONSULTAR TÉCNICO") { } }
    public sealed class VentanaActualizarTecnico : VentanaTecnicoCaso { public VentanaActualizarTecnico() : base("actualizar", "ACTUALIZAR TÉCNICO") { } }
    public sealed class VentanaEliminarTecnico : VentanaTecnicoCaso { public VentanaEliminarTecnico() : base("eliminar", "ELIMINAR TÉCNICO") { } }
}
