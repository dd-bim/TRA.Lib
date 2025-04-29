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

namespace TRA.Tool
{
    public partial class TransformPanel_VA : TransformPanelBase
    {
        public TransformPanel_VA() : base()
        {
            InitializeComponent();
            //Load available Options
            List<ComboBoxItem> options = new List<ComboBoxItem>();
            foreach (valib.Convert.CRS value in Enum.GetValues(typeof(valib.Convert.CRS)))
            {
                options.Add(new ComboBoxItem(value.ToString()));
            }
            comboBox_TransformFrom.Items.AddRange(options.ToArray());
            comboBox_TransformTo.Items.AddRange(options.ToArray());
            //comboBox_TransformFrom.SelectedItem = valib.Convert.CRS.Bessel_GK2;
            //Load Target CRS from CSV
            string relativePath = @"dbref_va_syst.csv";
            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);
            LoadOptionsFromCSV(fullPath);
            comboBox_TransformFrom.SelectedIndexChanged += comboBox_Transform_SelectedIndexChanged;
            comboBox_TransformTo.SelectedIndexChanged += comboBox_Transform_SelectedIndexChanged;
            comboBox_Transform_SelectedIndexChanged(this, EventArgs.Empty);
        }

        class ComboBoxItem
        {
            public ComboBoxItem(string Name, string VA_ID = "", string fullString = "")
            {
                this.Name = Name;
                this.VA_ID = VA_ID;
                FullString = (fullString != "" ? fullString : Name);
            }
            public string VA_ID { get; set; }
            public string Name { get; set; }
            public string FullString { get; set; }
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
                    ComboBoxItem item = new ComboBoxItem(values[1], values[0],line);
                    comboBox_TransformTo.Items.Add(item); // Assuming the first column contains the options
                    comboBox_TransformFrom.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading options: {ex.Message}");
            }
        }

        private TransformSetup transformSetup;
        internal override TransformSetup GetTransformSetup()
        {
            return transformSetup;
        }
        private void comboBox_Transform_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox_TransformFrom.SelectedItem == null || comboBox_TransformTo.SelectedItem == null) return;
            ComboBoxItem CRSFrom = (ComboBoxItem)comboBox_TransformFrom.SelectedItem;
            ComboBoxItem CRSTo = (ComboBoxItem)comboBox_TransformTo.SelectedItem;
            string info;
            bool result = valib.Convert.GetConversion(CRSFrom.FullString, CRSTo.FullString, out transformSetup.ConvertFunc, out info) 
                && valib.Convert.GetGammaKCalculation(CRSFrom.FullString, out transformSetup.GammaK_From)
                && valib.Convert.GetGammaKCalculation(CRSTo.FullString, out transformSetup.GammaK_To);
            toolTip.SetToolTip(comboBox_TransformFrom, info);
            toolTip.SetToolTip(comboBox_TransformTo, info);
            btn_Transform.Enabled = result;
        }
    }
}
