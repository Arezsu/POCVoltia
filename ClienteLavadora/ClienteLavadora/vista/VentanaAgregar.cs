using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ClienteLavadora.controlador;
using ClienteLavadora.modelo;
using ClienteLavadora.servicios;
using ClienteLavadora.util;
using ClienteLavadora.vista.controles;

namespace ClienteLavadora.vista
{
    /// <summary>
    /// CASO DE USO AGREGAR (ventana independiente).
    ///
    /// El usuario llena el formulario, el control lanza un mensaje claro si algo esta
    /// mal y, cuando todo esta bien, el CONTROLADOR guarda la lavadora en el servidor.
    ///
    /// VISTA PREVIA DEL PRECIO FINAL:
    /// mientras se escribe, la ventana pide al controlador una lavadora "de papel"
    /// (patron BUILDER, sin red) y muestra el precio final que va a quedar. Asi el
    /// usuario entiende de donde sale el numero antes de guardar.
    /// </summary>
    public class VentanaAgregar : VentanaMarco
    {
        private readonly ControladorLavadoras controlador = new ControladorLavadoras(new ServicioLavadoras());
        private readonly CamposLavadora campos = new CamposLavadora();

        private readonly TextBlock precioFinal = new TextBlock();
        private readonly TextBlock desglose = new TextBlock();
        private readonly BotonVoltia btnGuardar;

        public VentanaAgregar() : base("AGREGAR LAVADORA", 840, 740, false, 0)
        {
            btnGuardar = BotonVoltia.Principal("Guardar lavadora");
            btnGuardar.Width = 165;

            Grid raiz = new Grid();
            raiz.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            raiz.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            raiz.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // ---------- Formulario ----------
            Border panelFormulario = Ui.Cristal(campos, new Thickness(16, 14, 16, 6));
            Grid.SetRow(panelFormulario, 0);
            raiz.Children.Add(panelFormulario);

            // ---------- Vista previa del precio final ----------
            Border panelPrevio = Ui.Cristal(CrearVistaPrevia(), new Thickness(16, 10, 16, 10));
            panelPrevio.Margin = new Thickness(0, 8, 0, 0);
            Grid.SetRow(panelPrevio, 1);
            raiz.Children.Add(panelPrevio);

            // ---------- Botones ----------
            StackPanel botones = new StackPanel();
            botones.Orientation = Orientation.Horizontal;
            botones.HorizontalAlignment = HorizontalAlignment.Right;
            botones.Margin = new Thickness(0, 10, 0, 0);

            btnGuardar.Margin = new Thickness(0, 0, 10, 0);
            btnGuardar.Click += AlGuardar;
            botones.Children.Add(btnGuardar);

            BotonVoltia btnLimpiar = BotonVoltia.Secundario("Limpiar");
            btnLimpiar.Width = 110;
            btnLimpiar.Margin = new Thickness(0, 0, 10, 0);
            btnLimpiar.Click += (s, e) =>
            {
                campos.Limpiar();
                campos.PermitirCodigo();
                ActualizarVistaPrevia();
            };
            botones.Children.Add(btnLimpiar);

            BotonVoltia btnCerrar = BotonVoltia.Secundario("Cerrar");
            btnCerrar.Width = 100;
            btnCerrar.Click += (s, e) => Cerrar();
            botones.Children.Add(btnCerrar);

            Grid.SetRow(botones, 2);
            raiz.Children.Add(botones);

            Contenido = raiz;

            // Cada cambio en el formulario recalcula el precio final estimado.
            campos.Cambio += ActualizarVistaPrevia;

            Loaded += (s, e) =>
            {
                Anime.Escalonado(campos.Filas, 60, 260);
                Anime.Escalonado(new UIElement[] { panelPrevio, botones }, 80, 620);
                ActualizarVistaPrevia();
                campos.PermitirCodigo();
                btnGuardar.Habilitado = true;
            };
        }

        // ====================================================================
        //  Vista previa
        // ====================================================================

        private FrameworkElement CrearVistaPrevia()
        {
            StackPanel izquierda = new StackPanel();
            izquierda.VerticalAlignment = VerticalAlignment.Center;

            TextBlock rotulo = Ui.Rotulo("PRECIO FINAL ESTIMADO");
            rotulo.Margin = new Thickness(0, 0, 0, 3);
            izquierda.Children.Add(rotulo);

            desglose.Text = "Escriba el precio base y la capacidad para ver el calculo.";
            desglose.FontFamily = Tema.Numeros;
            desglose.FontSize = 10.5;
            desglose.Foreground = Tema.TextoSecundario;
            izquierda.Children.Add(desglose);

            TextBlock nota = Ui.Pista("El servidor recalcula este valor al guardar (precio base + bono por capacidad + bono inverter).");
            izquierda.Children.Add(nota);

            precioFinal.Text = "$ --";
            precioFinal.FontFamily = Tema.Numeros;
            precioFinal.FontSize = 21;
            precioFinal.FontWeight = FontWeights.Bold;
            precioFinal.Foreground = Tema.Ambar;
            precioFinal.VerticalAlignment = VerticalAlignment.Center;
            precioFinal.HorizontalAlignment = HorizontalAlignment.Right;

            Border pantalla = Ui.Pantalla(precioFinal);
            pantalla.MinWidth = 190;
            pantalla.VerticalAlignment = VerticalAlignment.Center;
            pantalla.Margin = new Thickness(14, 0, 0, 0);

            Grid rejilla = new Grid();
            rejilla.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            rejilla.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            rejilla.Children.Add(izquierda);
            Grid.SetColumn(pantalla, 1);
            rejilla.Children.Add(pantalla);
            return rejilla;
        }

        /// <summary>
        /// Construye una lavadora "de papel" con lo que hay escrito y muestra su precio
        /// final. Si todavia faltan numeros, muestra "--" sin molestar con mensajes.
        /// </summary>
        private void ActualizarVistaPrevia()
        {
            LavadoraInput parcial = campos.LeerParaVistaPrevia();
            if (parcial == null)
            {
                precioFinal.Text = "$ --";
                desglose.Text = "Escriba el precio base y la capacidad para ver el calculo.";
                return;
            }

            LavadoraData previa = controlador.VistaPrevia(parcial);
            precioFinal.Text = previa.PrecioFinalTexto;

            double bonoCapacidad = LavadoraData.BonoPorCapacidad(parcial.CapacidadCargaKilos);
            double bonoInverter = LavadoraData.BonoPorInverter(parcial.TecnologiaInverter);
            desglose.Text = "Base " + Formato.Moneda(parcial.PrecioBase) +
                            " + capacidad " + Formato.Moneda(bonoCapacidad) +
                            " + inverter " + Formato.Moneda(bonoInverter);
        }

        // ====================================================================
        //  Guardar
        // ====================================================================

        private async void AlGuardar(object remitente, RoutedEventArgs argumentos)
        {
            try
            {
                // 1. Se leen y validan los datos (si algo esta mal, salta al catch).
                LavadoraInput input = campos.LeerInput();

                // 2. Se envia al servidor sin congelar la ventana.
                btnGuardar.Habilitado = false;
                LavadoraData creada = await Task.Run(() => controlador.AgregarAsync(input));

                // 3. Todo salio bien: suena, hay confeti y se avisa a las otras ventanas.
                Sonidos.Lavadora();
                Celebrar();
                Anime.Pulso(Marco);
                Informar("Lavadora agregada",
                    "Se guardo la lavadora " + creada.Codigo + " - " + creada.Marca +
                    " con precio final " + creada.PrecioFinalTexto + ".");

                campos.Limpiar();
                campos.PermitirCodigo();
                ActualizarVistaPrevia();
                campos.EnfocarCodigo();
            }
            catch (Exception ex)
            {
                Anime.Sacudir(Marco);
                Aviso("No se pudo agregar", ex.Message);
            }
            finally
            {
                btnGuardar.Habilitado = true;
            }
        }
    }
}
