using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace de4dot_gui
{
    public partial class Form1 : Form
    {
        private int index;
        private int index_;
        private string arguments;

        public Form1()
        {
            InitializeComponent();
            this.AllowDrop = true;
            this.DragEnter += new DragEventHandler(formDragEnter);
            this.DragDrop += new DragEventHandler(formDragDrop);

            if (Properties.Settings.Default.deobfPath.Length <= 0)
                tbDeobfPath.Text = Application.StartupPath + @"\de4dot.exe";

            if (Properties.Settings.Default.deobfPath64.Length <= 0)
                tbDeobfPath64.Text = Application.StartupPath + @"\de4dot64.exe";
        }

        #region "Drag&Drop Events"

        private void formDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None; // Invalid data
        }

        private void formDragDrop(object sender, DragEventArgs e)
        {
            string[] fileList = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            // Add files to our listbox and check for .exe and .dll files
            foreach (string file in fileList)
                if (file.EndsWith(".exe") || file.EndsWith(".dll"))
                    lbFilesToDeobfuscate.Items.Add(file);
                else
                    MessageBox.Show("Invalid file format, use .dll or .exe files", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        #endregion "Drag&Drop Events"

        #region "Listbox controls"

        public void lbFilesToDeobfuscate_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                index = lbFilesToDeobfuscate.IndexFromPoint(e.Location);
                if (index != -1)
                    contextMenuStrip1.Show(Cursor.Position.X, Cursor.Position.Y);
            }
        }

        private void lbDecryptionMethods_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                index_ = lbDecryptionMethods.IndexFromPoint(e.Location);
                if (index_ != -1)
                    contextMenuStrip2.Show(Cursor.Position.X, Cursor.Position.Y);
            }
        }

        private void toolStripMenuDeleteBtn_Click(object sender, EventArgs e)
        {
            lbFilesToDeobfuscate.Items.RemoveAt(index);
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            lbDecryptionMethods.Items.RemoveAt(index_);
        }

        private void btnAddDecryptionMethod_Click(object sender, EventArgs e)
        {
            lbDecryptionMethods.Items.Add(tbDecryptionMethod.Text);
        }

        #endregion "Listbox controls"

        #region "Settings Tab"

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog browsePath = new OpenFileDialog())
            {
                browsePath.Filter = "Executable files (*.exe)|*.exe";

                if (browsePath.ShowDialog() == DialogResult.OK)
                {
                    tbDeobfPath.Text = browsePath.FileName;
                    Properties.Settings.Default.deobfPath = browsePath.FileName;
                }
            }
        }

        private void btnBrowse64_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog browsePath = new OpenFileDialog())
            {
                browsePath.Filter = "Executable files (*.exe)|*.exe";

                if (browsePath.ShowDialog() == DialogResult.OK)
                {
                    tbDeobfPath64.Text = browsePath.FileName;
                    Properties.Settings.Default.deobfPath64 = browsePath.FileName;
                }
            }
        }

        #endregion "Settings Tab"

        //Display warning when using emulation mode for string decryption
        private void comboDeobfMethod_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboDeobfMethod.SelectedIndex == 1)
                MessageBox.Show("Using emulation will execute code, do not use this feature on unknown or pontentially malicious code!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void btnDeobfuscate_Click_1(object sender, EventArgs e)
        {
            //Add files to arguments
            string[] files = lbFilesToDeobfuscate.Items.OfType<string>().ToArray();
            if (files.Length == 0)
                MessageBox.Show("Please choose valid files for deobfuscation!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            foreach (string file in files)
                arguments += file + " ";

            //Force obfuscator Detection
            if (cbForceObf.Checked)
            {
                string obfname = Enum.GetName(typeof(obfuscatorTypes), comboObfuscators.SelectedIndex);
                arguments += "-p " + obfname + " ";
            }

            //Dont rename
            if (cbNoRenaming.Checked)
                arguments += "--dont-rename ";

            //Advanced/Manual String Decryption
            if (cbEnableAdvStrings.Checked)
            {
                if (comboDeobfMethod.SelectedIndex == -1)
                    MessageBox.Show("Choose a method for string decryption!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                if (comboDeobfMethod.SelectedIndex == 0)
                    arguments += "--strtyp delegate ";
                else
                    arguments += "--strtyp emulate ";

                //todo implement regex check to automaticly determine if a token or path is used
                string[] methods = lbDecryptionMethods.Items.OfType<string>().ToArray();
                if (methods.Length == 0)
                    MessageBox.Show("Please enter valid string decryption functions!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                foreach (string method in methods)
                    arguments += "--strtok " + method + " ";
            }

            if (cbUse64Bit.Checked)
                Process.Start(tbDeobfPath64.Text, arguments);
            else
                Process.Start(tbDeobfPath.Text, arguments);

            //MessageBox.Show(arguments);
        }

        private void cbEnableAdvStrings_CheckedChanged(object sender, EventArgs e)
        {
            bool enabled = cbEnableAdvStrings.Checked;
            comboDeobfMethod.Enabled = enabled;
            lbDecryptionMethods.Enabled = enabled;
            tbDecryptionMethod.Enabled = enabled;
            btnAddDecryptionMethod.Enabled = enabled;
        }

        private void cbForceObf_CheckedChanged(object sender, EventArgs e)
        {
            comboObfuscators.Enabled = cbForceObf.Checked;
        }
    }
}