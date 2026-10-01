using System.Globalization;

namespace ClienteLavadora
{
    /// <summary>
    /// Ayudas para leer lo que escribe el usuario. Los numeros se leen con la
    /// configuracion regional de Windows (en Colombia el decimal es la coma: 12,5),
    /// sin aceptar separador de miles para evitar que "12,5" se lea como 125.
    /// </summary>
    internal static class Entrada
    {
        public static bool TryLeerDouble(string texto, out double valor)
        {
            return double.TryParse((texto ?? "").Trim(), NumberStyles.Float, CultureInfo.CurrentCulture, out valor);
        }

        public static string DoubleATexto(double valor)
        {
            return valor.ToString(CultureInfo.CurrentCulture);
        }

        public static string FechaParaServidor(System.DateTime fecha)
        {
            return fecha.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
        }
    }
}
