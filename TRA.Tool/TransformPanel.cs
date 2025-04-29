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
            comboBox_TransformFrom.DataSource = Enum.GetValues(typeof(egbt22lib.Convert.CRS));
            comboBox_TransformTo.DataSource = Enum.GetValues(typeof(egbt22lib.Convert.CRS));
            comboBox_TransformFrom.SelectedItem = egbt22lib.Convert.CRS.DB_Ref_GK5;
            comboBox_TransformTo.SelectedItem = egbt22lib.Convert.CRS.ETRS89_EGBT22_LDP;
            comboBox_TransformFrom.SelectedIndexChanged += comboBox_Transform_SelectedIndexChanged;
            comboBox_TransformTo.SelectedIndexChanged += comboBox_Transform_SelectedIndexChanged;
            comboBox_Transform_SelectedIndexChanged(this,EventArgs.Empty);
        }
        private TransformSetup transformSetup;
        internal override TransformSetup GetTransformSetup()
        {
            return transformSetup;
        }


        private void comboBox_Transform_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(comboBox_TransformFrom.SelectedItem == null || comboBox_TransformTo.SelectedItem == null) return;
            egbt22lib.Convert.CRS CRSFrom = (egbt22lib.Convert.CRS)comboBox_TransformFrom.SelectedItem;
            egbt22lib.Convert.CRS CRSTo = (egbt22lib.Convert.CRS)comboBox_TransformTo.SelectedItem;
            string info;
            bool result = egbt22lib.Convert.GetConversion(CRSFrom, CRSTo,out transformSetup.ConvertFunc, out info, true)
                && egbt22lib.Convert.GetGammaKCalculation(CRSFrom, out transformSetup.GammaK_From)
                && egbt22lib.Convert.GetGammaKCalculation(CRSTo, out transformSetup.GammaK_To);
            toolTip.SetToolTip(comboBox_TransformFrom, info);
            toolTip.SetToolTip(comboBox_TransformTo, info);
            btn_Transform.Enabled = result;
        }

        private void comboBox_Transform_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(comboBox_TransformFrom.SelectedItem == null || comboBox_TransformTo.SelectedItem == null) return;
            egbt22lib.Convert.CRS CRSFrom = (egbt22lib.Convert.CRS)comboBox_TransformFrom.SelectedItem;
            egbt22lib.Convert.CRS CRSTo = (egbt22lib.Convert.CRS)comboBox_TransformTo.SelectedItem;
            string info;
            bool result = egbt22lib.Convert.GetConversion(CRSFrom, CRSTo,out transformSetup.ConvertFunc, out info, true)
                && egbt22lib.Convert.GetGammaKCalculation(CRSFrom, out transformSetup.GammaK_From)
                && egbt22lib.Convert.GetGammaKCalculation(CRSTo, out transformSetup.GammaK_To);
            toolTip.SetToolTip(comboBox_TransformFrom, info);
            toolTip.SetToolTip(comboBox_TransformTo, info);
            btn_Transform.Enabled = result;
        }
    }
}
