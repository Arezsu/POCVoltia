using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ClienteLavadora.util;

namespace ClienteLavadora.gui
{
    /// <summary>
    /// Puerta redonda de la lavadora (ojo de buey) con la ropa girando, olas de agua
    /// y burbujas. Esta dentro de un Viewbox: se agranda o se achica cambiando "lado".
    /// Las animaciones nunca paran. Al pasar el mouse la ropa gira mas rapido.
    /// </summary>
    public class OjoDeBuey : Viewbox
    {
        private readonly Canvas ropa;
        private readonly Canvas burbujas;
        private readonly Path olaAtras;
        private readonly Path olaFrente;
        private bool animado;

        public OjoDeBuey(double lado)
        {
            Width = lado;
            Height = lado;
            Stretch = Stretch.Uniform;
            Cursor = Cursors.Hand;

            // Todo se dibuja en una cuadricula de 200 x 200 y el Viewbox la escala
            Grid raiz = new Grid();
            raiz.Width = 200;
            raiz.Height = 200;
            raiz.Background = Brushes.Transparent;

            // Aro cromado
            Ellipse aro = new Ellipse();
            aro.Width = 200;
            aro.Height = 200;
            aro.Fill = Ui.DegradadoDiagonal("#F5F7F9", "#90A4AE", "#455A64");
            raiz.Children.Add(aro);

            Ellipse sombra = new Ellipse();
            sombra.Width = 176;
            sombra.Height = 176;
            sombra.Fill = Ui.Pincel("#1B262E");
            sombra.Stroke = Ui.Pincel("#263238");
            sombra.StrokeThickness = 4;
            raiz.Children.Add(sombra);

            // Vidrio: todo lo de adentro se recorta en circulo
            Grid vidrio = new Grid();
            vidrio.Width = 160;
            vidrio.Height = 160;
            vidrio.Clip = new EllipseGeometry(new Point(80, 80), 80, 80); // Recorta en circulo

            RadialGradientBrush azul = new RadialGradientBrush();
            azul.GradientOrigin = new Point(0.35, 0.3);
            azul.GradientStops.Add(new GradientStop(Ui.ColorDe("#4FC3F7"), 0));
            azul.GradientStops.Add(new GradientStop(Ui.ColorDe("#0D47A1"), 0.7));
            azul.GradientStops.Add(new GradientStop(Ui.ColorDe("#06203F"), 1));
            Ellipse fondo = new Ellipse();
            fondo.Fill = azul;
            vidrio.Children.Add(fondo);

            // Agua: dos olas que se deslizan
            Canvas agua = new Canvas();
            agua.Width = 160;
            agua.Height = 160;
            olaAtras = CrearOla("#6642A5F5", 8);
            olaFrente = CrearOla("#9929B6F6", 22);
            agua.Children.Add(olaAtras);
            agua.Children.Add(olaFrente);
            vidrio.Children.Add(agua);

            // Ropa: este grupo es el que gira
            ropa = new Canvas();
            ropa.Width = 160;
            ropa.Height = 160;
            ropa.Children.Add(Prenda(34, 52, 48, 26, "#FFC107", -20));
            ropa.Children.Add(Prenda(84, 70, 44, 30, "#ECEFF1", 25));
            ropa.Children.Add(Prenda(52, 92, 50, 24, "#1E88E5", 8));
            ropa.Children.Add(Bola(100, 44, 26, "#FF7043"));
            ropa.Children.Add(Bola(30, 96, 22, "#B0BEC5"));
            vidrio.Children.Add(ropa);

            // Burbujas: suben y se desvanecen
            burbujas = new Canvas();
            burbujas.Width = 160;
            burbujas.Height = 160;
            burbujas.Children.Add(Burbuja(30, 140, 12));
            burbujas.Children.Add(Burbuja(62, 146, 9));
            burbujas.Children.Add(Burbuja(92, 138, 15));
            burbujas.Children.Add(Burbuja(120, 144, 10));
            burbujas.Children.Add(Burbuja(76, 150, 7));
            burbujas.Children.Add(Burbuja(46, 132, 8));
            vidrio.Children.Add(burbujas);

            // Brillo del vidrio
            Ellipse brillo = new Ellipse();
            brillo.Width = 96;
            brillo.Height = 54;
            brillo.HorizontalAlignment = HorizontalAlignment.Left;
            brillo.VerticalAlignment = VerticalAlignment.Top;
            brillo.Margin = new Thickness(16, 10, 0, 0);
            brillo.RenderTransform = new RotateTransform(-30, 48, 27);
            brillo.Fill = Ui.Degradado("#77FFFFFF", "#00FFFFFF");
            vidrio.Children.Add(brillo);

            raiz.Children.Add(vidrio);
            Child = raiz;

            Loaded += AlCargar;
            MouseLeftButtonDown += (s, e) => Anime.Pulso(this); // Click: la puerta "late"
            MouseEnter += (s, e) => Anime.Acelerar(ropa, 1500);  // La ropa gira rapido
            MouseLeave += (s, e) => Anime.Acelerar(ropa, 7000);  // y vuelve a su ritmo
        }

        // ---- Piezas del dibujo ----

        /// <summary>
        /// Dibuja una ola como una linea de curvas repetidas (Q = curva). Como el patron se repite
        /// cada 80 pixeles, al deslizarla 80 pixeles se ve como agua que nunca termina.
        /// </summary>
        private static Path CrearOla(string color, double arriba)
        {
            StringBuilder texto = new StringBuilder("M-160,70");
            int x = -160;
            bool suba = true;
            while (x < 320)
            {
                int control = suba ? 50 : 90;
                texto.Append(" Q" + (x + 20) + "," + control + " " + (x + 40) + ",70");
                suba = !suba;
                x += 40;
            }
            texto.Append(" V170 H-160 Z"); // Cierra la figura hacia abajo para rellenar el agua

            Path ola = new Path();
            ola.Fill = Ui.Pincel(color);
            ola.Data = Geometry.Parse(texto.ToString());
            Canvas.SetTop(ola, arriba);
            return ola;
        }

        private static Rectangle Prenda(double x, double y, double ancho, double alto, string color, double angulo)
        {
            Rectangle prenda = new Rectangle();
            prenda.Width = ancho;
            prenda.Height = alto;
            prenda.RadiusX = 9;
            prenda.RadiusY = 9;
            prenda.Fill = Ui.Pincel(color);
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
            bola.Fill = Ui.Pincel(color);
            Canvas.SetLeft(bola, x);
            Canvas.SetTop(bola, y);
            return bola;
        }

        private static Ellipse Burbuja(double x, double y, double diametro)
        {
            Ellipse burbuja = new Ellipse();
            burbuja.Width = diametro;
            burbuja.Height = diametro;
            burbuja.Stroke = Ui.Pincel("#CCFFFFFF");
            burbuja.StrokeThickness = 1.5;
            Canvas.SetLeft(burbuja, x);
            Canvas.SetTop(burbuja, y);
            return burbuja;
        }

        // ---- Animaciones que nunca paran ----

        private void AlCargar(object sender, RoutedEventArgs e)
        {
            if (animado)
            {
                return;
            }
            animado = true;

            // La ropa da vueltas sin parar (una vuelta cada 7 segundos)
            Anime.Girar(ropa, 0, 360, 7000, true);

            // Las olas se mueven 80 pixeles y se repiten
            Anime.Deslizar(olaAtras, 0, -80, 3200);
            Anime.Deslizar(olaFrente, -80, 0, 2300);

            // Cada burbuja sube a su ritmo (efecto escalonado: arrancan una tras otra)
            int numero = 0;
            foreach (UIElement burbuja in burbujas.Children)
            {
                int duracion = 2200 + (numero % 3) * 600;
                Anime.Subir(burbuja, 0, -130, duracion, numero * 350);
                numero++;
            }
        }
    }
}
