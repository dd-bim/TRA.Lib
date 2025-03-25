namespace TRA.Tool
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            splitContainer1 = new SplitContainer();
            treeView = new TreeView();
            splitContainer2 = new SplitContainer();
            flowLayoutPanel = new FlowLayoutPanel();
            trassenPanel1 = new TrassenPanel();
            panel_Buttons = new Panel();
            btn_SaveAll = new Button();
            btn_TransformationVA = new Button();
            btn_AddTransformation = new Button();
            btn_AddInterpolation = new Button();
            btn_AddTrassenPanel = new Button();
            tb_Log = new TextBox();
            contextMenuStrip_Log = new ContextMenuStrip(components);
            clearToolStripMenuItem = new ToolStripMenuItem();
            folderBrowserDialog = new FolderBrowserDialog();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
            splitContainer2.Panel1.SuspendLayout();
            splitContainer2.Panel2.SuspendLayout();
            splitContainer2.SuspendLayout();
            flowLayoutPanel.SuspendLayout();
            panel_Buttons.SuspendLayout();
            contextMenuStrip_Log.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Margin = new Padding(6);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(treeView);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(splitContainer2);
            splitContainer1.Size = new Size(1507, 960);
            splitContainer1.SplitterDistance = 498;
            splitContainer1.SplitterWidth = 7;
            splitContainer1.TabIndex = 0;
            // 
            // treeView
            // 
            treeView.Dock = DockStyle.Fill;
            treeView.Location = new Point(0, 0);
            treeView.Margin = new Padding(6);
            treeView.Name = "treeView";
            treeView.Size = new Size(498, 960);
            treeView.TabIndex = 1;
            // 
            // splitContainer2
            // 
            splitContainer2.Dock = DockStyle.Fill;
            splitContainer2.Location = new Point(0, 0);
            splitContainer2.Margin = new Padding(6);
            splitContainer2.Name = "splitContainer2";
            splitContainer2.Orientation = Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            splitContainer2.Panel1.Controls.Add(flowLayoutPanel);
            splitContainer2.Panel1.Controls.Add(panel_Buttons);
            // 
            // splitContainer2.Panel2
            // 
            splitContainer2.Panel2.Controls.Add(tb_Log);
            splitContainer2.Size = new Size(1002, 960);
            splitContainer2.SplitterDistance = 661;
            splitContainer2.SplitterWidth = 9;
            splitContainer2.TabIndex = 3;
            // 
            // flowLayoutPanel
            // 
            flowLayoutPanel.AllowDrop = true;
            flowLayoutPanel.AutoScroll = true;
            flowLayoutPanel.Controls.Add(trassenPanel1);
            flowLayoutPanel.Dock = DockStyle.Fill;
            flowLayoutPanel.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanel.Location = new Point(0, 0);
            flowLayoutPanel.Margin = new Padding(6);
            flowLayoutPanel.Name = "flowLayoutPanel";
            flowLayoutPanel.Size = new Size(1002, 608);
            flowLayoutPanel.TabIndex = 3;
            flowLayoutPanel.WrapContents = false;
            flowLayoutPanel.DragDrop += FlowLayoutPanel_DragDrop;
            flowLayoutPanel.DragEnter += FlowLayoutPanel_DragEnter;
            flowLayoutPanel.DragOver += FlowLayoutPanel_DragOver;
            // 
            // trassenPanel1
            // 
            trassenPanel1.AutoSize = true;
            trassenPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            trassenPanel1.BackColor = SystemColors.ActiveCaption;
            trassenPanel1.BorderStyle = BorderStyle.FixedSingle;
            trassenPanel1.Dock = DockStyle.Top;
            trassenPanel1.Location = new Point(11, 13);
            trassenPanel1.Margin = new Padding(11, 13, 11, 13);
            trassenPanel1.MinimumSize = new Size(700, 300);
            trassenPanel1.Name = "trassenPanel1";
            trassenPanel1.Size = new Size(700, 300);
            trassenPanel1.TabIndex = 3;
            // 
            // panel_Buttons
            // 
            panel_Buttons.Controls.Add(btn_SaveAll);
            panel_Buttons.Controls.Add(btn_TransformationVA);
            panel_Buttons.Controls.Add(btn_AddTransformation);
            panel_Buttons.Controls.Add(btn_AddInterpolation);
            panel_Buttons.Controls.Add(btn_AddTrassenPanel);
            panel_Buttons.Dock = DockStyle.Bottom;
            panel_Buttons.Location = new Point(0, 608);
            panel_Buttons.Name = "panel_Buttons";
            panel_Buttons.Size = new Size(1002, 53);
            panel_Buttons.TabIndex = 6;
            // 
            // btn_SaveAll
            // 
            btn_SaveAll.Dock = DockStyle.Left;
            btn_SaveAll.Location = new Point(808, 0);
            btn_SaveAll.Name = "btn_SaveAll";
            btn_SaveAll.Size = new Size(124, 53);
            btn_SaveAll.TabIndex = 12;
            btn_SaveAll.Text = "Save All";
            btn_SaveAll.UseVisualStyleBackColor = true;
            btn_SaveAll.Click += btn_SaveAll_Click;
            // 
            // btn_TransformationVA
            // 
            btn_TransformationVA.Dock = DockStyle.Left;
            btn_TransformationVA.Location = new Point(546, 0);
            btn_TransformationVA.Name = "btn_TransformationVA";
            btn_TransformationVA.Size = new Size(262, 53);
            btn_TransformationVA.TabIndex = 11;
            btn_TransformationVA.Text = "+ Transformation VA";
            btn_TransformationVA.UseVisualStyleBackColor = true;
            // 
            // btn_AddTransformation
            // 
            btn_AddTransformation.Dock = DockStyle.Left;
            btn_AddTransformation.Location = new Point(323, 0);
            btn_AddTransformation.Margin = new Padding(6);
            btn_AddTransformation.Name = "btn_AddTransformation";
            btn_AddTransformation.Size = new Size(223, 53);
            btn_AddTransformation.TabIndex = 10;
            btn_AddTransformation.Text = "+ Transformation";
            btn_AddTransformation.UseVisualStyleBackColor = true;
            btn_AddTransformation.Click += btn_AddTransformation_Click;
            // 
            // btn_AddInterpolation
            // 
            btn_AddInterpolation.Dock = DockStyle.Left;
            btn_AddInterpolation.Location = new Point(138, 0);
            btn_AddInterpolation.Margin = new Padding(6);
            btn_AddInterpolation.Name = "btn_AddInterpolation";
            btn_AddInterpolation.Size = new Size(185, 53);
            btn_AddInterpolation.TabIndex = 6;
            btn_AddInterpolation.Text = "+ Interpolation";
            btn_AddInterpolation.UseVisualStyleBackColor = true;
            btn_AddInterpolation.Click += btn_AddInterpolation_Click;
            // 
            // btn_AddTrassenPanel
            // 
            btn_AddTrassenPanel.Dock = DockStyle.Left;
            btn_AddTrassenPanel.Location = new Point(0, 0);
            btn_AddTrassenPanel.Margin = new Padding(6);
            btn_AddTrassenPanel.Name = "btn_AddTrassenPanel";
            btn_AddTrassenPanel.Size = new Size(138, 53);
            btn_AddTrassenPanel.TabIndex = 9;
            btn_AddTrassenPanel.Text = "+ Trasse";
            btn_AddTrassenPanel.UseVisualStyleBackColor = true;
            btn_AddTrassenPanel.Click += btn_AddTrassenPanel_Click;
            // 
            // tb_Log
            // 
            tb_Log.ContextMenuStrip = contextMenuStrip_Log;
            tb_Log.Dock = DockStyle.Fill;
            tb_Log.Location = new Point(0, 0);
            tb_Log.Margin = new Padding(6);
            tb_Log.Multiline = true;
            tb_Log.Name = "tb_Log";
            tb_Log.ReadOnly = true;
            tb_Log.ScrollBars = ScrollBars.Vertical;
            tb_Log.Size = new Size(1002, 290);
            tb_Log.TabIndex = 3;
            // 
            // contextMenuStrip_Log
            // 
            contextMenuStrip_Log.ImageScalingSize = new Size(32, 32);
            contextMenuStrip_Log.Items.AddRange(new ToolStripItem[] { clearToolStripMenuItem });
            contextMenuStrip_Log.Name = "contextMenuStrip1";
            contextMenuStrip_Log.Size = new Size(143, 42);
            // 
            // clearToolStripMenuItem
            // 
            clearToolStripMenuItem.Name = "clearToolStripMenuItem";
            clearToolStripMenuItem.Size = new Size(142, 38);
            clearToolStripMenuItem.Text = "Clear";
            clearToolStripMenuItem.Click += clearToolStripMenuItem_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1507, 960);
            Controls.Add(splitContainer1);
            Margin = new Padding(6);
            Name = "MainForm";
            Text = "TRA.Tool";
            Shown += MainForm_Shown;
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            splitContainer2.Panel1.ResumeLayout(false);
            splitContainer2.Panel2.ResumeLayout(false);
            splitContainer2.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).EndInit();
            splitContainer2.ResumeLayout(false);
            flowLayoutPanel.ResumeLayout(false);
            flowLayoutPanel.PerformLayout();
            panel_Buttons.ResumeLayout(false);
            contextMenuStrip_Log.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private SplitContainer splitContainer1;
        private TreeView treeView;
        private SplitContainer splitContainer2;
        private TextBox tb_Log;
        private Panel dropIndicatorPanel;
        private FlowLayoutPanel flowLayoutPanel1;
        private ContextMenuStrip contextMenuStrip_Log;
        private ToolStripMenuItem clearToolStripMenuItem;
        private FlowLayoutPanel flowLayoutPanel;
        private Panel panel_Buttons;
        private Button btn_AddInterpolation;
        private Button btn_AddTrassenPanel;
        private Button btn_AddTransformation;
        private TrassenPanel trassenPanel1;
        private Button btn_SaveAll;
        private Button btn_TransformationVA;
        private FolderBrowserDialog folderBrowserDialog;
    }
}