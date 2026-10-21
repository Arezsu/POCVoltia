using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Formas = System.Windows.Shapes;

namespace ClienteLavadora.util
{
    /// <summary>
    /// "Fabricantes" de controles: metodos cortos que devuelven un control ya pintado
    /// con el estilo de la casa.
    ///
    /// POR QUE ASI Y NO EN CADA VENTANA:
    /// escribir la interfaz en C# (sin XAML) obliga a repetir mucho codigo
    /// (crear el TextBlock, poner tamano, color, margen...). Si eso se copiara en
    /// nueve ventanas, seria imposible mantener el mismo estilo. Aqui se escribe
    /// una vez y cada ventana solo pide "dame una etiqueta" o "dame una grilla".
    ///
    /// Ejemplo:
    ///     StackPanel celda = Ui.Celda("Marca", miCampoTexto);
    ///     grilla.Columns.Add(Ui.Columna("Marca", "Marca", 1.2, false));
    /// </summary>
    public static class Ui
    {
        // ====================================================================
        //  Textos
        // ====================================================================

        /// <summary>Etiqueta pequena que va encima de un campo del formulario.</summary>
        public static TextBlock Etiqueta(string texto)
        {
            TextBlock etiqueta = new TextBlock();
            etiqueta.Text = texto;
            etiqueta.FontFamily = Tema.Fuente;
            etiqueta.FontSize = 11.5;
            etiqueta.FontWeight = FontWeights.SemiBold;
            etiqueta.Foreground = Tema.TextoSecundario;
            etiqueta.Margin = new Thickness(2, 0, 0, 4);
            return etiqueta;
        }

        /// <summary>Texto normal con el color y el tamano que se pidan.</summary>
        public static TextBlock Texto(string texto, double tamano, Brush color)
        {
            TextBlock bloque = new TextBlock();
            bloque.Text = texto;
            bloque.FontFamily = Tema.Fuente;
            bloque.FontSize = tamano;
            bloque.Foreground = color;
            bloque.TextWrapping = TextWrapping.Wrap;
            return bloque;
        }

        /// <summary>Titulo de seccion en mayusculas y con letra espaciada.</summary>
        public static TextBlock Rotulo(string texto)
        {
            TextBlock rotulo = new TextBlock();
            rotulo.Text = texto;
            rotulo.FontFamily = Tema.Fuente;
            rotulo.FontSize = 11;
            rotulo.FontWeight = FontWeights.Bold;
            rotulo.Foreground = Tema.Cian;
            rotulo.Margin = new Thickness(0, 0, 0, 6);
            return rotulo;
        }

        /// <summary>Numero o texto de pantalla digital (Consolas, ancho fijo).</summary>
        public static TextBlock Digital(string texto, double tamano, Brush color)
        {
            TextBlock bloque = new TextBlock();
            bloque.Text = texto;
            bloque.FontFamily = Tema.Numeros;
            bloque.FontSize = tamano;
            bloque.Foreground = color;
            bloque.VerticalAlignment = VerticalAlignment.Center;
            return bloque;
        }

        // ====================================================================
        //  Formularios
        // ====================================================================

        /// <summary>Una "celda" del formulario: la etiqueta arriba y el control debajo.</summary>
        public static StackPanel Celda(string etiqueta, UIElement control)
        {
            StackPanel celda = new StackPanel();
            if (etiqueta != null)
            {
                celda.Children.Add(Etiqueta(etiqueta));
            }
            if (control != null)
            {
                celda.Children.Add(control);
            }
            return celda;
        }

        /// <summary>Fila del formulario con dos columnas iguales y un espacio en el medio.</summary>
        public static Grid Fila(UIElement izquierda, UIElement derecha)
        {
            Grid fila = new Grid();
            fila.Margin = new Thickness(0, 0, 0, 10);
            fila.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            fila.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(14) });
            fila.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            if (izquierda != null)
            {
                Grid.SetColumn(izquierda, 0);
                fila.Children.Add(izquierda);
            }
            if (derecha != null)
            {
                Grid.SetColumn(derecha, 2);
                fila.Children.Add(derecha);
            }
            return fila;
        }

        /// <summary>Lista desplegable oscura con opciones ya cargadas.</summary>
        public static ComboBox Combo(params string[] opciones)
        {
            ComboBox combo = new ComboBox();
            combo.Height = 32;
            combo.FontFamily = Tema.Fuente;
            combo.FontSize = 13;
            combo.Background = Tema.Pincel("#0E1620");
            combo.Foreground = Tema.Texto;
            combo.BorderBrush = Tema.Pincel("#2A3A47");
            combo.BorderThickness = new Thickness(1);
            combo.Padding = new Thickness(6, 0, 6, 0);
            combo.VerticalContentAlignment = VerticalAlignment.Center;

            // WPF pinta el ComboBox con los colores del tema de Windows (blanco). Para
            // que se vea como el resto de la lavadora se le cambia la plantilla por una
            // oscura dibujada aqui mismo, sin XAML.
            combo.Template = PlantillaCombo();

            // El desplegable de WPF se pinta con su propio estilo: lo oscurecemos para
            // que las opciones no queden con letra clara sobre fondo blanco.
            Style estiloOpcion = new Style(typeof(ComboBoxItem));
            estiloOpcion.Setters.Add(new Setter(Control.BackgroundProperty, Tema.Pincel("#0E1620")));
            estiloOpcion.Setters.Add(new Setter(Control.ForegroundProperty, Tema.Texto));
            estiloOpcion.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(8, 6, 8, 6)));
            Trigger encima = new Trigger();
            encima.Property = UIElement.IsMouseOverProperty;
            encima.Value = true;
            encima.Setters.Add(new Setter(Control.BackgroundProperty, Tema.Acero));
            estiloOpcion.Triggers.Add(encima);
            combo.ItemContainerStyle = estiloOpcion;

            if (opciones != null)
            {
                foreach (string opcion in opciones)
                {
                    combo.Items.Add(opcion);
                }
                combo.SelectedIndex = 0;
            }
            return combo;
        }

        /// <summary>
        /// Plantilla oscura del ComboBox: una caja de cristal con la opcion escogida,
        /// una flecha y una lista flotante. Se arma con objetos porque el proyecto no
        /// usa XAML. Los nombres PART_ToggleButton y PART_Popup son los que WPF busca
        /// para abrir y cerrar la lista.
        /// </summary>
        private static ControlTemplate PlantillaCombo()
        {
            ControlTemplate plantilla = new ControlTemplate(typeof(ComboBox));

            // ----- Caja visible con la opcion escogida -----
            FrameworkElementFactory marco = new FrameworkElementFactory(typeof(Border));
            marco.Name = "Marco";
            marco.SetBinding(Border.BackgroundProperty,
                new Binding("Background") { RelativeSource = RelativeSource.TemplatedParent });
            marco.SetBinding(Border.BorderBrushProperty,
                new Binding("BorderBrush") { RelativeSource = RelativeSource.TemplatedParent });
            marco.SetValue(Border.BorderThicknessProperty, new Thickness(1));
            marco.SetValue(Border.CornerRadiusProperty, new CornerRadius(8));
            marco.SetBinding(TextElement.ForegroundProperty,
                new Binding("Foreground") { RelativeSource = RelativeSource.TemplatedParent });

            FrameworkElementFactory contenido = new FrameworkElementFactory(typeof(DockPanel));
            contenido.SetValue(DockPanel.LastChildFillProperty, true);

            FrameworkElementFactory flecha = new FrameworkElementFactory(typeof(Formas.Path));
            flecha.SetValue(FrameworkElement.WidthProperty, 9.0);
            flecha.SetValue(FrameworkElement.HeightProperty, 5.0);
            flecha.SetValue(FrameworkElement.MarginProperty, new Thickness(0, 0, 10, 0));
            flecha.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
            flecha.SetValue(DockPanel.DockProperty, Dock.Right);
            flecha.SetValue(Formas.Shape.StretchProperty, Stretch.Fill);
            flecha.SetValue(Formas.Shape.StrokeThicknessProperty, 1.5);
            flecha.SetBinding(Formas.Shape.StrokeProperty,
                new Binding("Foreground") { RelativeSource = RelativeSource.TemplatedParent });
            flecha.SetValue(Formas.Path.DataProperty, Geometry.Parse("M 0 0 L 4.5 5 L 9 0"));
            contenido.AppendChild(flecha);

            FrameworkElementFactory texto = new FrameworkElementFactory(typeof(ContentPresenter));
            texto.SetValue(FrameworkElement.MarginProperty, new Thickness(10, 0, 4, 0));
            texto.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
            texto.SetBinding(ContentPresenter.ContentProperty,
                new Binding("SelectionBoxItem") { RelativeSource = RelativeSource.TemplatedParent });
            texto.SetBinding(ContentPresenter.ContentTemplateProperty,
                new Binding("SelectionBoxItemTemplate") { RelativeSource = RelativeSource.TemplatedParent });
            contenido.AppendChild(texto);

            marco.AppendChild(contenido);

            // ----- Boton invisible encima de la caja: abre y cierra la lista -----
            FrameworkElementFactory boton = new FrameworkElementFactory(typeof(ToggleButton));
            boton.Name = "PART_ToggleButton";
            boton.SetValue(UIElement.FocusableProperty, false);
            boton.SetValue(FrameworkElement.CursorProperty, Cursors.Hand);
            boton.SetBinding(ToggleButton.IsCheckedProperty, new Binding("IsDropDownOpen")
            {
                RelativeSource = RelativeSource.TemplatedParent,
                Mode = BindingMode.TwoWay
            });

            ControlTemplate plantillaBoton = new ControlTemplate(typeof(ToggleButton));
            FrameworkElementFactory relleno = new FrameworkElementFactory(typeof(Border));
            relleno.SetValue(Border.BackgroundProperty, Brushes.Transparent);
            relleno.SetValue(Border.CornerRadiusProperty, new CornerRadius(8));
            plantillaBoton.VisualTree = relleno;
            boton.SetValue(Control.TemplateProperty, plantillaBoton);

            // ----- Lista flotante con las opciones (fondo oscuro) -----
            FrameworkElementFactory lista = new FrameworkElementFactory(typeof(Popup));
            lista.Name = "PART_Popup";
            lista.SetValue(Popup.PlacementProperty, PlacementMode.Bottom);
            lista.SetValue(Popup.AllowsTransparencyProperty, true);
            lista.SetValue(Popup.StaysOpenProperty, false);
            lista.SetValue(Popup.PopupAnimationProperty, PopupAnimation.Fade);
            lista.SetBinding(Popup.IsOpenProperty, new Binding("IsDropDownOpen")
            {
                RelativeSource = RelativeSource.TemplatedParent,
                Mode = BindingMode.TwoWay
            });

            FrameworkElementFactory bordeLista = new FrameworkElementFactory(typeof(Border));
            bordeLista.SetValue(Border.BackgroundProperty, Tema.Pincel("#0B131B"));
            bordeLista.SetValue(Border.BorderBrushProperty, Tema.Pincel("#2A3A47"));
            bordeLista.SetValue(Border.BorderThicknessProperty, new Thickness(1));
            bordeLista.SetValue(Border.CornerRadiusProperty, new CornerRadius(8));
            bordeLista.SetValue(Border.PaddingProperty, new Thickness(4));
            bordeLista.SetValue(TextElement.ForegroundProperty, Tema.Texto);
            bordeLista.SetBinding(FrameworkElement.MinWidthProperty,
                new Binding("ActualWidth") { RelativeSource = RelativeSource.TemplatedParent });

            FrameworkElementFactory desplazable = new FrameworkElementFactory(typeof(ScrollViewer));
            desplazable.SetValue(ScrollViewer.MaxHeightProperty, 230.0);
            desplazable.AppendChild(new FrameworkElementFactory(typeof(ItemsPresenter)));
            bordeLista.AppendChild(desplazable);
            lista.AppendChild(bordeLista);

            // Las tres piezas van una encima de otra dentro de una rejilla.
            FrameworkElementFactory raiz = new FrameworkElementFactory(typeof(Grid));
            raiz.AppendChild(marco);
            raiz.AppendChild(boton);
            raiz.AppendChild(lista);
            plantilla.VisualTree = raiz;

            // Al pasar el raton, la caja se aclara un poco.
            Trigger encima = new Trigger();
            encima.Property = UIElement.IsMouseOverProperty;
            encima.Value = true;
            Setter setter = new Setter(Border.BackgroundProperty, Tema.Pincel("#16222C"));
            setter.TargetName = "Marco";
            encima.Setters.Add(setter);
            plantilla.Triggers.Add(encima);

            return plantilla;
        }

        /// <summary>Casilla de verificacion con texto claro.</summary>
        public static CheckBox Check(string texto)
        {
            CheckBox casilla = new CheckBox();
            casilla.Content = texto;
            casilla.FontFamily = Tema.Fuente;
            casilla.FontSize = 13;
            casilla.Foreground = Tema.Texto;
            casilla.VerticalAlignment = VerticalAlignment.Center;
            return casilla;
        }

        /// <summary>Texto de ayuda pequeno (por ejemplo el formato de la fecha).</summary>
        public static TextBlock Pista(string texto)
        {
            TextBlock pista = new TextBlock();
            pista.Text = texto;
            pista.FontFamily = Tema.Fuente;
            pista.FontSize = 10.5;
            pista.Foreground = Tema.TextoSecundario;
            pista.TextWrapping = TextWrapping.Wrap;
            pista.Margin = new Thickness(2, 5, 0, 0);
            return pista;
        }

        // ====================================================================
        //  Paneles de cristal (glassmorphism tenue)
        // ====================================================================

        /// <summary>Panel semitransparente con borde claro fino: el "cristal" de la casa.</summary>
        public static Border Cristal(UIElement contenido)
        {
            return Cristal(contenido, new Thickness(14));
        }

        /// <summary>Igual que el anterior pero con el relleno interno que se pida.</summary>
        public static Border Cristal(UIElement contenido, Thickness relleno)
        {
            Border panel = new Border();
            panel.Background = Tema.Cristal;
            panel.BorderBrush = Tema.CristalBorde;
            panel.BorderThickness = new Thickness(1);
            panel.CornerRadius = new CornerRadius(14);
            panel.Padding = relleno;
            panel.Child = contenido;
            return panel;
        }

        /// <summary>Linea separadora muy sutil.</summary>
        public static Border Separador()
        {
            Border linea = new Border();
            linea.Height = 1;
            linea.Background = Tema.Linea;
            linea.Margin = new Thickness(0, 8, 0, 8);
            return linea;
        }

        /// <summary>Marco de pantalla digital: fondo casi negro, borde de acero y texto Consolas.</summary>
        public static Border Pantalla(UIElement contenido)
        {
            Border pantalla = new Border();
            pantalla.Background = Tema.PanelOscuro;
            pantalla.BorderBrush = Tema.Pincel("#2A3A47");
            pantalla.BorderThickness = new Thickness(1);
            pantalla.CornerRadius = new CornerRadius(8);
            pantalla.Padding = new Thickness(10, 6, 10, 6);
            pantalla.Child = contenido;
            return pantalla;
        }

        /// <summary>Etiqueta de estado con el color del acento (por ejemplo "PENDIENTE").</summary>
        public static Border Pastilla(string texto, string hexColor)
        {
            Color acento = Tema.ColorDe(hexColor);
            Border pastilla = new Border();
            pastilla.CornerRadius = new CornerRadius(9);
            pastilla.Background = new SolidColorBrush(Color.FromArgb(38, acento.R, acento.G, acento.B));
            pastilla.BorderBrush = new SolidColorBrush(Color.FromArgb(120, acento.R, acento.G, acento.B));
            pastilla.BorderThickness = new Thickness(1);
            pastilla.Padding = new Thickness(8, 2, 8, 2);
            pastilla.HorizontalAlignment = HorizontalAlignment.Left;

            TextBlock bloque = new TextBlock();
            bloque.Text = texto;
            bloque.FontFamily = Tema.Numeros;
            bloque.FontSize = 10.5;
            bloque.FontWeight = FontWeights.Bold;
            bloque.Foreground = new SolidColorBrush(acento);
            pastilla.Child = bloque;
            return pastilla;
        }

        /// <summary>Envoltorio con barra de desplazamiento para formularios largos.</summary>
        public static ScrollViewer Desplazable(UIElement contenido)
        {
            ScrollViewer marco = new ScrollViewer();
            marco.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            marco.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            marco.Content = contenido;
            return marco;
        }

        // ====================================================================
        //  Grilla de datos (DataGrid) con el estilo oscuro de la casa
        // ====================================================================

        /// <summary>
        /// Devuelve una grilla ya configurada: solo lectura, filas oscuras y
        /// encabezado de acero. Las ventanas solo agregan las columnas.
        /// </summary>
        public static DataGrid Grilla()
        {
            DataGrid grilla = new DataGrid();
            grilla.AutoGenerateColumns = false;
            grilla.IsReadOnly = true;
            grilla.CanUserAddRows = false;
            grilla.CanUserDeleteRows = false;
            grilla.CanUserResizeRows = false;
            grilla.SelectionMode = DataGridSelectionMode.Single;
            grilla.SelectionUnit = DataGridSelectionUnit.FullRow;
            grilla.HeadersVisibility = DataGridHeadersVisibility.Column;
            grilla.GridLinesVisibility = DataGridGridLinesVisibility.Horizontal;
            grilla.RowHeight = 30;
            grilla.ColumnHeaderHeight = 34;
            grilla.FontFamily = Tema.Fuente;
            grilla.FontSize = 12.5;
            grilla.Background = Tema.Pincel("#0E1620");
            grilla.RowBackground = Tema.Pincel("#0E1620");
            grilla.AlternatingRowBackground = Tema.Pincel("#121C25");
            grilla.Foreground = Tema.Texto;
            grilla.BorderBrush = Tema.CristalBorde;
            grilla.BorderThickness = new Thickness(1);
            grilla.HorizontalGridLinesBrush = Tema.Pincel("#1B2731");
            grilla.ColumnHeaderStyle = EstiloEncabezado();
            grilla.CellStyle = EstiloCelda();
            grilla.RowStyle = EstiloFila();
            return grilla;
        }

        /// <summary>Agrega una columna de texto a la grilla.</summary>
        public static DataGridTextColumn Columna(string titulo, string propiedad, double ancho, bool ordenarComoNumero)
        {
            DataGridTextColumn columna = new DataGridTextColumn();
            // El titulo va en un TextBlock que puede partirse en dos lineas: asi una
            // columna angosta muestra "Fecha fabricacion" completo y no cortado.
            TextBlock encabezado = new TextBlock();
            encabezado.Text = titulo;
            encabezado.TextWrapping = TextWrapping.Wrap;
            encabezado.TextTrimming = TextTrimming.CharacterEllipsis;
            columna.Header = encabezado;
            columna.Binding = new Binding(propiedad);
            columna.Width = new DataGridLength(ancho, DataGridLengthUnitType.Star);
            // Un ancho minimo evita que el titulo quede aplastado cuando la ventana
            // se mide antes de que la grilla tenga filas.
            columna.MinWidth = 62;
            if (ordenarComoNumero)
            {
                columna.SortMemberPath = propiedad; // Ordena por el valor, no por el texto
            }
            return columna;
        }

        private static Style EstiloEncabezado()
        {
            Style estilo = new Style(typeof(DataGridColumnHeader));
            estilo.Setters.Add(new Setter(Control.BackgroundProperty, Tema.Pincel(Tema.HexAcero)));
            estilo.Setters.Add(new Setter(Control.ForegroundProperty, Tema.Texto));
            estilo.Setters.Add(new Setter(Control.FontFamilyProperty, Tema.Fuente));
            estilo.Setters.Add(new Setter(Control.FontSizeProperty, 11.5));
            estilo.Setters.Add(new Setter(Control.FontWeightProperty, FontWeights.SemiBold));
            estilo.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(8, 7, 8, 7)));
            estilo.Setters.Add(new Setter(Control.BorderBrushProperty, Tema.Pincel("#2E4050")));
            estilo.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(0, 0, 1, 1)));
            estilo.Setters.Add(new Setter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Left));
            return estilo;
        }

        private static Style EstiloCelda()
        {
            Style estilo = new Style(typeof(DataGridCell));
            estilo.Setters.Add(new Setter(Control.ForegroundProperty, Tema.Texto));
            estilo.Setters.Add(new Setter(Control.BackgroundProperty, Brushes.Transparent));
            estilo.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(0)));
            estilo.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(8, 0, 8, 0)));
            estilo.Setters.Add(new Setter(Control.VerticalContentAlignmentProperty, VerticalAlignment.Center));

            Trigger seleccionada = new Trigger();
            seleccionada.Property = DataGridCell.IsSelectedProperty;
            seleccionada.Value = true;
            seleccionada.Setters.Add(new Setter(Control.BackgroundProperty, Brushes.Transparent));
            seleccionada.Setters.Add(new Setter(Control.ForegroundProperty, Tema.Cian));
            estilo.Triggers.Add(seleccionada);
            return estilo;
        }

        private static Style EstiloFila()
        {
            // El orden importa: el ultimo disparador que se cumple es el que manda.
            Style estilo = new Style(typeof(DataGridRow));
            estilo.Setters.Add(new Setter(Control.ForegroundProperty, Tema.Texto));

            Trigger encima = new Trigger();
            encima.Property = UIElement.IsMouseOverProperty;
            encima.Value = true;
            encima.Setters.Add(new Setter(Control.BackgroundProperty, Tema.Pincel("#182833")));
            estilo.Triggers.Add(encima);

            Trigger seleccionada = new Trigger();
            seleccionada.Property = DataGridRow.IsSelectedProperty;
            seleccionada.Value = true;
            seleccionada.Setters.Add(new Setter(Control.BackgroundProperty, Tema.Pincel("#1E3A46")));
            estilo.Triggers.Add(seleccionada);
            return estilo;
        }

        // ====================================================================
        //  Imagenes de la carpeta Assets
        // ====================================================================

        /// <summary>
        /// Carga una imagen de Assets. Se usa OnLoad para que el archivo quede libre
        /// (si no, Windows lo bloquea mientras la aplicacion este abierta).
        /// </summary>
        public static BitmapImage Imagen(string nombreArchivo)
        {
            try
            {
                BitmapImage imagen = new BitmapImage();
                imagen.BeginInit();
                imagen.UriSource = new Uri(Path.Combine(AppContext.BaseDirectory, "Assets", nombreArchivo));
                imagen.CacheOption = BitmapCacheOption.OnLoad;
                imagen.EndInit();
                return imagen;
            }
            catch (Exception)
            {
                return null; // Si falta la imagen, la ventana muestra el resto sin romperse
            }
        }
    }
}
