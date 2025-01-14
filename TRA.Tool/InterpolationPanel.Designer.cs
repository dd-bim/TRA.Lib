namespace TRA.Tool
{
    partial class InterpolationPanel
    {
        /// <summary> 
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Komponenten-Designer generierter Code

        /// <summary> 
        /// Erforderliche Methode für die Designerunterstützung. 
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            btn_Interpolate = new Button();
            num_InterpDist = new NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)num_InterpDist).BeginInit();
            SuspendLayout();
            // 
            // btn_Interpolate
            // 
            btn_Interpolate.Anchor = AnchorStyles.Right;
            btn_Interpolate.Location = new Point(261, 4);
            btn_Interpolate.Name = "btn_Interpolate";
            btn_Interpolate.Size = new Size(94, 47);
            btn_Interpolate.TabIndex = 0;
            btn_Interpolate.Text = "Interpolate";
            btn_Interpolate.UseVisualStyleBackColor = true;
            btn_Interpolate.Click += btn_Interpolate_Click;
            // 
            // num_InterpDist
            // 
            num_InterpDist.DecimalPlaces = 1;
            num_InterpDist.ImeMode = ImeMode.NoControl;
            num_InterpDist.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            num_InterpDist.Location = new Point(129, 17);
            num_InterpDist.Name = "num_InterpDist";
            num_InterpDist.Size = new Size(49, 23);
            num_InterpDist.TabIndex = 2;
            num_InterpDist.Value = new decimal(new int[] { 100, 0, 0, 65536 });
            // 
            // InterpolationPanel
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            BackColor = SystemColors.ControlDark;
            Controls.Add(num_InterpDist);
            Controls.Add(btn_Interpolate);
            Name = "InterpolationPanel";
            Size = new Size(358, 55);
            MouseDown += InterpolationPanel_MouseDown;
            ((System.ComponentModel.ISupportInitialize)num_InterpDist).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Button btn_Interpolate;
        private TextBox tb_InterpDist;
        private NumericUpDown num_InterpDist;
    }
}
