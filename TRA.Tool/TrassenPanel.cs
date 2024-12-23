using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TrassierungInterface;

namespace TRA.Tool
{
    public partial class TrassenPanel : UserControl
    {
        public TrassenPanel()
        {
            InitializeComponent();
            Resize += (s, e) => CenterElements();
        }
        private void CenterElements()
        {
            label_Trasse.Left = (ClientSize.Width - label_Trasse.Width) / 2;
            //int totalControlWidth = flowLayoutPanel.Controls.Cast<Control>().Sum(c => c.Width + flowLayoutPanel.Margin.Horizontal);
            //int availableWidth = flowLayoutPanel.ClientSize.Width;
            //if (totalControlWidth < availableWidth)
            //{
            //    int padding = (availableWidth - totalControlWidth) / 2;
            //    flowLayoutPanel.Padding = new Padding(padding, flowLayoutPanel.Padding.Top, padding, flowLayoutPanel.Padding.Bottom);
            //}
            //else
            //{
            //    flowLayoutPanel.Padding = new Padding(0);
            //}
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
                    tb.Tag = node;
                    tb.Text = fileInfo.Name;
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
                    tb.Tag = node;
                    tb.Text = fileInfo.Name;
                }
            }
        }

        private void tb_TRA_TextChanged(object sender, EventArgs e)
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
                        tb.Tag = n;
                        tb.Text = filename;
                        return;
                    }
                }
            }
        }

        private void btn_Load_Click(object sender, EventArgs e)
        {
            TRATrasse trasseS = null;
            GRATrasse gradientL = null;
            GRATrasse gradientR = null;

            FileInfo fileInfo;
            fileInfo = new FileInfo((tb_TRA_S.Tag as TreeNode).Tag.ToString());
            if (fileInfo.Exists)
            {
                trasseS = Trassierung.ImportTRA(fileInfo.FullName);
                trasseS.Interpolate(1);
                trasseS.Plot();
            }
            fileInfo = new FileInfo((tb_GRA_L.Tag as TreeNode).Tag.ToString());
            if (fileInfo.Exists)
            {
                (gradientL, _) = Trassierung.ImportGRA(fileInfo.FullName);
            }
            fileInfo = new FileInfo((tb_GRA_R.Tag as TreeNode).Tag.ToString());
            if (fileInfo.Exists)
            {
                (gradientR, _) = Trassierung.ImportGRA(fileInfo.FullName);
            }
            fileInfo = new FileInfo((tb_TRA_L.Tag as TreeNode).Tag.ToString());
            if (fileInfo.Exists)
            {
                TRATrasse trasseL = Trassierung.ImportTRA(fileInfo.FullName);
                trasseL.SetTrasseS = trasseS;
                trasseL.AssignGRA(gradientL);
                trasseL.Interpolate3D(null, 10);
                trasseL.Plot();
            }
            fileInfo = new FileInfo((tb_TRA_R.Tag as TreeNode).Tag.ToString());
            if (fileInfo.Exists)
            {
                TRATrasse trasseR = Trassierung.ImportTRA(fileInfo.FullName);
                trasseR.SetTrasseS = trasseS;
                trasseR.AssignGRA(gradientR);
                trasseR.Interpolate3D(null, 10);
                trasseR.Plot();
            }
        }
    }
}
