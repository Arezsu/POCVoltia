using System.Text.Json.Serialization;

namespace ClienteLavadora.model
{
    /// <summary>Los dos objetos del proyecto: el maestro (programa) y el detalle (lavadora).</summary>
    public enum TipoObjeto
    {
        ProgramaLavado,
        Lavadora
    }

    /// <summary>
    /// Clase padre de los datos que llegan del servidor. Las ventanas trabajan con
    /// esta clase y no necesitan saber si es un programa o una lavadora (polimorfismo).
    /// </summary>
    public abstract class DatoBase
    {
        // [JsonPropertyName] dice con que nombre viene cada dato en el JSON
        [JsonPropertyName("codigo")]
        public int Codigo { get; set; }
    }
}
