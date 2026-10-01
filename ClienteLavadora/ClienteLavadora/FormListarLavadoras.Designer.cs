namespace ClienteLavadora
{
    partial class FormListarLavadoras
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
            this.lblMarca = new System.Windows.Forms.Label();
            this.txtFiltroMarca = new System.Windows.Forms.TextBox();
            this.lblFuncionSecado = new System.Windows.Forms.Label();
            this.cmbFuncionSecado = new System.Windows.Forms.ComboBox();
            this.btnFiltrar = new System.Windows.Forms.Button();
            this.btnListarTodo = new System.Windows.Forms.Button();
            this.dgvLavadoras = new System.Windows.Forms.DataGridView();
            this.colCodigo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colMarca = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPrecioBase = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colFechaFabricacion = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCapacidadKilos = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colFuncionSecado = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLavadoras)).BeginInit();
            this.SuspendLayout();
            // 
            // lblTitulo
            // 
            this.lblTitulo.AutoSize = true;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.Location = new System.Drawing.Point(20, 15);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(260, 21);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "Listar / Filtrar Lavadoras";
            // 
            // lblMarca
            // 
            this.lblMarca.AutoSize = true;
            this.lblMarca.Location = new System.Drawing.Point(20, 58);
            this.lblMarca.Name = "lblMarca";
            this.lblMarca.Size = new System.Drawing.Size(42, 15);
            this.lblMarca.TabIndex = 1;
            this.lblMarca.Text = "Marca:";
            // 
            // txtFiltroMarca
            // 
            this.txtFiltroMarca.Location = new System.Drawing.Point(90, 55);
            this.txtFiltroMarca.Name = "txtFiltroMarca";
            this.txtFiltroMarca.Size = new System.Drawing.Size(150, 23);
            this.txtFiltroMarca.TabIndex = 2;
            // 
            // lblFuncionSecado
            // 
            this.lblFuncionSecado.AutoSize = true;
            this.lblFuncionSecado.Location = new System.Drawing.Point(260, 58);
            this.lblFuncionSecado.Name = "lblFuncionSecado";
            this.lblFuncionSecado.Size = new System.Drawing.Size(100, 15);
            this.lblFuncionSecado.TabIndex = 3;
            this.lblFuncionSecado.Text = "Función secado:";
            // 
            // cmbFuncionSecado
            // 
            this.cmbFuncionSecado.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFuncionSecado.FormattingEnabled = true;
            this.cmbFuncionSecado.Items.AddRange(new object[] {
            "Todas",
            "Sí",
            "No"});
            this.cmbFuncionSecado.Location = new System.Drawing.Point(366, 55);
            this.cmbFuncionSecado.Name = "cmbFuncionSecado";
            this.cmbFuncionSecado.Size = new System.Drawing.Size(90, 23);
            this.cmbFuncionSecado.TabIndex = 4;
            // 
            // btnFiltrar
            // 
            this.btnFiltrar.Location = new System.Drawing.Point(470, 54);
            this.btnFiltrar.Name = "btnFiltrar";
            this.btnFiltrar.Size = new System.Drawing.Size(110, 25);
            this.btnFiltrar.TabIndex = 5;
            this.btnFiltrar.Text = "Filtrar";
            this.btnFiltrar.UseVisualStyleBackColor = true;
            this.btnFiltrar.Click += new System.EventHandler(this.btnFiltrar_Click);
            // 
            // btnListarTodo
            // 
            this.btnListarTodo.Location = new System.Drawing.Point(20, 92);
            this.btnListarTodo.Name = "btnListarTodo";
            this.btnListarTodo.Size = new System.Drawing.Size(140, 25);
            this.btnListarTodo.TabIndex = 6;
            this.btnListarTodo.Text = "Listar todo";
            this.btnListarTodo.UseVisualStyleBackColor = true;
            this.btnListarTodo.Click += new System.EventHandler(this.btnListarTodo_Click);
            // 
            // dgvLavadoras
            // 
            this.dgvLavadoras.AllowUserToAddRows = false;
            this.dgvLavadoras.AllowUserToDeleteRows = false;
            this.dgvLavadoras.AutoGenerateColumns = false;
            this.dgvLavadoras.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvLavadoras.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colCodigo,
            this.colMarca,
            this.colPrecioBase,
            this.colFechaFabricacion,
            this.colCapacidadKilos,
            this.colFuncionSecado});
            this.dgvLavadoras.Location = new System.Drawing.Point(20, 130);
            this.dgvLavadoras.Name = "dgvLavadoras";
            this.dgvLavadoras.ReadOnly = true;
            this.dgvLavadoras.RowHeadersVisible = false;
            this.dgvLavadoras.AllowUserToResizeRows = false;
            this.dgvLavadoras.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvLavadoras.Size = new System.Drawing.Size(580, 260);
            this.dgvLavadoras.TabIndex = 7;
            // 
            // colCodigo
            // 
            this.colCodigo.HeaderText = "Código";
            this.colCodigo.Name = "colCodigo";
            this.colCodigo.ReadOnly = true;
            this.colCodigo.Width = 60;
            // 
            // colMarca
            // 
            this.colMarca.HeaderText = "Marca";
            this.colMarca.Name = "colMarca";
            this.colMarca.ReadOnly = true;
            this.colMarca.Width = 90;
            // 
            // colPrecioBase
            // 
            this.colPrecioBase.HeaderText = "Precio base";
            this.colPrecioBase.Name = "colPrecioBase";
            this.colPrecioBase.ReadOnly = true;
            this.colPrecioBase.Width = 90;
            // 
            // colFechaFabricacion
            // 
            this.colFechaFabricacion.HeaderText = "Fecha fabricación";
            this.colFechaFabricacion.Name = "colFechaFabricacion";
            this.colFechaFabricacion.ReadOnly = true;
            this.colFechaFabricacion.Width = 140;
            // 
            // colCapacidadKilos
            // 
            this.colCapacidadKilos.HeaderText = "Capacidad (kg)";
            this.colCapacidadKilos.Name = "colCapacidadKilos";
            this.colCapacidadKilos.ReadOnly = true;
            this.colCapacidadKilos.Width = 100;
            // 
            // colFuncionSecado
            // 
            this.colFuncionSecado.HeaderText = "Función secado";
            this.colFuncionSecado.Name = "colFuncionSecado";
            this.colFuncionSecado.ReadOnly = true;
            this.colFuncionSecado.Width = 95;
            // 
            // FormListarLavadoras
            // 
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(620, 410);
            this.Controls.Add(this.dgvLavadoras);
            this.Controls.Add(this.btnListarTodo);
            this.Controls.Add(this.btnFiltrar);
            this.Controls.Add(this.cmbFuncionSecado);
            this.Controls.Add(this.lblFuncionSecado);
            this.Controls.Add(this.txtFiltroMarca);
            this.Controls.Add(this.lblMarca);
            this.Controls.Add(this.lblTitulo);
            this.Name = "FormListarLavadoras";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Listar / Filtrar Lavadoras";
            this.Load += new System.EventHandler(this.FormListarLavadoras_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvLavadoras)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblMarca;
        private System.Windows.Forms.TextBox txtFiltroMarca;
        private System.Windows.Forms.Label lblFuncionSecado;
        private System.Windows.Forms.ComboBox cmbFuncionSecado;
        private System.Windows.Forms.Button btnFiltrar;
        private System.Windows.Forms.Button btnListarTodo;
        private System.Windows.Forms.DataGridView dgvLavadoras;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCodigo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMarca;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPrecioBase;
        private System.Windows.Forms.DataGridViewTextBoxColumn colFechaFabricacion;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCapacidadKilos;
        private System.Windows.Forms.DataGridViewTextBoxColumn colFuncionSecado;
    }
}
