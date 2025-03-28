﻿namespace TRA.Tool
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
            comboBox_TransformInput = new ComboBox();
            comboBox_TransformOutput = new ComboBox();
            label1 = new Label();
            label2 = new Label();
            SuspendLayout();
            // 
            // comboBox_TransformInput
            // 
            comboBox_TransformInput.FormattingEnabled = true;
            comboBox_TransformInput.Location = new Point(12, 52);
            comboBox_TransformInput.Margin = new Padding(6);
            comboBox_TransformInput.Name = "comboBox_TransformInput";
            comboBox_TransformInput.Size = new Size(437, 40);
            comboBox_TransformInput.TabIndex = 2;
            // 
            // comboBox_TransformOutput
            // 
            comboBox_TransformOutput.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            comboBox_TransformOutput.AutoCompleteSource = AutoCompleteSource.ListItems;
            comboBox_TransformOutput.FormattingEnabled = true;
            comboBox_TransformOutput.Location = new Point(461, 52);
            comboBox_TransformOutput.Margin = new Padding(6);
            comboBox_TransformOutput.Name = "comboBox_TransformOutput";
            comboBox_TransformOutput.Size = new Size(439, 40);
            comboBox_TransformOutput.TabIndex = 5;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 14);
            label1.Name = "label1";
            label1.Size = new Size(92, 32);
            label1.TabIndex = 6;
            label1.Text = "Source:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(461, 14);
            label2.Name = "label2";
            label2.Size = new Size(84, 32);
            label2.TabIndex = 7;
            label2.Text = "Target:";
            // 
            // TransformPanel_VA
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            BackColor = Color.SeaGreen;
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(comboBox_TransformOutput);
            Controls.Add(comboBox_TransformInput);
            Name = "TransformPanel_VA";
            Controls.SetChildIndex(comboBox_TransformInput, 0);
            Controls.SetChildIndex(comboBox_TransformOutput, 0);
            Controls.SetChildIndex(label1, 0);
            Controls.SetChildIndex(label2, 0);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private ComboBox comboBox_TransformInput;
        private ComboBox comboBox_TransformOutput;
        private Label label1;
        private Label label2;
    }
}
