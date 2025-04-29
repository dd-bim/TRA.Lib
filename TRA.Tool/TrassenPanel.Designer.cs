

namespace TRA.Tool
{
    partial class TrassenPanel
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
            panel1 = new Panel();
            btn_SaveTRA = new Button();
            btn_SaveCSV = new Button();
            tableLayoutPanel1 = new TableLayoutPanel();
            tb_TRA_S = new TextBox();
            label_TRA_L = new Label();
            tb_TRA_L = new TextBox();
            tb_TRA_R = new TextBox();
            label_TRA_R = new Label();
            label_TRA_S = new Label();
            label_GRA_R = new Label();
            label_GRA_L = new Label();
            tb_GRA_L = new TextBox();
            tb_GRA_R = new TextBox();
            folderBrowserDialog = new FolderBrowserDialog();
            panel1.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // label_Panel
            // 
            label_Panel.Size = new Size(1055, 56);
            // 
            // panel1
            // 
            panel1.Controls.Add(btn_SaveTRA);
            panel1.Controls.Add(btn_SaveCSV);
            panel1.Dock = DockStyle.Bottom;
            panel1.Location = new Point(0, 258);
            panel1.Margin = new Padding(6);
            panel1.Name = "panel1";
            panel1.Size = new Size(1055, 58);
            panel1.TabIndex = 1;
            // 
            // btn_SaveTRA
            // 
            btn_SaveTRA.Location = new Point(193, 9);
            btn_SaveTRA.Margin = new Padding(6);
            btn_SaveTRA.Name = "btn_SaveTRA";
            btn_SaveTRA.Size = new Size(195, 43);
            btn_SaveTRA.TabIndex = 2;
            btn_SaveTRA.Text = "Save TRA+GRA";
            btn_SaveTRA.UseVisualStyleBackColor = true;
            btn_SaveTRA.Click += btn_SaveTRA_Click;
            // 
            // btn_SaveCSV
            // 
            btn_SaveCSV.Location = new Point(6, 9);
            btn_SaveCSV.Margin = new Padding(6);
            btn_SaveCSV.Name = "btn_SaveCSV";
            btn_SaveCSV.Size = new Size(176, 43);
            btn_SaveCSV.TabIndex = 1;
            btn_SaveCSV.Text = "SaveCSV";
            btn_SaveCSV.UseVisualStyleBackColor = true;
            btn_SaveCSV.Click += btn_Save_Click;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 3;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33333F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333359F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333359F));
            tableLayoutPanel1.Controls.Add(tb_TRA_S, 1, 1);
            tableLayoutPanel1.Controls.Add(label_TRA_L, 0, 0);
            tableLayoutPanel1.Controls.Add(tb_TRA_L, 0, 1);
            tableLayoutPanel1.Controls.Add(tb_TRA_R, 2, 1);
            tableLayoutPanel1.Controls.Add(label_TRA_R, 2, 0);
            tableLayoutPanel1.Controls.Add(label_TRA_S, 1, 0);
            tableLayoutPanel1.Controls.Add(label_GRA_R, 2, 2);
            tableLayoutPanel1.Controls.Add(label_GRA_L, 0, 2);
            tableLayoutPanel1.Controls.Add(tb_GRA_L, 0, 3);
            tableLayoutPanel1.Controls.Add(tb_GRA_R, 2, 3);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 56);
            tableLayoutPanel1.Margin = new Padding(6);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 4;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 43F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 43F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.Size = new Size(1055, 202);
            tableLayoutPanel1.TabIndex = 4;
            // 
            // tb_TRA_S
            // 
            tb_TRA_S.AllowDrop = true;
            tb_TRA_S.Dock = DockStyle.Fill;
            tb_TRA_S.Location = new Point(357, 49);
            tb_TRA_S.Margin = new Padding(6);
            tb_TRA_S.Name = "tb_TRA_S";
            tb_TRA_S.ReadOnly = true;
            tb_TRA_S.Size = new Size(339, 39);
            tb_TRA_S.TabIndex = 9;
            tb_TRA_S.TextChanged += tb_XRA_TextChanged;
            tb_TRA_S.DragDrop += Tb_TRA_DragDrop;
            tb_TRA_S.DragEnter += Tb_TRA_DragEnter;
            // 
            // label_TRA_L
            // 
            label_TRA_L.AutoSize = true;
            label_TRA_L.Dock = DockStyle.Fill;
            label_TRA_L.Location = new Point(6, 0);
            label_TRA_L.Margin = new Padding(6, 0, 6, 0);
            label_TRA_L.Name = "label_TRA_L";
            label_TRA_L.Size = new Size(339, 43);
            label_TRA_L.TabIndex = 3;
            label_TRA_L.Text = "L.TRA";
            label_TRA_L.TextAlign = ContentAlignment.BottomCenter;
            // 
            // tb_TRA_L
            // 
            tb_TRA_L.AllowDrop = true;
            tb_TRA_L.Dock = DockStyle.Fill;
            tb_TRA_L.Location = new Point(6, 49);
            tb_TRA_L.Margin = new Padding(6);
            tb_TRA_L.Name = "tb_TRA_L";
            tb_TRA_L.ReadOnly = true;
            tb_TRA_L.Size = new Size(339, 39);
            tb_TRA_L.TabIndex = 0;
            tb_TRA_L.TextChanged += tb_XRA_TextChanged;
            tb_TRA_L.DragDrop += Tb_TRA_DragDrop;
            tb_TRA_L.DragEnter += Tb_TRA_DragEnter;
            // 
            // tb_TRA_R
            // 
            tb_TRA_R.AllowDrop = true;
            tb_TRA_R.Dock = DockStyle.Fill;
            tb_TRA_R.Location = new Point(708, 49);
            tb_TRA_R.Margin = new Padding(6);
            tb_TRA_R.Name = "tb_TRA_R";
            tb_TRA_R.ReadOnly = true;
            tb_TRA_R.Size = new Size(341, 39);
            tb_TRA_R.TabIndex = 2;
            tb_TRA_R.TextChanged += tb_XRA_TextChanged;
            tb_TRA_R.DragDrop += Tb_TRA_DragDrop;
            tb_TRA_R.DragEnter += Tb_TRA_DragEnter;
            // 
            // label_TRA_R
            // 
            label_TRA_R.AutoSize = true;
            label_TRA_R.Dock = DockStyle.Fill;
            label_TRA_R.Location = new Point(708, 0);
            label_TRA_R.Margin = new Padding(6, 0, 6, 0);
            label_TRA_R.Name = "label_TRA_R";
            label_TRA_R.Size = new Size(341, 43);
            label_TRA_R.TabIndex = 5;
            label_TRA_R.Text = "R.TRA";
            label_TRA_R.TextAlign = ContentAlignment.BottomCenter;
            // 
            // label_TRA_S
            // 
            label_TRA_S.AutoSize = true;
            label_TRA_S.Dock = DockStyle.Fill;
            label_TRA_S.Location = new Point(357, 0);
            label_TRA_S.Margin = new Padding(6, 0, 6, 0);
            label_TRA_S.Name = "label_TRA_S";
            label_TRA_S.Size = new Size(339, 43);
            label_TRA_S.TabIndex = 6;
            label_TRA_S.Text = "S.TRA";
            label_TRA_S.TextAlign = ContentAlignment.BottomCenter;
            // 
            // label_GRA_R
            // 
            label_GRA_R.AutoSize = true;
            label_GRA_R.Dock = DockStyle.Fill;
            label_GRA_R.Location = new Point(708, 94);
            label_GRA_R.Margin = new Padding(6, 0, 6, 0);
            label_GRA_R.Name = "label_GRA_R";
            label_GRA_R.Size = new Size(341, 43);
            label_GRA_R.TabIndex = 7;
            label_GRA_R.Text = "R.GRA";
            label_GRA_R.TextAlign = ContentAlignment.BottomCenter;
            // 
            // label_GRA_L
            // 
            label_GRA_L.AutoSize = true;
            label_GRA_L.Dock = DockStyle.Fill;
            label_GRA_L.Location = new Point(6, 94);
            label_GRA_L.Margin = new Padding(6, 0, 6, 0);
            label_GRA_L.Name = "label_GRA_L";
            label_GRA_L.Size = new Size(339, 43);
            label_GRA_L.TabIndex = 8;
            label_GRA_L.Text = "L.GRA";
            label_GRA_L.TextAlign = ContentAlignment.BottomCenter;
            // 
            // tb_GRA_L
            // 
            tb_GRA_L.AllowDrop = true;
            tb_GRA_L.Dock = DockStyle.Fill;
            tb_GRA_L.Location = new Point(6, 143);
            tb_GRA_L.Margin = new Padding(6);
            tb_GRA_L.Name = "tb_GRA_L";
            tb_GRA_L.ReadOnly = true;
            tb_GRA_L.Size = new Size(339, 39);
            tb_GRA_L.TabIndex = 10;
            tb_GRA_L.TextChanged += tb_XRA_TextChanged;
            tb_GRA_L.DragDrop += Tb_GRA_DragDrop;
            tb_GRA_L.DragEnter += Tb_GRA_DragEnter;
            // 
            // tb_GRA_R
            // 
            tb_GRA_R.AllowDrop = true;
            tb_GRA_R.Dock = DockStyle.Fill;
            tb_GRA_R.Location = new Point(708, 143);
            tb_GRA_R.Margin = new Padding(6);
            tb_GRA_R.Name = "tb_GRA_R";
            tb_GRA_R.ReadOnly = true;
            tb_GRA_R.Size = new Size(341, 39);
            tb_GRA_R.TabIndex = 11;
            tb_GRA_R.TextChanged += tb_XRA_TextChanged;
            tb_GRA_R.DragDrop += Tb_GRA_DragDrop;
            tb_GRA_R.DragEnter += Tb_GRA_DragEnter;
            // 
            // TrassenPanel
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.LightBlue;
            Controls.Add(tableLayoutPanel1);
            Controls.Add(panel1);
            Name = "TrassenPanel";
            Size = new Size(1055, 316);
            Controls.SetChildIndex(panel1, 0);
            Controls.SetChildIndex(tableLayoutPanel1, 0);
            panel1.ResumeLayout(false);
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private Panel panel1;
        private TableLayoutPanel tableLayoutPanel1;
        private TextBox tb_TRA_S;
        private Label label_TRA_L;
        private TextBox tb_TRA_L;
        private TextBox tb_TRA_R;
        private Label label_TRA_R;
        private Label label_TRA_S;
        private Label label_GRA_R;
        private Label label_GRA_L;
        private TextBox tb_GRA_L;
        private TextBox tb_GRA_R;
        private Button btn_SaveCSV;
        private FolderBrowserDialog folderBrowserDialog;
        private Button btn_SaveTRA;
    }
}
