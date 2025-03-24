namespace TRA.Tool
{
    partial class SaveScaleDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SaveScaleDialog));
            radioButton_discard = new RadioButton();
            groupBox1 = new GroupBox();
            textBox3 = new TextBox();
            radioButton_kSprung = new RadioButton();
            textBox2 = new TextBox();
            radioButton_multiply = new RadioButton();
            textBox1 = new TextBox();
            backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            textBox4 = new TextBox();
            button_OK = new Button();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // radioButton_discard
            // 
            radioButton_discard.AutoSize = true;
            radioButton_discard.Checked = true;
            radioButton_discard.Location = new Point(6, 120);
            radioButton_discard.Name = "radioButton_discard";
            radioButton_discard.Size = new Size(123, 36);
            radioButton_discard.TabIndex = 0;
            radioButton_discard.TabStop = true;
            radioButton_discard.Text = "Discard";
            radioButton_discard.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            groupBox1.AutoSize = true;
            groupBox1.Controls.Add(textBox3);
            groupBox1.Controls.Add(radioButton_kSprung);
            groupBox1.Controls.Add(textBox2);
            groupBox1.Controls.Add(radioButton_multiply);
            groupBox1.Controls.Add(textBox1);
            groupBox1.Controls.Add(radioButton_discard);
            groupBox1.Dock = DockStyle.Top;
            groupBox1.Location = new Point(0, 0);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(977, 478);
            groupBox1.TabIndex = 1;
            groupBox1.TabStop = false;
            groupBox1.Text = "Apply Scale";
            // 
            // textBox3
            // 
            textBox3.Location = new Point(158, 255);
            textBox3.Multiline = true;
            textBox3.Name = "textBox3";
            textBox3.ReadOnly = true;
            textBox3.Size = new Size(807, 132);
            textBox3.TabIndex = 5;
            textBox3.Text = "The length is multiplied by scale. The station values of the elements are not adjusted. The condition station value + L == subsequent element.station value is NOT met.";
            // 
            // radioButton_kSprung
            // 
            radioButton_kSprung.AutoSize = true;
            radioButton_kSprung.Location = new Point(12, 404);
            radioButton_kSprung.Name = "radioButton_kSprung";
            radioButton_kSprung.Size = new Size(186, 36);
            radioButton_kSprung.TabIndex = 6;
            radioButton_kSprung.Text = "Add KSprung";
            radioButton_kSprung.UseVisualStyleBackColor = true;
            // 
            // textBox2
            // 
            textBox2.Location = new Point(158, 117);
            textBox2.Multiline = true;
            textBox2.Name = "textBox2";
            textBox2.ReadOnly = true;
            textBox2.Size = new Size(807, 132);
            textBox2.TabIndex = 4;
            textBox2.Text = resources.GetString("textBox2.Text");
            // 
            // radioButton_multiply
            // 
            radioButton_multiply.AutoSize = true;
            radioButton_multiply.Location = new Point(6, 260);
            radioButton_multiply.Name = "radioButton_multiply";
            radioButton_multiply.Size = new Size(133, 36);
            radioButton_multiply.TabIndex = 3;
            radioButton_multiply.Text = "Multiply";
            radioButton_multiply.UseVisualStyleBackColor = true;
            // 
            // textBox1
            // 
            textBox1.Dock = DockStyle.Top;
            textBox1.Location = new Point(3, 35);
            textBox1.Multiline = true;
            textBox1.Name = "textBox1";
            textBox1.ReadOnly = true;
            textBox1.Size = new Size(971, 76);
            textBox1.TabIndex = 2;
            textBox1.Text = "Transformations of geometry elements may have led to a scaling of the element length. How do you wish to handle this scaling in the TRA files?";
            // 
            // textBox4
            // 
            textBox4.Location = new Point(204, 404);
            textBox4.Multiline = true;
            textBox4.Name = "textBox4";
            textBox4.ReadOnly = true;
            textBox4.Size = new Size(761, 132);
            textBox4.TabIndex = 7;
            textBox4.Text = "The length is multiplied by the scale. To satisfy the condition Stationswert + L == Folgeelement.Stationswert, additional KSprung elements are added.";
            // 
            // button_OK
            // 
            button_OK.Location = new Point(361, 610);
            button_OK.Name = "button_OK";
            button_OK.Size = new Size(230, 45);
            button_OK.TabIndex = 8;
            button_OK.Text = "OK";
            button_OK.UseVisualStyleBackColor = true;
            button_OK.Click += button_OK_Click;
            // 
            // SaveScaleDialog
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(977, 667);
            ControlBox = false;
            Controls.Add(button_OK);
            Controls.Add(textBox4);
            Controls.Add(groupBox1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Name = "SaveScaleDialog";
            Text = "SaveScaleDialog";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private RadioButton radioButton_discard;
        private GroupBox groupBox1;
        private TextBox textBox1;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private RadioButton radioButton_multiply;
        private TextBox textBox2;
        private TextBox textBox3;
        private TextBox textBox4;
        private RadioButton radioButton_kSprung;
        private Button button_OK;
    }
}