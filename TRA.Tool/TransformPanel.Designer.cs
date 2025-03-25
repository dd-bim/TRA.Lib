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
            comboBox_Transform = new ComboBox();
            SuspendLayout();

            // 
            // comboBox_Transform
            // 
            comboBox_Transform.FormattingEnabled = true;
            comboBox_Transform.Location = new Point(293, 81);
            comboBox_Transform.Margin = new Padding(6);
            comboBox_Transform.Name = "comboBox_Transform";
            comboBox_Transform.Size = new Size(338, 40);
            comboBox_Transform.TabIndex = 2;

            // 
            // TransformPanel
            // 
            Controls.Add(comboBox_Transform);
            ResumeLayout(false);
        }

        #endregion
        private ComboBox comboBox_Transform;
    }
}
