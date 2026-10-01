using System;
using System.Windows.Forms;
using ClienteLavadora.model;

namespace ClienteLavadora
{
    /// <summary>Caso de uso: Adicionar lavadora.</summary>
    public partial class FormAgregarLavadora : Form
    {
        public FormAgregarLavadora()
        {
            InitializeComponent();
            Tema.Aplicar(this);
            dtpFechaFabricacion.Value = DateTime.Now;
        }

        private async void btnGuardar_Click(object sender, EventArgs e)
        {
            int codigo;
            if (!int.TryParse(txtCodigo.Text.Trim(), out codigo))
            {
                Aviso("El código debe ser un número entero.");
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

            var input = new LavadoraInput
            {
                codigo = codigo,
                marca = txtMarca.Text.Trim(),
                precioBase = precioBase,
                fechaFabricacion = Entrada.FechaParaServidor(dtpFechaFabricacion.Value),
                capacidadKilos = capacidadKilos,
                funcionSecado = chkFuncionSecado.Checked
            };

            btnGuardar.Enabled = false;
            try
            {
                LavadoraData creada = await ClienteGraphQL.AgregarLavadoraAsync(input);
                Sonidos.ReproducirLavadora();   // suena la lavadora al adicionar
                MessageBox.Show("Lavadora creada con éxito:\r\n\r\n" + creada.ComoTexto(),
                    "Adicionar Lavadora", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LimpiarCampos();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "No se pudo crear la lavadora",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnGuardar.Enabled = true;
            }
        }

        private void Aviso(string mensaje)
        {
            MessageBox.Show(mensaje, "Dato inválido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void LimpiarCampos()
        {
            txtCodigo.Clear();
            txtMarca.Clear();
            txtPrecioBase.Clear();
            txtCapacidadKilos.Clear();
            chkFuncionSecado.Checked = false;
            dtpFechaFabricacion.Value = DateTime.Now;
            txtCodigo.Focus();
        }
    }
}
