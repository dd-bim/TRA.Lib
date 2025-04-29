namespace TRA.Tool
{
    partial class TransformPanel_VA
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
            comboBox_TransformFrom = new ComboBox();
            comboBox_TransformTo = new ComboBox();
            label_Source = new Label();
            label_Target = new Label();
            panel_TransformProperties.SuspendLayout();
            SuspendLayout();
            // 
            // panel_TransformProperties
            // 
            panel_TransformProperties.Controls.Add(label_Target);
            panel_TransformProperties.Controls.Add(label_Source);
            panel_TransformProperties.Controls.Add(comboBox_TransformOutput);
            panel_TransformProperties.Controls.Add(comboBox_TransformInput);
            panel_TransformProperties.Size = new Size(904, 196);
            panel_TransformProperties.Controls.SetChildIndex(comboBox_TransformInput, 0);
            panel_TransformProperties.Controls.SetChildIndex(comboBox_TransformOutput, 0);
            panel_TransformProperties.Controls.SetChildIndex(label_Source, 0);
            panel_TransformProperties.Controls.SetChildIndex(label_Target, 0);
            // 
            // label_Panel
            // 
            label_Panel.Size = new Size(904, 56);
            // 
            // comboBox_TransformInput
            // 
            comboBox_TransformFrom.FormattingEnabled = true;
            comboBox_TransformFrom.Location = new Point(12, 52);
            comboBox_TransformFrom.Margin = new Padding(6);
            comboBox_TransformFrom.Name = "comboBox_TransformInput";
            comboBox_TransformFrom.Size = new Size(437, 40);
            comboBox_TransformFrom.TabIndex = 2;
            // 
            // comboBox_TransformOutput
            // 
            comboBox_TransformOutput.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            comboBox_TransformOutput.AutoCompleteSource = AutoCompleteSource.ListItems;
            comboBox_TransformOutput.FormattingEnabled = true;
            comboBox_TransformOutput.Location = new Point(448, 41);
            comboBox_TransformOutput.Margin = new Padding(6);
            comboBox_TransformOutput.Name = "comboBox_TransformOutput";
            comboBox_TransformOutput.Size = new Size(439, 40);
            comboBox_TransformOutput.TabIndex = 5;
            // 
            // label_Source
            // 
            label_Source.AutoSize = true;
            label_Source.Location = new Point(14, 0);
            label_Source.Name = "label_Source";
            label_Source.Size = new Size(92, 32);
            label_Source.TabIndex = 6;
            label_Source.Text = "Source:";
            // 
            // label_Target
            // 
            label_Target.AutoSize = true;
            label_Target.Location = new Point(448, 3);
            label_Target.Name = "label_Target";
            label_Target.Size = new Size(84, 32);
            label_Target.TabIndex = 7;
            label_Target.Text = "Target:";
            // 
            // TransformPanel_VA
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            BackColor = Color.SeaGreen;
            Name = "TransformPanel_VA";
            Size = new Size(904, 252);
            panel_TransformProperties.ResumeLayout(false);
            panel_TransformProperties.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private ComboBox comboBox_TransformFrom;
        private ComboBox comboBox_TransformTo;
        private Label label_Source;
        private Label label_Target;
    }
}
