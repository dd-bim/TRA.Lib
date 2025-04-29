

namespace TRA.Tool
{
    partial class BasePanel
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BasePanel));
            panel_top = new Panel();
            btn_delete = new Button();
            label_Panel = new Label();
            panel_top.SuspendLayout();
            SuspendLayout();
            // 
            // panel_top
            // 
            panel_top.Controls.Add(btn_delete);
            panel_top.Controls.Add(label_Panel);
            panel_top.Dock = DockStyle.Top;
            panel_top.Location = new Point(0, 0);
            panel_top.Name = "panel_top";
            panel_top.Size = new Size(775, 56);
            panel_top.TabIndex = 10;
            // 
            // btn_delete
            // 
            btn_delete.AutoSize = true;
            btn_delete.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btn_delete.Dock = DockStyle.Right;
            btn_delete.FlatAppearance.BorderSize = 0;
            btn_delete.FlatStyle = FlatStyle.Flat;
            btn_delete.Image = (Image)resources.GetObject("btn_delete.Image");
            btn_delete.Location = new Point(753, 0);
            btn_delete.Margin = new Padding(0);
            btn_delete.Name = "btn_delete";
            btn_delete.Size = new Size(22, 56);
            btn_delete.TabIndex = 11;
            btn_delete.UseVisualStyleBackColor = true;
            btn_delete.Click += btn_delete_Click;
            // 
            // label_Panel
            // 
            label_Panel.Dock = DockStyle.Fill;
            label_Panel.Location = new Point(0, 0);
            label_Panel.Margin = new Padding(6, 0, 6, 0);
            label_Panel.Name = "label_Panel";
            label_Panel.Size = new Size(775, 56);
            label_Panel.TabIndex = 10;
            label_Panel.Text = "Base";
            label_Panel.TextAlign = ContentAlignment.MiddleCenter;
            label_Panel.MouseDown += label_Trasse_MouseDown;
            // 
            // BasePanel
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            BackColor = Color.White;
            BorderStyle = BorderStyle.FixedSingle;
            Controls.Add(panel_top);
            Margin = new Padding(6);
            Name = "BasePanel";
            Size = new Size(775, 221);
            panel_top.ResumeLayout(false);
            panel_top.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel panel_top;
        protected Label label_Panel;
        private Button btn_delete;
    }
}
