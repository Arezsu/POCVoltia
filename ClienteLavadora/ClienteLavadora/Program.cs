using System;
using System.Windows;
using System.Windows.Threading;
using ClienteLavadora.vista;

namespace ClienteLavadora
{
    /// <summary>
    /// Punto de entrada del cliente de escritorio.
    ///
    /// Como el proyecto NO usa XAML, aqui se crea a mano el objeto Application (que es
    /// el que administra la aplicacion WPF) y se abre la ventana principal. La
    /// aplicacion termina cuando se cierra esa ventana.
    ///
    /// [STAThread] es obligatorio en WPF: la interfaz grafica de Windows trabaja con
    /// apartamentos de un solo hilo (Single Thread Apartment).
    /// </summary>
    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            Application aplicacion = new Application();
            aplicacion.ShutdownMode = ShutdownMode.OnMainWindowClose;

            // Red de seguridad: si algo falla en la interfaz, se muestra un mensaje en
            // lugar de cerrar el programa de golpe.
            aplicacion.DispatcherUnhandledException += AlFallar;

            VentanaPrincipal principal = new VentanaPrincipal();
            aplicacion.Run(principal);
        }

        private static void AlFallar(object remitente, DispatcherUnhandledExceptionEventArgs argumentos)
        {
            MessageBox.Show(
                "Ocurrio un error inesperado:" + Environment.NewLine + argumentos.Exception.Message,
                "VOLTIA", MessageBoxButton.OK, MessageBoxImage.Error);
            argumentos.Handled = true;
        }
    }
}
