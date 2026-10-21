using System;
using System.Collections.Generic;
using System.IO;
using System.Media;

namespace ClienteLavadora.util
{
    /// <summary>
    /// Sonidos de la aplicacion. Los archivos .wav viven en la carpeta Assets y se
    /// copian junto al .exe al compilar.
    ///
    /// REGLA DE ORO: un adorno NUNCA debe tumbar el programa. Por eso todo esta
    /// dentro de try/catch: si el archivo no existe o la tarjeta de sonido esta
    /// ocupada, el programa sigue funcionando en silencio.
    /// </summary>
    public static class Sonidos
    {
        // Un reproductor por archivo, para no cargar el .wav en cada clic.
        private static readonly Dictionary<string, SoundPlayer> reproductores =
            new Dictionary<string, SoundPlayer>();

        /// <summary>Sonido de clic (se usa al abrir ventanas y en los botones importantes).</summary>
        public static void Click()
        {
            Reproducir("click.wav");
        }

        /// <summary>Sonido de la lavadora: se usa cuando una operacion sale bien.</summary>
        public static void Lavadora()
        {
            Reproducir("lavadora.wav");
        }

        private static void Reproducir(string archivo)
        {
            try
            {
                SoundPlayer reproductor;
                if (!reproductores.TryGetValue(archivo, out reproductor))
                {
                    string ruta = Path.Combine(AppContext.BaseDirectory, "Assets", archivo);
                    if (!File.Exists(ruta))
                    {
                        return; // No hay archivo: no suena nada, pero no falla
                    }
                    reproductor = new SoundPlayer(ruta);
                    reproductores[archivo] = reproductor;
                }
                reproductor.Play(); // Play (y no PlaySync) para no congelar la ventana
            }
            catch (Exception)
            {
                // El sonido es decoracion: se ignora cualquier problema.
            }
        }
    }
}
