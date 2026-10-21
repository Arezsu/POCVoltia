using System;
using System.Text.Json.Serialization;
using ClienteLavadora.util;

namespace ClienteLavadora.modelo
{
    /// <summary>
    /// Datos de un MANTENIMIENTO (el objeto DETALLE de la relacion maestro-detalle).
    ///
    /// Cada mantenimiento vive DENTRO de una lavadora: por eso no se busca por su
    /// cuenta, sino que siempre se pide junto con el codigo de su lavadora
    /// (ver IServicioMantenimientos). El "id" lo calcula el servidor y solo es
    /// unico dentro de esa lavadora.
    /// </summary>
    public class MantenimientoData
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("descripcion")]
        public string Descripcion { get; set; }

        [JsonPropertyName("tecnico")]
        public string Tecnico { get; set; }

        [JsonPropertyName("costo")]
        public double Costo { get; set; }

        // Llega como texto: yyyy-MM-ddTHH:mm:ss
        [JsonPropertyName("fecha")]
        public string Fecha { get; set; }

        [JsonPropertyName("completado")]
        public bool Completado { get; set; }

        // ---------- Apoyo para la interfaz ----------

        [JsonIgnore]
        public DateTime FechaComoDateTime
        {
            get { return Formato.ComoFecha(Fecha); }
        }

        [JsonIgnore]
        public string FechaTexto
        {
            get { return Formato.FechaHora(Fecha); }
        }

        [JsonIgnore]
        public string CostoTexto
        {
            get { return Formato.Moneda(Costo); }
        }

        [JsonIgnore]
        public string EstadoTexto
        {
            get { return Formato.Estado(Completado); }
        }

        /// <summary>Se usa en mensajes de confirmacion: "id 3 - Cambio de bomba".</summary>
        public override string ToString()
        {
            return "id " + Id + " - " + Descripcion;
        }
    }
}
