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

namespace TRA.Tool
{
    public partial class TransformPanel_VA : TransformPanelBase
    {
        public TransformPanel_VA() : base()
        {
            InitializeComponent();
            //Load Input CRS
            foreach (ETransformsInput value in Enum.GetValues(typeof(ETransformsInput)))
            {
                DescriptionAttribute attribute = (DescriptionAttribute)Attribute.GetCustomAttribute(value.GetType().GetField(value.ToString()), typeof(DescriptionAttribute));
                comboBox_TransformInput.Items.Add(attribute == null ? value.ToString() : attribute.Description);
            }
            comboBox_TransformInput.SelectedIndex = 0;
            //Load Target CRS from CSV
            string relativePath = @"dbref_va_syst.csv";
            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);
            LoadOptionsFromCSV(fullPath);
        }

        private enum ETransformsInput
        {
            [Description("5682 DB_REF GK Zone 2")]
            _5682_DB_REF_GK_Zone_2,
            [Description("5683 DB_REF GK Zone 3")]
            _5683_DB_REF_GK_Zone_3,
            [Description("5684 DB_REF GK Zone 4")]
            _5684_DB_REF_GK_Zone_4,
            [Description("5685 DB_REF GK Zone 5")]
            _5685_DB_REF_GK_Zone_5,
            [Description("9932 DB_REF GK Zone 2 + GNTRANS2016 Höhen")]
            _9932_DB_REF_GK_Zone_2_GNTRANS2016_Hoehen,
            [Description("9933 DB_REF GK Zone 3 + GNTRANS2016 Höhen")]
            _9933_DB_REF_GK_Zone_3_GNTRANS2016_Hoehen,
            [Description("9934 DB_REF GK Zone 4 + GNTRANS2016 Höhen")]
            _9934_DB_REF_GK_Zone_4_GNTRANS2016_Hoehen,
            [Description("9935 DB_REF GK Zone 5 + GNTRANS2016 Höhen")]
            _9935_DB_REF_GK_Zone_5_GNTRANS2016_Hoehen,
        }

        class ComboBoxItem
        {
            public ComboBoxItem(string Name, string VA_ID)
            {
                this.Name = Name;
                this.VA_ID = VA_ID;
            }
            public string VA_ID { get; set; }
            public string Name { get; set; }
            public override string ToString()
            {
                return Name + " (" + VA_ID + ")"; // Display "Name" in the ComboBox
            }
        }

        private void LoadOptionsFromCSV(string filePath)
        {
            try
            {
                // Read all lines from the CSV file
                var lines = File.ReadAllLines(filePath).Skip(1);

                // Loop through each line and add it to the ComboBox
                foreach (string line in lines)
                {
                    // Optionally, split by delimiter for multi-column CSV files
                    string[] values = line.Split(',');

                    // Add a specific column or the full line to the ComboBox
                    ComboBoxItem item = new ComboBoxItem(values[1], values[0]);
                    comboBox_TransformOutput.Items.Add(item); // Assuming the first column contains the options
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading options: {ex.Message}");
            }
        }

        internal override TransformSetup GetTransformSetup()
        {
            TransformSetup transformSetup = new TransformSetup();
            ETransformsInput eTransformSource = (ETransformsInput)comboBox_TransformInput.SelectedIndex;
            //Get Target SRS
            switch (eTransformSource)
            {
                case ETransformsInput._5682_DB_REF_GK_Zone_2:
                    //transformSetup.singleCoordinateTransform = egbt22lib.Convert.DBRef_GK5_to_EGBT22_Local_Ell;
                    //transformSetup.arrayCoordinateTransform = egbt22lib.Convert.DBRef_GK5_to_EGBT22_Local_Ell;
                    //transformSetup.singleIn_Gamma_k = egbt22lib.Convert.DBRef_GK5_Gamma_k;
                    //transformSetup.arrayIn_Gamma_K = egbt22lib.Convert.DBRef_GK5_Gamma_k;
                    //transformSetup.singleOut_Gamma_k = egbt22lib.Convert.EGBT22_Local_Gamma_k;
                    //transformSetup.arrayOut_Gamma_K = egbt22lib.Convert.EGBT22_Local_Gamma_k;
                    break;
                case ETransformsInput._5683_DB_REF_GK_Zone_3:
                    //transformSetup.singleCoordinateTransform = egbt22lib.Convert.EGBT22_Local_to_DBRef_GK5_Ell;
                    //transformSetup.arrayCoordinateTransform = egbt22lib.Convert.EGBT22_Local_to_DBRef_GK5_Ell;
                    //transformSetup.singleIn_Gamma_k = egbt22lib.Convert.EGBT22_Local_Gamma_k;
                    //transformSetup.arrayIn_Gamma_K = egbt22lib.Convert.EGBT22_Local_Gamma_k;
                    //transformSetup.singleOut_Gamma_k = egbt22lib.Convert.DBRef_GK5_Gamma_k;
                    //transformSetup.arrayOut_Gamma_K = egbt22lib.Convert.DBRef_GK5_Gamma_k;
                    break;
                case ETransformsInput._5684_DB_REF_GK_Zone_4:
                    //transformSetup.singleCoordinateTransform = egbt22lib.Convert.DBRef_GK5_to_ETRS89_UTM33_Ell;
                    //transformSetup.arrayCoordinateTransform = egbt22lib.Convert.DBRef_GK5_to_ETRS89_UTM33_Ell;
                    //transformSetup.singleIn_Gamma_k = egbt22lib.Convert.DBRef_GK5_Gamma_k;
                    //transformSetup.arrayIn_Gamma_K = egbt22lib.Convert.DBRef_GK5_Gamma_k;
                    //transformSetup.singleOut_Gamma_k = egbt22lib.Convert.ETRS89_UTM33_Gamma_k;
                    //transformSetup.arrayOut_Gamma_K = egbt22lib.Convert.ETRS89_UTM33_Gamma_k;
                    break;
                case ETransformsInput._5685_DB_REF_GK_Zone_5:
                    //transformSetup.singleCoordinateTransform = egbt22lib.Convert.ETRS89_UTM33_to_DBRef_GK5_Ell;
                    //transformSetup.arrayCoordinateTransform = egbt22lib.Convert.ETRS89_UTM33_to_DBRef_GK5_Ell;
                    //transformSetup.singleIn_Gamma_k = egbt22lib.Convert.ETRS89_UTM33_Gamma_k;
                    //transformSetup.arrayIn_Gamma_K = egbt22lib.Convert.ETRS89_UTM33_Gamma_k;
                    //transformSetup.singleOut_Gamma_k = egbt22lib.Convert.DBRef_GK5_Gamma_k;
                    //transformSetup.arrayOut_Gamma_K = egbt22lib.Convert.DBRef_GK5_Gamma_k;
                    break;
                case ETransformsInput._9932_DB_REF_GK_Zone_2_GNTRANS2016_Hoehen:
                    break;
                case ETransformsInput._9933_DB_REF_GK_Zone_3_GNTRANS2016_Hoehen:
                    break;
                case ETransformsInput._9934_DB_REF_GK_Zone_4_GNTRANS2016_Hoehen:
                    break;
                case ETransformsInput._9935_DB_REF_GK_Zone_5_GNTRANS2016_Hoehen:
                    break;
                default:
                    //transformSetup.singleCoordinateTransform = egbt22lib.Convert.DBRef_GK5_to_EGBT22_Local_Ell;
                    //transformSetup.arrayCoordinateTransform = egbt22lib.Convert.DBRef_GK5_to_EGBT22_Local_Ell;
                    //transformSetup.singleIn_Gamma_k = egbt22lib.Convert.DBRef_GK5_Gamma_k;
                    //transformSetup.arrayIn_Gamma_K = egbt22lib.Convert.DBRef_GK5_Gamma_k;
                    //transformSetup.singleOut_Gamma_k = egbt22lib.Convert.EGBT22_Local_Gamma_k;
                    //transformSetup.arrayOut_Gamma_K = egbt22lib.Convert.EGBT22_Local_Gamma_k;
                    break;
            }
            return transformSetup;
        }
    }
}
