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
using static SkiaSharp.HarfBuzz.SKShaper;

namespace TRA.Tool
{
    public partial class SaveScaleDialog : Form
    {
        public Trassierung.ESaveScale result { get; private set; }
        public SaveScaleDialog()
        {
            InitializeComponent();
            result = Trassierung.ESaveScale.discard;
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            if (radioButton_discard.Checked) result = Trassierung.ESaveScale.discard;
            if (radioButton_multiply.Checked) result = Trassierung.ESaveScale.multiply;
            if (radioButton_kSprung.Checked) result = Trassierung.ESaveScale.asKSprung;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
