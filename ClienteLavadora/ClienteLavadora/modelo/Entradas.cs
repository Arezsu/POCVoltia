using System.Text.Json.Serialization;

namespace ClienteLavadora.modelo
{
    /// <summary>
    /// Datos que el CLIENTE manda al servidor en las mutaciones (los "input" del schema).
    ///
    /// Se separan de LavadoraData y MantenimientoData a proposito:
    ///  - LavadoraData trae todo lo que el servidor devuelve (precioFinal, calculados...).
    ///  - LavadoraInput trae SOLO lo que el usuario puede escribir.
    /// Mezclarlos dejaria mandar campos que el servidor no acepta y confundiria al
    /// estudiante al sustentar.
    ///
    /// Los tipos con "?" (double?, int?, bool?) admiten null, y null significa
    /// "este dato no cambia": asi funcionan los UpdateInput del schema.
    /// </summary>
    public class LavadoraInput
    {
        [JsonPropertyName("codigo")]
        public int Codigo { get; set; }

        [JsonPropertyName("marca")]
        public string Marca { get; set; }

        [JsonPropertyName("precioBase")]
        public double PrecioBase { get; set; }

        [JsonPropertyName("fechaFabricacion")]
        public string FechaFabricacion { get; set; }

        [JsonPropertyName("capacidadCargaKilos")]
        public double CapacidadCargaKilos { get; set; }

        [JsonPropertyName("velocidadCentrifugadoRPM")]
        public int VelocidadCentrifugadoRPM { get; set; }

        [JsonPropertyName("tecnologiaInverter")]
        public bool TecnologiaInverter { get; set; }

        [JsonPropertyName("minutosLavado")]
        public int MinutosLavado { get; set; }
    }

    /// <summary>Datos para ACTUALIZAR una lavadora: todos opcionales y el codigo no se manda.</summary>
    public class LavadoraUpdateInput
    {
        [JsonPropertyName("marca")]
        public string Marca { get; set; }

        [JsonPropertyName("precioBase")]
        public double? PrecioBase { get; set; }

        [JsonPropertyName("fechaFabricacion")]
        public string FechaFabricacion { get; set; }

        [JsonPropertyName("capacidadCargaKilos")]
        public double? CapacidadCargaKilos { get; set; }

        [JsonPropertyName("velocidadCentrifugadoRPM")]
        public int? VelocidadCentrifugadoRPM { get; set; }

        [JsonPropertyName("tecnologiaInverter")]
        public bool? TecnologiaInverter { get; set; }

        [JsonPropertyName("minutosLavado")]
        public int? MinutosLavado { get; set; }
    }

    /// <summary>Datos para crear un mantenimiento. El id NO se manda: lo calcula el servidor.</summary>
    public class MantenimientoInput
    {
        [JsonPropertyName("descripcion")]
        public string Descripcion { get; set; }

        [JsonPropertyName("tecnico")]
        public string Tecnico { get; set; }

        [JsonPropertyName("costo")]
        public double Costo { get; set; }

        [JsonPropertyName("fecha")]
        public string Fecha { get; set; }

        [JsonPropertyName("completado")]
        public bool Completado { get; set; }
    }

    /// <summary>Datos para ACTUALIZAR un mantenimiento: todos opcionales.</summary>
    public class MantenimientoUpdateInput
    {
        [JsonPropertyName("descripcion")]
        public string Descripcion { get; set; }

        [JsonPropertyName("tecnico")]
        public string Tecnico { get; set; }

        [JsonPropertyName("costo")]
        public double? Costo { get; set; }

        [JsonPropertyName("fecha")]
        public string Fecha { get; set; }

        [JsonPropertyName("completado")]
        public bool? Completado { get; set; }
    }
}
