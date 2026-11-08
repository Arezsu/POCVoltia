using System.Windows;
using System.Windows.Controls;
using ClienteLavadora.util;
using ClienteLavadora.vista.controles;

namespace ClienteLavadora.vista
{
    /// <summary>
    /// Ventana ACERCA DE: los datos del curso, los integrantes y las decisiones
    /// tecnicas del proyecto.
    ///
    /// No hace ninguna peticion al servidor: es informacion fija del proyecto.
    /// </summary>
    public class VentanaAcercaDe : VentanaMarco
    {
        private static VentanaAcercaDe instancia;

        /// <summary>
        /// Muestra la unica ventana Acerca de abierta. Si ya existe, la activa;
        /// despues de cerrarla se puede crear una nueva instancia.
        /// </summary>
        public static void MostrarInstancia(Window propietaria)
        {
            if (instancia == null)
            {
                instancia = new VentanaAcercaDe();
                instancia.Closed += (s, e) =>
                {
                    if (ReferenceEquals(instancia, s))
                    {
                        instancia = null;
                    }
                };
            }

            if (!instancia.IsVisible)
            {
                if (propietaria != null)
                {
                    instancia.Owner = propietaria;
                    instancia.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                }
                instancia.Show();
            }

            if (instancia.WindowState == WindowState.Minimized)
            {
                instancia.WindowState = WindowState.Normal;
            }
            instancia.Activate();
        }

        private VentanaAcercaDe() : base("ACERCA DE VOLTIA", 600, 430, false, 0)
        {
            StackPanel contenido = new StackPanel();
            contenido.VerticalAlignment = VerticalAlignment.Center;
            contenido.Children.Add(Ui.Rotulo("INTEGRANTES"));
            contenido.Children.Add(NombreIntegrante("Juan Andres Bermeo Alvarez"));
            contenido.Children.Add(NombreIntegrante("Alejandro Sanchez Quimbayo"));
            contenido.Children.Add(NombreIntegrante("David Arredondo"));
            contenido.Children.Add(Ui.Separador());
            TextBlock version = Ui.Texto("Versión v4.5", 16, Tema.Cian);
            version.FontFamily = Tema.Numeros;
            version.FontWeight = FontWeights.Bold;
            version.HorizontalAlignment = HorizontalAlignment.Center;
            version.Margin = new Thickness(0, 8, 0, 0);
            contenido.Children.Add(version);

            ScrollViewer desplazable = Ui.Desplazable(contenido);
            desplazable.MaxHeight = 260;

            BotonVoltia btnCerrar = BotonVoltia.Principal("Cerrar");
            btnCerrar.Width = 130;
            btnCerrar.HorizontalAlignment = HorizontalAlignment.Right;
            btnCerrar.Margin = new Thickness(0, 12, 0, 0);
            btnCerrar.Click += (s, e) => Cerrar();

            Grid raiz = new Grid();
            raiz.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            raiz.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            raiz.Children.Add(Ui.Cristal(desplazable, new Thickness(16)));
            Grid.SetRow(btnCerrar, 1);
            raiz.Children.Add(btnCerrar);

            Contenido = raiz;

            Loaded += (s, e) => Anime.Escalonado(new UIElement[] { raiz }, 60, 240);
        }

        private static FrameworkElement NombreIntegrante(string nombre)
        {
            TextBlock texto = Ui.Texto(nombre, 15, Tema.Texto);
            texto.HorizontalAlignment = HorizontalAlignment.Center;
            texto.Margin = new Thickness(0, 7, 0, 7);
            return texto;
        }
    }
}
