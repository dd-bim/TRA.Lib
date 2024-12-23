using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TrassierungInterface;

namespace TRA.Tool
{
    public partial class MainForm : Form
    {
        private ILogger<MainForm> logger;
        public MainForm()
        {
            InitializeComponent();
            LoadRootDirectories();

            // Handle TreeView events
            treeView.ItemDrag += TreeView_ItemDrag;

            ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddProvider(new TextBoxLoggerProvider(tb_Log));
            });
            // Create logger
            logger = loggerFactory.CreateLogger<MainForm>();
            TrassierungLog.AssignLogger(loggerFactory);

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
                DoDragDrop(node,
                    DragDropEffects.Copy);
            }
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