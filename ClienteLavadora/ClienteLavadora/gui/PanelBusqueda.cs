using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ClienteLavadora.model;
using ClienteLavadora.util;

namespace ClienteLavadora.gui
{
    /// <summary>
    /// Barra de busqueda de las ventanas Consultar, Actualizar y Eliminar.
    /// El usuario puede buscar por UNO o por DOS criterios:
    ///   - el codigo, y/o
    ///   - la marca (lavadora) o el nombre (programa de lavado).
    /// Si escribe los dos, el objeto debe cumplir los dos.
    /// </summary>
    public class PanelBusqueda : Grid
    {
        /// <summary>Se dispara al presionar la lupa o la tecla Enter.</summary>
        public event RoutedEventHandler BuscarSolicitado; // Aviso a suscriptores

        private readonly CampoTexto txtCodigo;
        private readonly CampoTexto txtSegundo;

        public PanelBusqueda(TipoObjeto tipo)
        {
            Margin = new Thickness(0, 0, 0, 10);
            ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(16) });
            ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8) });
            ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            txtCodigo = new CampoTexto();
            txtSegundo = new CampoTexto();
            txtCodigo.TeclaPresionada += AlPresionarTecla;
            txtSegundo.TeclaPresionada += AlPresionarTecla;

            StackPanel celdaCodigo = Ui.Celda("Buscar por código", txtCodigo);
            StackPanel celdaSegundo = Ui.Celda("y/o por " + Catalogo.SegundoCriterio(tipo).ToLower(), txtSegundo);
            Grid.SetColumn(celdaCodigo, 0);
            Grid.SetColumn(celdaSegundo, 2);
            Children.Add(celdaCodigo);
            Children.Add(celdaSegundo);

            // Boton con la lupa
            Border lupa = new Border();
            lupa.Width = 42;
            lupa.Height = 32;
            lupa.VerticalAlignment = VerticalAlignment.Bottom;
            lupa.CornerRadius = new CornerRadius(8);
            lupa.Background = Brushes.White;
            lupa.BorderBrush = Ui.Pincel("#1E88E5");
            lupa.BorderThickness = new Thickness(1.5);
            lupa.Cursor = Cursors.Hand;
            lupa.ToolTip = "Buscar";
            Image icono = new Image();
            icono.Source = Ui.Imagen("buscar.png");
            icono.Width = 22;
            icono.Height = 22;
            lupa.Child = icono;
            lupa.MouseEnter += (s, e) => Anime.Hover(lupa, true);
            lupa.MouseLeave += (s, e) => Anime.Hover(lupa, false);
            lupa.MouseLeftButtonUp += (s, e) => PedirBusqueda();
            Grid.SetColumn(lupa, 4);
            Children.Add(lupa);
        }

        /// <summary>
        /// Lee los criterios. Devuelve false si el codigo no es un numero o si no escribio nada.
        /// Lo que el usuario deje vacio llega como null.
        /// </summary>
        public bool TryLeerCriterios(out int? codigo, out string texto)
        {
            codigo = null;
            texto = txtSegundo.Texto.Trim();

            string escrito = txtCodigo.Texto.Trim();
            if (escrito.Length > 0)
            {
                int valor;
                if (!int.TryParse(escrito, out valor) || valor <= 0)
                {
                    return false;
                }
                codigo = valor;
            }

            if (codigo == null && texto.Length == 0)
            {
                return false;
            }
            if (texto.Length == 0)
            {
                texto = null;
            }
            return true;
        }

        private void AlPresionarTecla(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PedirBusqueda();
            }
        }

        private void PedirBusqueda()
        {
            RoutedEventHandler manejador = BuscarSolicitado;
            if (manejador != null)
            {
                manejador(this, new RoutedEventArgs());
            }
        }
    }
}
