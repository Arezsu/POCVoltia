using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ClienteLavadora.model
{
    // ====================================================================
    //  Envoltorios de respuesta
    //  El nombre de cada propiedad debe ser igual al nombre de la operacion del schema.
    // ====================================================================

    public class ResponseProgramas
    {
        [JsonPropertyName("programasPorFiltro")]
        public List<ProgramaData> Programas { get; set; }
    }

    public class ResponseProgramaPorCriterios
    {
        [JsonPropertyName("programaPorCriterios")]
        public ProgramaData Programa { get; set; }
    }

    public class ResponseAddPrograma
    {
        [JsonPropertyName("addPrograma")]
        public ProgramaData Programa { get; set; }
    }

    public class ResponseActualizarPrograma
    {
        [JsonPropertyName("actualizarPrograma")]
        public ProgramaData Programa { get; set; }
    }

    public class ResponseEliminarPrograma
    {
        [JsonPropertyName("eliminarPrograma")]
        public bool Eliminado { get; set; }
    }

    public class ResponseLavadoras
    {
        [JsonPropertyName("lavadorasPorFiltro")]
        public List<LavadoraData> Lavadoras { get; set; }
    }

    public class ResponseLavadoraPorCriterios
    {
        [JsonPropertyName("lavadoraPorCriterios")]
        public LavadoraData Lavadora { get; set; }
    }

    public class ResponseAddLavadora
    {
        [JsonPropertyName("addLavadora")]
        public LavadoraData Lavadora { get; set; }
    }

    public class ResponseActualizarLavadora
    {
        [JsonPropertyName("actualizarLavadora")]
        public LavadoraData Lavadora { get; set; }
    }

    public class ResponseEliminarLavadora
    {
        [JsonPropertyName("eliminarLavadora")]
        public bool Eliminada { get; set; }
    }

    // ====================================================================
    //  Datos que se envian al servidor
    //  Los tipos con "?" (double?, int?, bool?) admiten null: "este dato no cambia".
    // ====================================================================

    public class ProgramaInput
    {
        [JsonPropertyName("codigo")]
        public int Codigo { get; set; }

        [JsonPropertyName("nombre")]
        public string Nombre { get; set; }

        [JsonPropertyName("minMinutos")]
        public int MinMinutos { get; set; }

        [JsonPropertyName("maxMinutos")]
        public int MaxMinutos { get; set; }

        [JsonPropertyName("consumoAguaLitros")]
        public double ConsumoAguaLitros { get; set; }

        [JsonPropertyName("fechaCreacion")]
        public string FechaCreacion { get; set; }

        [JsonPropertyName("aguaCaliente")]
        public bool AguaCaliente { get; set; }
    }

    public class ProgramaUpdateInput
    {
        [JsonPropertyName("nombre")]
        public string Nombre { get; set; }

        [JsonPropertyName("minMinutos")]
        public int? MinMinutos { get; set; }

        [JsonPropertyName("maxMinutos")]
        public int? MaxMinutos { get; set; }

        [JsonPropertyName("consumoAguaLitros")]
        public double? ConsumoAguaLitros { get; set; }

        [JsonPropertyName("fechaCreacion")]
        public string FechaCreacion { get; set; }

        [JsonPropertyName("aguaCaliente")]
        public bool? AguaCaliente { get; set; }
    }

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
}
