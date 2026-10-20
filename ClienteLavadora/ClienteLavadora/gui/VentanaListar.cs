using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using ClienteLavadora.model;
using ClienteLavadora.servicios;
using ClienteLavadora.util;

namespace ClienteLavadora.gui
{
    /// <summary>
    /// Caso de uso LISTAR: muestra todos los objetos (lavadoras o programas) en una grilla.
    /// Se puede filtrar por dos datos y el filtro lo hace el SERVIDOR: el cliente solo manda
    /// los parametros.
    ///
    /// Patron OBSERVER: si en otra ventana se adiciona, actualiza o elimina algo, esta grilla
    /// se recarga sola. La ventana se suscribe al Notificador y se quita al cerrarse.
    /// </summary>
    public class VentanaListar : VentanaMarco
    {
        private readonly TipoObjeto tipo;
        private readonly IServicioCrud servicio;

        private readonly CampoTexto txtFiltro = new CampoTexto();
        private readonly ComboBox cmbOpcion = Ui.Combo();
        private readonly DataGrid grilla = new DataGrid();
        private readonly TextBlock contador = new TextBlock();
        private readonly Grid panelFiltros = new Grid();
        private readonly Grid panelPie = new Grid();

        public VentanaListar(TipoObjeto tipo) : base("LISTAR " + Catalogo.Titulo(tipo), 1000, 700, true, 100)
        {
            this.tipo = tipo;
            servicio = FabricaServicios.Obtener(tipo);

            ArmarFiltros();
            ArmarGrilla();
            ArmarPie();

            Grid cuerpo = new Grid();
            cuerpo.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            cuerpo.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            cuerpo.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            Grid.SetRow(panelFiltros, 0);
            Grid.SetRow(grilla, 1);
            Grid.SetRow(panelPie, 2);
            cuerpo.Children.Add(panelFiltros);
            cuerpo.Children.Add(grilla);
            cuerpo.Children.Add(panelPie);
            Contenido = cuerpo;

            // Observer: nos suscribimos para enterarnos de los cambios y nos quitamos al cerrar
            Notificador.DatosCambiaron += RecargarPorAviso;
            Closed += (s, e) => Notificador.DatosCambiaron -= RecargarPorAviso;

            // Cada fila de la tabla entra con un pequeno retraso respecto a la anterior
            grilla.LoadingRow += (s, e) => Anime.Entrada(e.Row, Math.Min(e.Row.GetIndex(), 12) * 45);

            Loaded += async (s, e) => // Espera sin congelar
            {
                Anime.Escalonado(new UIElement[] { panelFiltros, grilla, panelPie }, 90, 350);
                await CargarAsync();
            };
        }

        // ================= Armado de la pantalla =================

        private void ArmarFiltros()
        {
            panelFiltros.Margin = new Thickness(0, 0, 0, 10);
            panelFiltros.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            panelFiltros.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(16) });
            panelFiltros.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            panelFiltros.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(16) });
            panelFiltros.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            panelFiltros.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // El segundo filtro cambia: inverter en la lavadora y agua caliente en el programa
            cmbOpcion.Items.Add("Todos");
            string etiqueta2;
            if (tipo == TipoObjeto.Lavadora)
            {
                etiqueta2 = "Filtro 2: Tecnología Inverter";
                cmbOpcion.Items.Add("Con inverter");
                cmbOpcion.Items.Add("Sin inverter");
            }
            else
            {
                etiqueta2 = "Filtro 2: Agua caliente";
                cmbOpcion.Items.Add("Con agua caliente");
                cmbOpcion.Items.Add("Sin agua caliente");
            }
            cmbOpcion.SelectedIndex = 0;

            StackPanel celda1 = Ui.Celda("Filtro 1: " + Catalogo.SegundoCriterio(tipo) + " (contiene el texto)", txtFiltro);
            StackPanel celda2 = Ui.Celda(etiqueta2, cmbOpcion);
            Grid.SetColumn(celda1, 0);
            Grid.SetColumn(celda2, 2);
            panelFiltros.Children.Add(celda1);
            panelFiltros.Children.Add(celda2);

            BotonVoltia btnFiltrar = new BotonVoltia("Filtrar", Ui.Azul());
            btnFiltrar.Width = 110;
            btnFiltrar.Height = 32;
            btnFiltrar.VerticalAlignment = VerticalAlignment.Bottom;
            btnFiltrar.Click += async (s, e) => await CargarAsync();
            Grid.SetColumn(btnFiltrar, 4);
            panelFiltros.Children.Add(btnFiltrar);

            BotonVoltia btnQuitar = new BotonVoltia("Quitar filtros", Ui.Gris());
            btnQuitar.Width = 120;
            btnQuitar.Height = 32;
            btnQuitar.Margin = new Thickness(8, 0, 0, 0);
            btnQuitar.VerticalAlignment = VerticalAlignment.Bottom;
            btnQuitar.Click += async (s, e) =>
            {
                txtFiltro.Texto = "";
                cmbOpcion.SelectedIndex = 0;
                await CargarAsync();
            };
            Grid.SetColumn(btnQuitar, 5);
            panelFiltros.Children.Add(btnQuitar);
        }

        /// <summary>La grilla se arma en codigo: columnas y estilos.</summary>
        private void ArmarGrilla()
        {
            grilla.AutoGenerateColumns = false;
            grilla.IsReadOnly = true;
            grilla.CanUserAddRows = false;
            grilla.CanUserDeleteRows = false;
            grilla.SelectionMode = DataGridSelectionMode.Single;
            grilla.HeadersVisibility = DataGridHeadersVisibility.Column;
            grilla.GridLinesVisibility = DataGridGridLinesVisibility.Horizontal;
            grilla.RowHeight = 32;
            grilla.FontSize = 13;
            grilla.Background = Brushes.White;
            grilla.AlternatingRowBackground = Ui.Pincel("#ECEFF1");
            grilla.BorderBrush = Ui.Pincel("#90A4AE");
            grilla.BorderThickness = new Thickness(1.5);
            grilla.HorizontalGridLinesBrush = Ui.Pincel("#CFD8DC");

            // Encabezado oscuro
            Style encabezado = new Style(typeof(DataGridColumnHeader));
            encabezado.Setters.Add(new Setter(Control.BackgroundProperty, Ui.Pincel("#37474F")));
            encabezado.Setters.Add(new Setter(Control.ForegroundProperty, Brushes.White));
            encabezado.Setters.Add(new Setter(Control.FontWeightProperty, FontWeights.Bold));
            encabezado.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(8)));
            encabezado.Setters.Add(new Setter(Control.BorderBrushProperty, Ui.Pincel("#546E7A")));
            encabezado.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(0, 0, 1, 0)));
            encabezado.Setters.Add(new Setter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Center));
            grilla.ColumnHeaderStyle = encabezado;

            // La fila se ilumina al pasar el mouse
            Style fila = new Style(typeof(DataGridRow));
            Trigger encima = new Trigger();
            encima.Property = UIElement.IsMouseOverProperty;
            encima.Value = true;
            encima.Setters.Add(new Setter(Control.BackgroundProperty, Ui.Pincel("#BBDEFB")));
            fila.Triggers.Add(encima);
            grilla.RowStyle = fila;

            // El texto de cada celda queda centrado verticalmente
            Style celda = new Style(typeof(DataGridCell));
            celda.Setters.Add(new Setter(Control.VerticalContentAlignmentProperty, VerticalAlignment.Center));
            celda.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(8, 0, 8, 0)));
            celda.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(0)));
            grilla.CellStyle = celda;

            // Columnas: las mismas del Java para cada objeto
            if (tipo == TipoObjeto.Lavadora)
            {
                AgregarColumna("Código", "Codigo", null, 0.7);
                AgregarColumna("Marca", "Marca", null, 1.1);
                AgregarColumna("Fecha Fab.", "FechaTexto", "FechaFabricacion", 1.5);
                AgregarColumna("Precio Base", "PrecioBaseTexto", "PrecioBase", 1.3);
                AgregarColumna("Capacidad", "CapacidadTexto", "CapacidadCargaKilos", 1.0);
                AgregarColumna("Inverter", "InverterTexto", null, 0.8);
                AgregarColumna("Centrifugado", "RpmTexto", "VelocidadCentrifugadoRPM", 1.2);
                AgregarColumna("Programa", "ProgramaTexto", null, 1.2);
                AgregarColumna("Precio Final", "PrecioFinalTexto", "PrecioFinal", 1.3);
            }
            else
            {
                AgregarColumna("Código", "Codigo", null, 0.7);
                AgregarColumna("Nombre", "Nombre", null, 1.3);
                AgregarColumna("Rango", "RangoTexto", "MinMinutos", 1.3);
                AgregarColumna("Agua", "AguaTexto", "ConsumoAguaLitros", 1.0);
                AgregarColumna("Agua caliente", "CalienteTexto", null, 1.2);
                AgregarColumna("Fecha creación", "FechaTexto", "FechaCreacion", 1.5);
                AgregarColumna("Lavadoras", "CantidadLavadoras", null, 1.0);
            }
        }

        private void AgregarColumna(string titulo, string propiedad, string ordenarPor, double ancho)
        {
            DataGridTextColumn columna = new DataGridTextColumn();
            columna.Header = titulo;
            columna.Binding = new Binding(propiedad); // De donde saca el dato
            if (ordenarPor != null)
            {
                columna.SortMemberPath = ordenarPor; // Para ordenar por el numero y no por el texto
            }
            columna.Width = new DataGridLength(ancho, DataGridLengthUnitType.Star);
            grilla.Columns.Add(columna);
        }

        /// <summary>Contador estilo pantalla digital y boton Cerrar.</summary>
        private void ArmarPie()
        {
            panelPie.Margin = new Thickness(0, 10, 0, 0);
            panelPie.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            panelPie.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            contador.Text = "0 encontrado(s)";
            contador.FontFamily = new FontFamily("Consolas");
            contador.FontSize = 16;
            contador.FontWeight = FontWeights.Bold;
            contador.Foreground = Ui.Pincel("#FFD54F");
            contador.VerticalAlignment = VerticalAlignment.Center;
            contador.Margin = new Thickness(14, 0, 14, 0);

            Border pantalla = new Border();
            pantalla.CornerRadius = new CornerRadius(10);
            pantalla.Background = Ui.Pincel("#08121C");
            pantalla.BorderBrush = Ui.Pincel("#546E7A");
            pantalla.BorderThickness = new Thickness(2);
            pantalla.Height = 40;
            pantalla.MinWidth = 300;
            pantalla.HorizontalAlignment = HorizontalAlignment.Left;
            pantalla.Child = contador;
            Grid.SetColumn(pantalla, 0);
            panelPie.Children.Add(pantalla);

            BotonVoltia btnCerrar = new BotonVoltia("Cerrar", Ui.Gris());
            btnCerrar.Width = 130;
            btnCerrar.Click += (s, e) => Marco.Cerrar();
            Grid.SetColumn(btnCerrar, 1);
            panelPie.Children.Add(btnCerrar);
        }

        // ================= Cargar datos =================

        /// <summary>Pide al servidor la lista con los filtros escogidos y la muestra en la grilla.</summary>
        private async Task CargarAsync() // Promesa de resultado
        {
            try
            {
                // Filtro 1: texto. Si esta vacio se manda null (sin filtro).
                string texto = txtFiltro.Texto.Trim();
                if (texto.Length == 0)
                {
                    texto = null;
                }

                // Filtro 2: null = todos, true = "con ...", false = "sin ..."
                bool? opcion = null;
                if (cmbOpcion.SelectedIndex == 1)
                {
                    opcion = true;
                }
                else if (cmbOpcion.SelectedIndex == 2)
                {
                    opcion = false;
                }

                List<DatoBase> lista = await servicio.ListarAsync(texto, opcion); // Espera la respuesta
                grilla.ItemsSource = lista;
                Anime.Contar(contador, lista.Count, "{0} encontrado(s)");
            }
            catch (Exception ex)
            {
                Ui.Aviso(this, ex.Message, "No se pudo listar");
            }
        }

        // Lo llama el Notificador cuando otra ventana cambio los datos
        private async void RecargarPorAviso()
        {
            await CargarAsync();
        }
    }
}
