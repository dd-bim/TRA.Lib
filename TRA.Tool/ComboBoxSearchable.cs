using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRA.Tool
{
    public class ComboBoxSearchable : ComboBox
    {
        private List<object> internalItems = new List<object>();
        private bool isInitialized = false;
        private bool suppressTextUpdate = false;

        public ComboBoxSearchable()
        {
            this.DropDownStyle = ComboBoxStyle.DropDown;
            this.TextUpdate += ComboBoxContainsSearch_TextUpdate;
            this.SelectionChangeCommitted += ComboBoxContainsSearch_SelectionChangeCommitted;
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            if (!isInitialized)
            {
                foreach (var item in this.Items)
                    internalItems.Add(item);
                isInitialized = true;
            }
        }

        private void ComboBoxContainsSearch_TextUpdate(object sender, EventArgs e)
        {
            if (suppressTextUpdate) return;

            string query = this.Text.ToLower();
            var matches = internalItems
                .Where(item => item.ToString().ToLower().Contains(query))
                .ToArray();

            BeginInvoke(new Action(() =>
            {
                var currentText = this.Text;
                this.Items.Clear();
                this.Items.AddRange(matches);
                this.DroppedDown = true;
                Cursor.Current = Cursors.Default;
                this.Text = currentText;
                this.SelectionStart = currentText.Length;
                this.SelectionLength = 0;
            }));
        }
        private void ComboBoxContainsSearch_SelectionChangeCommitted(object sender, EventArgs e)
        {
            // Temporarily suppress TextUpdate to avoid re-filtering
            suppressTextUpdate = true;
            BeginInvoke(new Action(() => suppressTextUpdate = false));
        }
    }

}
