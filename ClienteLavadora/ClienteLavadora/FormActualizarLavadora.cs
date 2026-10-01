using System;
using System.Windows.Forms;
using ClienteLavadora.model;

namespace ClienteLavadora
{
    /// <summary>
    /// Caso de uso: Actualizar lavadora. Primero se busca por código; se muestra
    /// toda la información en los campos y se habilita la edición.
    /// </summary>
    public partial class FormActualizarLavadora : Form
    {
        private int codigoActual = -1;

        public FormActualizarLavadora()
        {
            InitializeComponent();
            Tema.Aplicar(this);
            HabilitarEdicion(false);
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
                    HabilitarEdicion(false);
                    codigoActual = -1;
                    return;
                }

                codigoActual = lavadora.codigo;
                HabilitarEdicion(true);
                txtMarca.Text = lavadora.marca;
                txtPrecioBase.Text = Entrada.DoubleATexto(lavadora.precioBase);
                dtpFechaFabricacion.Value = lavadora.FechaComoDateTime();
                txtCapacidadKilos.Text = Entrada.DoubleATexto(lavadora.capacidadKilos);
                chkFuncionSecado.Checked = lavadora.funcionSecado;
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

        private async void btnActualizar_Click(object sender, EventArgs e)
        {
            if (codigoActual < 0)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(txtMarca.Text))
            {
                Aviso("La marca es obligatoria.");
                return;
            }

            double precioBase;
            if (!Entrada.TryLeerDouble(txtPrecioBase.Text, out precioBase) || precioBase <= 0)
            {
                Aviso("El precio base debe ser un número mayor que cero (sin puntos de miles, ejemplo 1450000).");
                return;
            }

            double capacidadKilos;
            if (!Entrada.TryLeerDouble(txtCapacidadKilos.Text, out capacidadKilos) || capacidadKilos <= 0)
            {
                Aviso("La capacidad debe ser un número mayor que cero (ejemplo 18 o 12,5).");
                return;
            }

            var input = new LavadoraUpdateInput
            {
                marca = txtMarca.Text.Trim(),
                precioBase = precioBase,
                fechaFabricacion = Entrada.FechaParaServidor(dtpFechaFabricacion.Value),
                capacidadKilos = capacidadKilos,
                funcionSecado = chkFuncionSecado.Checked
            };

            btnActualizar.Enabled = false;
            try
            {
                LavadoraData actualizada = await ClienteGraphQL.ActualizarLavadoraAsync(codigoActual, input);
                MessageBox.Show("Lavadora actualizada con éxito:\r\n\r\n" + actualizada.ComoTexto(),
                    "Actualizar Lavadora", MessageBoxButtons.OK, MessageBoxIcon.Information);
                HabilitarEdicion(false);
                codigoActual = -1;
                txtCodigoBuscar.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "No se pudo actualizar", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnActualizar.Enabled = true;
            }
        }

        private void Aviso(string mensaje)
        {
            MessageBox.Show(mensaje, "Dato inválido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void HabilitarEdicion(bool habilitar)
        {
            txtMarca.Enabled = habilitar;
            txtPrecioBase.Enabled = habilitar;
            dtpFechaFabricacion.Enabled = habilitar;
            txtCapacidadKilos.Enabled = habilitar;
            chkFuncionSecado.Enabled = habilitar;
            btnActualizar.Enabled = habilitar;

            if (!habilitar)
            {
                txtMarca.Clear();
                txtPrecioBase.Clear();
                txtCapacidadKilos.Clear();
                chkFuncionSecado.Checked = false;
            }
        }
    }
}
