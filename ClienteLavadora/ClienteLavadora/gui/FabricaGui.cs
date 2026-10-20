using ClienteLavadora.model;
using ClienteLavadora.util;

namespace ClienteLavadora.gui
{
    /// <summary>
    /// Entrega las piezas de pantalla que corresponden a cada objeto (patron "fabrica").
    /// Asi las ventanas no tienen que preguntar "es lavadora o programa?" a cada rato.
    /// </summary>
    public static class FabricaGui
    {
        /// <summary>El formulario de la lavadora o el del programa de lavado.</summary>
        public static ICampos CrearCampos(TipoObjeto tipo)
        {
            if (tipo == TipoObjeto.Lavadora)
            {
                return new CamposLavadora();
            }
            return new CamposPrograma();
        }

        /// <summary>La barra de busqueda con los criterios de ese objeto.</summary>
        public static PanelBusqueda CrearBusqueda(TipoObjeto tipo)
        {
            return new PanelBusqueda(tipo);
        }
    }
}
