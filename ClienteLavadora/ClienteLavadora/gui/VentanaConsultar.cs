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
    /// Caso de uso CONSULTAR: busca UN objeto por uno o dos criterios (codigo y/o marca o nombre)
    /// y muestra todos sus datos en el mismo formulario de Adicionar, pero de solo lectura.
    /// </summary>
    public class VentanaConsultar : VentanaMarco
    {
        private readonly TipoObjeto tipo;
        private readonly ICampos campos;
        private readonly PanelBusqueda busqueda;
        private readonly IServicioCrud servicio;
        private readonly StackPanel panelBotones;

        public VentanaConsultar(TipoObjeto tipo) : base("CONSULTAR " + Catalogo.Titulo(tipo), 640, 730, true, 100)
        {
            this.tipo = tipo;
            campos = FabricaGui.CrearCampos(tipo);
            busqueda = FabricaGui.CrearBusqueda(tipo);
            servicio = FabricaServicios.Obtener(tipo);
            campos.ConfigurarParaBuscar(); // Todo bloqueado hasta que se busque

            busqueda.BuscarSolicitado += async (s, e) => await BuscarAsync(); // Funcion anonima corta

            BotonVoltia btnCerrar = new BotonVoltia("Cerrar", Ui.Gris());
            btnCerrar.Width = 150;
            btnCerrar.Click += (s, e) => Marco.Cerrar();
            panelBotones = new StackPanel();
            panelBotones.Orientation = Orientation.Horizontal;
            panelBotones.HorizontalAlignment = HorizontalAlignment.Center;
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

        private async Task BuscarAsync() // Promesa de resultado
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
                DatoBase dato = await servicio.BuscarAsync(codigo, texto); // Espera la respuesta

                if (dato == null)
                {
                    // El servidor responde null cuando no existe
                    campos.Limpiar();
                    Anime.Sacudir(Marco);
                    Ui.Aviso(this, "No existe " + Catalogo.ConArticulo(tipo) + " con esos datos.", "No encontrado");
                    return;
                }

                campos.Mostrar(dato);
                Anime.Escalonado(campos.Filas, 50, 0); // Los datos aparecen uno tras otro
            }
            catch (Exception ex)
            {
                Anime.Sacudir(Marco);
                Ui.Aviso(this, ex.Message, "Error");
            }
        }
    }
}
