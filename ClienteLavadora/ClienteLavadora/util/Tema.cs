using System.Windows;
using System.Windows.Media;

namespace ClienteLavadora.util
{
    /// <summary>
    /// Paleta de colores y tipografias de la casa VOLTIA.
    ///
    /// POR QUE EXISTE ESTA CLASE:
    /// si cada ventana escribiera sus propios colores, en pocas semanas la aplicacion
    /// tendria veinte grises distintos. Aqui los colores se escriben UNA sola vez y
    /// todas las ventanas piden prestado el mismo pincel. Asi el diseno se siente
    /// de una sola pieza (metafora de lavadora industrial moderna).
    ///
    /// Ejemplo de uso:
    ///     texto.Foreground = Tema.Cian;
    ///     fondo.Background = Tema.Degradado(Tema.HexChasis, Tema.HexFondo);
    /// </summary>
    public static class Tema
    {
        public static bool EsClaro { get; private set; }

        public static void Alternar()
        {
            EsClaro = !EsClaro;
            Cambiar(Fondo, EsClaro ? "#EDF3F6" : HexFondo);
            Cambiar(FondoAlto, EsClaro ? "#DCE7EC" : HexFondoAlto);
            Cambiar(Chasis, EsClaro ? "#FFFFFF" : HexChasis);
            Cambiar(Acero, EsClaro ? "#DCE7EC" : HexAcero);
            Cambiar(AceroClaro, EsClaro ? "#B7C8D0" : HexAceroClaro);
            Cambiar(Cian, EsClaro ? "#087C96" : HexCian);
            Cambiar(Ambar, EsClaro ? "#9A5B00" : HexAmbar);
            Cambiar(Texto, EsClaro ? "#172832" : HexTexto);
            Cambiar(TextoSecundario, EsClaro ? "#526873" : HexTextoSecundario);
            Cambiar(Cristal, EsClaro ? "#D9FFFFFF" : HexCristal);
            Cambiar(CristalBorde, EsClaro ? "#55314958" : HexCristalBorde);
            Cambiar(Linea, EsClaro ? "#30314958" : HexLinea);
            Cambiar(PanelOscuro, EsClaro ? "#E4EDF1" : HexPanelOscuro);
            Cambiar(FondoFecha, EsClaro ? "#F8FBFC" : "#0B141D");
            Cambiar(BordeFecha, EsClaro ? "#9BB2BD" : "#405565");
            Cambiar(TextoFecha, EsClaro ? "#172832" : "#E7F0F5");
        }

        private static void Cambiar(SolidColorBrush pincel, string color)
        {
            pincel.Color = ColorDe(color);
        }
        // ====================================================================
        //  Colores de la casa (texto hexadecimal, igual que en CSS)
        //  Los ocho primero digitos son el alfa: FF = opaco, 33 = casi transparente.
        // ====================================================================

        public const string HexFondo = "#14212A";          // fondo general
        public const string HexFondoAlto = "#1B2B36";      // fondo un poco mas claro
        public const string HexChasis = "#243743";         // chasis de la lavadora
        public const string HexAcero = "#304957";          // acero de paneles y encabezados
        public const string HexAceroClaro = "#8AA6B5";     // bordes y separadores de acero
        public const string HexCian = "#6FD3E8";           // acento principal (pantallas)
        public const string HexAmbar = "#E8B15A";          // acento secundario (alertas suaves)
        public const string HexTexto = "#E6EDF3";          // texto principal
        public const string HexTextoSecundario = "#9FB0BF"; // etiquetas y ayudas
        public const string HexError = "#E06C6C";          // error
        public const string HexExito = "#7ED09A";          // confirmacion

        // Cristal (glassmorphism tenue): blanco con muy poca opacidad.
        public const string HexCristal = "#0FFFFFFF";      // blanco al 6 por ciento
        public const string HexCristalBorde = "#24FFFFFF"; // blanco al 14 por ciento
        public const string HexLinea = "#17FFFFFF";        // separadores sutiles
        public const string HexPanelOscuro = "#070D13";    // pantallas digitales

        // ====================================================================
        //  Pinceles de uso comun
        //  Se crean una sola vez y se comparten: es mas rapido y gasta menos memoria.
        //  (Un pincel de WPF es de solo lectura para pintar, se puede reutilizar.)
        // ====================================================================

        public static readonly SolidColorBrush Fondo = Pincel(HexFondo);
        public static readonly SolidColorBrush FondoAlto = Pincel(HexFondoAlto);
        public static readonly SolidColorBrush Chasis = Pincel(HexChasis);
        public static readonly SolidColorBrush Acero = Pincel(HexAcero);
        public static readonly SolidColorBrush AceroClaro = Pincel(HexAceroClaro);
        public static readonly SolidColorBrush Cian = Pincel(HexCian);
        public static readonly SolidColorBrush Ambar = Pincel(HexAmbar);
        public static readonly SolidColorBrush Texto = Pincel(HexTexto);
        public static readonly SolidColorBrush TextoSecundario = Pincel(HexTextoSecundario);
        public static readonly SolidColorBrush Error = Pincel(HexError);
        public static readonly SolidColorBrush Exito = Pincel(HexExito);
        public static readonly SolidColorBrush Cristal = Pincel(HexCristal);
        public static readonly SolidColorBrush CristalBorde = Pincel(HexCristalBorde);
        public static readonly SolidColorBrush Linea = Pincel(HexLinea);
        public static readonly SolidColorBrush PanelOscuro = Pincel(HexPanelOscuro);
        public static readonly SolidColorBrush FondoFecha = Pincel("#0B141D");
        public static readonly SolidColorBrush BordeFecha = Pincel("#405565");
        public static readonly SolidColorBrush TextoFecha = Pincel("#E7F0F5");

        // ====================================================================
        //  Tipografias
        //  Segoe UI para todo el texto y Consolas para los numeros: Consolas es
        //  de ancho fijo, entonces las cifras quedan alineadas como en un tablero.
        // ====================================================================

        public static readonly FontFamily Fuente = new FontFamily("Segoe UI");
        public static readonly FontFamily Numeros = new FontFamily("Consolas");

        // ====================================================================
        //  Conversores y degradados
        // ====================================================================

        /// <summary>Convierte "#6FD3E8" en un color de WPF.</summary>
        public static Color ColorDe(string hex)
        {
            return (Color)ColorConverter.ConvertFromString(hex);
        }

        /// <summary>Crea un pincel de un solo color.</summary>
        public static SolidColorBrush Pincel(string hex)
        {
            return new SolidColorBrush(ColorDe(hex));
        }

        /// <summary>Degradado vertical: el primer color arriba y el ultimo abajo.</summary>
        public static LinearGradientBrush Degradado(params string[] colores)
        {
            return Degradado(new Point(0, 0), new Point(0, 1), colores);
        }

        /// <summary>Degradado diagonal (de la esquina superior izquierda a la inferior derecha).</summary>
        public static LinearGradientBrush DegradadoDiagonal(params string[] colores)
        {
            return Degradado(new Point(0, 0), new Point(1, 1), colores);
        }

        /// <summary>Degradado entre dos puntos de la pantalla. Los colores se reparten parejo.</summary>
        public static LinearGradientBrush Degradado(Point inicio, Point fin, params string[] colores)
        {
            LinearGradientBrush pincel = new LinearGradientBrush();
            pincel.StartPoint = inicio;
            pincel.EndPoint = fin;
            for (int i = 0; i < colores.Length; i++)
            {
                double posicion = colores.Length == 1 ? 0 : (double)i / (colores.Length - 1);
                pincel.GradientStops.Add(new GradientStop(ColorDe(colores[i]), posicion));
            }
            return pincel;
        }

        /// <summary>Degradado circular (radial): util para vidrios y brillos metalicos.</summary>
        public static RadialGradientBrush Radial(Point origen, params string[] colores)
        {
            RadialGradientBrush pincel = new RadialGradientBrush();
            pincel.GradientOrigin = origen;
            pincel.Center = origen;
            for (int i = 0; i < colores.Length; i++)
            {
                double posicion = colores.Length == 1 ? 0 : (double)i / (colores.Length - 1);
                pincel.GradientStops.Add(new GradientStop(ColorDe(colores[i]), posicion));
            }
            return pincel;
        }

        // ====================================================================
        //  Pinturas armadas (se usan en varias ventanas)
        // ====================================================================

        /// <summary>Fondo de la ventana: azul noche muy oscuro.</summary>
        public static LinearGradientBrush FondoVentana()
        {
            return EsClaro
                ? DegradadoDiagonal("#F4F8FA", "#DCE8ED")
                : DegradadoDiagonal(HexFondoAlto, HexFondo);
        }

        /// <summary>Chasis de la lavadora: metal oscuro con un brillo arriba.</summary>
        public static LinearGradientBrush ChasisMetal()
        {
            return EsClaro
                ? Degradado("#FFFFFF", "#E8F0F3", "#D9E5EA")
                : Degradado("#2B414E", HexChasis, "#1B2B36");
        }

        /// <summary>Consola superior: un poco mas clara que el chasis para separarse.</summary>
        public static LinearGradientBrush Consola()
        {
            return EsClaro
                ? Degradado("#FFFFFF", "#E2EDF1")
                : Degradado("#304957", "#20333F");
        }

        /// <summary>Aro metalico plateado del ojo de buey.</summary>
        public static LinearGradientBrush AroMetal()
        {
            return DegradadoDiagonal("#F2F6F8", "#9AAEBC", "#44586A", "#B9C8D2");
        }

        /// <summary>Vidrio del ojo de buey: azul profundo con un brillo arriba a la izquierda.</summary>
        public static RadialGradientBrush Vidrio()
        {
            return Radial(new Point(0.35, 0.28), "#274B60", "#13293A", "#061019");
        }
    }
}
