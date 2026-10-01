using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json.Serialization;

namespace ClienteLavadora.model
{
    /// <summary>Lavadora tal como la devuelve el servidor GraphQL.</summary>
    public class LavadoraData
    {
        [JsonPropertyName("codigo")]
        public int codigo { get; set; }

        [JsonPropertyName("marca")]
        public string marca { get; set; }

        [JsonPropertyName("precioBase")]
        public double precioBase { get; set; }

        [JsonPropertyName("fechaFabricacion")]
        public string fechaFabricacion { get; set; }

        [JsonPropertyName("capacidadKilos")]
        public double capacidadKilos { get; set; }

        [JsonPropertyName("funcionSecado")]
        public bool funcionSecado { get; set; }

        /// <summary>Convierte el texto de fecha del servidor a DateTime (si no se puede, usa la fecha actual).</summary>
        public DateTime FechaComoDateTime()
        {
            DateTime f;
            if (DateTime.TryParse(fechaFabricacion, CultureInfo.InvariantCulture, DateTimeStyles.None, out f))
            {
                return f;
            }
            return DateTime.Now;
        }

        public string FechaBonita()
        {
            return FechaComoDateTime().ToString("dd/MM/yyyy HH:mm");
        }

        /// <summary>Toda la informacion de la lavadora en texto (para mostrarla al usuario).</summary>
        public string ComoTexto()
        {
            return
                "Código: " + codigo + "\r\n" +
                "Marca: " + marca + "\r\n" +
                "Precio base: " + precioBase.ToString("N0") + "\r\n" +
                "Fecha de fabricación: " + FechaBonita() + "\r\n" +
                "Capacidad (kg): " + capacidadKilos + "\r\n" +
                "Función de secado: " + (funcionSecado ? "Sí" : "No");
        }
    }

    // ---- Envoltorios de respuesta: el nombre debe ser igual al campo del schema GraphQL ----

    internal class ResponseLavadoras
    {
        [JsonPropertyName("lavadoras")]
        public List<LavadoraData> lavadoras { get; set; }
    }

    internal class ResponseLavadoraPorCodigo
    {
        [JsonPropertyName("lavadoraPorCodigo")]
        public LavadoraData lavadoraPorCodigo { get; set; }
    }

    internal class ResponseLavadorasPorFiltro
    {
        [JsonPropertyName("lavadorasPorFiltro")]
        public List<LavadoraData> lavadorasPorFiltro { get; set; }
    }

    internal class ResponseAddLavadora
    {
        [JsonPropertyName("addLavadora")]
        public LavadoraData addLavadora { get; set; }
    }

    internal class ResponseActualizarLavadora
    {
        [JsonPropertyName("actualizarLavadora")]
        public LavadoraData actualizarLavadora { get; set; }
    }

    internal class ResponseEliminarLavadora
    {
        [JsonPropertyName("eliminarLavadora")]
        public bool eliminarLavadora { get; set; }
    }

    // ---- Datos que se envian al servidor ----

    public class LavadoraInput
    {
        [JsonPropertyName("codigo")]
        public int codigo { get; set; }

        [JsonPropertyName("marca")]
        public string marca { get; set; }

        [JsonPropertyName("precioBase")]
        public double precioBase { get; set; }

        [JsonPropertyName("fechaFabricacion")]
        public string fechaFabricacion { get; set; }

        [JsonPropertyName("capacidadKilos")]
        public double capacidadKilos { get; set; }

        [JsonPropertyName("funcionSecado")]
        public bool funcionSecado { get; set; }
    }

    public class LavadoraUpdateInput
    {
        [JsonPropertyName("marca")]
        public string marca { get; set; }

        [JsonPropertyName("precioBase")]
        public double? precioBase { get; set; }

        [JsonPropertyName("fechaFabricacion")]
        public string fechaFabricacion { get; set; }

        [JsonPropertyName("capacidadKilos")]
        public double? capacidadKilos { get; set; }

        [JsonPropertyName("funcionSecado")]
        public bool? funcionSecado { get; set; }
    }
}
