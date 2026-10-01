namespace ClienteLavadora
{
    partial class FormEliminarLavadora
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
            this.txtInfo = new System.Windows.Forms.TextBox();
            this.btnEliminar = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblTitulo
            // 
            this.lblTitulo.AutoSize = true;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.Location = new System.Drawing.Point(20, 15);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(170, 21);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "Eliminar Lavadora";
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
            // txtInfo
            // 
            this.txtInfo.Location = new System.Drawing.Point(20, 95);
            this.txtInfo.Multiline = true;
            this.txtInfo.Name = "txtInfo";
            this.txtInfo.ReadOnly = true;
            this.txtInfo.Size = new System.Drawing.Size(340, 160);
            this.txtInfo.TabIndex = 4;
            // 
            // btnEliminar
            // 
            this.btnEliminar.BackColor = System.Drawing.Color.IndianRed;
            this.btnEliminar.ForeColor = System.Drawing.Color.White;
            this.btnEliminar.Location = new System.Drawing.Point(140, 268);
            this.btnEliminar.Name = "btnEliminar";
            this.btnEliminar.Size = new System.Drawing.Size(140, 30);
            this.btnEliminar.TabIndex = 5;
            this.btnEliminar.Text = "Eliminar";
            this.btnEliminar.UseVisualStyleBackColor = false;
            this.btnEliminar.Click += new System.EventHandler(this.btnEliminar_Click);
            // 
            // FormEliminarLavadora
            // 
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(380, 320);
            this.Controls.Add(this.btnEliminar);
            this.Controls.Add(this.txtInfo);
            this.Controls.Add(this.btnBuscar);
            this.Controls.Add(this.txtCodigoBuscar);
            this.Controls.Add(this.lblCodigoBuscar);
            this.Controls.Add(this.lblTitulo);
            this.AcceptButton = this.btnBuscar;
            this.Name = "FormEliminarLavadora";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Eliminar Lavadora";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblCodigoBuscar;
        private System.Windows.Forms.TextBox txtCodigoBuscar;
        private System.Windows.Forms.Button btnBuscar;
        private System.Windows.Forms.TextBox txtInfo;
        private System.Windows.Forms.Button btnEliminar;
    }
}
