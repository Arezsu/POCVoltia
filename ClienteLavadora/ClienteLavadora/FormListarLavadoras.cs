using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClienteLavadora.model;

namespace ClienteLavadora
{
    /// <summary>
    /// Caso de uso: Listar. Muestra todas las lavadoras en una grilla y permite
    /// filtrar por 2 parámetros (marca y función de secado). El filtro lo aplica el servidor.
    /// </summary>
    public partial class FormListarLavadoras : Form
    {
        public FormListarLavadoras()
        {
            InitializeComponent();
            Tema.Aplicar(this);
            cmbFuncionSecado.SelectedIndex = 0;
        }

        private async void FormListarLavadoras_Load(object sender, EventArgs e)
        {
            await CargarTodasAsync();
        }

        private async void btnListarTodo_Click(object sender, EventArgs e)
        {
            txtFiltroMarca.Clear();
            cmbFuncionSecado.SelectedIndex = 0;
            await CargarTodasAsync();
        }

        private async void btnFiltrar_Click(object sender, EventArgs e)
        {
            string marca = txtFiltroMarca.Text.Trim();

            bool? funcionSecado = null;
            if (cmbFuncionSecado.SelectedIndex == 1)
            {
                funcionSecado = true;
            }
            else if (cmbFuncionSecado.SelectedIndex == 2)
            {
                funcionSecado = false;
            }

            try
            {
                List<LavadoraData> lista = await ClienteGraphQL.ListarPorFiltroAsync(
                    string.IsNullOrWhiteSpace(marca) ? null : marca, funcionSecado);
                MostrarEnGrilla(lista);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error al filtrar", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task CargarTodasAsync()
        {
            try
            {
                List<LavadoraData> lista = await ClienteGraphQL.ListarLavadorasAsync();
                MostrarEnGrilla(lista);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error al listar", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MostrarEnGrilla(List<LavadoraData> lista)
        {
            dgvLavadoras.Rows.Clear();
            foreach (LavadoraData l in lista)
            {
                dgvLavadoras.Rows.Add(
                    l.codigo,
                    l.marca,
                    l.precioBase.ToString("N0"),
                    l.FechaBonita(),
                    l.capacidadKilos,
                    l.funcionSecado ? "Sí" : "No");
            }
        }
    }
}
