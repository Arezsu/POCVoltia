using System;
using System.Windows;
using ClienteLavadora.gui;

namespace ClienteLavadora
{
    /// <summary>
    /// Punto de entrada de la aplicacion. Como no hay XAML, aqui se crea el objeto
    /// Application y se abre la ventana principal.
    /// </summary>
    public static class Program
    {
        [STAThread] // Obligatorio para aplicaciones de ventanas WPF
        public static void Main()
        {
            Application aplicacion = new Application();
            VentanaPrincipal principal = new VentanaPrincipal();
            aplicacion.Run(principal); // Corre hasta que se cierre la ventana principal
        }
    }
}
