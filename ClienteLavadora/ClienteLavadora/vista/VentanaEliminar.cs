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
    /// CASO DE USO ELIMINAR (ventana independiente).
    ///
    /// TIENE BUSQUEDA PREVIA OBLIGATORIA y muestra TODOS los datos de la lavadora antes
    /// de borrar, para que el usuario vea exactamente que va a perder. El boton de
    /// eliminar es rojo y pide una confirmacion destructiva (dialogo con boton rojo).
    ///
    /// OJO CON LA RELACION MAESTRO-DETALLE: al eliminar la lavadora se van con ella
    /// todos sus mantenimientos. La ventana lo advierte con el numero exacto.
    /// </summary>
    public class VentanaEliminar : VentanaMarco
    {
        private readonly ControladorLavadoras controlador = new ControladorLavadoras(new ServicioLavadoras());

        private readonly CampoTexto txtCodigo = new CampoTexto();

        private readonly TextBlock tituloFicha = new TextBlock();
        private readonly TextBlock advertencia = new TextBlock();
        private readonly CamposLavadora ficha = new CamposLavadora();
        private readonly ListaMantenimientos lista = new ListaMantenimientos();
        private readonly BotonVoltia btnEliminar;

        private LavadoraData cargada;

        public VentanaEliminar() : base("ELIMINAR LAVADORA", 880, 780, false, 0)
        {
            ficha.PermitirEdicion(false);
            ficha.BloquearCodigo();
            btnEliminar = BotonVoltia.Peligro("Eliminar definitivamente");
            btnEliminar.Width = 200;
            btnEliminar.Habilitado = false;

            Grid raiz = new Grid();
            raiz.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            raiz.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            raiz.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            FrameworkElement busqueda = CrearBusqueda();
            FrameworkElement detalle = CrearDetalle();
            FrameworkElement botones = CrearBotones();

            Grid.SetRow(busqueda, 0);
            Grid.SetRow(detalle, 1);
            Grid.SetRow(botones, 2);
            raiz.Children.Add(busqueda);
            raiz.Children.Add(detalle);
            raiz.Children.Add(botones);
            Contenido = raiz;

            Loaded += (s, e) =>
            {
                Anime.Escalonado(new UIElement[] { busqueda, detalle, botones }, 80, 240);
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


            BotonVoltia btnBuscar = BotonVoltia.Principal("Buscar");
            btnBuscar.Width = 110;
            btnBuscar.Height = 32;
            btnBuscar.VerticalAlignment = VerticalAlignment.Bottom;
            btnBuscar.Click += async (s, e) => await BuscarAsync();
            Grid.SetColumn(btnBuscar, 2);
            rejilla.Children.Add(btnBuscar);

            Border panel = Ui.Cristal(rejilla, new Thickness(12, 10, 12, 10));
            panel.Margin = new Thickness(0, 0, 0, 10);
            return panel;
        }

        private FrameworkElement CrearDetalle()
        {
            tituloFicha.Text = "PRIMERO BUSQUE LA LAVADORA";
            tituloFicha.FontFamily = Tema.Numeros;
            tituloFicha.FontSize = 12;
            tituloFicha.FontWeight = FontWeights.Bold;
            tituloFicha.Foreground = Tema.Cian;
            tituloFicha.Margin = new Thickness(0, 0, 0, 8);

            advertencia.Text = "Al eliminar una lavadora tambien se eliminan TODOS sus mantenimientos.";
            advertencia.FontFamily = Tema.Fuente;
            advertencia.FontSize = 11;
            advertencia.Foreground = Tema.Error;
            advertencia.Margin = new Thickness(0, 0, 0, 8);
            advertencia.TextWrapping = TextWrapping.Wrap;

            ficha.Limpiar();

            ScrollViewer desplazable = Ui.Desplazable(ficha);
            desplazable.MaxHeight = 260;

            TextBlock rotulo = Ui.Rotulo("MANTENIMIENTOS QUE SE PERDERIAN");
            rotulo.Margin = new Thickness(0, 8, 0, 6);

            Grid panel = new Grid();
            panel.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            panel.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            panel.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            panel.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            panel.RowDefinitions.Add(new RowDefinition { Height = new GridLength(140) });

            panel.Children.Add(tituloFicha);
            Grid.SetRow(advertencia, 1);
            panel.Children.Add(advertencia);
            Grid.SetRow(desplazable, 2);
            panel.Children.Add(desplazable);
            Grid.SetRow(rotulo, 3);
            panel.Children.Add(rotulo);
            Grid.SetRow(lista, 4);
            panel.Children.Add(lista);

            return Ui.Cristal(panel, new Thickness(14));
        }

        private FrameworkElement CrearBotones()
        {
            StackPanel botones = new StackPanel();
            botones.Orientation = Orientation.Horizontal;
            botones.HorizontalAlignment = HorizontalAlignment.Right;
            botones.Margin = new Thickness(0, 10, 0, 0);

            btnEliminar.Margin = new Thickness(0, 0, 10, 0);
            btnEliminar.Click += AlEliminar;
            botones.Children.Add(btnEliminar);

            BotonVoltia btnCerrar = BotonVoltia.Secundario("Cerrar");
            btnCerrar.Width = 100;
            btnCerrar.Click += (s, e) => Cerrar();
            botones.Children.Add(btnCerrar);

            return botones;
        }

        // ====================================================================
        //  Busqueda previa
        // ====================================================================

        private async Task BuscarAsync()
        {
            try
            {
                int? codigo = LeerCodigoOpcional();
                if (codigo == null)
                {
                    throw new ArgumentException("Escriba el codigo de la lavadora para buscar.");
                }

                LavadoraData lavadora = await Task.Run(() => controlador.BuscarAsync(codigo, null));
                if (lavadora == null)
                {
                    Limpiar();
                    Aviso("Lavadora no encontrada", "No existe una lavadora con el codigo " + codigo + ".");
                    return;
                }
                Mostrar(lavadora);
                Sonidos.Click();
            }
            catch (Exception ex)
            {
                Limpiar();
                Aviso("No se pudo buscar", ex.Message);
            }
        }

        private void Mostrar(LavadoraData lavadora)
        {
            cargada = lavadora;
            tituloFicha.Text = "LAVADORA  " + lavadora.Codigo + "  -  " + lavadora.Marca;
            ficha.Mostrar(lavadora);
            lista.Mostrar(lavadora.ListaMantenimientos, "Esta lavadora no tiene mantenimientos: solo se borrara la lavadora.");
            advertencia.Text = "Al eliminar la lavadora " + lavadora.Codigo + " tambien se eliminaran sus " +
                               lavadora.CantidadMantenimientos + " mantenimiento(s). Esta accion no se puede deshacer.";
            btnEliminar.Habilitado = true;
        }

        private int? LeerCodigoOpcional()
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
            cargada = null;
            btnEliminar.Habilitado = false;
            tituloFicha.Text = "PRIMERO BUSQUE LA LAVADORA";
            advertencia.Text = "Al eliminar una lavadora tambien se eliminan TODOS sus mantenimientos.";
            ficha.Limpiar();
            lista.Mostrar(null, "Sin mantenimientos para mostrar.");
        }

        // ====================================================================
        //  Eliminar (con confirmacion destructiva)
        // ====================================================================

        private async void AlEliminar(object remitente, RoutedEventArgs argumentos)
        {
            if (cargada == null)
            {
                Aviso("Falta la busqueda", "Primero busque la lavadora que quiere eliminar.");
                return;
            }

            int codigo = cargada.Codigo;
            string mensaje = "Se eliminara la lavadora " + codigo + " - " + cargada.Marca +
                             " y sus " + cargada.CantidadMantenimientos + " mantenimiento(s)." +
                             Environment.NewLine + Environment.NewLine +
                             "Esta seguro? Esta accion no se puede deshacer.";

            if (!Confirmar("CONFIRMAR ELIMINACION", mensaje, "Si, eliminar", true))
            {
                return; // El usuario se arrepintio: no se hace nada
            }

            try
            {
                btnEliminar.Habilitado = false;
                await Task.Run(() => controlador.EliminarAsync(codigo));

                Sonidos.Click();
                Limpiar();
                Informar("Lavadora eliminada", "Se elimino la lavadora " + codigo + " con todos sus mantenimientos.");
            }
            catch (Exception ex)
            {
                Anime.Sacudir(Marco);
                Aviso("No se pudo eliminar", ex.Message);
                btnEliminar.Habilitado = cargada != null;
            }
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
