using System;
using System.Globalization;
using System.Text.Json.Serialization;

namespace ClienteLavadora.model
{
    /// <summary>Lavadora (objeto DETALLE) tal como la devuelve el servidor.</summary>
    public class LavadoraData : DatoBase
    {
        [JsonPropertyName("marca")]
        public string Marca { get; set; }

        [JsonPropertyName("precioBase")]
        public double PrecioBase { get; set; }

        // Llega como texto: yyyy-MM-ddTHH:mm:ss
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

        // El programa (maestro) que usa esta lavadora. Solo se piden su codigo y su nombre.
        [JsonPropertyName("programaLavado")]
        public ProgramaData ProgramaLavado { get; set; }

        // Lo calcula el servidor
        [JsonPropertyName("precioFinal")]
        public double PrecioFinal { get; set; }

        // ---- Propiedades de apoyo para mostrar los datos en la tabla ----

        [JsonIgnore]
        public DateTime FechaComoDateTime
        {
            get
            {
                DateTime fecha;
                if (DateTime.TryParse(FechaFabricacion, CultureInfo.InvariantCulture, DateTimeStyles.None, out fecha))
                {
                    return fecha;
                }
                return DateTime.Now;
            }
        }

        [JsonIgnore]
        public string FechaTexto { get { return FechaComoDateTime.ToString("dd/MM/yyyy HH:mm"); } }

        [JsonIgnore]
        public string PrecioBaseTexto { get { return "$ " + PrecioBase.ToString("N0"); } }

        [JsonIgnore]
        public string PrecioFinalTexto { get { return "$ " + PrecioFinal.ToString("N0"); } }

        [JsonIgnore]
        public string CapacidadTexto { get { return CapacidadCargaKilos + " kg"; } }

        [JsonIgnore]
        public string RpmTexto { get { return VelocidadCentrifugadoRPM + " RPM"; } }

        [JsonIgnore]
        public string InverterTexto { get { return TecnologiaInverter ? "Sí" : "No"; } }

        [JsonIgnore]
        public string ProgramaTexto { get { return ProgramaLavado == null ? "" : ProgramaLavado.Nombre; } }
    }
}
