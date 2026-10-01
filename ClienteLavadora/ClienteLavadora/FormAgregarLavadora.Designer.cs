namespace ClienteLavadora
{
    partial class FormAgregarLavadora
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        private void InitializeComponent()
        {
            this.lblTitulo = new System.Windows.Forms.Label();
            this.lblCodigo = new System.Windows.Forms.Label();
            this.txtCodigo = new System.Windows.Forms.TextBox();
            this.lblMarca = new System.Windows.Forms.Label();
            this.txtMarca = new System.Windows.Forms.TextBox();
            this.lblPrecioBase = new System.Windows.Forms.Label();
            this.txtPrecioBase = new System.Windows.Forms.TextBox();
            this.lblFechaFabricacion = new System.Windows.Forms.Label();
            this.dtpFechaFabricacion = new System.Windows.Forms.DateTimePicker();
            this.lblCapacidadKilos = new System.Windows.Forms.Label();
            this.txtCapacidadKilos = new System.Windows.Forms.TextBox();
            this.chkFuncionSecado = new System.Windows.Forms.CheckBox();
            this.btnGuardar = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblTitulo
            // 
            this.lblTitulo.AutoSize = true;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.Location = new System.Drawing.Point(20, 15);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(180, 21);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "Adicionar Lavadora";
            // 
            // lblCodigo
            // 
            this.lblCodigo.AutoSize = true;
            this.lblCodigo.Location = new System.Drawing.Point(20, 58);
            this.lblCodigo.Name = "lblCodigo";
            this.lblCodigo.Size = new System.Drawing.Size(50, 15);
            this.lblCodigo.TabIndex = 1;
            this.lblCodigo.Text = "Código:";
            // 
            // txtCodigo
            // 
            this.txtCodigo.Location = new System.Drawing.Point(160, 55);
            this.txtCodigo.Name = "txtCodigo";
            this.txtCodigo.Size = new System.Drawing.Size(180, 23);
            this.txtCodigo.TabIndex = 2;
            // 
            // lblMarca
            // 
            this.lblMarca.AutoSize = true;
            this.lblMarca.Location = new System.Drawing.Point(20, 93);
            this.lblMarca.Name = "lblMarca";
            this.lblMarca.Size = new System.Drawing.Size(42, 15);
            this.lblMarca.TabIndex = 3;
            this.lblMarca.Text = "Marca:";
            // 
            // txtMarca
            // 
            this.txtMarca.Location = new System.Drawing.Point(160, 90);
            this.txtMarca.Name = "txtMarca";
            this.txtMarca.Size = new System.Drawing.Size(180, 23);
            this.txtMarca.TabIndex = 4;
            // 
            // lblPrecioBase
            // 
            this.lblPrecioBase.AutoSize = true;
            this.lblPrecioBase.Location = new System.Drawing.Point(20, 128);
            this.lblPrecioBase.Name = "lblPrecioBase";
            this.lblPrecioBase.Size = new System.Drawing.Size(72, 15);
            this.lblPrecioBase.TabIndex = 5;
            this.lblPrecioBase.Text = "Precio base:";
            // 
            // txtPrecioBase
            // 
            this.txtPrecioBase.Location = new System.Drawing.Point(160, 125);
            this.txtPrecioBase.Name = "txtPrecioBase";
            this.txtPrecioBase.Size = new System.Drawing.Size(180, 23);
            this.txtPrecioBase.TabIndex = 6;
            // 
            // lblFechaFabricacion
            // 
            this.lblFechaFabricacion.AutoSize = true;
            this.lblFechaFabricacion.Location = new System.Drawing.Point(20, 163);
            this.lblFechaFabricacion.Name = "lblFechaFabricacion";
            this.lblFechaFabricacion.Size = new System.Drawing.Size(112, 15);
            this.lblFechaFabricacion.TabIndex = 7;
            this.lblFechaFabricacion.Text = "Fecha fabricación:";
            // 
            // dtpFechaFabricacion
            // 
            this.dtpFechaFabricacion.CustomFormat = "dd/MM/yyyy HH:mm";
            this.dtpFechaFabricacion.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpFechaFabricacion.Location = new System.Drawing.Point(160, 160);
            this.dtpFechaFabricacion.Name = "dtpFechaFabricacion";
            this.dtpFechaFabricacion.Size = new System.Drawing.Size(180, 23);
            this.dtpFechaFabricacion.TabIndex = 8;
            // 
            // lblCapacidadKilos
            // 
            this.lblCapacidadKilos.AutoSize = true;
            this.lblCapacidadKilos.Location = new System.Drawing.Point(20, 198);
            this.lblCapacidadKilos.Name = "lblCapacidadKilos";
            this.lblCapacidadKilos.Size = new System.Drawing.Size(103, 15);
            this.lblCapacidadKilos.TabIndex = 9;
            this.lblCapacidadKilos.Text = "Capacidad (kg):";
            // 
            // txtCapacidadKilos
            // 
            this.txtCapacidadKilos.Location = new System.Drawing.Point(160, 195);
            this.txtCapacidadKilos.Name = "txtCapacidadKilos";
            this.txtCapacidadKilos.Size = new System.Drawing.Size(180, 23);
            this.txtCapacidadKilos.TabIndex = 10;
            // 
            // chkFuncionSecado
            // 
            this.chkFuncionSecado.AutoSize = true;
            this.chkFuncionSecado.Location = new System.Drawing.Point(160, 230);
            this.chkFuncionSecado.Name = "chkFuncionSecado";
            this.chkFuncionSecado.Size = new System.Drawing.Size(140, 19);
            this.chkFuncionSecado.TabIndex = 11;
            this.chkFuncionSecado.Text = "Función de secado";
            this.chkFuncionSecado.UseVisualStyleBackColor = true;
            // 
            // btnGuardar
            // 
            this.btnGuardar.Location = new System.Drawing.Point(160, 270);
            this.btnGuardar.Name = "btnGuardar";
            this.btnGuardar.Size = new System.Drawing.Size(100, 30);
            this.btnGuardar.TabIndex = 12;
            this.btnGuardar.Text = "Guardar";
            this.btnGuardar.UseVisualStyleBackColor = true;
            this.btnGuardar.Click += new System.EventHandler(this.btnGuardar_Click);
            // 
            // FormAgregarLavadora
            // 
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(380, 325);
            this.Controls.Add(this.btnGuardar);
            this.Controls.Add(this.chkFuncionSecado);
            this.Controls.Add(this.txtCapacidadKilos);
            this.Controls.Add(this.lblCapacidadKilos);
            this.Controls.Add(this.dtpFechaFabricacion);
            this.Controls.Add(this.lblFechaFabricacion);
            this.Controls.Add(this.txtPrecioBase);
            this.Controls.Add(this.lblPrecioBase);
            this.Controls.Add(this.txtMarca);
            this.Controls.Add(this.lblMarca);
            this.Controls.Add(this.txtCodigo);
            this.Controls.Add(this.lblCodigo);
            this.Controls.Add(this.lblTitulo);
            this.Name = "FormAgregarLavadora";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Adicionar Lavadora";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblCodigo;
        private System.Windows.Forms.TextBox txtCodigo;
        private System.Windows.Forms.Label lblMarca;
        private System.Windows.Forms.TextBox txtMarca;
        private System.Windows.Forms.Label lblPrecioBase;
        private System.Windows.Forms.TextBox txtPrecioBase;
        private System.Windows.Forms.Label lblFechaFabricacion;
        private System.Windows.Forms.DateTimePicker dtpFechaFabricacion;
        private System.Windows.Forms.Label lblCapacidadKilos;
        private System.Windows.Forms.TextBox txtCapacidadKilos;
        private System.Windows.Forms.CheckBox chkFuncionSecado;
        private System.Windows.Forms.Button btnGuardar;
    }
}
