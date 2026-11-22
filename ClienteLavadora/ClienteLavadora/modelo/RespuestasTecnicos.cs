using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ClienteLavadora.modelo
{
    public class RespuestaListaTecnicos { [JsonPropertyName("tecnicosConLavadoras")] public List<TecnicoData> Tecnicos { get; set; } }
    public class RespuestaTecnico { [JsonPropertyName("tecnicoPorCodigo")] public TecnicoData Tecnico { get; set; } }
    public class RespuestaAgregarTecnico { [JsonPropertyName("addTecnico")] public TecnicoData Tecnico { get; set; } }
    public class RespuestaActualizarTecnico { [JsonPropertyName("actualizarTecnico")] public TecnicoData Tecnico { get; set; } }
    public class RespuestaEliminarTecnico { [JsonPropertyName("eliminarTecnico")] public bool Eliminado { get; set; } }
}
