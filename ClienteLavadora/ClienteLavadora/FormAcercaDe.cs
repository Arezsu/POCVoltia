using System;
using System.Windows.Forms;

namespace ClienteLavadora
{
    /// <summary>Ayuda > Acerca de...: integrantes del equipo y versión.</summary>
    public partial class FormAcercaDe : Form
    {
        // Integrantes del grupo (uno por línea).
        private static readonly string[] Integrantes =
        {
            "Juan Andres Bermeo Alvarez - 2220241053",
            "Alejandro Sanchez Quimbayo - 2220241077",
            "David Arredondo - 2220241062"
        };

        public FormAcercaDe()
        {
            InitializeComponent();
            Tema.Aplicar(this);
            lblIntegrantes.Text = string.Join("\r\n", Integrantes);
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
