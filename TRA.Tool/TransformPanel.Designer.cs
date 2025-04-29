namespace TRA.Tool
{
    partial class TransformPanel
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
            comboBox_TransformFrom = new ComboBox();
            comboBox_TransformTo = new ComboBox();
            SuspendLayout();
            // 
            // comboBox_TransformFrom
            // comboBox_TransformFrom
            // 
            comboBox_TransformFrom.FormattingEnabled = true;
            comboBox_TransformFrom.Location = new Point(12, 81);
            comboBox_TransformFrom.Margin = new Padding(6);
            comboBox_TransformFrom.Name = "comboBox_TransformFrom";
            comboBox_TransformFrom.Size = new Size(338, 40);
            comboBox_TransformFrom.TabIndex = 2;
            // 
            // comboBox_TransformTo
            // 
            comboBox_TransformTo.FormattingEnabled = true;
            comboBox_TransformTo.Location = new Point(420, 81);
            comboBox_TransformTo.Margin = new Padding(6);
            comboBox_TransformTo.Name = "comboBox_TransformTo";
            comboBox_TransformTo.Size = new Size(338, 40);
            comboBox_TransformTo.TabIndex = 5;
            comboBox_TransformFrom.FormattingEnabled = true;
            comboBox_TransformFrom.Location = new Point(12, 81);
            comboBox_TransformFrom.Margin = new Padding(6);
            comboBox_TransformFrom.Name = "comboBox_TransformFrom";
            comboBox_TransformFrom.Size = new Size(338, 40);
            comboBox_TransformFrom.TabIndex = 2;
            // 
            // comboBox_TransformTo
            // 
            comboBox_TransformTo.FormattingEnabled = true;
            comboBox_TransformTo.Location = new Point(420, 81);
            comboBox_TransformTo.Margin = new Padding(6);
            comboBox_TransformTo.Name = "comboBox_TransformTo";
            comboBox_TransformTo.Size = new Size(338, 40);
            comboBox_TransformTo.TabIndex = 5;
            // 
            // TransformPanel
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            BackColor = Color.OliveDrab;
            Name = "TransformPanel";
            Controls.SetChildIndex(btn_Transform, 0);
            ResumeLayout(false);
            //
            // base.panel_TransformProperties
            //
            panel_TransformProperties.Controls.Add(comboBox_TransformFrom);
            panel_TransformProperties.Controls.Add(comboBox_TransformTo);
        }

        #endregion
        private ComboBox comboBox_TransformFrom;
        private ComboBox comboBox_TransformTo;
        private ComboBox comboBox_TransformFrom;
        private ComboBox comboBox_TransformTo;
    }
}
