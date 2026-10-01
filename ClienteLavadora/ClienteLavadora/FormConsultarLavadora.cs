using System;
using System.Windows.Forms;
using ClienteLavadora.model;

namespace ClienteLavadora
{
    /// <summary>Caso de uso: Consultar UNA lavadora por un parámetro (el código).</summary>
    public partial class FormConsultarLavadora : Form
    {
        public FormConsultarLavadora()
        {
            InitializeComponent();
            Tema.Aplicar(this);
        }

        private async void btnBuscar_Click(object sender, EventArgs e)
        {
            int codigo;
            if (!int.TryParse(txtCodigo.Text.Trim(), out codigo))
            {
                MessageBox.Show("El código debe ser un número entero.", "Dato inválido",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnBuscar.Enabled = false;
            txtResultado.Clear();
            try
            {
                LavadoraData lavadora = await ClienteGraphQL.BuscarPorCodigoAsync(codigo);
                if (lavadora == null)
                {
                    MessageBox.Show("No existe una lavadora con ese código.", "Sin resultados",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    txtResultado.Text = lavadora.ComoTexto();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error al consultar", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnBuscar.Enabled = true;
            }
        }
    }
}
