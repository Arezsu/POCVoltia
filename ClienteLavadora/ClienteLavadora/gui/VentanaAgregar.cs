using System;
using System.Windows;
using System.Windows.Controls;
using ClienteLavadora.model;
using ClienteLavadora.servicios;
using ClienteLavadora.util;

namespace ClienteLavadora.gui
{
    /// <summary>
    /// Caso de uso ADICIONAR: el usuario llena el formulario y el objeto
    /// (lavadora o programa de lavado) se guarda en el servidor.
    /// </summary>
    public class VentanaAgregar : VentanaMarco
    {
        private readonly TipoObjeto tipo;
        private readonly ICampos campos;               // Formulario (de lavadora o de programa)
        private readonly IServicioCrud servicio;       // Habla con el servidor
        private readonly BotonVoltia btnAceptar;
        private readonly StackPanel panelBotones;

        public VentanaAgregar(TipoObjeto tipo) : base("ADICIONAR " + Catalogo.Titulo(tipo), 640, 730, true, 100)
        {
            this.tipo = tipo;
            campos = FabricaGui.CrearCampos(tipo);
            servicio = FabricaServicios.Obtener(tipo);
            campos.ConfigurarParaAgregar();

            btnAceptar = new BotonVoltia("Aceptar", Ui.Azul());
            btnAceptar.Width = 150;
            btnAceptar.Margin = new Thickness(6, 0, 6, 0);
            btnAceptar.Click += BtnAceptar_Click;

            BotonVoltia btnLimpiar = new BotonVoltia("Limpiar", Ui.Gris());
            btnLimpiar.Width = 150;
            btnLimpiar.Margin = new Thickness(6, 0, 6, 0);
            btnLimpiar.Click += (s, e) => campos.Limpiar(); // Funcion anonima corta

            panelBotones = new StackPanel();
            panelBotones.Orientation = Orientation.Horizontal;
            panelBotones.HorizontalAlignment = HorizontalAlignment.Center;
            panelBotones.Children.Add(btnAceptar);
            panelBotones.Children.Add(btnLimpiar);

            // El cuerpo tiene dos filas: el formulario arriba y los botones abajo
            Grid cuerpo = new Grid();
            cuerpo.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            cuerpo.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            Grid.SetRow(campos.Vista, 0);
            Grid.SetRow(panelBotones, 1);
            cuerpo.Children.Add(campos.Vista);
            cuerpo.Children.Add(panelBotones);
            Contenido = cuerpo;

            // Cuando la ventana carga, las filas del formulario entran una tras otra
            Loaded += (s, e) =>
            {
                Anime.Escalonado(campos.Filas, 70, 350);
                Anime.Escalonado(new UIElement[] { panelBotones }, 70, 900);
            };
        }

        // async = el metodo espera al servidor sin congelar la ventana
        private async void BtnAceptar_Click(object sender, RoutedEventArgs e) // Espera sin congelar
        {
            try
            {
                // 1. Se leen y validan los datos del formulario
                object datos = campos.ObtenerInput();

                // 2. Se mandan al servidor y se espera su respuesta
                btnAceptar.Habilitado = false;
                DatoBase creado = await servicio.AgregarAsync(datos); // Espera la respuesta

                // 3. Todo salio bien: suena la lavadora, hay confeti y se avisa a las ventanas de Listar
                Sonidos.Lavadora();
                Anime.Pulso(Marco);
                Marco.Celebrar();
                Notificador.Avisar();
                Ui.Informacion(this, "Se adicionó " + Catalogo.ConArticulo(tipo) + " con el código " + creado.Codigo + ".", "Adicionado");
                campos.Limpiar();
            }
            catch (Exception ex)
            {
                // Cualquier error (dato mal escrito, codigo repetido, servidor apagado...) sacude la ventana
                Anime.Sacudir(Marco);
                Ui.Aviso(this, ex.Message, "No se pudo adicionar");
            }
            finally
            {
                btnAceptar.Habilitado = true;
            }
        }
    }
}
