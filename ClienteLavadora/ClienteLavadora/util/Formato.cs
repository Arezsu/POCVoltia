using System;
using System.Globalization;

namespace ClienteLavadora.util
{
    /// <summary>
    /// Traduce datos "de la logica" a textos "de la pantalla", y al contrario:
    /// convierte lo que el usuario escribe en numeros y fechas validas.
    ///
    /// POR QUE ESTA CLASE ES ESTATICA Y SEPARADA:
    /// formatear es una tarea repetida (precios, fechas, estados) y no depende de
    /// ninguna ventana. Al tenerla en un solo lugar, la grilla y el formulario
    /// muestran el mismo texto para el mismo dato.
    ///
    /// La cultura es fija ("es-CO") para que la aplicacion muestre los precios igual
    /// en cualquier computador: $ 1.450.000 y no $1,450,000.
    /// </summary>
    public static class Formato
    {
        /// <summary>Formato de fecha que exige el servidor GraphQL.</summary>
        public const string PatronFecha = "yyyy-MM-dd'T'HH:mm:ss";

        // Cultura fija: asi el programa se ve igual en cualquier Windows.
        private static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("es-CO");

        // ====================================================================
        //  Datos -> texto (para mostrar)
        // ====================================================================

        /// <summary>Precio con separador de miles: 1450000 se ve "$ 1.450.000".</summary>
        public static string Moneda(double valor)
        {
            return "$ " + valor.ToString("N0", Cultura);
        }

        /// <summary>Numero corto sin ceros de sobra: 18.0 se ve "18" y 15.5 se ve "15,5".</summary>
        public static string Numero(double valor)
        {
            return valor.ToString("0.##", Cultura);
        }

        /// <summary>Fecha y hora legibles: "2024-03-10T09:00:00" se ve "10/03/2024 09:00".</summary>
        public static string FechaHora(string fechaTexto)
        {
            DateTime fecha = ComoFecha(fechaTexto);
            if (fecha == DateTime.MinValue)
            {
                return fechaTexto ?? "";
            }
            return fecha.ToString("dd/MM/yyyy HH:mm", Cultura);
        }

        /// <summary>Solo la fecha, sin la hora.</summary>
        public static string FechaCorta(string fechaTexto)
        {
            DateTime fecha = ComoFecha(fechaTexto);
            if (fecha == DateTime.MinValue)
            {
                return fechaTexto ?? "";
            }
            return fecha.ToString("dd/MM/yyyy", Cultura);
        }

        /// <summary>Convierte el texto del servidor en DateTime. Si no se puede, devuelve MinValue.</summary>
        public static DateTime ComoFecha(string fechaTexto)
        {
            DateTime fecha;
            if (!string.IsNullOrWhiteSpace(fechaTexto)
                && DateTime.TryParse(fechaTexto, CultureInfo.InvariantCulture, DateTimeStyles.None, out fecha))
            {
                return fecha;
            }
            return DateTime.MinValue;
        }

        /// <summary>Convierte un DateTime al texto que espera el servidor.</summary>
        public static string FechaServidor(DateTime fecha)
        {
            return fecha.ToString(PatronFecha, CultureInfo.InvariantCulture);
        }

        /// <summary>Estado del mantenimiento en una sola palabra.</summary>
        public static string Estado(bool completado)
        {
            return completado ? "Completado" : "Pendiente";
        }

        /// <summary>Devuelve "Si" o "No" (sin tilde, como pide el estilo del proyecto).</summary>
        public static string SiNo(bool valor)
        {
            return valor ? "Si" : "No";
        }

        // ====================================================================
        //  Texto -> dato (validacion de los formularios)
        //
        //  Todos estos metodos lanzan ArgumentException con un mensaje en espanol
        //  cuando el usuario escribio algo raro. La ventana solo atrapa la excepcion
        //  y muestra el mensaje: asi la validacion se escribe una sola vez.
        // ====================================================================

        /// <summary>Lee un entero mayor o igual a "minimo".</summary>
        public static int LeerEntero(string texto, int minimo, string mensaje)
        {
            int valor;
            if (!int.TryParse((texto ?? "").Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out valor)
                || valor < minimo)
            {
                throw new ArgumentException(mensaje);
            }
            return valor;
        }

        /// <summary>Lee un entero que debe estar entre "minimo" y "maximo".</summary>
        public static int LeerEnteroEnRango(string texto, int minimo, int maximo, string mensaje)
        {
            int valor;
            if (!int.TryParse((texto ?? "").Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out valor)
                || valor < minimo || valor > maximo)
            {
                throw new ArgumentException(mensaje);
            }
            return valor;
        }

        /// <summary>
        /// Lee un numero con decimales mayor que cero. Acepta "15.5" y tambien "15,5"
        /// (en Colombia se usa coma, en otros paises punto; aceptamos las dos).
        /// </summary>
        public static double LeerDecimal(string texto, string mensaje)
        {
            double valor;
            string limpio = (texto ?? "").Trim().Replace(" ", "").Replace(',', '.');
            if (!double.TryParse(limpio, NumberStyles.Float, CultureInfo.InvariantCulture, out valor) || valor <= 0)
            {
                throw new ArgumentException(mensaje);
            }
            return valor;
        }

        /// <summary>Lee un numero decimal que puede ser CERO (por ejemplo el costo de un mantenimiento).</summary>
        public static double LeerDecimalCeroPermitido(string texto, string mensaje)
        {
            double valor;
            string limpio = (texto ?? "").Trim().Replace(" ", "").Replace(',', '.');
            if (!double.TryParse(limpio, NumberStyles.Float, CultureInfo.InvariantCulture, out valor) || valor < 0)
            {
                throw new ArgumentException(mensaje);
            }
            return valor;
        }

        /// <summary>
        /// Lee una fecha escrita como yyyy-MM-ddTHH:mm:ss y la devuelve en ese mismo formato.
        /// Si el usuario escribe solo la fecha (yyyy-MM-dd) se completa con medianoche.
        /// </summary>
        public static string LeerFechaServidor(string texto, string mensaje)
        {
            string limpio = (texto ?? "").Trim();
            string[] patrones = { PatronFecha, PatronFecha.Replace("'T'", " "), "yyyy-MM-dd" };
            DateTime fecha;
            if (!DateTime.TryParseExact(limpio, patrones, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out fecha))
            {
                throw new ArgumentException(mensaje);
            }
            return FechaServidor(fecha);
        }

        /// <summary>Texto obligatorio (marca, descripcion, tecnico...).</summary>
        public static string LeerTexto(string texto, string mensaje)
        {
            string limpio = (texto ?? "").Trim();
            if (limpio.Length == 0)
            {
                throw new ArgumentException(mensaje);
            }
            return limpio;
        }
    }
}
