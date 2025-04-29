
using System.Data;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace TRA.Tool
{
    public partial class BasePanel : UserControl
    {

        public BasePanel()
        {
            InitializeComponent();
        }

        private void label_Trasse_MouseDown(object sender, MouseEventArgs e)
        {
            DoDragDrop(this, DragDropEffects.Move);
        }

        protected virtual void btn_delete_Click(object sender, EventArgs e)
        {
            Parent.Controls.Remove(this);
        }
    }
}
