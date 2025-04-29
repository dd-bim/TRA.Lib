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
            foreach (ETransforms value in Enum.GetValues(typeof(ETransforms)))
            {
                DescriptionAttribute attribute = (DescriptionAttribute)Attribute.GetCustomAttribute(value.GetType().GetField(value.ToString()), typeof(DescriptionAttribute));
                comboBox_Transform.Items.Add(attribute == null ? value.ToString() : attribute.Description);
            }
            comboBox_Transform.SelectedIndex = 0;
        }

        private enum ETransforms
        {
            [Description("DBRef_GK5 -> EGBT22_Local")]
            DBRef_GK5_to_EGBT22_Local,
            [Description("EGBT22_Local -> DBRef_GK5")]
            EGBT22_Local_to_DBRef_GK5,
            [Description("DBRef_GK5 -> ETRS89_UTM33")]
            DBRef_GK5_to_ETRS89_UTM33,
            [Description("ETRS89_UTM33 -> DBRef_GK5")]
            ETRS89_UTM33_to_DBRef_GK5
        }

        internal override TransformSetup SetupTransform()  
        {
            TransformSetup transformSetup = new TransformSetup();
            ETransforms eTransform = (ETransforms)comboBox_Transform.SelectedIndex;
            //Get Target SRS
            switch (eTransform)
            {
                case ETransforms.DBRef_GK5_to_EGBT22_Local:
                    transformSetup.singleCoordinateTransform = egbt22lib.Convert.DBRef_GK5_to_EGBT22_Local_Ell;
                    transformSetup.arrayCoordinateTransform = egbt22lib.Convert.DBRef_GK5_to_EGBT22_Local_Ell;
                    transformSetup.singleIn_Gamma_k = egbt22lib.Convert.DBRef_GK5_Gamma_k;
                    transformSetup.arrayIn_Gamma_K = egbt22lib.Convert.DBRef_GK5_Gamma_k;
                    transformSetup.singleOut_Gamma_k = egbt22lib.Convert.EGBT22_Local_Gamma_k;
                    transformSetup.arrayOut_Gamma_K = egbt22lib.Convert.EGBT22_Local_Gamma_k;
                    break;
                case ETransforms.EGBT22_Local_to_DBRef_GK5:
                    transformSetup.singleCoordinateTransform = egbt22lib.Convert.EGBT22_Local_to_DBRef_GK5_Ell;
                    transformSetup.arrayCoordinateTransform = egbt22lib.Convert.EGBT22_Local_to_DBRef_GK5_Ell;
                    transformSetup.singleIn_Gamma_k = egbt22lib.Convert.EGBT22_Local_Gamma_k;
                    transformSetup.arrayIn_Gamma_K = egbt22lib.Convert.EGBT22_Local_Gamma_k;
                    transformSetup.singleOut_Gamma_k = egbt22lib.Convert.DBRef_GK5_Gamma_k;
                    transformSetup.arrayOut_Gamma_K = egbt22lib.Convert.DBRef_GK5_Gamma_k;
                    break;
                case ETransforms.DBRef_GK5_to_ETRS89_UTM33:
                    transformSetup.singleCoordinateTransform = egbt22lib.Convert.DBRef_GK5_to_ETRS89_UTM33_Ell;
                    transformSetup.arrayCoordinateTransform = egbt22lib.Convert.DBRef_GK5_to_ETRS89_UTM33_Ell;
                    transformSetup.singleIn_Gamma_k = egbt22lib.Convert.DBRef_GK5_Gamma_k;
                    transformSetup.arrayIn_Gamma_K = egbt22lib.Convert.DBRef_GK5_Gamma_k;
                    transformSetup.singleOut_Gamma_k = egbt22lib.Convert.ETRS89_UTM33_Gamma_k;
                    transformSetup.arrayOut_Gamma_K = egbt22lib.Convert.ETRS89_UTM33_Gamma_k;
                    break;
                case ETransforms.ETRS89_UTM33_to_DBRef_GK5:
                    transformSetup.singleCoordinateTransform = egbt22lib.Convert.ETRS89_UTM33_to_DBRef_GK5_Ell;
                    transformSetup.arrayCoordinateTransform = egbt22lib.Convert.ETRS89_UTM33_to_DBRef_GK5_Ell;
                    transformSetup.singleIn_Gamma_k = egbt22lib.Convert.ETRS89_UTM33_Gamma_k;
                    transformSetup.arrayIn_Gamma_K = egbt22lib.Convert.ETRS89_UTM33_Gamma_k;
                    transformSetup.singleOut_Gamma_k = egbt22lib.Convert.DBRef_GK5_Gamma_k;
                    transformSetup.arrayOut_Gamma_K = egbt22lib.Convert.DBRef_GK5_Gamma_k;
                    break;
                default:
                    transformSetup.singleCoordinateTransform = egbt22lib.Convert.DBRef_GK5_to_EGBT22_Local_Ell;
                    transformSetup.arrayCoordinateTransform = egbt22lib.Convert.DBRef_GK5_to_EGBT22_Local_Ell;
                    transformSetup.singleIn_Gamma_k = egbt22lib.Convert.DBRef_GK5_Gamma_k;
                    transformSetup.arrayIn_Gamma_K = egbt22lib.Convert.DBRef_GK5_Gamma_k;
                    transformSetup.singleOut_Gamma_k = egbt22lib.Convert.EGBT22_Local_Gamma_k;
                    transformSetup.arrayOut_Gamma_K = egbt22lib.Convert.EGBT22_Local_Gamma_k;
                    break;
            }
            return transformSetup;
        }

  
    }
}
