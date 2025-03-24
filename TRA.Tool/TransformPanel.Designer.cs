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
            btn_SaveAll = new Button();
            folderBrowserDialog = new FolderBrowserDialog();
            groupBox = new GroupBox();
            checkBox_RecalcLength = new CheckBox();
            groupBox.SuspendLayout();
            SuspendLayout();
            // 
            // btn_Transform
            // 
            btn_Transform.Anchor = AnchorStyles.Right;
            btn_Transform.Location = new Point(646, 62);
            btn_Transform.Margin = new Padding(6);
            btn_Transform.Name = "btn_Transform";
            btn_Transform.Size = new Size(254, 81);
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
            checkBox_RecalcHeading.Location = new Point(9, 37);
            checkBox_RecalcHeading.Margin = new Padding(6);
            checkBox_RecalcHeading.Name = "checkBox_RecalcHeading";
            checkBox_RecalcHeading.Size = new Size(136, 36);
            checkBox_RecalcHeading.TabIndex = 1;
            checkBox_RecalcHeading.Text = "Heading";
            checkBox_RecalcHeading.UseVisualStyleBackColor = true;
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
            // btn_SaveAll
            // 
            btn_SaveAll.Anchor = AnchorStyles.Right;
            btn_SaveAll.Location = new Point(646, 155);
            btn_SaveAll.Margin = new Padding(6);
            btn_SaveAll.Name = "btn_SaveAll";
            btn_SaveAll.Size = new Size(254, 44);
            btn_SaveAll.TabIndex = 3;
            btn_SaveAll.Text = "Save all";
            btn_SaveAll.UseVisualStyleBackColor = true;
            btn_SaveAll.Click += btn_SaveAll_Click;
            // 
            // groupBox
            // 
            groupBox.Controls.Add(checkBox_RecalcLength);
            groupBox.Controls.Add(checkBox_RecalcHeading);
            groupBox.Location = new Point(3, 130);
            groupBox.Name = "groupBox";
            groupBox.Size = new Size(287, 81);
            groupBox.TabIndex = 4;
            groupBox.TabStop = false;
            groupBox.Text = "Optimize";
            // 
            // checkBox_RecalcLength
            // 
            checkBox_RecalcLength.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            checkBox_RecalcLength.AutoSize = true;
            checkBox_RecalcLength.Checked = true;
            checkBox_RecalcLength.CheckState = CheckState.Checked;
            checkBox_RecalcLength.Location = new Point(157, 36);
            checkBox_RecalcLength.Margin = new Padding(6);
            checkBox_RecalcLength.Name = "checkBox_RecalcLength";
            checkBox_RecalcLength.Size = new Size(120, 36);
            checkBox_RecalcLength.TabIndex = 2;
            checkBox_RecalcLength.Text = "Length";
            checkBox_RecalcLength.UseVisualStyleBackColor = true;
            // 
            // TransformPanel
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.WindowFrame;
            Controls.Add(groupBox);
            Controls.Add(btn_SaveAll);
            Controls.Add(comboBox_Transform);
            Controls.Add(btn_Transform);
            Margin = new Padding(6);
            Name = "TransformPanel";
            Size = new Size(906, 228);
            MouseDown += TransformPanel_MouseDown;
            groupBox.ResumeLayout(false);
            groupBox.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Button btn_Transform;
        private CheckBox checkBox_RecalcHeading;
        private ComboBox comboBox_Transform;
        private Button btn_SaveAll;
        private FolderBrowserDialog folderBrowserDialog;
        private GroupBox groupBox;
        private CheckBox checkBox_RecalcLength;
    }
}
