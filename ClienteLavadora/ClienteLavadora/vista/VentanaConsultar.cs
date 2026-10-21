using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ClienteLavadora.controlador;
using ClienteLavadora.modelo;
using ClienteLavadora.servicios;
using ClienteLavadora.util;
using ClienteLavadora.vista.controles;

namespace ClienteLavadora.vista
{
    /// <summary>
    /// CASO DE USO CONSULTAR (ventana independiente).
    ///
    /// Busca por codigo usando la consulta
    /// del servidor y muestra TODOS los campos del resultado,
    /// incluida la lista de mantenimientos.
    ///
    /// Todo se ve en modo lectura: esta ventana no modifica nada.
    /// </summary>
    public class VentanaConsultar : VentanaMarco
    {
        private readonly ControladorLavadoras controlador = new ControladorLavadoras(new ServicioLavadoras());

        private readonly CampoTexto txtCodigo = new CampoTexto();

        private readonly TextBlock tituloResultado = new TextBlock();
        private readonly CamposLavadora ficha = new CamposLavadora();
        private readonly ListaMantenimientos lista = new ListaMantenimientos();

        public VentanaConsultar() : base("CONSULTAR LAVADORA", 860, 760, false, 0)
        {
            ficha.PermitirEdicion(false);
            ficha.BloquearCodigo();
            Grid raiz = new Grid();
            raiz.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            raiz.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            FrameworkElement busqueda = CrearBusqueda();
            FrameworkElement resultado = CrearResultado();

            Grid.SetRow(busqueda, 0);
            Grid.SetRow(resultado, 1);
            raiz.Children.Add(busqueda);
            raiz.Children.Add(resultado);
            Contenido = raiz;

            Loaded += (s, e) =>
            {
                Anime.Escalonado(new UIElement[] { busqueda, resultado }, 90, 260);
                txtCodigo.Enfocar();
            };
        }

        // ====================================================================
        //  Armado
        // ====================================================================

        private FrameworkElement CrearBusqueda()
        {
            Grid rejilla = new Grid();
            rejilla.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            rejilla.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) });
            rejilla.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            txtCodigo.Fuente = Tema.Numeros;
            txtCodigo.TeclaPresionada += AlPresionarTecla;

            StackPanel celdaCodigo = Ui.Celda("Buscar por codigo", txtCodigo);
            Grid.SetColumn(celdaCodigo, 0);
            rejilla.Children.Add(celdaCodigo);


            StackPanel botones = new StackPanel();
            botones.Orientation = Orientation.Horizontal;
            botones.VerticalAlignment = VerticalAlignment.Bottom;

            BotonVoltia btnBuscar = BotonVoltia.Principal("Buscar");
            btnBuscar.Width = 105;
            btnBuscar.Height = 32;
            btnBuscar.Margin = new Thickness(0, 0, 8, 0);
            btnBuscar.Click += async (s, e) => await BuscarAsync();
            botones.Children.Add(btnBuscar);

            BotonVoltia btnLimpiar = BotonVoltia.Secundario("Limpiar");
            btnLimpiar.Width = 100;
            btnLimpiar.Height = 32;
            btnLimpiar.Click += (s, e) => Limpiar();
            botones.Children.Add(btnLimpiar);

            Grid.SetColumn(botones, 2);
            rejilla.Children.Add(botones);

            Border panel = Ui.Cristal(rejilla, new Thickness(12, 10, 12, 10));
            panel.Margin = new Thickness(0, 0, 0, 10);
            return panel;
        }

        private FrameworkElement CrearResultado()
        {
            tituloResultado.Text = "RESULTADO";
            tituloResultado.FontFamily = Tema.Numeros;
            tituloResultado.FontSize = 12;
            tituloResultado.FontWeight = FontWeights.Bold;
            tituloResultado.Foreground = Tema.Cian;
            tituloResultado.Margin = new Thickness(0, 0, 0, 8);

            ficha.Limpiar();

            TextBlock rotulo = Ui.Rotulo("MANTENIMIENTOS DE LA LAVADORA");
            rotulo.Margin = new Thickness(0, 6, 0, 6);

            Grid panel = new Grid();
            panel.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            panel.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            panel.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            panel.RowDefinitions.Add(new RowDefinition { Height = new GridLength(190) });

            panel.Children.Add(tituloResultado);
            ScrollViewer desplazable = Ui.Desplazable(ficha);
            Grid.SetRow(desplazable, 1);
            panel.Children.Add(desplazable);
            Grid.SetRow(rotulo, 2);
            panel.Children.Add(rotulo);
            Grid.SetRow(lista, 3);
            panel.Children.Add(lista);

            return Ui.Cristal(panel, new Thickness(14));
        }

        // ====================================================================
        //  Buscar
        // ====================================================================

        private async Task BuscarAsync()
        {
            try
            {
                int? codigo = LeerCodigo();
                if (codigo == null)
                {
                    throw new ArgumentException("Escriba el codigo de la lavadora para buscar.");
                }

                LavadoraData lavadora = await Task.Run(() => controlador.BuscarAsync(codigo, null));
                if (lavadora == null)
                {
                    tituloResultado.Text = "SIN RESULTADO";
                    ficha.Limpiar();
                    lista.Mostrar(null, "Sin mantenimientos para mostrar.");
                    return;
                }

                tituloResultado.Text = "RESULTADO  " + lavadora.Codigo + "  -  " + lavadora.Marca;
                ficha.Mostrar(lavadora);
                lista.Mostrar(lavadora.ListaMantenimientos, "Esta lavadora no tiene mantenimientos registrados.");
                Anime.Pulso(Marco);
            }
            catch (Exception ex)
            {
                tituloResultado.Text = "SIN RESULTADO";
                ficha.Limpiar();
                lista.Mostrar(null, "Sin mantenimientos para mostrar.");
                if (IsLoaded && IsVisible)
                {
                    Aviso("No se pudo consultar", ex.Message);
                }
            }
        }

        private int? LeerCodigo()
        {
            string texto = txtCodigo.Texto.Trim();
            if (texto.Length == 0)
            {
                return null;
            }
            return Formato.LeerEntero(texto, 1, "El codigo debe ser un numero entero mayor que cero.");
        }

        private void Limpiar()
        {
            txtCodigo.Texto = "";
            tituloResultado.Text = "RESULTADO";
            ficha.Limpiar();
            lista.Mostrar(null, "Sin mantenimientos para mostrar.");
            txtCodigo.Enfocar();
        }

        private async void AlPresionarTecla(object remitente, KeyEventArgs argumentos)
        {
            if (argumentos.Key == Key.Enter)
            {
                await BuscarAsync();
            }
        }
    }
}
