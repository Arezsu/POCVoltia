using System.Text.Json.Serialization;

namespace ClienteLavadora.modelo
{
    public class TecnicoInput
    {
        [JsonPropertyName("codigo")] public int Codigo { get; set; }
        [JsonPropertyName("documento")] public string Documento { get; set; }
        [JsonPropertyName("nombre")] public string Nombre { get; set; }
        [JsonPropertyName("especialidad")] public string Especialidad { get; set; }
        [JsonPropertyName("telefono")] public string Telefono { get; set; }
        [JsonPropertyName("fechaIngreso")] public string FechaIngreso { get; set; }
        [JsonPropertyName("activo")] public bool Activo { get; set; }
    }
}
