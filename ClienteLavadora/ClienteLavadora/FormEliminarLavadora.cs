using System;
using System.Windows.Forms;
using ClienteLavadora.model;

namespace ClienteLavadora
{
    /// <summary>
    /// Caso de uso: Eliminar lavadora. Primero se busca por código y se muestra
    /// TODA la información; solo entonces se habilita el botón Eliminar.
    /// </summary>
    public partial class FormEliminarLavadora : Form
    {
        private int codigoActual = -1;

        public FormEliminarLavadora()
        {
            InitializeComponent();
            Tema.Aplicar(this);
            btnEliminar.Enabled = false;
        }

        private async void btnBuscar_Click(object sender, EventArgs e)
        {
            int codigo;
            if (!int.TryParse(txtCodigoBuscar.Text.Trim(), out codigo))
            {
                MessageBox.Show("El código debe ser un número entero.", "Dato inválido",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnBuscar.Enabled = false;
            try
            {
                LavadoraData lavadora = await ClienteGraphQL.BuscarPorCodigoAsync(codigo);
                if (lavadora == null)
                {
                    MessageBox.Show("No existe una lavadora con ese código.", "Sin resultados",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    txtInfo.Clear();
                    btnEliminar.Enabled = false;
                    codigoActual = -1;
                    return;
                }

                codigoActual = lavadora.codigo;
                txtInfo.Text = lavadora.ComoTexto();
                btnEliminar.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error al buscar", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnBuscar.Enabled = true;
            }
        }

        private async void btnEliminar_Click(object sender, EventArgs e)
        {
            if (codigoActual < 0)
            {
                return;
            }

            DialogResult confirmacion = MessageBox.Show(
                "¿Está seguro de eliminar la lavadora con código " + codigoActual + "?\r\nEsta acción no se puede deshacer.",
                "Confirmar eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirmacion != DialogResult.Yes)
            {
                return;
            }

            btnEliminar.Enabled = false;
            try
            {
                bool eliminada = await ClienteGraphQL.EliminarLavadoraAsync(codigoActual);
                if (eliminada)
                {
                    MessageBox.Show("Lavadora eliminada con éxito.", "Eliminar Lavadora",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("No se pudo eliminar la lavadora.", "Eliminar Lavadora",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                txtInfo.Clear();
                txtCodigoBuscar.Clear();
                codigoActual = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "No se pudo eliminar", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnEliminar.Enabled = codigoActual >= 0;
            }
        }
    }
}
