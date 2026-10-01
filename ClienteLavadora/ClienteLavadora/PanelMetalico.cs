using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ClienteLavadora
{
    /// <summary>Panel con degradado gris metálico mate (se usa en encabezados y en el pie).</summary>
    public class PanelMetalico : Panel
    {
        private static readonly Color ColorArriba = Color.FromArgb(78, 85, 96);
        private static readonly Color ColorAbajo = Color.FromArgb(40, 44, 51);

        public PanelMetalico()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint
                     | ControlStyles.UserPaint
                     | ControlStyles.OptimizedDoubleBuffer
                     | ControlStyles.ResizeRedraw, true);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (ClientRectangle.Width <= 0 || ClientRectangle.Height <= 0)
            {
                return;
            }

            using (var pincel = new LinearGradientBrush(ClientRectangle, ColorArriba, ColorAbajo, LinearGradientMode.Vertical))
            {
                e.Graphics.FillRectangle(pincel, ClientRectangle);
            }

            // Brillo fino arriba: da el efecto de metal
            using (var lapiz = new Pen(Color.FromArgb(95, 255, 255, 255)))
            {
                e.Graphics.DrawLine(lapiz, 0, 0, Width, 0);
            }
        }
    }
}
