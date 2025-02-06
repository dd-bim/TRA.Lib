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
            btn_Transform = new Button();
            SuspendLayout();
            // 
            // btn_Transform
            // 
            btn_Transform.Anchor = AnchorStyles.Right;
            btn_Transform.Location = new Point(348, 29);
            btn_Transform.Name = "btn_Transform";
            btn_Transform.Size = new Size(137, 38);
            btn_Transform.TabIndex = 0;
            btn_Transform.Text = "Transform";
            btn_Transform.UseVisualStyleBackColor = true;
            btn_Transform.Click += btn_Transform_Click;
            // 
            // TransformPanel
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.WindowFrame;
            Controls.Add(btn_Transform);
            Name = "TransformPanel";
            Size = new Size(488, 107);
            MouseDown += TransformPanel_MouseDown;
            ResumeLayout(false);
        }

        #endregion

        private Button btn_Transform;
    }
}
