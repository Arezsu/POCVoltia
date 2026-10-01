using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ClienteLavadora
{
    /// <summary>
    /// Tema visual de la aplicación (Voltia): gris grafito mate, acero, azul eléctrico y amarillo.
    /// Se aplica por código a cada ventana, así los formularios no necesitan estilos propios.
    /// </summary>
    internal static class Tema
    {
        public static readonly Color Grafito = ColorTranslator.FromHtml("#22262D");
        public static readonly Color Superficie = ColorTranslator.FromHtml("#2E333B");
        public static readonly Color Campo = ColorTranslator.FromHtml("#3A4049");
        public static readonly Color Acero = ColorTranslator.FromHtml("#4A515C");
        public static readonly Color AceroClaro = ColorTranslator.FromHtml("#5C6470");
        public static readonly Color TextoClaro = ColorTranslator.FromHtml("#E8ECEF");
        public static readonly Color TextoSuave = ColorTranslator.FromHtml("#9AA3AD");
        public static readonly Color Azul = ColorTranslator.FromHtml("#1E88E5");
        public static readonly Color AzulOscuro = ColorTranslator.FromHtml("#1565C0");
        public static readonly Color Amarillo = ColorTranslator.FromHtml("#FFC107");
        public static readonly Color AmarilloOscuro = ColorTranslator.FromHtml("#FFB300");
        public static readonly Color Rojo = ColorTranslator.FromHtml("#D9534F");
        public static readonly Color RojoOscuro = ColorTranslator.FromHtml("#B93A36");
        public static readonly Color LineaGrilla = ColorTranslator.FromHtml("#454C57");

        /// <summary>
        /// Da el estilo a una ventana de caso de uso: encabezado metálico con el título
        /// (el label "lblTitulo") y un cuerpo oscuro con los controles estilizados.
        /// </summary>
        public static void Aplicar(Form form)
        {
            form.BackColor = Grafito;
            form.ForeColor = TextoClaro;

            Label titulo = null;
            foreach (Control c in form.Controls)
            {
                if (c is Label && c.Name == "lblTitulo")
                {
                    titulo = (Label)c;
                }
            }

            int altoEncabezado = (titulo != null) ? titulo.Bottom + 14 : 0;

            // Todo lo que no sea el título pasa a un contenedor que queda debajo del encabezado
            var cuerpo = new List<Control>();
            foreach (Control c in form.Controls)
            {
                if (c != titulo && !(c is MenuStrip))
                {
                    cuerpo.Add(c);
                }
            }

            var contenedor = new Panel();
            contenedor.Dock = DockStyle.Fill;
            contenedor.BackColor = Grafito;
            foreach (Control c in cuerpo)
            {
                c.Top = c.Top - altoEncabezado;
                contenedor.Controls.Add(c);
            }
            form.Controls.Add(contenedor);

            if (titulo != null)
            {
                var encabezado = new PanelMetalico();
                encabezado.Dock = DockStyle.Top;
                encabezado.Height = altoEncabezado;

                titulo.BackColor = Color.Transparent;
                titulo.ForeColor = Color.White;
                encabezado.Controls.Add(titulo);

                var barraIzquierda = new Panel();
                barraIzquierda.Dock = DockStyle.Left;
                barraIzquierda.Width = 6;
                barraIzquierda.BackColor = Amarillo;
                encabezado.Controls.Add(barraIzquierda);

                var lineaInferior = new Panel();
                lineaInferior.Dock = DockStyle.Bottom;
                lineaInferior.Height = 3;
                lineaInferior.BackColor = Amarillo;
                encabezado.Controls.Add(lineaInferior);

                form.Controls.Add(encabezado);
            }

            EstilizarHijos(contenedor);
        }

        private static void EstilizarHijos(Control raiz)
        {
            foreach (Control c in raiz.Controls)
            {
                if (c is Button)
                {
                    EstilizarBoton((Button)c);
                }
                else if (c is TextBox)
                {
                    var t = (TextBox)c;
                    t.BackColor = Campo;
                    t.ForeColor = Color.White;
                    t.BorderStyle = BorderStyle.FixedSingle;
                }
                else if (c is ComboBox)
                {
                    var cb = (ComboBox)c;
                    cb.FlatStyle = FlatStyle.Flat;
                    cb.BackColor = Campo;
                    cb.ForeColor = Color.White;
                }
                else if (c is DataGridView)
                {
                    EstilizarGrilla((DataGridView)c);
                }
                else if (c is Label)
                {
                    c.ForeColor = (c.Name == "lblVersion" || c.Name == "lblServidor") ? TextoSuave : TextoClaro;
                }
                else if (c is CheckBox)
                {
                    c.ForeColor = TextoClaro;
                }

                if (c.HasChildren)
                {
                    EstilizarHijos(c);
                }
            }
        }

        private static void EstilizarBoton(Button b)
        {
            Color fondo = Azul;
            Color hover = AzulOscuro;
            Color texto = Color.White;
            string nombre = b.Name.ToLowerInvariant();

            if (nombre.Contains("eliminar"))
            {
                fondo = Rojo;
                hover = RojoOscuro;
            }
            else if (nombre.Contains("cerrar") || nombre.Contains("listartodo") || nombre.Contains("limpiar"))
            {
                fondo = Acero;
                hover = AceroClaro;
            }
            else if (nombre.Contains("guardar") || nombre.Contains("actualizar"))
            {
                fondo = Amarillo;
                hover = AmarilloOscuro;
                texto = Grafito;
            }

            b.UseVisualStyleBackColor = false;
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = hover;
            b.FlatAppearance.MouseDownBackColor = hover;
            b.Cursor = Cursors.Hand;
            b.Font = new Font(b.Font, FontStyle.Bold);
            b.ForeColor = texto;
            b.BackColor = b.Enabled ? fondo : Acero;

            // Cuando el botón se habilita o deshabilita, cambia el color
            b.EnabledChanged += delegate (object sender, EventArgs e)
            {
                b.BackColor = b.Enabled ? fondo : Acero;
            };
        }

        private static void EstilizarGrilla(DataGridView g)
        {
            g.EnableHeadersVisualStyles = false;
            g.BackgroundColor = Grafito;
            g.GridColor = LineaGrilla;
            g.BorderStyle = BorderStyle.None;
            g.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            g.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;

            g.ColumnHeadersDefaultCellStyle.BackColor = AzulOscuro;
            g.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            g.ColumnHeadersDefaultCellStyle.SelectionBackColor = AzulOscuro;
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            g.DefaultCellStyle.BackColor = Superficie;
            g.DefaultCellStyle.ForeColor = TextoClaro;
            g.DefaultCellStyle.SelectionBackColor = Amarillo;
            g.DefaultCellStyle.SelectionForeColor = Grafito;

            g.AlternatingRowsDefaultCellStyle.BackColor = ColorTranslator.FromHtml("#363C45");
            g.AlternatingRowsDefaultCellStyle.ForeColor = TextoClaro;
            g.AlternatingRowsDefaultCellStyle.SelectionBackColor = Amarillo;
            g.AlternatingRowsDefaultCellStyle.SelectionForeColor = Grafito;
        }

        /// <summary>Menú oscuro con resaltado azul (para la ventana principal).</summary>
        public static void EstilizarMenu(MenuStrip menu)
        {
            menu.Renderer = new ToolStripProfessionalRenderer(new ColoresMenu());
            menu.BackColor = Acero;
            menu.ForeColor = Color.White;
            EstilizarItems(menu.Items);
        }

        private static void EstilizarItems(ToolStripItemCollection items)
        {
            foreach (ToolStripItem item in items)
            {
                item.ForeColor = Color.White;
                var menuItem = item as ToolStripMenuItem;
                if (menuItem != null && menuItem.HasDropDownItems)
                {
                    menuItem.DropDown.BackColor = Superficie;
                    EstilizarItems(menuItem.DropDownItems);
                }
            }
        }

        /// <summary>Colores del menú: gris acero, selección azul y borde amarillo.</summary>
        private class ColoresMenu : ProfessionalColorTable
        {
            public ColoresMenu()
            {
                UseSystemColors = false;
            }

            public override Color MenuStripGradientBegin { get { return Tema.AceroClaro; } }
            public override Color MenuStripGradientEnd { get { return Tema.Acero; } }
            public override Color ToolStripDropDownBackground { get { return Tema.Superficie; } }
            public override Color ImageMarginGradientBegin { get { return Tema.Superficie; } }
            public override Color ImageMarginGradientMiddle { get { return Tema.Superficie; } }
            public override Color ImageMarginGradientEnd { get { return Tema.Superficie; } }
            public override Color MenuItemSelected { get { return Tema.Azul; } }
            public override Color MenuItemSelectedGradientBegin { get { return Tema.Azul; } }
            public override Color MenuItemSelectedGradientEnd { get { return Tema.AzulOscuro; } }
            public override Color MenuItemPressedGradientBegin { get { return Tema.AzulOscuro; } }
            public override Color MenuItemPressedGradientMiddle { get { return Tema.AzulOscuro; } }
            public override Color MenuItemPressedGradientEnd { get { return Tema.AzulOscuro; } }
            public override Color MenuItemBorder { get { return Tema.Amarillo; } }
            public override Color MenuBorder { get { return Tema.Grafito; } }
            public override Color SeparatorDark { get { return Tema.LineaGrilla; } }
            public override Color SeparatorLight { get { return Tema.LineaGrilla; } }
        }
    }
}
