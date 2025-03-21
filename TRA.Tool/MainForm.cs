using Microsoft.Extensions.Logging;
using System;
using System.Windows.Forms;
using TRA_Lib;

namespace TRA.Tool
{
    public partial class MainForm : Form
    {
        ILogger<MainForm> logger;
        public MainForm()
        {
            InitializeComponent();
            LoadRootDirectories();

            // Handle TreeView events
            treeView.ItemDrag += TreeView_ItemDrag;

            // Create a Panel to indicate the drop location
            dropIndicatorPanel = new Panel
            {
                Size = new Size(flowLayoutPanel.Width, 2),
                BackColor = Color.DimGray,
                Visible = false
            };
            flowLayoutPanel.Controls.Add(dropIndicatorPanel);
        }

        private void LoadRootDirectories()
        {
            treeView.Nodes.Clear();
            foreach (var drive in Directory.GetLogicalDrives())
            {
                var rootNode = new TreeNode(drive);
                rootNode.Nodes.Add("Loading...");
                treeView.Nodes.Add(rootNode);
            }
            treeView.BeforeExpand += TreeView_BeforeExpand;
        }

        private void TreeView_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            var node = e.Node;
            if (node.Nodes[0].Text == "Loading...")
            {
                node.Nodes.Clear();
                LoadDirectoriesAndFiles(node);
            }
        }

        private void LoadDirectoriesAndFiles(TreeNode node)
        {
            try
            {
                var path = node.FullPath;
                foreach (var directory in Directory.GetDirectories(path))
                {
                    var dirNode = new TreeNode(Path.GetFileName(directory)) { Tag = directory };
                    dirNode.Nodes.Add("Loading...");
                    node.Nodes.Add(dirNode);
                }

                foreach (var file in Directory.GetFiles(path))
                {
                    var fileNode = new TreeNode(Path.GetFileName(file)) { Tag = file };
                    node.Nodes.Add(fileNode);
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Handle access exceptions
            }
        }

        private void TreeView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            TreeNode node = (TreeNode)e.Item;
            if (node.Tag != null)
            {
                DoDragDrop(node, DragDropEffects.Copy);
            }
        }

        private void FlowLayoutPanel_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(UserControl))) 
            {
                e.Effect = DragDropEffects.Move;
            }
            else if (e.Data.GetDataPresent(typeof(TreeNode)))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }
        private void FlowLayoutPanel_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(TreeNode)))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.Move;
            }
            Point point = flowLayoutPanel.PointToClient(new Point(e.X, e.Y));
            Control item = flowLayoutPanel.GetChildAtPoint(point);
            // Display the drop indicator panel at the appropriate position
            if (item != null && item != flowLayoutPanel)
            {
                int index = flowLayoutPanel.Controls.GetChildIndex(item);
                if (flowLayoutPanel.Controls.IndexOf(dropIndicatorPanel) != index)
                {
                    flowLayoutPanel.Controls.SetChildIndex(dropIndicatorPanel, index + 1);
                    dropIndicatorPanel.Visible = true;
                    flowLayoutPanel.Invalidate();
                }
            }
            else
            {
                dropIndicatorPanel.Visible = false;
            }
        }
        private void FlowLayoutPanel_DragDrop(object sender, DragEventArgs e)
        {

            //DragDrop for Panel Order
            UserControl control = null;
            if (e.Data.GetDataPresent(typeof(TrassenPanel)))
            {
                control = e.Data.GetData(typeof(TrassenPanel)) as TrassenPanel;
            }
            else if (e.Data.GetDataPresent(typeof(InterpolationPanel)))
            {
                control = e.Data.GetData(typeof(InterpolationPanel)) as InterpolationPanel;
            }
            else if (e.Data.GetDataPresent(typeof(TransformPanel)))
            {
                control = e.Data.GetData(typeof(TransformPanel)) as TransformPanel;
            }
            FlowLayoutPanel panel = sender as FlowLayoutPanel;
            if (control != null && panel != null)
            {
                Point point = panel.PointToClient(new Point(e.X, e.Y));
                Control item = panel.GetChildAtPoint(point);
                int index = panel.Controls.IndexOf(item);
                if (index >= 0)
                {
                    panel.Controls.SetChildIndex(control, index);
                }
                else
                {
                    panel.Controls.SetChildIndex(control, panel.Controls.Count - 2);
                }
            }
            //DragDrop for Import
            if (e.Data.GetDataPresent(typeof(TreeNode)))
            {
                TreeNode node = (TreeNode)e.Data.GetData(typeof(TreeNode));
                FileInfo fileInfo = new FileInfo(node.Tag.ToString());
                Color selectedColor = TrassenPanel.DefaultBackColor;
                if (fileInfo.Extension.Equals(".tra", StringComparison.OrdinalIgnoreCase))
                {
                    TrassenPanel trassenPanel = new TrassenPanel();
                    trassenPanel.set_TRA_L_Path(node);
                    node.BackColor = trassenPanel.BackColor;
                    panel.Controls.Add(trassenPanel);
                    panel.Controls.SetChildIndex(trassenPanel, panel.Controls.Count - 2);                  
                }
                else if(fileInfo.Attributes == System.IO.FileAttributes.Directory)
                {
                    LoadDirectoriesAndFiles(node);
                    foreach (TreeNode childNode in node.Nodes)
                    {
                        fileInfo = new FileInfo(childNode.Tag.ToString());
                        if (fileInfo.Extension.Equals(".tra", StringComparison.OrdinalIgnoreCase) && childNode.BackColor != selectedColor)
                        {
                            TrassenPanel trassenPanel = new TrassenPanel();
                            trassenPanel.set_TRA_L_Path(childNode);
                            childNode.BackColor = trassenPanel.BackColor;
                            selectedColor = trassenPanel.BackColor;
                            panel.Controls.Add(trassenPanel);
                            panel.Controls.SetChildIndex(trassenPanel, panel.Controls.Count - 2);
                        }
                        if (fileInfo.Extension.Equals(".gra", StringComparison.OrdinalIgnoreCase) && childNode.BackColor != selectedColor)
                        {
                            TrassenPanel trassenPanel = new TrassenPanel();
                            trassenPanel.set_GRA_L_Path(childNode);
                            childNode.BackColor = trassenPanel.BackColor;
                            selectedColor = trassenPanel.BackColor;
                            panel.Controls.Add(trassenPanel);
                            panel.Controls.SetChildIndex(trassenPanel, panel.Controls.Count - 2);
                        }
                    }
                }
            }
            dropIndicatorPanel.Visible = false;
        }

        private void btn_AddTrassenPanel_Click(object sender, EventArgs e)
        {
            FlowLayoutPanel panel = ((Control)sender).Parent.Parent as FlowLayoutPanel;
            if (panel != null)
            {
                TrassenPanel control = new TrassenPanel();
                panel.Controls.Add(control);
                control.Dock = DockStyle.Top;
                panel.Controls.SetChildIndex(control, panel.Controls.GetChildIndex(((Control)sender).Parent));
                panel.Invalidate();
            }
        }

        private void btn_AddInterpolation_Click(object sender, EventArgs e)
        {
            FlowLayoutPanel panel = ((Control)sender).Parent.Parent as FlowLayoutPanel;
            if (panel != null)
            {
                InterpolationPanel control = new InterpolationPanel();
                panel.Controls.Add(control);
                control.Dock = DockStyle.Top;
                panel.Controls.SetChildIndex(control, panel.Controls.GetChildIndex(((Control)sender).Parent));
                panel.Invalidate();
            }
        }

        private void btn_AddTransformation_Click(object sender, EventArgs e)
        {
            FlowLayoutPanel panel = ((Control)sender).Parent.Parent as FlowLayoutPanel;
            if (panel != null)
            {
                TransformPanel control = new TransformPanel();
                panel.Controls.Add(control);
                control.Dock = DockStyle.Top;
                panel.Controls.SetChildIndex(control, panel.Controls.GetChildIndex(((Control)sender).Parent));
                panel.Invalidate();
            }
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tb_Log.Clear();
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddProvider(new TextBoxLoggerProvider(tb_Log));
            });
            // Create logger
            logger = loggerFactory.CreateLogger<MainForm>();
            TrassierungLog.AssignLogger(loggerFactory);
            egbt22lib.LoggingInitializer.InitializeLogging(loggerFactory);
        }
    }

    public class TextBoxLogger : ILogger
    {
        private readonly string _name;
        private readonly TextBox _textBox;

        public TextBoxLogger(string name, TextBox textBox)
        {
            _name = name;
            _textBox = textBox;
        }

        public IDisposable BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            string message = formatter(state, exception);
            if (!string.IsNullOrEmpty(message))
            {
                _textBox.Invoke((Action)(() => _textBox.AppendText($"{DateTime.Now}: {logLevel} - {message}{Environment.NewLine}")));
            }
        }
    }

    public class TextBoxLoggerProvider : ILoggerProvider
    {
        private readonly TextBox _textBox;

        public TextBoxLoggerProvider(TextBox textBox)
        {
            _textBox = textBox;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new TextBoxLogger(categoryName, _textBox);
        }

        public void Dispose() { }
    }

}