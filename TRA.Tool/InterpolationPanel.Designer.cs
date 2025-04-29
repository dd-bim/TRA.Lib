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
            num_allowedTolerance = new NumericUpDown();
            label1 = new Label();
            label2 = new Label();
            ((System.ComponentModel.ISupportInitialize)num_InterpDist).BeginInit();
            ((System.ComponentModel.ISupportInitialize)num_allowedTolerance).BeginInit();
            SuspendLayout();
            // 
            // btn_Interpolate
            // 
            btn_Interpolate.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btn_Interpolate.Location = new Point(555, 79);
            btn_Interpolate.Margin = new Padding(6);
            btn_Interpolate.Name = "btn_Interpolate";
            btn_Interpolate.Size = new Size(175, 90);
            btn_Interpolate.TabIndex = 0;
            btn_Interpolate.Text = "Interpolate";
            btn_Interpolate.UseVisualStyleBackColor = true;
            btn_Interpolate.Click += btn_Interpolate_Click;
            // 
            // num_InterpDist
            // 
            num_InterpDist.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            num_InterpDist.DecimalPlaces = 1;
            num_InterpDist.ImeMode = ImeMode.NoControl;
            num_InterpDist.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            num_InterpDist.Location = new Point(452, 79);
            num_InterpDist.Margin = new Padding(6);
            num_InterpDist.Name = "num_InterpDist";
            num_InterpDist.Size = new Size(91, 39);
            num_InterpDist.TabIndex = 2;
            num_InterpDist.Value = new decimal(new int[] { 500, 0, 0, 65536 });
            // 
            // num_allowedTolerance
            // 
            num_allowedTolerance.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            num_allowedTolerance.DecimalPlaces = 1;
            num_allowedTolerance.ImeMode = ImeMode.NoControl;
            num_allowedTolerance.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            num_allowedTolerance.Location = new Point(452, 130);
            num_allowedTolerance.Margin = new Padding(6);
            num_allowedTolerance.Name = "num_allowedTolerance";
            num_allowedTolerance.Size = new Size(91, 39);
            num_allowedTolerance.TabIndex = 3;
            num_allowedTolerance.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            label1.AutoSize = true;
            label1.Location = new Point(342, 83);
            label1.Margin = new Padding(6, 0, 6, 0);
            label1.Name = "label1";
            label1.Size = new Size(105, 32);
            label1.TabIndex = 4;
            label1.Text = "Delta[m]";
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            label2.AutoSize = true;
            label2.Location = new Point(279, 134);
            label2.Margin = new Padding(6, 0, 6, 0);
            label2.Name = "label2";
            label2.Size = new Size(172, 32);
            label2.TabIndex = 5;
            label2.Text = "Tolerance[mm]";
            // 
            // InterpolationPanel
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            BackColor = Color.Goldenrod;
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(num_allowedTolerance);
            Controls.Add(num_InterpDist);
            Controls.Add(btn_Interpolate);
            Name = "InterpolationPanel";
            Size = new Size(736, 190);
            Controls.SetChildIndex(btn_Interpolate, 0);
            Controls.SetChildIndex(num_InterpDist, 0);
            Controls.SetChildIndex(num_allowedTolerance, 0);
            Controls.SetChildIndex(label1, 0);
            Controls.SetChildIndex(label2, 0);
            ((System.ComponentModel.ISupportInitialize)num_InterpDist).EndInit();
            ((System.ComponentModel.ISupportInitialize)num_allowedTolerance).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btn_Interpolate;
        private TextBox tb_InterpDist;
        private NumericUpDown num_InterpDist;
        private NumericUpDown num_allowedTolerance;
        private Label label1;
        private Label label2;
    }
}
