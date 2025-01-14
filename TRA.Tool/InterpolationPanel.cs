using LowDistortionProjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TRA_Lib;

namespace TRA.Tool
{
    public partial class InterpolationPanel : UserControl
    {
        public InterpolationPanel()
        {
            InitializeComponent();
        }

        private void InterpolationPanel_MouseDown(object sender, MouseEventArgs e)
        {
            DoDragDrop(this, DragDropEffects.Move);
        }

        private void btn_Interpolate_Click(object sender, EventArgs e)
        {
            //Get TRA-Files to Interpolate
            FlowLayoutPanel owner = Parent as FlowLayoutPanel;
            if (owner == null) { return; }
            int idx = owner.Controls.GetChildIndex(this) - 1;
            while (idx >= 0 && owner.Controls[idx].GetType() != typeof(InterpolationPanel))
            {
                if (owner.Controls[idx].GetType() == typeof(TrassenPanel))
                {
                    TrassenPanel panel = (TrassenPanel)owner.Controls[idx];
                    if (panel.trasseS != null)
                    {
                        panel.trasseS.Interpolate((double)num_InterpDist.Value);
#if USE_SCOTTPLOT
                        panel.trasseS.Plot();
#endif
                    }
                    if (panel.trasseL != null)
                    {
                        panel.trasseL.Interpolate3D(null, (double)num_InterpDist.Value);
#if USE_SCOTTPLOT
                        panel.trasseL.Plot();
#endif
                    }
                    if (panel.trasseR != null)
                    {
                        panel.trasseR.Interpolate3D(null, (double)num_InterpDist.Value);
#if USE_SCOTTPLOT
                        panel.trasseR.Plot();
#endif
                    }
                }
                idx--;
            }
        }
    }
}
