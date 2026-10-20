using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ClienteLavadora.servicios;
using ClienteLavadora.util;

namespace ClienteLavadora.gui
{
    /// <summary>
    /// Menu Ayuda > Acerca de...: nombre de los integrantes del equipo y version de la aplicacion
    /// (los datos vienen de EmpresaInfo).
    /// </summary>
    public class VentanaAcercaDe : VentanaMarco
    {
        public VentanaAcercaDe() : base("ACERCA DE", 600, 640, true, 100)
        {
            EmpresaInfo info = EmpresaInfo.Instancia;

            TextBlock nombre = Texto(info.NombreApp, 20, FontWeights.Bold, "#0D47A1");
            nombre.Margin = new Thickness(0, 0, 0, 12);

            TextBlock version = Texto("Versión: " + info.Version + "\n" + info.Fecha, 15, FontWeights.SemiBold, "#263238");
            version.Margin = new Thickness(0, 0, 0, 18);

            TextBlock titulo = Texto("Integrantes del equipo", 13, FontWeights.Bold, "#546E7A");
            titulo.Margin = new Thickness(0, 0, 0, 6);

            TextBlock integrantes = Texto(info.Desarrolladores, 15, FontWeights.Normal, "#263238");
            integrantes.LineHeight = 26;
            Border tarjeta = new Border();
            tarjeta.Background = Brushes.White;
            tarjeta.CornerRadius = new CornerRadius(12);
            tarjeta.BorderBrush = Ui.Pincel("#FFC107");
            tarjeta.BorderThickness = new Thickness(2);
            tarjeta.Padding = new Thickness(16, 12, 16, 12);
            tarjeta.Child = integrantes;

            StackPanel datos = new StackPanel();
            datos.HorizontalAlignment = HorizontalAlignment.Center;
            datos.VerticalAlignment = VerticalAlignment.Top;
            datos.Children.Add(nombre);
            datos.Children.Add(version);
            datos.Children.Add(titulo);
            datos.Children.Add(tarjeta);

            BotonVoltia btnCerrar = new BotonVoltia("Cerrar", Ui.Azul());
            btnCerrar.Width = 150;
            btnCerrar.Click += (s, e) => Marco.Cerrar();

            Grid cuerpo = new Grid();
            cuerpo.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            cuerpo.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            Grid.SetRow(datos, 0);
            Grid.SetRow(btnCerrar, 1);
            cuerpo.Children.Add(datos);
            cuerpo.Children.Add(btnCerrar);
            Contenido = cuerpo;

            Loaded += (s, e) => Anime.Escalonado(new UIElement[] { nombre, version, titulo, tarjeta, btnCerrar }, 110, 450);
        }

        private static TextBlock Texto(string contenido, double tamano, FontWeight peso, string color)
        {
            TextBlock texto = new TextBlock();
            texto.Text = contenido;
            texto.FontSize = tamano;
            texto.FontWeight = peso;
            texto.Foreground = Ui.Pincel(color);
            texto.TextAlignment = TextAlignment.Center;
            texto.TextWrapping = TextWrapping.Wrap;
            return texto;
        }
    }
}
