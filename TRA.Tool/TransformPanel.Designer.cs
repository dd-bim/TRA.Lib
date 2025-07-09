#if USE_EGBT22LIB
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
            comboBox_TransformFromVCS = new ComboBox();
            groupBox_Source = new GroupBox();
            groupBox_Target = new GroupBox();
            panel_TransformProperties.SuspendLayout();
            groupBox_Source.SuspendLayout();
            groupBox_Target.SuspendLayout();
            SuspendLayout();
            // 
            // btn_Transform
            // 
            btn_Transform.Location = new Point(953, 146);
            // 
            // panel_TransformProperties
            // 
            panel_TransformProperties.Controls.Add(groupBox_Target);
            panel_TransformProperties.Controls.Add(groupBox_Source);
            panel_TransformProperties.Size = new Size(1222, 241);
            panel_TransformProperties.Controls.SetChildIndex(groupBox_Source, 0);
            panel_TransformProperties.Controls.SetChildIndex(btn_Transform, 0);
            panel_TransformProperties.Controls.SetChildIndex(groupBox_Target, 0);
            // 
            // label_Panel
            // 
            label_Panel.Size = new Size(1222, 56);
            // 
            // comboBox_TransformFrom
            // 
            comboBox_TransformFrom.FormattingEnabled = true;
            comboBox_TransformFrom.Location = new Point(9, 41);
            comboBox_TransformFrom.Margin = new Padding(6);
            comboBox_TransformFrom.Name = "comboBox_TransformFrom";
            comboBox_TransformFrom.Size = new Size(420, 40);
            comboBox_TransformFrom.TabIndex = 2;
            // 
            // comboBox_TransformTo
            // 
            comboBox_TransformTo.FormattingEnabled = true;
            comboBox_TransformTo.Location = new Point(9, 41);
            comboBox_TransformTo.Margin = new Padding(6);
            comboBox_TransformTo.Name = "comboBox_TransformTo";
            comboBox_TransformTo.Size = new Size(338, 40);
            comboBox_TransformTo.TabIndex = 5;
            // 
            // comboBox_TransformFromVCS
            // 
            comboBox_TransformFromVCS.FormattingEnabled = true;
            comboBox_TransformFromVCS.Location = new Point(225, 41);
            comboBox_TransformFromVCS.Margin = new Padding(6);
            comboBox_TransformFromVCS.Name = "comboBox_TransformFromVCS";
            comboBox_TransformFromVCS.Size = new Size(204, 40);
            comboBox_TransformFromVCS.TabIndex = 3;
            comboBox_TransformFromVCS.Visible = false;
            // 
            // groupBox_Source
            // 
            groupBox_Source.Controls.Add(comboBox_TransformFrom);
            groupBox_Source.Controls.Add(comboBox_TransformFromVCS);
            groupBox_Source.Location = new Point(23, 43);
            groupBox_Source.Name = "groupBox_Source";
            groupBox_Source.Size = new Size(445, 94);
            groupBox_Source.TabIndex = 10;
            groupBox_Source.TabStop = false;
            groupBox_Source.Text = "Source";
            // 
            // groupBox_Target
            // 
            groupBox_Target.Controls.Add(comboBox_TransformTo);
            groupBox_Target.Location = new Point(505, 43);
            groupBox_Target.Name = "groupBox_Target";
            groupBox_Target.Size = new Size(377, 94);
            groupBox_Target.TabIndex = 11;
            groupBox_Target.TabStop = false;
            groupBox_Target.Text = "Target";
            // 
            // TransformPanel
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            BackColor = Color.OliveDrab;
            Name = "TransformPanel";
            Size = new Size(1222, 297);
            panel_TransformProperties.ResumeLayout(false);
            groupBox_Source.ResumeLayout(false);
            groupBox_Target.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private ComboBox comboBox_TransformFrom;
        private ComboBox comboBox_TransformTo;
        private ComboBox comboBox_TransformFromVCS;
        private GroupBox groupBox_Source;
        private GroupBox groupBox_Target;
    }
}
#endif