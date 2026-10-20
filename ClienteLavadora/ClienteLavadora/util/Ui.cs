using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ClienteLavadora.util
{
    /// <summary>
    /// Herramientas para armar la interfaz sin XAML: colores, degradados y
    /// pequenos "fabricantes" de controles (etiquetas, campos en fila, etc.).
    /// Asi cada ventana solo dice QUE quiere y no repite el codigo de como se ve.
    /// </summary>
    public static class Ui
    {
        // ====================================================================
        //  Colores y degradados (metalico mate con azul, gris y amarillo)
        // ====================================================================

        /// <summary>Convierte un texto como "#1E88E5" en color.</summary>
        public static Color ColorDe(string hex)
        {
            return (Color)ColorConverter.ConvertFromString(hex);
        }

        public static SolidColorBrush Pincel(string hex)
        {
            return new SolidColorBrush(ColorDe(hex));
        }

        /// <summary>Degradado vertical (de arriba hacia abajo). Los colores se reparten parejo.</summary>
        public static LinearGradientBrush Degradado(params string[] colores)
        {
            return CrearDegradado(new Point(0, 0), new Point(0, 1), colores);
        }

        /// <summary>Degradado en diagonal.</summary>
        public static LinearGradientBrush DegradadoDiagonal(params string[] colores)
        {
            return CrearDegradado(new Point(0, 0), new Point(1, 1), colores);
        }

        private static LinearGradientBrush CrearDegradado(Point inicio, Point fin, string[] colores)
        {
            LinearGradientBrush pincel = new LinearGradientBrush();
            pincel.StartPoint = inicio;
            pincel.EndPoint = fin;
            for (int i = 0; i < colores.Length; i++)
            {
                double posicion = colores.Length == 1 ? 0 : (double)i / (colores.Length - 1);
                pincel.GradientStops.Add(new GradientStop(ColorDe(colores[i]), posicion));
            }
            return pincel;
        }

        // Los colores de los botones
        public static Brush Azul() { return Degradado("#42A5F5", "#1565C0"); }
        public static Brush Amarillo() { return Degradado("#FFD54F", "#FFA000"); }
        public static Brush Gris() { return Degradado("#90A4AE", "#546E7A"); }
        public static Brush Rojo() { return Degradado("#EF5350", "#B71C1C"); }

        /// <summary>Pintura del cuerpo de la lavadora: gris metalico mate.</summary>
        public static Brush Metal() { return Degradado("#E6EAEE", "#C7CED5", "#A7B1BB"); }

        /// <summary>Pintura de la consola (la parte de arriba): gris azulado oscuro.</summary>
        public static Brush Consola() { return Degradado("#4B6070", "#263640"); }

        // ====================================================================
        //  Fabricantes de controles
        // ====================================================================

        /// <summary>Etiqueta pequena que va encima de cada campo.</summary>
        public static TextBlock Etiqueta(string texto)
        {
            TextBlock etiqueta = new TextBlock();
            etiqueta.Text = texto;
            etiqueta.FontSize = 12;
            etiqueta.FontWeight = FontWeights.SemiBold;
            etiqueta.Foreground = Pincel("#263238");
            etiqueta.Margin = new Thickness(2, 0, 0, 3);
            return etiqueta;
        }

        /// <summary>Una "celda" del formulario: la etiqueta arriba y el control debajo.</summary>
        public static StackPanel Celda(string etiqueta, UIElement control)
        {
            StackPanel celda = new StackPanel();
            if (etiqueta != null)
            {
                celda.Children.Add(Etiqueta(etiqueta));
            }
            celda.Children.Add(control);
            return celda;
        }

        /// <summary>Una fila del formulario con dos columnas separadas por un espacio.</summary>
        public static Grid Fila(UIElement izquierda, UIElement derecha)
        {
            Grid fila = new Grid();
            fila.Margin = new Thickness(0, 0, 0, 9);
            fila.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            fila.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(16) });
            fila.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            Grid.SetColumn(izquierda, 0);
            fila.Children.Add(izquierda);
            if (derecha != null)
            {
                Grid.SetColumn(derecha, 2);
                fila.Children.Add(derecha);
            }
            return fila;
        }

        public static ComboBox Combo()
        {
            ComboBox combo = new ComboBox();
            combo.Height = 32;
            combo.FontSize = 14;
            combo.VerticalContentAlignment = VerticalAlignment.Center;
            return combo;
        }

        public static DatePicker Fecha()
        {
            DatePicker fecha = new DatePicker();
            fecha.Height = 32;
            fecha.FontSize = 14;
            fecha.VerticalContentAlignment = VerticalAlignment.Center;
            return fecha;
        }

        public static CheckBox Check(string texto)
        {
            CheckBox check = new CheckBox();
            check.Content = texto;
            check.FontSize = 14;
            check.FontWeight = FontWeights.SemiBold;
            check.Foreground = Pincel("#263238");
            check.VerticalAlignment = VerticalAlignment.Bottom;
            check.Margin = new Thickness(2, 0, 0, 7);
            return check;
        }

        /// <summary>Carga una imagen de la carpeta Assets (queda copiada junto al .exe).</summary>
        public static BitmapImage Imagen(string nombreArchivo)
        {
            BitmapImage imagen = new BitmapImage();
            imagen.BeginInit();
            imagen.UriSource = new Uri(Path.Combine(AppContext.BaseDirectory, "Assets", nombreArchivo));
            imagen.CacheOption = BitmapCacheOption.OnLoad; // Carga todo de una vez y libera el archivo
            imagen.EndInit();
            return imagen;
        }

        /// <summary>Muestra un mensaje de error con el icono de advertencia.</summary>
        public static void Aviso(Window dueno, string mensaje, string titulo)
        {
            MessageBox.Show(dueno, mensaje, titulo, MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        public static void Informacion(Window dueno, string mensaje, string titulo)
        {
            MessageBox.Show(dueno, mensaje, titulo, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
