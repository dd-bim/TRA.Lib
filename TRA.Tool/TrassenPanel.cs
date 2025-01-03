
using System.Data;
using System.IO;
using System.Windows.Forms;
using TRA_Lib;

namespace TRA.Tool
{
    public partial class TrassenPanel : UserControl
    {
        TRATrasse trasseS = null;
        TRATrasse trasseL = null;
        TRATrasse trasseR = null;
        GRATrasse gradientL = null;
        GRATrasse gradientR = null;

        public TrassenPanel()
        {
            InitializeComponent();
            Resize += (s, e) => CenterElements();
        }
        private void CenterElements()
        {
            label_Trasse.Left = (ClientSize.Width - label_Trasse.Width) / 2;
        }
        private void Tb_TRA_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data != null && e.Data.GetData(typeof(TreeNode)) != null)//.GetDataPresent(DataFormats.StringFormat))
            {
                TreeNode node = (TreeNode)e.Data.GetData(typeof(TreeNode));
                string path = node.Tag.ToString();
                FileInfo fileInfo = new FileInfo(path);
                if (fileInfo.Extension.Equals(".tra", StringComparison.OrdinalIgnoreCase))
                {
                    e.Effect = DragDropEffects.Copy;
                }
                else
                {
                    e.Effect = DragDropEffects.None;
                }
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void Tb_TRA_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data != null && e.Data.GetData(typeof(TreeNode)) != null)
            {
                TreeNode node = (TreeNode)e.Data.GetData(typeof(TreeNode));
                string path = node.Tag.ToString();
                FileInfo fileInfo = new FileInfo(path);
                TextBox tb = (TextBox)sender;
                if (tb != null && fileInfo.Extension.Equals(".tra", StringComparison.OrdinalIgnoreCase))
                {
                    TreeNode previousNode = (TreeNode)tb.Tag;
                    if (previousNode != null) { previousNode.BackColor = Color.Empty; }
                    tb.Tag = node;
                    tb.Text = fileInfo.Name;
                    node.BackColor = BackColor;
                }
            }
        }

        private void Tb_GRA_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data != null && e.Data.GetData(typeof(TreeNode)) != null)
            {
                TreeNode node = (TreeNode)e.Data.GetData(typeof(TreeNode));
                string path = node.Tag.ToString();
                FileInfo fileInfo = new FileInfo(path);
                if (fileInfo.Extension.Equals(".gra", StringComparison.OrdinalIgnoreCase))
                {
                    e.Effect = DragDropEffects.Copy;
                }
                else
                {
                    e.Effect = DragDropEffects.None;
                }
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }
        private void Tb_GRA_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data != null && e.Data.GetData(typeof(TreeNode)) != null)
            {
                TreeNode node = (TreeNode)e.Data.GetData(typeof(TreeNode));
                string path = node.Tag.ToString();
                FileInfo fileInfo = new FileInfo(path);
                TextBox tb = (TextBox)sender;
                if (tb != null && fileInfo.Extension.Equals(".gra", StringComparison.OrdinalIgnoreCase))
                {
                    TreeNode previousNode = (TreeNode)tb.Tag;
                    if (previousNode != null) { previousNode.BackColor = Color.Empty; }
                    tb.Tag = node;
                    tb.Text = fileInfo.Name;
                    node.BackColor = BackColor;
                }
            }
        }

        private void tb_XRA_TextChanged(object sender, EventArgs e)
        {
            List<TextBox> textBoxes = new List<TextBox> { tb_TRA_L, tb_TRA_S, tb_TRA_R, tb_GRA_L, tb_GRA_R };
            textBoxes.Remove(sender as TextBox);
            TreeNode node = (sender as TextBox).Tag as TreeNode;
            string path = node.Tag.ToString();
            FileInfo fileInfo = new FileInfo(path);

            TreeNode[] treeNodes = node.Parent.Nodes.Cast<TreeNode>().ToArray();
            foreach (TextBox tb in textBoxes)
            {
                if (tb.Text != "") continue;
                string filename = fileInfo.Name.Remove(fileInfo.Name.Length - 5) + tb.Name.Last() + "." + tb.Name.Substring(3, 3);
                foreach (TreeNode n in treeNodes)
                {
                    if (n.Tag.ToString().EndsWith(filename, StringComparison.OrdinalIgnoreCase))
                    {
                        TreeNode previousNode = (TreeNode)tb.Tag;
                        if (previousNode != null) { previousNode.BackColor = Color.Empty; }
                        tb.Tag = n;
                        tb.Text = filename;
                        n.BackColor = BackColor;
                        return;
                    }
                }
            }
            LoadData();
        }

        private void LoadData()
        {

            FileInfo fileInfo;
            fileInfo = tb_TRA_S.Tag != null ? new FileInfo((tb_TRA_S.Tag as TreeNode).Tag.ToString()) : null;
            if (fileInfo != null && fileInfo.Exists && (trasseS == null || trasseS.Filename != fileInfo.Name))
            {
                trasseS = Trassierung.ImportTRA(fileInfo.FullName);
                if (trasseL != null) { trasseL.AssignTrasseS(trasseS); }
                if (trasseR != null) { trasseR.AssignTrasseS(trasseS); }
            }
            fileInfo = tb_GRA_L.Tag != null ? new FileInfo((tb_GRA_L.Tag as TreeNode).Tag.ToString()) : null;
            if (fileInfo != null && fileInfo.Exists && (gradientL == null || gradientL.Filename != fileInfo.Name))
            {
                (gradientL, _) = Trassierung.ImportGRA(fileInfo.FullName);
                if (trasseL != null) { trasseL.AssignGRA(gradientL); }
            }
            fileInfo = tb_GRA_R.Tag != null ? new FileInfo((tb_GRA_R.Tag as TreeNode).Tag.ToString()) : null;
            if (fileInfo != null && fileInfo.Exists && (gradientR == null || gradientR.Filename != fileInfo.Name))
            {
                (gradientR, _) = Trassierung.ImportGRA(fileInfo.FullName);
                if (trasseR != null) { trasseR.AssignGRA(gradientR); }
            }
            fileInfo = tb_TRA_L.Tag != null ? new FileInfo((tb_TRA_L.Tag as TreeNode).Tag.ToString()) : null;
            if (fileInfo != null && fileInfo.Exists && (trasseL == null || trasseL.Filename != fileInfo.Name))
            {
                trasseL = Trassierung.ImportTRA(fileInfo.FullName);
                trasseL.AssignTrasseS(trasseS);
                trasseL.AssignGRA(gradientL);
            }
            fileInfo = tb_TRA_R.Tag != null ? new FileInfo((tb_TRA_R.Tag as TreeNode).Tag.ToString()) : null;
            if (fileInfo != null && fileInfo.Exists && (trasseR == null || trasseR.Filename != fileInfo.Name))
            {
                trasseR = Trassierung.ImportTRA(fileInfo.FullName);
                trasseR.AssignTrasseS(trasseS);
                trasseR.AssignGRA(gradientR);
            }
        }

        private void btn_Interpolate_Click(object sender, EventArgs e)
        {
            if (trasseS != null)
            {
                trasseS.Interpolate(1);
#if USE_SCOTTPLOT
                trasseS.Plot();
#endif
            }
            if (trasseL != null)
            {
                trasseL.Interpolate3D(null, 10.0);
#if USE_SCOTTPLOT
                trasseL.Plot();
#endif
            }
            if (trasseR != null)
            {
                trasseR.Interpolate3D(null, 10.0);
#if USE_SCOTTPLOT
                trasseR.Plot();
#endif
            }
        }

        private void btn_Save_Click(object sender, EventArgs e)
        {
            FileInfo fileInfo = new FileInfo((tb_TRA_L.Tag as TreeNode).Tag.ToString());
            folderBrowserDialog_CSV.InitialDirectory = fileInfo.DirectoryName;
            DialogResult result = folderBrowserDialog_CSV.ShowDialog();
            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowserDialog_CSV.SelectedPath))
            {
                if (trasseL != null)
                {
                    string filename = Path.Combine(folderBrowserDialog_CSV.SelectedPath, trasseL.Filename.Substring(0, trasseL.Filename.Length - 3) + "csv");
                    try
                    {
                        using (FileStream fileStream = File.Open(filename, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite))
                        using (StreamWriter writer = new StreamWriter(fileStream))
                        {
                            trasseL.SaveCSV(writer);
                        }
                    }
                    catch (IOException)
                    {
                        MessageBox.Show("Can not write to File " + filename);
                    }
                }
                if (trasseS != null)
                {
                    string filename = Path.Combine(folderBrowserDialog_CSV.SelectedPath, trasseS.Filename.Substring(0, trasseS.Filename.Length - 3) + "csv");
                    try
                    {
                        using (FileStream fileStream = File.Open(filename, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite))
                        using (StreamWriter writer = new StreamWriter(fileStream))
                        {
                            trasseS.SaveCSV(writer);
                        }
                    }
                    catch (IOException)
                    {
                        MessageBox.Show("Can not write to File " + filename);
                    }
                }
                if (trasseR != null)
                {
                    string filename = Path.Combine(folderBrowserDialog_CSV.SelectedPath, trasseR.Filename.Substring(0, trasseR.Filename.Length - 3) + "csv");
                    try
                    {
                        using (FileStream fileStream = File.Open(filename, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite))
                        using (StreamWriter writer = new StreamWriter(fileStream))
                        {
                            trasseR.SaveCSV(writer);
                        }
                    }
                    catch (IOException)
                    {
                        MessageBox.Show("Can not write to File " + filename);
                    }
                }
            }
        }

    }
}
