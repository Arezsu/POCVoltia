using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ClienteLavadora.util;

namespace ClienteLavadora.vista.controles
{
    /// <summary>
    /// El OJO DE BUEY: la compuerta circular de la lavadora con su aro metalico, el
    /// vidrio, el agua, la ropa y las burbujas que NUNCA dejan de girar.
    ///
    /// Es un Viewbox, o sea que todo el dibujo se hace en una cuadricula de 200x200
    /// y despues se agranda o se achica cambiando la propiedad Width. Asi el mismo
    /// control sirve para la puerta grande de la ventana principal y para la pequena
    /// del marco de las demas ventanas.
    ///
    /// CONCURRENCIA VISIBLE: cuando hay peticiones al servidor en camino, el tambor
    /// gira mas rapido. Para saberlo no le pregunta a nadie: se SUSCRIBE al evento
    /// NotificadorDatos.Trabajando (patron OBSERVER) y se desuscribe al desaparecer.
    /// </summary>
    public class OjoDeBuey : Viewbox
    {
        private readonly Canvas ropa;
        private readonly Canvas burbujas;
        private readonly Path olaAtras;
        private readonly Path olaFrente;

        private bool animado;
        private bool suscrito;
        private bool girandoRapido;

        public OjoDeBuey(double lado)
        {
            Width = lado;
            Height = lado;
            Stretch = Stretch.Uniform;
            Cursor = Cursors.Hand;

            // ------------------------------------------------------------------
            //  Todo se dibuja en 200 x 200 y el Viewbox lo escala.
            // ------------------------------------------------------------------
            Grid raiz = new Grid();
            raiz.Width = 200;
            raiz.Height = 200;
            raiz.Background = Brushes.Transparent;

            // Aro metalico exterior (gradiente de plata)
            Ellipse aro = new Ellipse();
            aro.Width = 200;
            aro.Height = 200;
            aro.Fill = Tema.AroMetal();
            raiz.Children.Add(aro);

            // Bisel oscuro entre el aro y el vidrio
            Ellipse bisel = new Ellipse();
            bisel.Width = 176;
            bisel.Height = 176;
            bisel.Fill = Tema.Pincel("#121B23");
            bisel.Stroke = Tema.Pincel("#3A4C5C");
            bisel.StrokeThickness = 3;
            raiz.Children.Add(bisel);

            // ------------------------------------------------------------------
            //  Vidrio: todo lo de adentro se recorta en circulo con un Clip.
            // ------------------------------------------------------------------
            Grid vidrio = new Grid();
            vidrio.Width = 158;
            vidrio.Height = 158;
            vidrio.Clip = new EllipseGeometry(new Point(79, 79), 79, 79);

            Ellipse fondo = new Ellipse();
            fondo.Fill = Tema.Vidrio();
            vidrio.Children.Add(fondo);

            // Agua: dos olas que se deslizan en sentidos contrarios.
            Canvas agua = new Canvas();
            agua.Width = 158;
            agua.Height = 158;
            olaAtras = CrearOla("#336FD3E8", 10);
            olaFrente = CrearOla("#556FD3E8", 24);
            agua.Children.Add(olaAtras);
            agua.Children.Add(olaFrente);
            vidrio.Children.Add(agua);

            // Ropa: este Canvas es el que gira.
            ropa = new Canvas();
            ropa.Width = 158;
            ropa.Height = 158;
            ropa.Children.Add(Prenda(34, 50, 46, 24, "#E8B15A", -18));   // toalla ambar
            ropa.Children.Add(Prenda(82, 68, 44, 28, "#C9D6DF", 22));    // camisa clara
            ropa.Children.Add(Prenda(50, 90, 48, 22, "#4C7F92", 6));     // jean azul
            ropa.Children.Add(Bola(98, 42, 24, "#2E4050"));              // media oscura
            ropa.Children.Add(Bola(28, 94, 20, "#7E93A3"));              // media gris
            vidrio.Children.Add(ropa);

            // Burbujas: suben y se desvanecen una y otra vez.
            burbujas = new Canvas();
            burbujas.Width = 158;
            burbujas.Height = 158;
            burbujas.Children.Add(Burbuja(30, 138, 12));
            burbujas.Children.Add(Burbuja(60, 144, 9));
            burbujas.Children.Add(Burbuja(90, 136, 15));
            burbujas.Children.Add(Burbuja(118, 142, 10));
            burbujas.Children.Add(Burbuja(74, 148, 7));
            burbujas.Children.Add(Burbuja(46, 130, 8));
            vidrio.Children.Add(burbujas);

            // Brillo del vidrio (reflejo diagonal)
            Ellipse brillo = new Ellipse();
            brillo.Width = 96;
            brillo.Height = 52;
            brillo.HorizontalAlignment = HorizontalAlignment.Left;
            brillo.VerticalAlignment = VerticalAlignment.Top;
            brillo.Margin = new Thickness(14, 8, 0, 0);
            brillo.RenderTransform = new RotateTransform(-32, 48, 26);
            brillo.Fill = Tema.Degradado("#66FFFFFF", "#00FFFFFF");
            brillo.IsHitTestVisible = false;
            vidrio.Children.Add(brillo);

            raiz.Children.Add(vidrio);

            // Reflejo fino en el borde del aro
            Ellipse reflejo = new Ellipse();
            reflejo.Width = 190;
            reflejo.Height = 190;
            reflejo.Stroke = Tema.Pincel("#40FFFFFF");
            reflejo.StrokeThickness = 1;
            reflejo.IsHitTestVisible = false;
            raiz.Children.Add(reflejo);

            Child = raiz;

            Loaded += AlCargar;
            Unloaded += AlDescargar;
            MouseLeftButtonDown += (s, e) => Anime.Pulso(this);
        }

        /// <summary>true mientras el tambor gira rapido (hay una peticion en curso).</summary>
        public bool Rapido
        {
            get { return girandoRapido; }
        }

        // ====================================================================
        //  Dibujo
        // ====================================================================

        /// <summary>
        /// Dibuja una ola como una linea de curvas repetidas (Q = curva cuadratica).
        /// Como el patron se repite cada 40 pixeles, al deslizarla 80 pixeles parece
        /// agua que nunca termina.
        /// </summary>
        private static Path CrearOla(string color, double arriba)
        {
            StringBuilder texto = new StringBuilder("M-160,70");
            int x = -160;
            bool sube = true;
            while (x < 320)
            {
                int control = sube ? 50 : 90;
                texto.Append(" Q" + (x + 20) + "," + control + " " + (x + 40) + ",70");
                sube = !sube;
                x += 40;
            }
            texto.Append(" V170 H-160 Z"); // Cierra la figura hacia abajo para rellenar el agua

            Path ola = new Path();
            ola.Fill = Tema.Pincel(color);
            ola.Data = Geometry.Parse(texto.ToString());
            Canvas.SetTop(ola, arriba);
            return ola;
        }

        private static Rectangle Prenda(double x, double y, double ancho, double alto, string color, double angulo)
        {
            Rectangle prenda = new Rectangle();
            prenda.Width = ancho;
            prenda.Height = alto;
            prenda.RadiusX = 8;
            prenda.RadiusY = 8;
            prenda.Fill = Tema.Pincel(color);
            prenda.RenderTransform = new RotateTransform(angulo, ancho / 2, alto / 2);
            Canvas.SetLeft(prenda, x);
            Canvas.SetTop(prenda, y);
            return prenda;
        }

        private static Ellipse Bola(double x, double y, double diametro, string color)
        {
            Ellipse bola = new Ellipse();
            bola.Width = diametro;
            bola.Height = diametro;
            bola.Fill = Tema.Pincel(color);
            Canvas.SetLeft(bola, x);
            Canvas.SetTop(bola, y);
            return bola;
        }

        private static Ellipse Burbuja(double x, double y, double diametro)
        {
            Ellipse burbuja = new Ellipse();
            burbuja.Width = diametro;
            burbuja.Height = diametro;
            burbuja.Stroke = Tema.Pincel("#B3FFFFFF");
            burbuja.Fill = Tema.Pincel("#186FD3E8");
            burbuja.StrokeThickness = 1.2;
            Canvas.SetLeft(burbuja, x);
            Canvas.SetTop(burbuja, y);
            return burbuja;
        }

        // ====================================================================
        //  Animaciones
        // ====================================================================

        private void AlCargar(object remitente, RoutedEventArgs argumentos)
        {
            if (animado)
            {
                return;
            }
            animado = true;

            // La ropa gira SIEMPRE: una vuelta cada 6,5 segundos.
            Anime.Girar(ropa, 0, 360, 6500, true);

            // Las olas se mueven 80 pixeles y se repiten: parece agua corriendo.
            Anime.Deslizar(olaAtras, 0, -80, 3200);
            Anime.Deslizar(olaFrente, -80, 0, 2300);

            // Cada burbuja sube a su ritmo y arranca un poquito despues que la anterior.
            int numero = 0;
            foreach (UIElement burbuja in burbujas.Children)
            {
                int duracion = 2200 + (numero % 3) * 600;
                Anime.Subir(burbuja, 0, -130, duracion, numero * 350);
                numero++;
            }

            // Observer: si en este momento ya hay una peticion en curso, arranca rapido.
            Suscribirse();
            CambiarVelocidad(NotificadorDatos.Ocupado);
        }

        private void AlDescargar(object remitente, RoutedEventArgs argumentos)
        {
            Desuscribirse();
        }

        private void Suscribirse()
        {
            if (suscrito)
            {
                return;
            }
            NotificadorDatos.Trabajando += AlTrabajar;
            suscrito = true;
        }

        private void Desuscribirse()
        {
            if (!suscrito)
            {
                return;
            }
            NotificadorDatos.Trabajando -= AlTrabajar;
            suscrito = false;
        }

        /// <summary>
        /// Llega desde cualquier hilo (la respuesta del servidor puede venir de un
        /// hilo de trabajo), por eso se pasa siempre al hilo de la interfaz.
        /// </summary>
        private void AlTrabajar(bool trabajando)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(new Action<bool>(AlTrabajar), trabajando);
                return;
            }
            CambiarVelocidad(trabajando);
        }

        /// <summary>Acelera o calma el giro del tambor.</summary>
        public void CambiarVelocidad(bool rapido)
        {
            girandoRapido = rapido;
            if (!animado)
            {
                return;
            }
            // Rapido = una vuelta cada 1,3 segundos; normal = cada 6,5 segundos.
            Anime.Acelerar(ropa, rapido ? 1300 : 6500);
        }
    }
}
