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
    /// CASO DE USO ACTUALIZAR (ventana independiente).
    ///
    /// TIENE BUSQUEDA PREVIA OBLIGATORIA: no se puede escribir el codigo a mano y
    /// guardar; primero hay que buscar la lavadora, y entonces sus datos actuales se
    /// PRECARGAN en el formulario. El codigo queda bloqueado porque es la llave.
    ///
    /// Al guardar se envia la mutacion "actualizarLavadora" y el controlador avisa por
    /// NotificadorDatos. Esta misma ventana escucha ese aviso para refrescar lo que
    /// muestra si OTRO cliente cambio la lavadora mientras la tenia abierta.
    /// </summary>
    public class VentanaActualizar : VentanaMarco
    {
        private readonly ControladorLavadoras controlador = new ControladorLavadoras(new ServicioLavadoras());

        private readonly CampoTexto txtCodigo = new CampoTexto();
        private readonly CamposLavadora campos = new CamposLavadora();

        private readonly TextBlock tituloFormulario = new TextBlock();
        private readonly TextBlock precioServidor = new TextBlock();
        private readonly BotonVoltia btnGuardar;

        private LavadoraData cargada;
        private bool guardando;

        public VentanaActualizar() : base("ACTUALIZAR LAVADORA", 880, 780, false, 0)
        {
            btnGuardar = BotonVoltia.Principal("Guardar cambios");
            btnGuardar.Width = 165;
            btnGuardar.Habilitado = false;

            Grid raiz = new Grid();
            raiz.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            raiz.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            raiz.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            FrameworkElement busqueda = CrearBusqueda();
            FrameworkElement formulario = CrearFormulario();
            FrameworkElement botones = CrearBotones();

            Grid.SetRow(busqueda, 0);
            Grid.SetRow(formulario, 1);
            Grid.SetRow(botones, 2);
            raiz.Children.Add(busqueda);
            raiz.Children.Add(formulario);
            raiz.Children.Add(botones);
            Contenido = raiz;

            // El formulario arranca bloqueado: primero hay que buscar.
            campos.PermitirEdicion(false);
            campos.BloquearCodigo();

            NotificadorDatos.DatosCambiaron += AlCambiarDatos;
            Closed += AlCerrar;

            Loaded += (s, e) =>
            {
                Anime.Escalonado(new UIElement[] { busqueda, formulario, botones }, 80, 240);
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

        private FrameworkElement CrearFormulario()
        {
            tituloFormulario.Text = "PRIMERO BUSQUE LA LAVADORA";
            tituloFormulario.FontFamily = Tema.Numeros;
            tituloFormulario.FontSize = 12;
            tituloFormulario.FontWeight = FontWeights.Bold;
            tituloFormulario.Foreground = Tema.Cian;
            tituloFormulario.Margin = new Thickness(0, 0, 0, 10);

            precioServidor.Text = "";
            precioServidor.FontFamily = Tema.Numeros;
            precioServidor.FontSize = 11;
            precioServidor.Foreground = Tema.TextoSecundario;
            precioServidor.Margin = new Thickness(2, 6, 0, 0);

            StackPanel contenido = new StackPanel();
            contenido.Children.Add(tituloFormulario);
            contenido.Children.Add(campos);
            contenido.Children.Add(Ui.Pista(
                "El codigo no se puede cambiar porque es la llave de la lavadora. El precio final lo recalcula el servidor."));
            contenido.Children.Add(precioServidor);

            ScrollViewer desplazable = Ui.Desplazable(contenido);
            desplazable.MaxHeight = 430;

            Border panel = Ui.Cristal(desplazable, new Thickness(16, 14, 16, 10));
            Grid.SetRow(panel, 1);
            return panel;
        }

        private FrameworkElement CrearBotones()
        {
            StackPanel botones = new StackPanel();
            botones.Orientation = Orientation.Horizontal;
            botones.HorizontalAlignment = HorizontalAlignment.Right;
            botones.Margin = new Thickness(0, 10, 0, 0);

            btnGuardar.Margin = new Thickness(0, 0, 10, 0);
            btnGuardar.Click += AlGuardar;
            botones.Children.Add(btnGuardar);

            BotonVoltia btnCerrar = BotonVoltia.Secundario("Cerrar");
            btnCerrar.Width = 100;
            btnCerrar.Click += (s, e) => Cerrar();
            botones.Children.Add(btnCerrar);

            return botones;
        }

        // ====================================================================
        //  Busqueda previa obligatoria
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
                    cargada = null;
                    campos.Limpiar();
                    campos.PermitirEdicion(false);
                    campos.BloquearCodigo();
                    btnGuardar.Habilitado = false;
                    tituloFormulario.Text = "NO SE ENCONTRO LA LAVADORA";
                    precioServidor.Text = "Verifique el codigo e intente de nuevo.";
                    return;
                }
                Precargar(lavadora);
                Sonidos.Click();
            }
            catch (Exception ex)
            {
                Aviso("No se pudo buscar", ex.Message);
            }
        }

        /// <summary>Pone en el formulario los datos actuales de la lavadora.</summary>
        private void Precargar(LavadoraData lavadora)
        {
            cargada = lavadora;
            campos.Mostrar(lavadora);
            campos.BloquearCodigo();
            campos.PermitirEdicion(true);
            btnGuardar.Habilitado = true;

            tituloFormulario.Text = "EDITANDO  " + lavadora.Codigo + "  -  " + lavadora.Marca;
            precioServidor.Text = "Precio final actual en el servidor: " + lavadora.PrecioFinalTexto +
                                  "  (con " + lavadora.CantidadMantenimientos + " mantenimiento(s))";
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

        // ====================================================================
        //  Guardar (mutacion)
        // ====================================================================

        private async void AlGuardar(object remitente, RoutedEventArgs argumentos)
        {
            if (cargada == null)
            {
                Aviso("Falta la busqueda", "Primero busque la lavadora que quiere actualizar.");
                return;
            }

            try
            {
                LavadoraUpdateInput input = campos.LeerUpdateInput();
                int codigo = cargada.Codigo;

                btnGuardar.Habilitado = false;
                guardando = true; // Para que el Observer no se confunda con nuestro propio cambio
                LavadoraData actualizada = await Task.Run(() => controlador.ActualizarAsync(codigo, input));
                guardando = false;

                Sonidos.Lavadora();
                Celebrar();
                Anime.Pulso(Marco);
                Precargar(actualizada);
                Informar("Lavadora actualizada",
                    "Se guardaron los cambios de la lavadora " + actualizada.Codigo +
                    ". Precio final: " + actualizada.PrecioFinalTexto + ".");
            }
            catch (Exception ex)
            {
                guardando = false;
                Anime.Sacudir(Marco);
                Aviso("No se pudo actualizar", ex.Message);
            }
            finally
            {
                btnGuardar.Habilitado = cargada != null;
            }
        }

        // ====================================================================
        //  Observer: si otro cliente cambia la lavadora, refrescamos
        // ====================================================================

        private void AlCambiarDatos()
        {
            if (guardando || cargada == null)
            {
                return; // Nuestro propio cambio: ya se refresco en AlGuardar
            }
            // El aviso puede llegar de otro hilo: se pasa al de la interfaz.
            Dispatcher.Invoke(new Action(RefrescarPorAviso));
        }

        private async void RefrescarPorAviso()
        {
            if (cargada == null)
            {
                return;
            }
            try
            {
                int codigo = cargada.Codigo;
                LavadoraData lavadora = await Task.Run(() => controlador.BuscarAsync(codigo, null));
                if (lavadora == null)
                {
                    cargada = null;
                    campos.Limpiar();
                    campos.PermitirEdicion(false);
                    campos.BloquearCodigo();
                    btnGuardar.Habilitado = false;
                    tituloFormulario.Text = "NO SE ENCONTRO LA LAVADORA";
                    precioServidor.Text = "Verifique el codigo e intente de nuevo.";
                    return;
                }
                Precargar(lavadora);
            }
            catch (Exception ex)
            {
                // Pudo ser que otro cliente la eliminara: se avisa y se limpia.
                cargada = null;
                btnGuardar.Habilitado = false;
                tituloFormulario.Text = "LA LAVADORA YA NO EXISTE";
                precioServidor.Text = "";
                Aviso("Datos desactualizados", ex.Message);
            }
        }

        private void AlCerrar(object remitente, EventArgs argumentos)
        {
            NotificadorDatos.DatosCambiaron -= AlCambiarDatos;
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
