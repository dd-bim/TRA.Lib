
using System.Data;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using TRA_Lib;

namespace TRA.Tool
{
    public partial class TrassenPanel : UserControl
    {
        internal TRATrasse trasseS = null;
        internal TRATrasse trasseL = null;
        internal TRATrasse trasseR = null;
        internal GRATrasse gradientL = null;
        internal GRATrasse gradientR = null;

        public TrassenPanel()
        {
            InitializeComponent();
        }

        public void set_TRA_L_Path(TreeNode treeNode)
        {
            tb_TRA_L.Tag = treeNode;
            FileInfo fileInfo = new FileInfo(treeNode.Tag.ToString());
            tb_TRA_L.Text = fileInfo.Name;
        }
        public void set_TRA_S_Path(TreeNode treeNode)
        {
            tb_TRA_S.Tag = treeNode;
            FileInfo fileInfo = new FileInfo(treeNode.Tag.ToString());
            tb_TRA_S.Text = fileInfo.Name;
        }
        public void set_TRA_R_Path(TreeNode treeNode)
        {
            tb_TRA_R.Tag = treeNode;
            FileInfo fileInfo = new FileInfo(treeNode.Tag.ToString());
            tb_TRA_R.Text = fileInfo.Name;
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
            //Autofill
            //try to find Files following a L.TRA, S.TRA, R.TRA, L.GRA, R.GRA naming convention
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
                        break;
                    }
                }
            }
            //try to find Files with same name but different ending, to set TRA/GRA pais.
            foreach (TextBox tb in textBoxes.FindAll(x => x.Name.EndsWith((sender as TextBox).Name.Substring(4, 4))))
            {
                if (tb.Text != "") continue;
                string filename = fileInfo.Name.Remove(fileInfo.Name.Length - 3) + tb.Name.Substring(3, 3);
                foreach (TreeNode n in treeNodes)
                {
                    if (n.Tag.ToString().EndsWith(filename, StringComparison.OrdinalIgnoreCase))
                    {
                        TreeNode previousNode = (TreeNode)tb.Tag;
                        if (previousNode != null) { previousNode.BackColor = Color.Empty; }
                        tb.Tag = n;
                        tb.Text = filename;
                        n.BackColor = BackColor;
                        break;
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
                gradientL = Trassierung.ImportGRA(fileInfo.FullName);
                if (trasseL != null) { trasseL.AssignGRA(gradientL); }
            }
            fileInfo = tb_GRA_R.Tag != null ? new FileInfo((tb_GRA_R.Tag as TreeNode).Tag.ToString()) : null;
            if (fileInfo != null && fileInfo.Exists && (gradientR == null || gradientR.Filename != fileInfo.Name))
            {
                gradientR = Trassierung.ImportGRA(fileInfo.FullName);
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

        private void btn_Save_Click(object sender, EventArgs e)
        {
            object tag = tb_TRA_L.Tag ?? tb_TRA_R.Tag ?? tb_TRA_S.Tag;
            if (tag != null)
            {
                FileInfo fileInfo = new FileInfo((tag as TreeNode).Tag.ToString());
                folderBrowserDialog.InitialDirectory = fileInfo.DirectoryName;
            }
            DialogResult result = folderBrowserDialog.ShowDialog();
            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowserDialog.SelectedPath))
            {
                if (trasseL != null)
                {
                    Trassierung.ExportTRA_CSV(trasseL, Path.Combine(folderBrowserDialog.SelectedPath, trasseL.Filename.Substring(0, trasseL.Filename.Length - 3) + "csv"));
                }
                if (trasseS != null)
                {
                    Trassierung.ExportTRA_CSV(trasseS, Path.Combine(folderBrowserDialog.SelectedPath, trasseS.Filename.Substring(0, trasseS.Filename.Length - 3) + "csv"));
                }
                if (trasseR != null)
                {
                    Trassierung.ExportTRA_CSV(trasseR, Path.Combine(folderBrowserDialog.SelectedPath, trasseR.Filename.Substring(0, trasseR.Filename.Length - 3) + "csv"));
                }
            }
        }

        private void label_Trasse_MouseDown(object sender, MouseEventArgs e)
        {
            DoDragDrop(this, DragDropEffects.Move);
        }

        private void btn_delete_Click(object sender, EventArgs e)
        {
            Parent.Controls.Remove(this);
        }

        private void btn_SaveTRA_Click(object sender, EventArgs e)
        {
            object tag = tb_TRA_L.Tag ?? tb_TRA_R.Tag ?? tb_TRA_S.Tag;
            if (tag != null)
            {
                FileInfo fileInfo = new FileInfo((tag as TreeNode).Tag.ToString());
                folderBrowserDialog.InitialDirectory = fileInfo.DirectoryName;
            }
            DialogResult result = folderBrowserDialog.ShowDialog();
            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowserDialog.SelectedPath))
            {
                if (trasseL != null)
                {
                    Trassierung.ExportTRA(trasseL, Path.Combine(folderBrowserDialog.SelectedPath, trasseL.Filename));
                }
                if (trasseS != null)
                {
                    Trassierung.ExportTRA(trasseS, Path.Combine(folderBrowserDialog.SelectedPath, trasseS.Filename));
                }
                if (trasseR != null)
                {
                    Trassierung.ExportTRA(trasseR, Path.Combine(folderBrowserDialog.SelectedPath, trasseR.Filename));
                }
                if (gradientR != null)
                {
                    Trassierung.ExportGRA(gradientR, Path.Combine(folderBrowserDialog.SelectedPath, gradientR.Filename));
                }
                if (gradientL != null)
                {
                    Trassierung.ExportGRA(gradientL, Path.Combine(folderBrowserDialog.SelectedPath, gradientL.Filename));
                }
            }
        }
    }
}
