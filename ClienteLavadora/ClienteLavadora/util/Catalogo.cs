using ClienteLavadora.model;

namespace ClienteLavadora.util
{
    /// <summary>Datos fijos y textos que comparten varias ventanas.</summary>
    public static class Catalogo
    {
        /// <summary>Las marcas del combo (las mismas de las ventanas del proyecto en Java).</summary>
        public static readonly string[] Marcas =
        {
            "Samsung", "LG", "Whirlpool", "Mabe", "Haceb",
            "Electrolux", "Bosch", "Haier", "Panasonic", "Frigidaire"
        };

        /// <summary>Nombre del objeto para los titulos, por ejemplo "LAVADORA".</summary>
        public static string Titulo(TipoObjeto tipo)
        {
            return tipo == TipoObjeto.Lavadora ? "LAVADORA" : "PROGRAMA DE LAVADO";
        }

        /// <summary>Nombre con articulo para los mensajes, por ejemplo "la lavadora".</summary>
        public static string ConArticulo(TipoObjeto tipo)
        {
            return tipo == TipoObjeto.Lavadora ? "la lavadora" : "el programa de lavado";
        }

        /// <summary>Texto del segundo criterio de busqueda: marca en la lavadora y nombre en el programa.</summary>
        public static string SegundoCriterio(TipoObjeto tipo)
        {
            return tipo == TipoObjeto.Lavadora ? "Marca" : "Nombre";
        }
    }
}
