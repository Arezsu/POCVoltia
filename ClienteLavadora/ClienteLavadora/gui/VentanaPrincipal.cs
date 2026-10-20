using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using ClienteLavadora.model;
using ClienteLavadora.util;

namespace ClienteLavadora.gui
{
    /// <summary>
    /// Ventana principal: menu, logo siempre visible, la puerta grande de la lavadora y
    /// botones rapidos. Desde aqui se abre cada caso de uso en SU PROPIA ventana
    /// (una funcion por ventana).
    /// </summary>
    public class VentanaPrincipal : VentanaMarco
    {
        // Los cinco casos de uso de cada objeto
        private enum Accion
        {
            Agregar,
            Consultar,
            Actualizar,
            Eliminar,
            Listar
        }

        private readonly Border barraMenu = new Border();
        private readonly Border placaLogo = new Border();
        private readonly OjoDeBuey ojo = new OjoDeBuey(250);
        private readonly StackPanel atajos = new StackPanel();

        public VentanaPrincipal() : base("VOLTIA  PANEL", 700, 730, false, 100)
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            ArmarMenu();
            ArmarLogo();
            ArmarAtajos();

            ojo.HorizontalAlignment = HorizontalAlignment.Center;
            ojo.VerticalAlignment = VerticalAlignment.Center;
            ojo.Margin = new Thickness(0, 8, 0, 8);

            Grid cuerpo = new Grid();
            cuerpo.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            cuerpo.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            cuerpo.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            cuerpo.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            Grid.SetRow(barraMenu, 0);
            Grid.SetRow(placaLogo, 1);
            Grid.SetRow(ojo, 2);
            Grid.SetRow(atajos, 3);
            cuerpo.Children.Add(barraMenu);
            cuerpo.Children.Add(placaLogo);
            cuerpo.Children.Add(ojo);
            cuerpo.Children.Add(atajos);
            Contenido = cuerpo;

            Loaded += (s, e) =>
            {
                Sonidos.Click(); // El sonido suena UNA sola vez, al abrir la aplicacion
                Anime.Escalonado(new UIElement[] { barraMenu, placaLogo, atajos }, 110, 450);

                // Cuando termina la entrada, la puerta flota suavemente
                Anime.Flotar(ojo, 5, 3200, 1600);
            };
        }

        // ================= Armado de la pantalla =================

        /// <summary>
        /// Menu: Archivo, Programa de lavado, Lavadora y Ayuda.
        /// Los titulos de arriba son blancos y los items de adentro negros para que se lean.
        /// </summary>
        private void ArmarMenu()
        {
            Menu menu = new Menu();
            menu.Background = Brushes.Transparent;
            menu.FontSize = 14;

            MenuItem archivo = Encabezado("_Archivo");
            archivo.Items.Add(Item("_Salir", () => Application.Current.Shutdown()));
            menu.Items.Add(archivo);

            menu.Items.Add(MenuDeObjeto("_Programa de lavado", TipoObjeto.ProgramaLavado));
            menu.Items.Add(MenuDeObjeto("_Lavadora", TipoObjeto.Lavadora));

            MenuItem ayuda = Encabezado("A_yuda");
            ayuda.Items.Add(Item("_Acerca de...", () => Abrir(new VentanaAcercaDe())));
            menu.Items.Add(ayuda);

            barraMenu.Background = Ui.Pincel("#37474F");
            barraMenu.CornerRadius = new CornerRadius(10);
            barraMenu.Padding = new Thickness(6, 2, 6, 2);
            barraMenu.Child = menu;
        }

        private MenuItem MenuDeObjeto(string titulo, TipoObjeto tipo)
        {
            MenuItem menu = Encabezado(titulo);
            menu.Items.Add(Item("_Adicionar", () => Abrir(tipo, Accion.Agregar)));
            menu.Items.Add(Item("_Consultar", () => Abrir(tipo, Accion.Consultar)));
            menu.Items.Add(Item("A_ctualizar", () => Abrir(tipo, Accion.Actualizar)));
            menu.Items.Add(Item("_Eliminar", () => Abrir(tipo, Accion.Eliminar)));
            menu.Items.Add(Item("_Listar", () => Abrir(tipo, Accion.Listar)));
            return menu;
        }

        private static MenuItem Encabezado(string texto)
        {
            MenuItem item = new MenuItem();
            item.Header = texto;
            item.Foreground = Brushes.White;
            return item;
        }

        private static MenuItem Item(string texto, System.Action alClic)
        {
            MenuItem item = new MenuItem();
            item.Header = texto;
            item.Foreground = Brushes.Black;
            item.Click += (s, e) => alClic();
            return item;
        }

        /// <summary>El logo siempre visible, sobre una placa blanca con borde amarillo.</summary>
        private void ArmarLogo()
        {
            Image logo = new Image();
            logo.Source = Ui.Imagen("logo.jpg");
            logo.Height = 78;
            logo.Stretch = Stretch.Uniform;

            placaLogo.Margin = new Thickness(0, 12, 0, 0);
            placaLogo.Background = Brushes.White;
            placaLogo.CornerRadius = new CornerRadius(14);
            placaLogo.BorderBrush = Ui.Pincel("#FFC107");
            placaLogo.BorderThickness = new Thickness(2);
            placaLogo.Padding = new Thickness(8);
            placaLogo.Child = logo;
        }

        /// <summary>Botones rapidos: una fila para el programa de lavado y otra para la lavadora.</summary>
        private void ArmarAtajos()
        {
            atajos.Children.Add(Rotulo("PROGRAMA DE LAVADO", 0));
            atajos.Children.Add(FilaDeBotones(TipoObjeto.ProgramaLavado));
            atajos.Children.Add(Rotulo("LAVADORA", 4));
            atajos.Children.Add(FilaDeBotones(TipoObjeto.Lavadora));
        }

        private static TextBlock Rotulo(string texto, double arriba)
        {
            TextBlock rotulo = new TextBlock();
            rotulo.Text = texto;
            rotulo.FontSize = 12;
            rotulo.FontWeight = FontWeights.Bold;
            rotulo.Foreground = Ui.Pincel("#37474F");
            rotulo.Margin = new Thickness(6, arriba, 0, 0);
            return rotulo;
        }

        private UniformGrid FilaDeBotones(TipoObjeto tipo)
        {
            UniformGrid fila = new UniformGrid();
            fila.Columns = 5;
            fila.Children.Add(Atajo("Adicionar", Ui.Azul(), Brushes.White, tipo, Accion.Agregar));
            fila.Children.Add(Atajo("Consultar", Ui.Azul(), Brushes.White, tipo, Accion.Consultar));
            fila.Children.Add(Atajo("Actualizar", Ui.Amarillo(), Ui.Pincel("#263238"), tipo, Accion.Actualizar));
            fila.Children.Add(Atajo("Eliminar", Ui.Rojo(), Brushes.White, tipo, Accion.Eliminar));
            fila.Children.Add(Atajo("Listar", Ui.Gris(), Brushes.White, tipo, Accion.Listar));
            return fila;
        }

        private BotonVoltia Atajo(string texto, Brush fondo, Brush colorTexto, TipoObjeto tipo, Accion accion)
        {
            BotonVoltia boton = new BotonVoltia(texto, fondo, colorTexto);
            boton.Margin = new Thickness(4);
            boton.MinWidth = 0;
            boton.Click += (s, e) => Abrir(tipo, accion);
            return boton;
        }

        // ================= Abrir ventanas =================

        /// <summary>Crea la ventana que corresponde al objeto y al caso de uso.</summary>
        private void Abrir(TipoObjeto tipo, Accion accion)
        {
            Window ventana;
            switch (accion)
            {
                case Accion.Agregar:
                    ventana = new VentanaAgregar(tipo);
                    break;
                case Accion.Consultar:
                    ventana = new VentanaConsultar(tipo);
                    break;
                case Accion.Actualizar:
                    ventana = new VentanaActualizar(tipo);
                    break;
                case Accion.Eliminar:
                    ventana = new VentanaEliminar(tipo);
                    break;
                default:
                    ventana = new VentanaListar(tipo);
                    break;
            }
            Abrir(ventana);
        }

        /// <summary>
        /// Abre una ventana hija. Con Owner = this, la hija se cierra si se cierra la principal
        /// y siempre aparece encima de ella.
        /// </summary>
        private void Abrir(Window ventana)
        {
            ventana.Owner = this;
            ventana.Show(); // Show (no ShowDialog): se pueden tener varias abiertas a la vez
        }
    }
}
