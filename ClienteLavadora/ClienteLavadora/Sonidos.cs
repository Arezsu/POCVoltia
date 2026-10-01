using System;
using System.IO;
using System.Media;

namespace ClienteLavadora
{
    /// <summary>Sonidos de la aplicación. El archivo está en Assets\lavadora.wav (se copia junto al .exe).</summary>
    internal static class Sonidos
    {
        private static SoundPlayer reproductor;

        /// <summary>Suena al adicionar una lavadora. Si no hay audio o falla, simplemente no suena.</summary>
        public static void ReproducirLavadora()
        {
            try
            {
                string ruta = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "lavadora.wav");
                if (!File.Exists(ruta))
                {
                    return;
                }

                if (reproductor == null)
                {
                    reproductor = new SoundPlayer(ruta);
                }
                reproductor.Play();   // no bloquea la ventana
            }
            catch (Exception)
            {
                // El sonido es solo decorativo: nunca debe tumbar la aplicación.
            }
        }
    }
}
