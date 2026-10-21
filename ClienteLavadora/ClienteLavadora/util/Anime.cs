using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace ClienteLavadora.util
{
    /// <summary>
    /// Mini libreria de animaciones inspirada en anime.js (animejs.com).
    /// anime.js es de JavaScript y no se puede usar en C#, pero WPF trae su propio
    /// sistema de animaciones y aqui lo usamos para lograr los mismos efectos:
    ///   - Escalonado (stagger): los elementos entran uno tras otro.
    ///   - Resorte (elastic): rebotan al llegar, como un muelle.
    ///   - Bucles infinitos: la ropa girando, las burbujas subiendo, los LED parpadeando.
    ///   - Contador: un numero que sube de 0 hasta su valor.
    ///   - Maquina de escribir: el texto aparece letra por letra.
    /// Cada metodo de esta clase hace UN efecto. Asi las ventanas solo escriben
    /// una linea, por ejemplo: Anime.Aparecer(Marco);
    /// </summary>
    public static class Anime
    {
        // ====================================================================
        //  CURVAS DE MOVIMIENTO (easing)
        //  Una curva dice COMO se mueve algo: rapido al inicio, rebota al final, etc.
        // ====================================================================

        /// <summary>Rebota varias veces al llegar, como un resorte.</summary>
        public static IEasingFunction Elastico() // Curva de aceleracion
        {
            return new ElasticEase { EasingMode = EasingMode.EaseOut, Oscillations = 2, Springiness = 5 };
        }

        /// <summary>Se pasa un poquito y regresa (efecto "pop").</summary>
        public static IEasingFunction Rebote()
        {
            return new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.7 };
        }

        /// <summary>Empieza rapido y frena suave.</summary>
        public static IEasingFunction Suave()
        {
            return new CubicEase { EasingMode = EasingMode.EaseOut };
        }

        /// <summary>Va y viene de forma suave (para bucles).</summary>
        public static IEasingFunction Ola()
        {
            return new SineEase { EasingMode = EasingMode.EaseInOut };
        }

        // ====================================================================
        //  BASE: transformaciones y animacion generica
        // ====================================================================

        /// <summary>
        /// Le pone al elemento un grupo de 3 transformaciones: [0] escala, [1] giro, [2] movimiento.
        /// Con ellas se puede agrandar, rotar y mover cualquier elemento sin cambiar su diseno.
        /// </summary>
        private static TransformGroup Preparar(UIElement elemento)
        {
            TransformGroup grupo = elemento.RenderTransform as TransformGroup;
            if (grupo != null && grupo.Children.Count == 3
                && grupo.Children[0] is ScaleTransform
                && grupo.Children[1] is RotateTransform
                && grupo.Children[2] is TranslateTransform)
            {
                return grupo; // Ya estaba preparado
            }

            grupo = new TransformGroup();
            grupo.Children.Add(new ScaleTransform(1, 1));
            grupo.Children.Add(new RotateTransform(0));
            grupo.Children.Add(new TranslateTransform(0, 0));
            elemento.RenderTransform = grupo;
            elemento.RenderTransformOrigin = new Point(0.5, 0.5); // Gira y crece desde el centro
            return grupo;
        }

        private static ScaleTransform Escala(UIElement elemento)
        {
            return (ScaleTransform)Preparar(elemento).Children[0];
        }

        private static RotateTransform Giro(UIElement elemento)
        {
            return (RotateTransform)Preparar(elemento).Children[1];
        }

        private static TranslateTransform Movimiento(UIElement elemento)
        {
            return (TranslateTransform)Preparar(elemento).Children[2];
        }

        /// <summary>
        /// Anima UNA propiedad numerica (opacidad, escala, giro, posicion...).
        /// desde = null significa "desde el valor actual".
        /// </summary>
        private static void Lanzar(IAnimatable destino, DependencyProperty propiedad, double? desde,
                                   double hasta, int ms, IEasingFunction curva, int retrasoMs,
                                   bool infinito, bool vaiven)
        {
            DoubleAnimation animacion = new DoubleAnimation();
            if (desde.HasValue)
            {
                animacion.From = desde.Value;
            }
            animacion.To = hasta;
            animacion.Duration = TimeSpan.FromMilliseconds(ms);
            animacion.BeginTime = TimeSpan.FromMilliseconds(retrasoMs);
            if (curva != null)
            {
                animacion.EasingFunction = curva;
            }
            if (infinito)
            {
                animacion.RepeatBehavior = RepeatBehavior.Forever;
                Timeline.SetDesiredFrameRate(animacion, 30); // 30 cuadros: gasta menos computador
            }
            animacion.AutoReverse = vaiven; // Va y regresa
            destino.BeginAnimation(propiedad, animacion);
        }

        // ====================================================================
        //  EFECTOS DE ENTRADA Y SALIDA
        // ====================================================================

        /// <summary>
        /// La ventana "salta" a la pantalla: crece con resorte, sube y se hace visible.
        /// </summary>
        public static void Aparecer(UIElement elemento, int retrasoMs = 0)
        {
            ScaleTransform escala = Escala(elemento);
            TranslateTransform mover = Movimiento(elemento);

            elemento.Opacity = 0;
            Lanzar(elemento, UIElement.OpacityProperty, 0, 1, 350, Suave(), retrasoMs, false, false);
            Lanzar(escala, ScaleTransform.ScaleXProperty, 0.6, 1, 800, Elastico(), retrasoMs, false, false);
            Lanzar(escala, ScaleTransform.ScaleYProperty, 0.6, 1, 800, Elastico(), retrasoMs, false, false);
            Lanzar(mover, TranslateTransform.YProperty, 40, 0, 650, Rebote(), retrasoMs, false, false);
        }

        /// <summary>
        /// Efecto escalonado (stagger de anime.js): cada elemento entra un poquito
        /// despues del anterior. pasoMs = espera entre uno y otro.
        /// </summary>
        public static void Escalonado(IEnumerable<UIElement> elementos, int pasoMs = 70, int inicioMs = 200) // Lista recorrible
        {
            int posicion = 0;
            foreach (UIElement elemento in elementos)
            {
                int retraso = inicioMs + posicion * pasoMs;
                TranslateTransform mover = Movimiento(elemento);

                elemento.Opacity = 0; // Empieza invisible
                Lanzar(elemento, UIElement.OpacityProperty, 0, 1, 420, Suave(), retraso, false, false);
                Lanzar(mover, TranslateTransform.YProperty, 28, 0, 600, Rebote(), retraso, false, false);
                posicion++;
            }
        }

        /// <summary>Entrada lateral (para las filas de la tabla).</summary>
        public static void Entrada(UIElement elemento, int retrasoMs)
        {
            TranslateTransform mover = Movimiento(elemento);
            elemento.Opacity = 0;
            Lanzar(elemento, UIElement.OpacityProperty, 0, 1, 350, Suave(), retrasoMs, false, false);
            Lanzar(mover, TranslateTransform.XProperty, -40, 0, 500, Rebote(), retrasoMs, false, false);
        }

        /// <summary>La ventana se encoge y desaparece. Al terminar ejecuta alTerminar (cerrar la ventana).</summary>
        public static void Salir(UIElement elemento, Action alTerminar)
        {
            ScaleTransform escala = Escala(elemento);
            IEasingFunction curva = new BackEase { EasingMode = EasingMode.EaseIn, Amplitude = 0.6 };

            Lanzar(elemento, UIElement.OpacityProperty, null, 0, 230, Suave(), 0, false, false);
            Lanzar(escala, ScaleTransform.ScaleYProperty, null, 0.7, 230, curva, 0, false, false);

            DoubleAnimation encogerX = new DoubleAnimation();
            encogerX.To = 0.7;
            encogerX.Duration = TimeSpan.FromMilliseconds(230);
            encogerX.EasingFunction = curva;
            encogerX.Completed += (s, e) => // Funcion anonima corta
            {
                if (alTerminar != null)
                {
                    alTerminar();
                }
            };
            escala.BeginAnimation(ScaleTransform.ScaleXProperty, encogerX);
        }

        // ====================================================================
        //  EFECTOS DE REACCION (cuando el usuario hace algo)
        // ====================================================================

        /// <summary>Sacudida de izquierda a derecha. Se usa cuando hay un error.</summary>
        public static void Sacudir(UIElement elemento)
        {
            double[] pasos = { -14, 14, -10, 10, -5, 5, 0 };
            DoubleAnimationUsingKeyFrames animacion = new DoubleAnimationUsingKeyFrames();
            for (int i = 0; i < pasos.Length; i++)
            {
                TimeSpan momento = TimeSpan.FromMilliseconds(55 * (i + 1));
                animacion.KeyFrames.Add(new LinearDoubleKeyFrame(pasos[i], KeyTime.FromTimeSpan(momento)));
            }
            Movimiento(elemento).BeginAnimation(TranslateTransform.XProperty, animacion);
        }

        /// <summary>Un latido: crece un poco y vuelve con resorte. Se usa cuando algo sale bien.</summary>
        public static void Pulso(UIElement elemento)
        {
            ScaleTransform escala = Escala(elemento);
            DependencyProperty[] ejes = { ScaleTransform.ScaleXProperty, ScaleTransform.ScaleYProperty };
            foreach (DependencyProperty eje in ejes)
            {
                DoubleAnimationUsingKeyFrames animacion = new DoubleAnimationUsingKeyFrames();
                animacion.KeyFrames.Add(new EasingDoubleKeyFrame(1.05,
                    KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(120)), Suave()));
                animacion.KeyFrames.Add(new EasingDoubleKeyFrame(1.0,
                    KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(650)), Elastico()));
                escala.BeginAnimation(eje, animacion);
            }
        }

        /// <summary>Efecto de los botones: crecen al pasar el mouse y vuelven al salir.</summary>
        public static void Hover(UIElement elemento, bool entrando)
        {
            ScaleTransform escala = Escala(elemento);
            double destino = entrando ? 1.07 : 1.0;
            Lanzar(escala, ScaleTransform.ScaleXProperty, null, destino, 450, Elastico(), 0, false, false);
            Lanzar(escala, ScaleTransform.ScaleYProperty, null, destino, 450, Elastico(), 0, false, false);
        }

        /// <summary>Gira el elemento "grados" mas de donde estaba (por ejemplo un indicador circular).</summary>
        public static void GirarMas(UIElement elemento, double grados, int ms)
        {
            RotateTransform giro = Giro(elemento);
            double actual = giro.Angle;
            Lanzar(giro, RotateTransform.AngleProperty, actual, actual + grados, ms, Elastico(), 0, false, false);
        }

        // ====================================================================
        //  BUCLES INFINITOS (decoracion que nunca se detiene)
        // ====================================================================

        /// <summary>Gira de "desde" a "hasta" grados. Con infinito = true no para nunca.</summary>
        public static void Girar(UIElement elemento, double desde, double hasta, int ms,
                                 bool infinito, IEasingFunction curva = null, int retrasoMs = 0)
        {
            Lanzar(Giro(elemento), RotateTransform.AngleProperty, desde, hasta, ms, curva, retrasoMs, infinito, false);
        }

        /// <summary>Se mece de un lado a otro, como una hamaca.</summary>
        public static void Mecer(UIElement elemento, double grados, int ms)
        {
            Lanzar(Giro(elemento), RotateTransform.AngleProperty, -grados, grados, ms, Ola(), 0, true, true);
        }

        /// <summary>Se desliza de lado a lado sin parar (las olas del agua).</summary>
        public static void Deslizar(UIElement elemento, double desdeX, double hastaX, int ms)
        {
            Lanzar(Movimiento(elemento), TranslateTransform.XProperty, desdeX, hastaX, ms, null, 0, true, false);
        }

        /// <summary>Sube y se desvanece una y otra vez (las burbujas).</summary>
        public static void Subir(UIElement elemento, double desdeY, double hastaY, int ms, int retrasoMs)
        {
            Lanzar(Movimiento(elemento), TranslateTransform.YProperty, desdeY, hastaY, ms, Suave(), retrasoMs, true, false);
            Lanzar(elemento, UIElement.OpacityProperty, 0.9, 0, ms, null, retrasoMs, true, false);
        }

        /// <summary>Parpadea suave (los LED de la consola).</summary>
        public static void Parpadeo(UIElement elemento, int retrasoMs)
        {
            Lanzar(elemento, UIElement.OpacityProperty, 1, 0.25, 700, Ola(), retrasoMs, true, true);
        }

        // ====================================================================
        //  EFECTOS EXTRA (mas movimiento)
        // ====================================================================

        /// <summary>La tarjeta se voltea como una carta: pasa de "delgadita" a su ancho normal con rebote.</summary>
        public static void Voltear(UIElement elemento, int retrasoMs)
        {
            ScaleTransform escala = Escala(elemento);
            elemento.Opacity = 0;
            Lanzar(elemento, UIElement.OpacityProperty, 0, 1, 250, Suave(), retrasoMs, false, false);
            Lanzar(escala, ScaleTransform.ScaleXProperty, 0, 1, 650, Rebote(), retrasoMs, false, false);
        }

        /// <summary>
        /// El elemento sale disparado hacia (dx, dy), gira y se desvanece (el confeti).
        /// Al terminar ejecuta alTerminar (para quitarlo de la pantalla).
        /// </summary>
        public static void Explotar(UIElement elemento, double dx, double dy, int ms, Action alTerminar)
        {
            TransformGroup grupo = Preparar(elemento);
            TranslateTransform mover = (TranslateTransform)grupo.Children[2];
            RotateTransform giro = (RotateTransform)grupo.Children[1];

            Lanzar(mover, TranslateTransform.XProperty, 0, dx, ms, Suave(), 0, false, false);
            Lanzar(mover, TranslateTransform.YProperty, 0, dy, ms, Suave(), 0, false, false);
            Lanzar(giro, RotateTransform.AngleProperty, 0, dx > 0 ? 540 : -540, ms, null, 0, false, false);

            DoubleAnimation desvanecer = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(ms));
            desvanecer.BeginTime = TimeSpan.FromMilliseconds(ms * 0.3); // Primero viaja y despues se desvanece
            desvanecer.Completed += (s, e) =>
            {
                if (alTerminar != null)
                {
                    alTerminar();
                }
            };
            elemento.BeginAnimation(UIElement.OpacityProperty, desvanecer);
        }

        /// <summary>Cambia la velocidad de un giro infinito (el tambor del ojo de buey gira mas rapido al procesar).</summary>
        public static void Acelerar(UIElement elemento, int msPorVuelta)
        {
            RotateTransform giro = Giro(elemento);
            double actual = giro.Angle; // Sigue desde donde iba
            Lanzar(giro, RotateTransform.AngleProperty, actual, actual + 360, msPorVuelta, null, 0, true, false);
        }

        /// <summary>Flota suavemente hacia arriba y abajo para siempre (como si no pesara).</summary>
        public static void Flotar(UIElement elemento, double distancia, int ms, int retrasoMs)
        {
            Lanzar(Movimiento(elemento), TranslateTransform.YProperty, -distancia, distancia, ms, Ola(), retrasoMs, true, true);
        }

        /// <summary>Detiene un giro infinito y deja el elemento en su posicion actual.</summary>
        public static void Parar(UIElement elemento)
        {
            RotateTransform giro = Giro(elemento);
            double actual = giro.Angle;
            giro.BeginAnimation(RotateTransform.AngleProperty, null); // Quita la animacion
            giro.Angle = actual;
        }

        // ====================================================================
        //  TEXTO Y NUMEROS
        // ====================================================================

        /// <summary>
        /// Efecto maquina de escribir: el texto que ya tiene el TextBlock aparece letra por letra.
        /// Al terminar la animacion, el TextBlock vuelve a tener su texto normal.
        /// </summary>
        public static void Escribir(TextBlock texto, int msPorLetra = 45, int retrasoMs = 300)
        {
            string completo = texto.Text;
            if (string.IsNullOrEmpty(completo))
            {
                return;
            }

            StringAnimationUsingKeyFrames animacion = new StringAnimationUsingKeyFrames();
            animacion.FillBehavior = FillBehavior.Stop; // Al terminar, vuelve al texto completo
            animacion.KeyFrames.Add(new DiscreteStringKeyFrame("", KeyTime.FromTimeSpan(TimeSpan.Zero)));
            for (int i = 1; i <= completo.Length; i++)
            {
                TimeSpan momento = TimeSpan.FromMilliseconds(retrasoMs + i * msPorLetra);
                animacion.KeyFrames.Add(new DiscreteStringKeyFrame(completo.Substring(0, i), KeyTime.FromTimeSpan(momento)));
            }
            texto.BeginAnimation(TextBlock.TextProperty, animacion);
        }

        /// <summary>
        /// Contador: el numero sube de 0 hasta "hasta" (como el contador de anime.js).
        /// formato es un texto como "{0} lavadora(s)", donde {0} es el numero.
        /// </summary>
        public static void Contar(TextBlock texto, int hasta, string formato, int ms = 700)
        {
            DispatcherTimer reloj = new DispatcherTimer();
            reloj.Interval = TimeSpan.FromMilliseconds(20);
            DateTime inicio = DateTime.Now;
            IEasingFunction curva = Suave();

            reloj.Tick += (s, e) => // Cada 20 milisegundos
            {
                double avance = (DateTime.Now - inicio).TotalMilliseconds / ms;
                if (avance >= 1)
                {
                    avance = 1;
                    reloj.Stop();
                }
                int valor = (int)Math.Round(hasta * curva.Ease(avance));
                texto.Text = string.Format(formato, valor);
            };
            reloj.Start();
        }
    }
}
