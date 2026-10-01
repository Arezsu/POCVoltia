using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ClienteLavadora
{
    /// <summary>Ventana principal: muestra el logo de Voltia y el menú para ir a cada caso de uso.</summary>
    public partial class FormPrincipal : Form
    {
        public FormPrincipal()
        {
            InitializeComponent();
            AplicarTema();
            CargarLogo();
            lblServidor.Text = "Servidor GraphQL: " + ClienteGraphQL.UrlServidor;
        }

        private void AplicarTema()
        {
            BackColor = Tema.Grafito;
            ForeColor = Tema.TextoClaro;
            Tema.EstilizarMenu(menuStrip1);
            lblSubtitulo.ForeColor = Tema.Amarillo;
            lblAyuda.ForeColor = Tema.TextoSuave;
            lblServidor.ForeColor = Tema.TextoClaro;
        }

        /// <summary>El logo está en Assets\logo.jpg (se copia junto al .exe).</summary>
        private void CargarLogo()
        {
            try
            {
                string ruta = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "logo.jpg");
                if (File.Exists(ruta))
                {
                    using (Image original = Image.FromFile(ruta))
                    {
                        picLogo.Image = new Bitmap(original);
                    }
                }
            }
            catch (Exception)
            {
                // Sin logo la aplicación sigue funcionando.
            }
        }

        private void adicionarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new FormAgregarLavadora().Show();
        }

        private void consultarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new FormConsultarLavadora().Show();
        }

        private void listarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new FormListarLavadoras().Show();
        }

        private void actualizarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new FormActualizarLavadora().Show();
        }

        private void eliminarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new FormEliminarLavadora().Show();
        }

        private void acercaDeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var acerca = new FormAcercaDe())
            {
                acerca.ShowDialog(this);
            }
        }

        private void salirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
