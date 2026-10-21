using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using ClienteLavadora.util;

namespace ClienteLavadora.modelo
{
    /// <summary>
    /// Datos de una LAVADORA (el objeto MAESTRO de la relacion maestro-detalle).
    ///
    /// Esta clase es solo un "molde de datos": los nombres de las propiedades
    /// coinciden con los campos del schema GraphQL gracias a [JsonPropertyName].
    /// No hace peticiones de red (eso es trabajo de la capa servicios) y no dibuja
    /// nada (eso es trabajo de la capa vista). Ademas trae unos cuantos calculos de
    /// PRESENTACION (textos ya formateados) para que la vista no tenga que formatear.
    /// </summary>
    public class LavadoraData
    {
        // ====================================================================
        //  Reglas de negocio del precio final
        //  Son las MISMAS del servidor Java (Lavadora.calcularPrecioFinal):
        //  si la capacidad supera 15 kilos se suman 120.000 y si es inverter 250.000.
        //  El cliente las copia solo para la VISTA PREVIA del formulario; el valor
        //  oficial siempre es el "precioFinal" que calcula y devuelve el servidor.
        // ====================================================================

        public const double KilosParaBono = 15;
        public const double BonoCapacidad = 120000;
        public const double BonoInverter = 250000;

        // ---------- Campos tal como viajan por GraphQL ----------

        [JsonPropertyName("codigo")]
        public int Codigo { get; set; }

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

        // Lo calcula el servidor a partir del precio base y los extras.
        [JsonPropertyName("precioFinal")]
        public double PrecioFinal { get; set; }

        // ---------- Detalle (0..*): cada lavadora tiene sus mantenimientos ----------

        [JsonPropertyName("mantenimientos")]
        public List<MantenimientoData> Mantenimientos { get; set; }

        [JsonPropertyName("cantidadMantenimientos")]
        public int CantidadMantenimientos { get; set; }

        [JsonPropertyName("mantenimientosPendientes")]
        public int MantenimientosPendientes { get; set; }

        [JsonPropertyName("costoMantenimientos")]
        public double CostoMantenimientos { get; set; }

        // ====================================================================
        //  Apoyo para la interfaz
        //  [JsonIgnore] = "esto no viaja por GraphQL, es solo para mostrar".
        // ====================================================================

        /// <summary>Devuelve siempre una lista (nunca null) para poder recorrerla sin miedo.</summary>
        [JsonIgnore]
        public List<MantenimientoData> ListaMantenimientos
        {
            get
            {
                if (Mantenimientos == null)
                {
                    return new List<MantenimientoData>();
                }
                return Mantenimientos;
            }
        }

        [JsonIgnore]
        public DateTime FechaComoDateTime
        {
            get { return Formato.ComoFecha(FechaFabricacion); }
        }

        [JsonIgnore]
        public string FechaTexto
        {
            get { return Formato.FechaHora(FechaFabricacion); }
        }

        [JsonIgnore]
        public string PrecioBaseTexto
        {
            get { return Formato.Moneda(PrecioBase); }
        }

        [JsonIgnore]
        public string PrecioFinalTexto
        {
            get { return Formato.Moneda(PrecioFinal); }
        }

        [JsonIgnore]
        public string CapacidadTexto
        {
            get { return Formato.Numero(CapacidadCargaKilos) + " kg"; }
        }

        [JsonIgnore]
        public string RpmTexto
        {
            get { return VelocidadCentrifugadoRPM + " RPM"; }
        }

        [JsonIgnore]
        public string MinutosTexto
        {
            get { return MinutosLavado + " min"; }
        }

        [JsonIgnore]
        public string InverterTexto
        {
            get { return Formato.SiNo(TecnologiaInverter); }
        }

        [JsonIgnore]
        public string MantenimientosTexto
        {
            get { return CantidadMantenimientos + " en total"; }
        }

        [JsonIgnore]
        public string PendientesTexto
        {
            get { return MantenimientosPendientes + " pendiente(s)"; }
        }

        [JsonIgnore]
        public string CostoMantenimientosTexto
        {
            get { return Formato.Moneda(CostoMantenimientos); }
        }

        /// <summary>La usa el ComboBox de lavadoras: se ve "101 - LG".</summary>
        public override string ToString()
        {
            return Codigo + " - " + Marca;
        }

        // ====================================================================
        //  Calculos (los usa el patron Builder para la vista previa)
        // ====================================================================

        /// <summary>Bono que se suma por capacidad. 0 si no supera los 15 kilos.</summary>
        public static double BonoPorCapacidad(double capacidadCargaKilos)
        {
            return capacidadCargaKilos > KilosParaBono ? BonoCapacidad : 0;
        }

        /// <summary>Bono que se suma por tecnologia inverter.</summary>
        public static double BonoPorInverter(bool tecnologiaInverter)
        {
            return tecnologiaInverter ? BonoInverter : 0;
        }

        /// <summary>Precio final estimado: precio base + bono de capacidad + bono inverter.</summary>
        public static double CalcularPrecioFinal(double precioBase, double capacidadCargaKilos, bool tecnologiaInverter)
        {
            return precioBase + BonoPorCapacidad(capacidadCargaKilos) + BonoPorInverter(tecnologiaInverter);
        }
    }
}
