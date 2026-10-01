namespace ClienteLavadora
{
    partial class FormAcercaDe
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
            this.lblCurso = new System.Windows.Forms.Label();
            this.lblIntegrantesTitulo = new System.Windows.Forms.Label();
            this.lblIntegrantes = new System.Windows.Forms.Label();
            this.lblVersion = new System.Windows.Forms.Label();
            this.btnCerrar = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblTitulo
            // 
            this.lblTitulo.AutoSize = true;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.Location = new System.Drawing.Point(20, 20);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(275, 21);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "Voltia - Gestor de Lavadoras";
            // 
            // lblCurso
            // 
            this.lblCurso.AutoSize = true;
            this.lblCurso.Location = new System.Drawing.Point(20, 50);
            this.lblCurso.Name = "lblCurso";
            this.lblCurso.Size = new System.Drawing.Size(310, 15);
            this.lblCurso.TabIndex = 1;
            this.lblCurso.Text = "Diseño de Soluciones - Segundo Taller 2026B";
            // 
            // lblIntegrantesTitulo
            // 
            this.lblIntegrantesTitulo.AutoSize = true;
            this.lblIntegrantesTitulo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblIntegrantesTitulo.Location = new System.Drawing.Point(20, 90);
            this.lblIntegrantesTitulo.Name = "lblIntegrantesTitulo";
            this.lblIntegrantesTitulo.Size = new System.Drawing.Size(87, 15);
            this.lblIntegrantesTitulo.TabIndex = 2;
            this.lblIntegrantesTitulo.Text = "Integrantes:";
            // 
            // lblIntegrantes
            // 
            this.lblIntegrantes.Location = new System.Drawing.Point(20, 110);
            this.lblIntegrantes.Name = "lblIntegrantes";
            this.lblIntegrantes.Size = new System.Drawing.Size(340, 90);
            this.lblIntegrantes.TabIndex = 3;
            this.lblIntegrantes.Text = "";
            // 
            // lblVersion
            // 
            this.lblVersion.AutoSize = true;
            this.lblVersion.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblVersion.Location = new System.Drawing.Point(20, 205);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(85, 15);
            this.lblVersion.TabIndex = 4;
            this.lblVersion.Text = "Versión 1.0.0";
            // 
            // btnCerrar
            // 
            this.btnCerrar.Location = new System.Drawing.Point(260, 235);
            this.btnCerrar.Name = "btnCerrar";
            this.btnCerrar.Size = new System.Drawing.Size(100, 28);
            this.btnCerrar.TabIndex = 5;
            this.btnCerrar.Text = "Cerrar";
            this.btnCerrar.UseVisualStyleBackColor = true;
            this.btnCerrar.Click += new System.EventHandler(this.btnCerrar_Click);
            // 
            // FormAcercaDe
            // 
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(380, 280);
            this.Controls.Add(this.btnCerrar);
            this.Controls.Add(this.lblVersion);
            this.Controls.Add(this.lblIntegrantes);
            this.Controls.Add(this.lblIntegrantesTitulo);
            this.Controls.Add(this.lblCurso);
            this.Controls.Add(this.lblTitulo);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormAcercaDe";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Acerca de...";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblCurso;
        private System.Windows.Forms.Label lblIntegrantesTitulo;
        private System.Windows.Forms.Label lblIntegrantes;
        private System.Windows.Forms.Label lblVersion;
        private System.Windows.Forms.Button btnCerrar;
    }
}
