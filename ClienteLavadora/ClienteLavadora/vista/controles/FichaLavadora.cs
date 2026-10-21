using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ClienteLavadora.modelo;
using ClienteLavadora.util;

namespace ClienteLavadora.vista.controles
{
    /// <summary>
    /// FICHA de una lavadora: muestra todos sus datos como una lista de
    /// "etiqueta - valor". Es la vista de solo lectura que usan las ventanas
    /// Consultar y Eliminar, y el panel de detalle de la ventana Listar.
    ///
    /// POR QUE UN CONTROL Y NO UN METODO EN CADA VENTANA:
    /// las tres ventanas necesitan exactamente la misma presentacion. Al tenerla aqui,
    /// si el dia de manana se agrega un campo a la lavadora, se agrega en UN solo lugar.
    /// </summary>
    public class FichaLavadora : StackPanel
    {
        /// <summary>Borra lo anterior y pinta los datos de la lavadora.</summary>
        public void Mostrar(LavadoraData lavadora)
        {
            Children.Clear();

            if (lavadora == null)
            {
                MostrarVacio("Seleccione una lavadora para ver sus datos.");
                return;
            }

            Children.Add(Fila("Codigo", lavadora.Codigo.ToString(), Tema.Texto, false));
            Children.Add(Fila("Marca", lavadora.Marca, Tema.Texto, false));
            Children.Add(Fila("Fecha de fabricacion", lavadora.FechaTexto, Tema.Texto, false));
            Children.Add(Fila("Precio base", lavadora.PrecioBaseTexto, Tema.Texto, false));
            Children.Add(Fila("Capacidad de carga", lavadora.CapacidadTexto, Tema.Texto, false));
            Children.Add(Fila("Velocidad de centrifugado", lavadora.RpmTexto, Tema.Texto, false));
            Children.Add(Fila("Tecnologia inverter", lavadora.InverterTexto, Tema.Texto, false));
            Children.Add(Fila("Tiempo de lavado", lavadora.MinutosTexto, Tema.Texto, false));

            // Datos que calcula el servidor: se destacan en ambar.
            Children.Add(Fila("Precio final", lavadora.PrecioFinalTexto, Tema.Ambar, true));

            Children.Add(Ui.Separador());

            Children.Add(Fila("Mantenimientos", lavadora.MantenimientosTexto, Tema.Texto, false));
            Children.Add(Fila("Pendientes", lavadora.PendientesTexto,
                lavadora.MantenimientosPendientes > 0 ? Tema.Ambar : Tema.Exito, false));
            Children.Add(Fila("Costo de mantenimientos", lavadora.CostoMantenimientosTexto, Tema.Texto, false));
        }

        /// <summary>Muestra un mensaje en lugar de los datos.</summary>
        public void MostrarVacio(string mensaje)
        {
            Children.Clear();
            TextBlock aviso = Ui.Texto(mensaje, 12, Tema.TextoSecundario);
            aviso.Margin = new Thickness(4, 8, 4, 8);
            Children.Add(aviso);
        }

        /// <summary>Una fila: etiqueta a la izquierda y valor a la derecha.</summary>
        private static FrameworkElement Fila(string etiqueta, string valor, Brush color, bool destacada)
        {
            Grid rejilla = new Grid();
            rejilla.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            rejilla.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            TextBlock textoEtiqueta = Ui.Texto(etiqueta, 11.5, Tema.TextoSecundario);
            textoEtiqueta.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(textoEtiqueta, 0);
            rejilla.Children.Add(textoEtiqueta);

            TextBlock textoValor = new TextBlock();
            textoValor.Text = valor;
            textoValor.FontFamily = Tema.Numeros;
            textoValor.FontSize = destacada ? 14 : 12.5;
            textoValor.FontWeight = destacada ? FontWeights.Bold : FontWeights.Normal;
            textoValor.Foreground = color;
            textoValor.VerticalAlignment = VerticalAlignment.Center;
            textoValor.TextTrimming = TextTrimming.CharacterEllipsis;
            Grid.SetColumn(textoValor, 1);
            rejilla.Children.Add(textoValor);

            Border fondo = new Border();
            fondo.CornerRadius = new CornerRadius(6);
            fondo.Padding = new Thickness(9, 4, 9, 4);
            fondo.Margin = new Thickness(0, 0, 0, 2);
            fondo.Background = Tema.Pincel(destacada ? "#14E8B15A" : "#08FFFFFF");
            fondo.Child = rejilla;
            return fondo;
        }
    }
}
