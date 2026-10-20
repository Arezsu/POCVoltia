using System;
using System.Globalization;
using System.Text.Json.Serialization;

namespace ClienteLavadora.model
{
    /// <summary>Programa de lavado (objeto MAESTRO) tal como lo devuelve el servidor.</summary>
    public class ProgramaData : DatoBase
    {
        [JsonPropertyName("nombre")]
        public string Nombre { get; set; }

        [JsonPropertyName("minMinutos")]
        public int MinMinutos { get; set; }

        [JsonPropertyName("maxMinutos")]
        public int MaxMinutos { get; set; }

        [JsonPropertyName("consumoAguaLitros")]
        public double ConsumoAguaLitros { get; set; }

        // Llega como texto: yyyy-MM-ddTHH:mm:ss
        [JsonPropertyName("fechaCreacion")]
        public string FechaCreacion { get; set; }

        [JsonPropertyName("aguaCaliente")]
        public bool AguaCaliente { get; set; }

        // Cuantas lavadoras usan este programa (la parte 0..* de la relacion)
        [JsonPropertyName("cantidadLavadoras")]
        public int CantidadLavadoras { get; set; }

        // ---- Propiedades de apoyo para mostrar los datos en la tabla ----
        // [JsonIgnore] = no viajan en el JSON

        [JsonIgnore]
        public DateTime FechaComoDateTime
        {
            get
            {
                DateTime fecha;
                if (DateTime.TryParse(FechaCreacion, CultureInfo.InvariantCulture, DateTimeStyles.None, out fecha))
                {
                    return fecha;
                }
                return DateTime.Now;
            }
        }

        [JsonIgnore]
        public string FechaTexto { get { return FechaComoDateTime.ToString("dd/MM/yyyy HH:mm"); } }

        [JsonIgnore]
        public string RangoTexto { get { return MinMinutos + " a " + MaxMinutos + " min"; } }

        [JsonIgnore]
        public string AguaTexto { get { return ConsumoAguaLitros + " L"; } }

        [JsonIgnore]
        public string CalienteTexto { get { return AguaCaliente ? "Sí" : "No"; } }
    }
}
