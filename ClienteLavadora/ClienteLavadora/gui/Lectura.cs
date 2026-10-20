using System;
using System.Globalization;
using System.Windows.Controls;

namespace ClienteLavadora.gui
{
    /// <summary>
    /// Lee y valida lo que el usuario escribio en los formularios.
    /// Si algo esta mal lanza una ArgumentException con un mensaje claro
    /// que la ventana le muestra al usuario.
    /// </summary>
    public static class Lectura
    {
        /// <summary>
        /// Une la fecha y la hora en el formato del servidor (yyyy-MM-ddTHH:mm:ss).
        /// La hora se escribe como 14:30 (tambien sirve 9:05).
        /// </summary>
        public static string FechaHora(DatePicker fecha, CampoTexto hora, string nombre)
        {
            if (!fecha.SelectedDate.HasValue)
            {
                throw new ArgumentException("Escoja la " + nombre + ".");
            }

            DateTime horaLeida;
            if (!DateTime.TryParseExact(hora.Texto.Trim(), new string[] { "H:mm", "HH:mm" },
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out horaLeida))
            {
                throw new ArgumentException("La hora debe escribirse así: 14:30");
            }

            DateTime completa = fecha.SelectedDate.Value.Date + horaLeida.TimeOfDay;
            return completa.ToString("yyyy-MM-dd'T'HH:mm:ss", CultureInfo.InvariantCulture);
        }

        /// <summary>Lee un numero con decimales mayor que cero. Acepta "15.5" y tambien "15,5".</summary>
        public static double Decimal(CampoTexto campo, string mensaje)
        {
            string limpio = (campo.Texto ?? "").Trim().Replace(" ", "").Replace(',', '.'); // Si es nulo, vacio
            double valor;
            if (!double.TryParse(limpio, NumberStyles.Float, CultureInfo.InvariantCulture, out valor) || valor <= 0)
            {
                throw new ArgumentException(mensaje);
            }
            return valor;
        }

        /// <summary>Lee un numero entero que sea por lo menos "minimo".</summary>
        public static int Entero(CampoTexto campo, int minimo, string mensaje)
        {
            int valor;
            if (!int.TryParse((campo.Texto ?? "").Trim(), out valor) || valor < minimo)
            {
                throw new ArgumentException(mensaje);
            }
            return valor;
        }

        /// <summary>Lee el codigo (entero mayor que cero).</summary>
        public static int Codigo(CampoTexto campo)
        {
            return Entero(campo, 1, "El código debe ser un número entero mayor que cero.");
        }
    }
}
