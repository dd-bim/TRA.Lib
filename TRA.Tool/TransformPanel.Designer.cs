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
            checkBox_RecalcHeading = new CheckBox();
            comboBox_Transform = new ComboBox();
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
            // checkBox_RecalcHeading
            // 
            checkBox_RecalcHeading.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            checkBox_RecalcHeading.AutoSize = true;
            checkBox_RecalcHeading.Checked = true;
            checkBox_RecalcHeading.CheckState = CheckState.Checked;
            checkBox_RecalcHeading.Location = new Point(3, 85);
            checkBox_RecalcHeading.Name = "checkBox_RecalcHeading";
            checkBox_RecalcHeading.Size = new Size(134, 19);
            checkBox_RecalcHeading.TabIndex = 1;
            checkBox_RecalcHeading.Text = "Recalculate Heading";
            checkBox_RecalcHeading.UseVisualStyleBackColor = true;
            // 
            // comboBox_Transform
            // 
            comboBox_Transform.FormattingEnabled = true;
            comboBox_Transform.Location = new Point(158, 38);
            comboBox_Transform.Name = "comboBox_Transform";
            comboBox_Transform.Size = new Size(184, 23);
            comboBox_Transform.TabIndex = 2;
            // 
            // TransformPanel
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.WindowFrame;
            Controls.Add(comboBox_Transform);
            Controls.Add(checkBox_RecalcHeading);
            Controls.Add(btn_Transform);
            Name = "TransformPanel";
            Size = new Size(488, 107);
            MouseDown += TransformPanel_MouseDown;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btn_Transform;
        private CheckBox checkBox_RecalcHeading;
        private ComboBox comboBox_Transform;
    }
}
