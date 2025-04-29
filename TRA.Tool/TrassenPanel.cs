
using System.Data;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using TRA_Lib;

namespace TRA.Tool
{
    public partial class TrassenPanel : BasePanel
    {
        internal TRATrasse trasseS = null;
        internal TRATrasse trasseL = null;
        internal TRATrasse trasseR = null;
        internal GRATrasse gradientL = null;
        internal GRATrasse gradientR = null;

        public TrassenPanel()
        {
            InitializeComponent();
            this.label_Panel.Text = "Trasse";
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
        public void set_GRA_L_Path(TreeNode treeNode)
        {
            tb_GRA_L.Tag = treeNode;
            FileInfo fileInfo = new FileInfo(treeNode.Tag.ToString());
            tb_GRA_L.Text = fileInfo.Name;
        }
        public void set_GRA_R_Path(TreeNode treeNode)
        {
            tb_GRA_R.Tag = treeNode;
            FileInfo fileInfo = new FileInfo(treeNode.Tag.ToString());
            tb_GRA_R.Text = fileInfo.Name;
        }

        private void Tb_TRA_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data != null && e.Data.GetData(typeof(ReferenceTreeNode)) != null)//.GetDataPresent(DataFormats.StringFormat))
            {
                ReferenceTreeNode node = (ReferenceTreeNode)e.Data.GetData(typeof(ReferenceTreeNode));
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
            if (e.Data != null && e.Data.GetData(typeof(ReferenceTreeNode)) != null)
            {
                ReferenceTreeNode node = (ReferenceTreeNode)e.Data.GetData(typeof(ReferenceTreeNode));
                string path = node.Tag.ToString();
                FileInfo fileInfo = new FileInfo(path);
                TextBox tb = (TextBox)sender;
                if (tb != null && fileInfo.Extension.Equals(".tra", StringComparison.OrdinalIgnoreCase))
                {
                    ReferenceTreeNode previousNode = (ReferenceTreeNode)tb.Tag;
                    if (previousNode != null) {
                        previousNode.References.Remove(this);
                        if (previousNode.References.Count() == 0) previousNode.BackColor = Color.Empty; 
                    }
                    tb.Tag = node;
                    tb.Text = fileInfo.Name;
                    node.BackColor = BackColor;
                    node.References.Add(this);
                }
            }
        }

        private void Tb_GRA_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data != null && (e.Data.GetData(typeof(ReferenceTreeNode)) != null))
            {
                ReferenceTreeNode node = (ReferenceTreeNode)e.Data.GetData(typeof(ReferenceTreeNode));
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
            if (e.Data != null && e.Data.GetData(typeof(ReferenceTreeNode)) != null)
            {
                ReferenceTreeNode node = (ReferenceTreeNode)e.Data.GetData(typeof(ReferenceTreeNode));
                string path = node.Tag.ToString();
                FileInfo fileInfo = new FileInfo(path);
                TextBox tb = (TextBox)sender;
                if (tb != null && fileInfo.Extension.Equals(".gra", StringComparison.OrdinalIgnoreCase))
                {
                    ReferenceTreeNode previousNode = (ReferenceTreeNode)tb.Tag;
                    if (previousNode != null)
                    {
                        previousNode.References.Remove(this);
                        if (previousNode.References.Count() == 0) previousNode.BackColor = Color.Empty;
                    }
                    tb.Tag = node;
                    tb.Text = fileInfo.Name;
                    node.BackColor = BackColor;
                    node.References.Add(this);
                }
            }
        }

        private void tb_XRA_TextChanged(object sender, EventArgs e)
        {
            List<TextBox> textBoxes = new List<TextBox> { tb_TRA_L, tb_TRA_S, tb_TRA_R, tb_GRA_L, tb_GRA_R };
            textBoxes.Remove(sender as TextBox);
            ReferenceTreeNode node = (sender as TextBox).Tag as ReferenceTreeNode;
            string path = node.Tag.ToString();
            FileInfo fileInfo = new FileInfo(path);
            //Autofill
            //try to find Files following a L.TRA, S.TRA, R.TRA, L.GRA, R.GRA naming convention
            ReferenceTreeNode[] treeNodes = node.Parent.Nodes.OfType<ReferenceTreeNode>().ToArray();
            foreach (TextBox tb in textBoxes)
            {
                if (tb.Text != "") continue;
                string filename = fileInfo.Name.Remove(fileInfo.Name.Length - 5) + tb.Name.Last() + "." + tb.Name.Substring(3, 3);
                foreach (ReferenceTreeNode n in treeNodes)
                {
                    if (n.Tag.ToString().EndsWith(filename, StringComparison.OrdinalIgnoreCase))
                    {
                        ReferenceTreeNode previousNode = (ReferenceTreeNode)tb.Tag;
                        if (previousNode != null)
                        {
                            previousNode.References.Remove(this);
                            if (previousNode.References.Count() == 0) previousNode.BackColor = Color.Empty;
                        }
                        tb.Tag = n;
                        tb.Text = filename;
                        n.BackColor = BackColor;
                        n.References.Add(this);
                        break;
                    }
                }
            }
            //try to find Files with same name but different ending, to set TRA/GRA pairs.
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

        protected override void btn_delete_Click(object sender, EventArgs e)
        {
            ReferenceTreeNode? referenceNode = tb_TRA_L.Tag as ReferenceTreeNode;
            if (referenceNode != null)
            {
                referenceNode.References.Remove(this);
                if(referenceNode.References.Count() == 0) referenceNode.BackColor = Color.Empty;
            }
            referenceNode = tb_TRA_S.Tag as ReferenceTreeNode;
            if (referenceNode != null)
            {
                referenceNode.References.Remove(this);
                if (referenceNode.References.Count() == 0) referenceNode.BackColor = Color.Empty;
            }
            referenceNode = tb_TRA_R.Tag as ReferenceTreeNode;
            if (referenceNode != null)
            {
                referenceNode.References.Remove(this);
                if (referenceNode.References.Count() == 0) referenceNode.BackColor = Color.Empty;
            }
            referenceNode = tb_GRA_L.Tag as ReferenceTreeNode;
            if (referenceNode != null)
            {
                referenceNode.References.Remove(this);
                if (referenceNode.References.Count() == 0) referenceNode.BackColor = Color.Empty;
            }
            referenceNode = tb_GRA_R.Tag as ReferenceTreeNode;
            if (referenceNode != null)
            {
                referenceNode.References.Remove(this);
                if (referenceNode.References.Count() == 0) referenceNode.BackColor = Color.Empty;
            }
            // Call the parent class's delete function
            base.btn_delete_Click(sender, e);
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
            SaveScaleDialog ScaleDialog = new SaveScaleDialog();
            DialogResult resultScale = ScaleDialog.ShowDialog();
            Trassierung.ESaveScale saveScale = Trassierung.ESaveScale.discard;
            if (resultScale == DialogResult.OK) saveScale = ScaleDialog.result;

            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowserDialog.SelectedPath))
            {
                if (trasseL != null)
                {
                    Trassierung.ExportTRA(trasseL, Path.Combine(folderBrowserDialog.SelectedPath, trasseL.Filename), saveScale);
                }
                if (trasseS != null)
                {
                    Trassierung.ExportTRA(trasseS, Path.Combine(folderBrowserDialog.SelectedPath, trasseS.Filename), saveScale);
                }
                if (trasseR != null)
                {
                    Trassierung.ExportTRA(trasseR, Path.Combine(folderBrowserDialog.SelectedPath, trasseR.Filename), saveScale);
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
