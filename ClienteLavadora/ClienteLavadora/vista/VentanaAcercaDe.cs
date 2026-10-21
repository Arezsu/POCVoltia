using System.Windows;
using System.Windows.Controls;
using ClienteLavadora.util;
using ClienteLavadora.vista.controles;

namespace ClienteLavadora.vista
{
    /// <summary>
    /// Ventana ACERCA DE: los datos del curso, los integrantes y las decisiones
    /// tecnicas del proyecto.
    ///
    /// No hace ninguna peticion al servidor: es informacion fija del proyecto.
    /// </summary>
    public class VentanaAcercaDe : VentanaMarco
    {
        public VentanaAcercaDe() : base("ACERCA DE VOLTIA", 680, 640, false, 0)
        {
            StackPanel contenido = new StackPanel();

            // ---------- Encabezado ----------
            TextBlock nombre = new TextBlock();
            nombre.Text = "VOLTIA";
            nombre.FontFamily = Tema.Numeros;
            nombre.FontSize = 26;
            nombre.FontWeight = FontWeights.Bold;
            nombre.Foreground = Tema.Cian;
            contenido.Children.Add(nombre);

            TextBlock descripcion = Ui.Texto("Sistema de gestion de lavadoras industriales", 12, Tema.TextoSecundario);
            descripcion.Margin = new Thickness(0, 2, 0, 0);
            contenido.Children.Add(descripcion);

            contenido.Children.Add(Ui.Separador());

            // ---------- Curso ----------
            contenido.Children.Add(Ui.Rotulo("CURSO"));
            contenido.Children.Add(Fila("Asignatura", "Diseno de Soluciones"));
            contenido.Children.Add(Fila("Proyecto", "Segundo Proyecto 2026B"));
            contenido.Children.Add(Fila("Programa", "Ingenieria de Sistemas"));

            contenido.Children.Add(Ui.Separador());

            // ---------- Integrantes ----------
            contenido.Children.Add(Ui.Rotulo("INTEGRANTES"));
            contenido.Children.Add(Fila("Juan Andres Bermeo Alvarez", "2220241053"));
            contenido.Children.Add(Fila("Alejandro Sanchez Quimbayo", "2220241077"));
            contenido.Children.Add(Fila("David Arredondo", "2220241062"));

            contenido.Children.Add(Ui.Separador());

            // ---------- Tecnologias ----------
            contenido.Children.Add(Ui.Rotulo("TECNOLOGIAS"));
            contenido.Children.Add(Fila("Cliente", "C# WPF sin XAML (.NET Framework 4.8)"));
            contenido.Children.Add(Fila("Servidor", "Java Spring Boot con GraphQL"));
            contenido.Children.Add(Fila("Datos", "En memoria dentro del servidor"));

            contenido.Children.Add(Ui.Separador());

            // ---------- Patrones ----------
            contenido.Children.Add(Ui.Rotulo("PATRONES Y PRINCIPIOS"));
            contenido.Children.Add(Fila("Arquitectura", "MVC por paquetes (modelo, servicios, controlador, vista)"));
            contenido.Children.Add(Fila("Patrones", "Singleton, Builder, Observer y segregacion de interfaces"));
            contenido.Children.Add(Fila("Concurrencia", "async/await con Task.Run y Dispatcher.Invoke"));
            contenido.Children.Add(Fila("Relacion", "Lavadora (1) *-- (0..*) Mantenimiento"));

            ScrollViewer desplazable = Ui.Desplazable(contenido);
            desplazable.MaxHeight = 440;

            BotonVoltia btnCerrar = BotonVoltia.Principal("Cerrar");
            btnCerrar.Width = 130;
            btnCerrar.HorizontalAlignment = HorizontalAlignment.Right;
            btnCerrar.Margin = new Thickness(0, 12, 0, 0);
            btnCerrar.Click += (s, e) => Cerrar();

            Grid raiz = new Grid();
            raiz.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            raiz.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            raiz.Children.Add(Ui.Cristal(desplazable, new Thickness(16)));
            Grid.SetRow(btnCerrar, 1);
            raiz.Children.Add(btnCerrar);

            Contenido = raiz;

            Loaded += (s, e) => Anime.Escalonado(new UIElement[] { raiz }, 60, 240);
        }

        /// <summary>Una fila "dato - valor" del panel.</summary>
        private static FrameworkElement Fila(string etiqueta, string valor)
        {
            Grid rejilla = new Grid();
            rejilla.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(215) });
            rejilla.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            TextBlock izquierda = Ui.Texto(etiqueta, 11.5, Tema.TextoSecundario);
            izquierda.VerticalAlignment = VerticalAlignment.Center;
            rejilla.Children.Add(izquierda);

            TextBlock derecha = Ui.Texto(valor, 12, Tema.Texto);
            derecha.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(derecha, 1);
            rejilla.Children.Add(derecha);

            Border fondo = new Border();
            fondo.Padding = new Thickness(8, 4, 8, 4);
            fondo.Margin = new Thickness(0, 0, 0, 2);
            fondo.CornerRadius = new CornerRadius(6);
            fondo.Background = Tema.Pincel("#08FFFFFF");
            fondo.Child = rejilla;
            return fondo;
        }
    }
}
