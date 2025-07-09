#if USE_EGBT22LIB
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

namespace TRA.Tool
{
    public partial class TransformPanel : TransformPanelBase
    {
        public TransformPanel() : base()
        {
            InitializeComponent();

            this.label_Panel.Text = "Transform";
            comboBox_TransformFrom.DataSource = new BindingList<string>(egbt22lib.Convert.Defined_CRS);
            comboBox_TransformFromVCS.DataSource = egbt22lib.Convert.Defined_VRS;
            comboBox_TransformTo.DataSource = new BindingList<string>(egbt22lib.Convert.Defined_CRS);
            comboBox_TransformFrom.SelectedIndex = 8;
            comboBox_TransformTo.SelectedItem = 0;
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
            string CRSFromVCS = comboBox_TransformFromVCS.SelectedItem != null ? (string)comboBox_TransformFromVCS.SelectedItem : "";
            string CRSTo = (string)comboBox_TransformTo.SelectedItem;
            string info;
            bool result = egbt22lib.Convert.GetConversion(CRSFrom, CRSTo, out transformSetup.ConvertFunc, out info, true)
                && egbt22lib.Convert.GetGammaKInsideCalculation(CRSFrom, out transformSetup.GammaK_From, true)
                && egbt22lib.Convert.GetGammaKInsideCalculation(CRSTo, out transformSetup.GammaK_To, true);
            transformSetup.Target_CRS = CRSTo;
            toolTip.SetToolTip(comboBox_TransformFrom, info);
            toolTip.SetToolTip(comboBox_TransformTo, info);
            btn_Transform.Enabled = result;
        }
    }
}
#endif