using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ClienteLavadora.model;
using ClienteLavadora.servicios;
using ClienteLavadora.util;

namespace ClienteLavadora.gui
{
    /// <summary>
    /// Caso de uso ACTUALIZAR: primero se busca el objeto, se muestra TODA su informacion,
    /// el usuario cambia lo que quiera y se manda al servidor.
    /// </summary>
    public class VentanaActualizar : VentanaMarco
    {
        private readonly TipoObjeto tipo;
        private readonly ICampos campos;
        private readonly PanelBusqueda busqueda;
        private readonly IServicioCrud servicio;
        private readonly BotonVoltia btnActualizar;
        private readonly StackPanel panelBotones;

        // Codigo del objeto que se encontro. Se actualiza SIEMPRE ese codigo.
        private int codigoCargado = -1;

        public VentanaActualizar(TipoObjeto tipo) : base("ACTUALIZAR " + Catalogo.Titulo(tipo), 640, 730, true, 100)
        {
            this.tipo = tipo;
            campos = FabricaGui.CrearCampos(tipo);
            busqueda = FabricaGui.CrearBusqueda(tipo);
            servicio = FabricaServicios.Obtener(tipo);
            campos.ConfigurarParaBuscar();

            busqueda.BuscarSolicitado += async (s, e) => await BuscarAsync();

            btnActualizar = new BotonVoltia("Actualizar", Ui.Amarillo(), Ui.Pincel("#263238"));
            btnActualizar.Width = 150;
            btnActualizar.Margin = new Thickness(6, 0, 6, 0);
            btnActualizar.Habilitado = false; // Se habilita cuando se encuentra algo
            btnActualizar.Click += BtnActualizar_Click;

            BotonVoltia btnCerrar = new BotonVoltia("Cerrar", Ui.Gris());
            btnCerrar.Width = 150;
            btnCerrar.Margin = new Thickness(6, 0, 6, 0);
            btnCerrar.Click += (s, e) => Marco.Cerrar();

            panelBotones = new StackPanel();
            panelBotones.Orientation = Orientation.Horizontal;
            panelBotones.HorizontalAlignment = HorizontalAlignment.Center;
            panelBotones.Children.Add(btnActualizar);
            panelBotones.Children.Add(btnCerrar);

            Grid cuerpo = new Grid();
            cuerpo.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            cuerpo.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            cuerpo.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            Grid.SetRow(busqueda, 0);
            Grid.SetRow(campos.Vista, 1);
            Grid.SetRow(panelBotones, 2);
            cuerpo.Children.Add(busqueda);
            cuerpo.Children.Add(campos.Vista);
            cuerpo.Children.Add(panelBotones);
            Contenido = cuerpo;

            Loaded += (s, e) =>
            {
                Anime.Escalonado(new UIElement[] { busqueda }, 70, 300);
                Anime.Escalonado(campos.Filas, 60, 450);
                Anime.Escalonado(new UIElement[] { panelBotones }, 70, 950);
            };
        }

        private async Task BuscarAsync()
        {
            int? codigo;
            string texto;
            if (!busqueda.TryLeerCriterios(out codigo, out texto))
            {
                Anime.Sacudir(Marco);
                Ui.Aviso(this, "Escriba un código numérico, un dato de " + Catalogo.SegundoCriterio(tipo).ToLower() + " o los dos.", "Falta un criterio");
                return;
            }

            try
            {
                DatoBase dato = await servicio.BuscarAsync(codigo, texto);

                if (dato == null)
                {
                    BloquearEdicion();
                    campos.Limpiar();
                    Anime.Sacudir(Marco);
                    Ui.Aviso(this, "No se encontró " + Catalogo.ConArticulo(tipo) + " con esos datos.", "No encontrado");
                    return;
                }

                // Se muestra toda la informacion y se habilita la edicion
                campos.Mostrar(dato);
                campos.PermitirEdicion(true);
                codigoCargado = dato.Codigo;
                btnActualizar.Habilitado = true;
                Anime.Escalonado(campos.Filas, 50, 0);
            }
            catch (Exception ex)
            {
                Anime.Sacudir(Marco);
                Ui.Aviso(this, ex.Message, "Error");
            }
        }

        private async void BtnActualizar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                object cambios = campos.ObtenerUpdateInput();

                btnActualizar.Habilitado = false;
                DatoBase actualizado = await servicio.ActualizarAsync(codigoCargado, cambios);

                campos.Mostrar(actualizado); // Se ven los datos calculados nuevos
                Anime.Pulso(Marco);
                Marco.Celebrar();
                Notificador.Avisar();
                Ui.Informacion(this, "Se actualizó " + Catalogo.ConArticulo(tipo) + " con éxito.", "Actualizado");
            }
            catch (Exception ex)
            {
                Anime.Sacudir(Marco);
                Ui.Aviso(this, ex.Message, "No se pudo actualizar");
            }
            finally
            {
                btnActualizar.Habilitado = codigoCargado > 0;
            }
        }

        private void BloquearEdicion()
        {
            codigoCargado = -1;
            campos.PermitirEdicion(false);
            btnActualizar.Habilitado = false;
        }
    }
}
