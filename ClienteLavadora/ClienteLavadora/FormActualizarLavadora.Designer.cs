namespace ClienteLavadora
{
    partial class FormActualizarLavadora
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
            this.lblCodigoBuscar = new System.Windows.Forms.Label();
            this.txtCodigoBuscar = new System.Windows.Forms.TextBox();
            this.btnBuscar = new System.Windows.Forms.Button();
            this.lblMarca = new System.Windows.Forms.Label();
            this.txtMarca = new System.Windows.Forms.TextBox();
            this.lblPrecioBase = new System.Windows.Forms.Label();
            this.txtPrecioBase = new System.Windows.Forms.TextBox();
            this.lblFechaFabricacion = new System.Windows.Forms.Label();
            this.dtpFechaFabricacion = new System.Windows.Forms.DateTimePicker();
            this.lblCapacidadKilos = new System.Windows.Forms.Label();
            this.txtCapacidadKilos = new System.Windows.Forms.TextBox();
            this.chkFuncionSecado = new System.Windows.Forms.CheckBox();
            this.btnActualizar = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblTitulo
            // 
            this.lblTitulo.AutoSize = true;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.Location = new System.Drawing.Point(20, 15);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(190, 21);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "Actualizar Lavadora";
            // 
            // lblCodigoBuscar
            // 
            this.lblCodigoBuscar.AutoSize = true;
            this.lblCodigoBuscar.Location = new System.Drawing.Point(20, 58);
            this.lblCodigoBuscar.Name = "lblCodigoBuscar";
            this.lblCodigoBuscar.Size = new System.Drawing.Size(105, 15);
            this.lblCodigoBuscar.TabIndex = 1;
            this.lblCodigoBuscar.Text = "Código a buscar:";
            // 
            // txtCodigoBuscar
            // 
            this.txtCodigoBuscar.Location = new System.Drawing.Point(160, 55);
            this.txtCodigoBuscar.Name = "txtCodigoBuscar";
            this.txtCodigoBuscar.Size = new System.Drawing.Size(100, 23);
            this.txtCodigoBuscar.TabIndex = 2;
            // 
            // btnBuscar
            // 
            this.btnBuscar.Location = new System.Drawing.Point(270, 54);
            this.btnBuscar.Name = "btnBuscar";
            this.btnBuscar.Size = new System.Drawing.Size(90, 25);
            this.btnBuscar.TabIndex = 3;
            this.btnBuscar.Text = "Buscar";
            this.btnBuscar.UseVisualStyleBackColor = true;
            this.btnBuscar.Click += new System.EventHandler(this.btnBuscar_Click);
            // 
            // lblMarca
            // 
            this.lblMarca.AutoSize = true;
            this.lblMarca.Location = new System.Drawing.Point(20, 98);
            this.lblMarca.Name = "lblMarca";
            this.lblMarca.Size = new System.Drawing.Size(42, 15);
            this.lblMarca.TabIndex = 4;
            this.lblMarca.Text = "Marca:";
            // 
            // txtMarca
            // 
            this.txtMarca.Location = new System.Drawing.Point(160, 95);
            this.txtMarca.Name = "txtMarca";
            this.txtMarca.Size = new System.Drawing.Size(200, 23);
            this.txtMarca.TabIndex = 5;
            // 
            // lblPrecioBase
            // 
            this.lblPrecioBase.AutoSize = true;
            this.lblPrecioBase.Location = new System.Drawing.Point(20, 133);
            this.lblPrecioBase.Name = "lblPrecioBase";
            this.lblPrecioBase.Size = new System.Drawing.Size(72, 15);
            this.lblPrecioBase.TabIndex = 6;
            this.lblPrecioBase.Text = "Precio base:";
            // 
            // txtPrecioBase
            // 
            this.txtPrecioBase.Location = new System.Drawing.Point(160, 130);
            this.txtPrecioBase.Name = "txtPrecioBase";
            this.txtPrecioBase.Size = new System.Drawing.Size(200, 23);
            this.txtPrecioBase.TabIndex = 7;
            // 
            // lblFechaFabricacion
            // 
            this.lblFechaFabricacion.AutoSize = true;
            this.lblFechaFabricacion.Location = new System.Drawing.Point(20, 168);
            this.lblFechaFabricacion.Name = "lblFechaFabricacion";
            this.lblFechaFabricacion.Size = new System.Drawing.Size(112, 15);
            this.lblFechaFabricacion.TabIndex = 8;
            this.lblFechaFabricacion.Text = "Fecha fabricación:";
            // 
            // dtpFechaFabricacion
            // 
            this.dtpFechaFabricacion.CustomFormat = "dd/MM/yyyy HH:mm";
            this.dtpFechaFabricacion.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpFechaFabricacion.Location = new System.Drawing.Point(160, 165);
            this.dtpFechaFabricacion.Name = "dtpFechaFabricacion";
            this.dtpFechaFabricacion.Size = new System.Drawing.Size(200, 23);
            this.dtpFechaFabricacion.TabIndex = 9;
            // 
            // lblCapacidadKilos
            // 
            this.lblCapacidadKilos.AutoSize = true;
            this.lblCapacidadKilos.Location = new System.Drawing.Point(20, 203);
            this.lblCapacidadKilos.Name = "lblCapacidadKilos";
            this.lblCapacidadKilos.Size = new System.Drawing.Size(103, 15);
            this.lblCapacidadKilos.TabIndex = 10;
            this.lblCapacidadKilos.Text = "Capacidad (kg):";
            // 
            // txtCapacidadKilos
            // 
            this.txtCapacidadKilos.Location = new System.Drawing.Point(160, 200);
            this.txtCapacidadKilos.Name = "txtCapacidadKilos";
            this.txtCapacidadKilos.Size = new System.Drawing.Size(200, 23);
            this.txtCapacidadKilos.TabIndex = 11;
            // 
            // chkFuncionSecado
            // 
            this.chkFuncionSecado.AutoSize = true;
            this.chkFuncionSecado.Location = new System.Drawing.Point(160, 235);
            this.chkFuncionSecado.Name = "chkFuncionSecado";
            this.chkFuncionSecado.Size = new System.Drawing.Size(140, 19);
            this.chkFuncionSecado.TabIndex = 12;
            this.chkFuncionSecado.Text = "Función de secado";
            this.chkFuncionSecado.UseVisualStyleBackColor = true;
            // 
            // btnActualizar
            // 
            this.btnActualizar.Location = new System.Drawing.Point(160, 275);
            this.btnActualizar.Name = "btnActualizar";
            this.btnActualizar.Size = new System.Drawing.Size(140, 30);
            this.btnActualizar.TabIndex = 13;
            this.btnActualizar.Text = "Guardar cambios";
            this.btnActualizar.UseVisualStyleBackColor = true;
            this.btnActualizar.Click += new System.EventHandler(this.btnActualizar_Click);
            // 
            // FormActualizarLavadora
            // 
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(400, 330);
            this.Controls.Add(this.btnActualizar);
            this.Controls.Add(this.chkFuncionSecado);
            this.Controls.Add(this.txtCapacidadKilos);
            this.Controls.Add(this.lblCapacidadKilos);
            this.Controls.Add(this.dtpFechaFabricacion);
            this.Controls.Add(this.lblFechaFabricacion);
            this.Controls.Add(this.txtPrecioBase);
            this.Controls.Add(this.lblPrecioBase);
            this.Controls.Add(this.txtMarca);
            this.Controls.Add(this.lblMarca);
            this.Controls.Add(this.btnBuscar);
            this.Controls.Add(this.txtCodigoBuscar);
            this.Controls.Add(this.lblCodigoBuscar);
            this.Controls.Add(this.lblTitulo);
            this.Name = "FormActualizarLavadora";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Actualizar Lavadora";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblCodigoBuscar;
        private System.Windows.Forms.TextBox txtCodigoBuscar;
        private System.Windows.Forms.Button btnBuscar;
        private System.Windows.Forms.Label lblMarca;
        private System.Windows.Forms.TextBox txtMarca;
        private System.Windows.Forms.Label lblPrecioBase;
        private System.Windows.Forms.TextBox txtPrecioBase;
        private System.Windows.Forms.Label lblFechaFabricacion;
        private System.Windows.Forms.DateTimePicker dtpFechaFabricacion;
        private System.Windows.Forms.Label lblCapacidadKilos;
        private System.Windows.Forms.TextBox txtCapacidadKilos;
        private System.Windows.Forms.CheckBox chkFuncionSecado;
        private System.Windows.Forms.Button btnActualizar;
    }
}
