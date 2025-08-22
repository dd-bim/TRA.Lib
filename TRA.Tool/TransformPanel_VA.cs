#if USE_VALIB
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using OSGeo.OSR;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
using TRA_Lib;
using Microsoft.Extensions.Logging;
using System.Xml.Linq;
using System.Diagnostics;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Reflection;
using ScottPlot;
using HarfBuzzSharp;
using System.Collections;
using static TRA.Tool.TransformPanelBase;
using System.Windows.Markup;
using static System.Runtime.InteropServices.JavaScript.JSType;
using ScottPlot.Plottables;
using ScottPlot.Plottables;

namespace TRA.Tool
{
    public partial class TransformPanel_VA : TransformPanelBase
    {
        public TransformPanel_VA() : base()
        {
            InitializeComponent();
            this.label_Panel.Text = "Transform VA";
            comboBox_TransformFrom.Items.AddRange(valib.Convert.Defined_CRS);
            comboBox_TransformTo.Items.AddRange(valib.Convert.Defined_CRS);
            comboBox_TransformFrom.SelectedIndex = 1;
            comboBox_TransformFrom.SelectedIndexChanged += comboBox_Transform_SelectedIndexChanged;
            comboBox_TransformTo.SelectedIndexChanged += comboBox_Transform_SelectedIndexChanged;
            comboBox_Transform_SelectedIndexChanged(this, EventArgs.Empty);
        }

        private TransformSetup transformSetup;
        internal override TransformSetup GetTransformSetup()
        {
            return transformSetup;
        }
        private void comboBox_Transform_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox_TransformFrom.SelectedItem == null || comboBox_TransformTo.SelectedItem == null) return;
            string CRSFrom = (string)comboBox_TransformFrom.SelectedItem;
            string CRSTo = (string)comboBox_TransformTo.SelectedItem;
            string info;
            bool result = valib.Convert.GetConversion(CRSFrom, CRSTo, out transformSetup.ConvertFunc, out info) 
                && valib.Convert.GetGammaKInsideCalculation(CRSFrom, out transformSetup.GammaK_From)
                && valib.Convert.GetGammaKInsideCalculation(CRSTo, out transformSetup.GammaK_To);
            transformSetup.Target_CRS = CRSTo;
            toolTip.SetToolTip(comboBox_TransformFrom, info);
            toolTip.SetToolTip(comboBox_TransformTo, info);
            btn_Transform.Enabled = result;
        }
    }
}
#endif