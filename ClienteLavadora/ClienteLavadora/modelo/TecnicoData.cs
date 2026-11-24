using System;
using System.Globalization;
using System.Text.Json.Serialization;

namespace ClienteLavadora.modelo
{
    public class TecnicoData
    {
        [JsonPropertyName("codigo")] public int Codigo { get; set; }
        [JsonPropertyName("documento")] public string Documento { get; set; }
        [JsonPropertyName("nombre")] public string Nombre { get; set; }
        [JsonPropertyName("especialidad")] public string Especialidad { get; set; }
        [JsonPropertyName("telefono")] public string Telefono { get; set; }
        [JsonPropertyName("fechaIngreso")] public string FechaIngreso { get; set; }
        [JsonIgnore]
        public string FechaIngresoTexto
        {
            get
            {
                return DateTime.TryParse(FechaIngreso, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fecha)
                    ? fecha.ToString("dd/MM/yyyy", CultureInfo.GetCultureInfo("es-CO"))
                    : FechaIngreso ?? "";
            }
        }
        [JsonPropertyName("activo")] public bool Activo { get; set; }
        [JsonPropertyName("codigoLavadora")] public int? CodigoLavadora { get; set; }
        [JsonPropertyName("marcaLavadora")] public string MarcaLavadora { get; set; }
    }
}
