using System;
using System.Collections.Generic;
using System.IO;
using System.Media;

namespace ClienteLavadora.util
{
    /// <summary>
    /// Sonidos de la aplicacion. Los archivos .wav estan en la carpeta Assets
    /// (se copian junto al .exe). Si un sonido falla, la aplicacion sigue normal.
    /// </summary>
    public static class Sonidos
    {
        // Un reproductor por archivo, para no cargarlo cada vez
        private static readonly Dictionary<string, SoundPlayer> reproductores =
            new Dictionary<string, SoundPlayer>();

        /// <summary>Suena al adicionar una lavadora.</summary>
        public static void Lavadora()
        {
            Reproducir("lavadora.wav");
        }

        /// <summary>Suena una sola vez, cuando se abre la aplicacion.</summary>
        public static void Click()
        {
            Reproducir("click.wav");
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
                        return;
                    }
                    reproductor = new SoundPlayer(ruta);
                    reproductores[archivo] = reproductor;
                }
                reproductor.Play(); // No bloquea la ventana
            }
            catch (Exception)
            {
                // El sonido es solo decoracion: nunca debe tumbar la aplicacion.
            }
        }
    }
}
