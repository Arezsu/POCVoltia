using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ClienteLavadora.modelo;
using ClienteLavadora.util;

namespace ClienteLavadora.vista.controles
{
    /// <summary>
    /// LISTA de mantenimientos en forma de tarjetas (no de grilla), pensada para el
    /// panel de detalle: cada mantenimiento se ve con su descripcion, su tecnico, la
    /// fecha, el costo y una etiqueta de estado.
    ///
    /// El color de la barra lateral dice el estado de un vistazo:
    ///  - ambar = pendiente
    ///  - verde = completado
    /// Asi el usuario no tiene que leer palabra por palabra.
    /// </summary>
    public class ListaMantenimientos : ScrollViewer
    {
        private readonly StackPanel contenedor = new StackPanel();

        public ListaMantenimientos()
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            Content = contenedor;
        }

        /// <summary>Borra las tarjetas anteriores y pinta la lista recibida.</summary>
        public void Mostrar(List<MantenimientoData> mantenimientos, string mensajeVacio)
        {
            contenedor.Children.Clear();

            if (mantenimientos == null || mantenimientos.Count == 0)
            {
                TextBlock aviso = Ui.Texto(mensajeVacio, 11.5, Tema.TextoSecundario);
                aviso.Margin = new Thickness(4, 6, 4, 6);
                contenedor.Children.Add(aviso);
                return;
            }

            foreach (MantenimientoData mantenimiento in mantenimientos)
            {
                contenedor.Children.Add(Tarjeta(mantenimiento));
            }
        }

        private static Border Tarjeta(MantenimientoData mantenimiento)
        {
            string color = mantenimiento.Completado ? Tema.HexExito : Tema.HexAmbar;

            // Barra de color a la izquierda
            Border barra = new Border();
            barra.Width = 3;
            barra.CornerRadius = new CornerRadius(2);
            barra.Background = Tema.Pincel(color);
            barra.Margin = new Thickness(0, 2, 0, 2);

            // Descripcion y estado arriba
            Grid cabecera = new Grid();
            cabecera.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            cabecera.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            TextBlock descripcion = Ui.Texto(mantenimiento.Descripcion ?? "(sin descripcion)", 12, Tema.Texto);
            descripcion.FontWeight = FontWeights.SemiBold;
            descripcion.TextTrimming = TextTrimming.CharacterEllipsis;
            descripcion.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(descripcion, 0);
            cabecera.Children.Add(descripcion);

            Border estado = Ui.Pastilla(mantenimiento.EstadoTexto, color);
            Grid.SetColumn(estado, 1);
            cabecera.Children.Add(estado);

            // Tecnico y fecha abajo a la izquierda; costo a la derecha
            Grid pie = new Grid();
            pie.Margin = new Thickness(0, 3, 0, 0);
            pie.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            pie.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            string detalle = "id " + mantenimiento.Id + "  -  " + (mantenimiento.Tecnico ?? "") +
                             "  -  " + mantenimiento.FechaTexto;
            TextBlock textoDetalle = Ui.Texto(detalle, 10.5, Tema.TextoSecundario);
            textoDetalle.TextTrimming = TextTrimming.CharacterEllipsis;
            Grid.SetColumn(textoDetalle, 0);
            pie.Children.Add(textoDetalle);

            TextBlock costo = Ui.Digital(mantenimiento.CostoTexto, 11.5, Tema.Texto);
            Grid.SetColumn(costo, 1);
            pie.Children.Add(costo);

            StackPanel textos = new StackPanel();
            textos.Children.Add(cabecera);
            textos.Children.Add(pie);

            Grid rejilla = new Grid();
            rejilla.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            rejilla.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            rejilla.Children.Add(barra);
            Grid.SetColumn(textos, 1);
            textos.Margin = new Thickness(8, 0, 0, 0);
            rejilla.Children.Add(textos);

            Border tarjeta = new Border();
            tarjeta.Background = Tema.Pincel("#0E1620");
            tarjeta.BorderBrush = Tema.Pincel("#1E2A34");
            tarjeta.BorderThickness = new Thickness(1);
            tarjeta.CornerRadius = new CornerRadius(9);
            tarjeta.Padding = new Thickness(8, 6, 9, 6);
            tarjeta.Margin = new Thickness(0, 0, 2, 6);
            tarjeta.Child = rejilla;
            return tarjeta;
        }
    }
}
