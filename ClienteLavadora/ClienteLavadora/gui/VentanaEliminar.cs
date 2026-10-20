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
    /// Caso de uso ELIMINAR: se busca el objeto, se muestra TODA su informacion
    /// y antes de borrarlo se pide confirmacion.
    /// </summary>
    public class VentanaEliminar : VentanaMarco
    {
        private readonly TipoObjeto tipo;
        private readonly ICampos campos;
        private readonly PanelBusqueda busqueda;
        private readonly IServicioCrud servicio;
        private readonly BotonVoltia btnEliminar;
        private readonly StackPanel panelBotones;

        // Codigo del objeto encontrado (el que se va a borrar)
        private int codigoCargado = -1;

        public VentanaEliminar(TipoObjeto tipo) : base("ELIMINAR " + Catalogo.Titulo(tipo), 640, 730, true, 100)
        {
            this.tipo = tipo;
            campos = FabricaGui.CrearCampos(tipo);
            busqueda = FabricaGui.CrearBusqueda(tipo);
            servicio = FabricaServicios.Obtener(tipo);
            campos.ConfigurarParaBuscar();

            busqueda.BuscarSolicitado += async (s, e) => await BuscarAsync();

            btnEliminar = new BotonVoltia("Eliminar", Ui.Rojo());
            btnEliminar.Width = 150;
            btnEliminar.Margin = new Thickness(6, 0, 6, 0);
            btnEliminar.Habilitado = false; // Se habilita cuando se encuentra algo
            btnEliminar.Click += BtnEliminar_Click;

            BotonVoltia btnCerrar = new BotonVoltia("Cerrar", Ui.Gris());
            btnCerrar.Width = 150;
            btnCerrar.Margin = new Thickness(6, 0, 6, 0);
            btnCerrar.Click += (s, e) => Marco.Cerrar();

            panelBotones = new StackPanel();
            panelBotones.Orientation = Orientation.Horizontal;
            panelBotones.HorizontalAlignment = HorizontalAlignment.Center;
            panelBotones.Children.Add(btnEliminar);
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
                    codigoCargado = -1;
                    btnEliminar.Habilitado = false;
                    campos.Limpiar();
                    Anime.Sacudir(Marco);
                    Ui.Aviso(this, "No existe " + Catalogo.ConArticulo(tipo) + " con esos datos.", "No encontrado");
                    return;
                }

                campos.Mostrar(dato); // Se muestra TODA la informacion antes de eliminar
                codigoCargado = dato.Codigo;
                btnEliminar.Habilitado = true;
                Anime.Escalonado(campos.Filas, 50, 0);
            }
            catch (Exception ex)
            {
                Anime.Sacudir(Marco);
                Ui.Aviso(this, ex.Message, "Error");
            }
        }

        private async void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            // Se pide confirmacion antes de borrar
            MessageBoxResult respuesta = MessageBox.Show(this,
                "¿Seguro que desea eliminar " + Catalogo.ConArticulo(tipo) + " con código " + codigoCargado + "?",
                "Confirmar eliminación", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (respuesta != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                btnEliminar.Habilitado = false;
                bool eliminado = await servicio.EliminarAsync(codigoCargado);

                if (eliminado)
                {
                    Notificador.Avisar();
                    Ui.Informacion(this, "Se eliminó " + Catalogo.ConArticulo(tipo) + " con éxito.", "Eliminado");
                    codigoCargado = -1;
                    campos.Limpiar();
                }
                else
                {
                    Anime.Sacudir(Marco);
                    Ui.Aviso(this, "No se pudo eliminar.", "Error");
                    btnEliminar.Habilitado = true;
                }
            }
            catch (Exception ex)
            {
                // Por ejemplo: un programa que todavia usan lavadoras no se puede eliminar
                Anime.Sacudir(Marco);
                Ui.Aviso(this, ex.Message, "No se pudo eliminar");
                btnEliminar.Habilitado = codigoCargado > 0;
            }
        }
    }
}
