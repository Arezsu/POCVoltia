using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ClienteLavadora.modelo
{
    /// <summary>
    /// "Envoltorios" de las respuestas GraphQL.
    ///
    /// GraphQL siempre contesta con un objeto JSON que tiene una llave por cada
    /// operacion pedida, por ejemplo: { "data": { "lavadorasPorFiltro": [ ... ] } }.
    /// El cliente de GraphQL ya quita la capa "data"; para que el deserializador sepa
    /// donde estan los datos, cada operacion necesita una clase con una propiedad cuyo
    /// [JsonPropertyName] sea EXACTAMENTE el nombre de la operacion del schema.
    ///
    /// Eso es todo lo que hace este archivo: mapear nombre de operacion -> dato.
    /// </summary>
    public class RespuestaListaLavadoras
    {
        [JsonPropertyName("lavadorasPorFiltro")]
        public List<LavadoraData> Lavadoras { get; set; }
    }

    public class RespuestaLavadoraPorCriterios
    {
        [JsonPropertyName("lavadoraPorCriterios")]
        public LavadoraData Lavadora { get; set; }
    }

    public class RespuestaAgregarLavadora
    {
        [JsonPropertyName("addLavadora")]
        public LavadoraData Lavadora { get; set; }
    }

    public class RespuestaActualizarLavadora
    {
        [JsonPropertyName("actualizarLavadora")]
        public LavadoraData Lavadora { get; set; }
    }

    public class RespuestaEliminarLavadora
    {
        [JsonPropertyName("eliminarLavadora")]
        public bool Eliminada { get; set; }
    }

    public class RespuestaListaMantenimientos
    {
        [JsonPropertyName("mantenimientosPorFiltro")]
        public List<MantenimientoData> Mantenimientos { get; set; }
    }

    public class RespuestaMantenimientoPorId
    {
        [JsonPropertyName("mantenimientoPorId")]
        public MantenimientoData Mantenimiento { get; set; }
    }

    public class RespuestaAgregarMantenimiento
    {
        [JsonPropertyName("addMantenimiento")]
        public MantenimientoData Mantenimiento { get; set; }
    }

    public class RespuestaActualizarMantenimiento
    {
        [JsonPropertyName("actualizarMantenimiento")]
        public MantenimientoData Mantenimiento { get; set; }
    }

    public class RespuestaEliminarMantenimiento
    {
        [JsonPropertyName("eliminarMantenimiento")]
        public bool Eliminado { get; set; }
    }

    public class RespuestaVersionDatos
    {
        [JsonPropertyName("versionDatos")]
        public int Version { get; set; }
    }
}
