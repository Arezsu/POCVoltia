using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ClienteLavadora.util;
using ClienteLavadora.vista.controles;

namespace ClienteLavadora.vista
{
    /// <summary>
    /// Ventana base con marco nativo de Windows y una vista independiente por caso
    /// de uso. El contenido y las reglas de negocio siguen separados por MVC.
    ///
    /// Cada caso de uso (listar, agregar, consultar...) hereda de esta clase y solo
    /// escribe su contenido con la propiedad Contenido. Aqui quedan resueltos de una
    /// vez: el tamano, el color de fondo, la tecla ESC para cerrar y los avisos.
    ///
    /// Esa es la "V" de MVC: esta capa NO hace peticiones de red ni sabe que existe
    /// GraphQL; para eso estan los controladores.
    /// </summary>
    public class VentanaMarco : Window
    {
        /// <summary>Contenedor de titulo, estado y contenido de la vista.</summary>
        protected MarcoLavadora Marco { get; private set; }

        public VentanaMarco(string titulo, double ancho, double alto, bool mostrarOjo, double ladoOjo)
        {
            Title = titulo;
            Rect areaTrabajo = SystemParameters.WorkArea;
            Width = Math.Min(ancho, Math.Max(360, areaTrabajo.Width - 24));
            Height = Math.Min(alto, Math.Max(320, areaTrabajo.Height - 24));
            WindowStyle = WindowStyle.SingleBorderWindow;
            AllowsTransparency = false;
            Background = Tema.Fondo;
            ResizeMode = ResizeMode.CanResize;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ShowInTaskbar = true;
            FontFamily = Tema.Fuente;

            Marco = new MarcoLavadora(titulo, mostrarOjo, ladoOjo);
            Content = Marco;

            KeyDown += AlPresionarTecla;
        }

        /// <summary>Lo que va dentro del cuerpo de la lavadora.</summary>
        protected UIElement Contenido
        {
            set { Marco.Contenido = value; }
        }

        // ====================================================================
        //  Apoyos que usan todas las ventanas hijas
        // ====================================================================

        /// <summary>Cierra la ventana actual.</summary>
        protected void Cerrar()
        {
            Marco.Cerrar();
        }

        /// <summary>Actualiza el estado visible despues de una operacion exitosa.</summary>
        protected void Celebrar()
        {
            Marco.Celebrar();
        }

        /// <summary>Muestra un mensaje de error con el estilo de la casa.</summary>
        protected void Aviso(string titulo, string mensaje)
        {
            DialogoVoltia.Avisar(this, titulo, mensaje);
        }

        /// <summary>Muestra un mensaje informativo (algo salio bien).</summary>
        protected void Informar(string titulo, string mensaje)
        {
            DialogoVoltia.Informar(this, titulo, mensaje);
        }

        /// <summary>Pregunta antes de una accion destructiva. Devuelve true si el usuario acepta.</summary>
        protected bool Confirmar(string titulo, string mensaje, string textoAceptar, bool peligro)
        {
            return DialogoVoltia.Confirmar(this, titulo, mensaje, textoAceptar, peligro);
        }

        // ====================================================================
        //  Teclado
        // ====================================================================

        private void AlPresionarTecla(object remitente, KeyEventArgs argumentos)
        {
            if (argumentos.Key == Key.Escape)
            {
                argumentos.Handled = true;
                Cerrar();
            }
        }
    }
}
