using System;
using System.Windows.Forms;
using Interface_pattaya.Configuration;

namespace Interface_pattaya
{
    public partial class Form1 : Form
    {
        private DatabaseConfig dbConfig;

        public Form1()
        {
            InitializeComponent();
            dbConfig = new DatabaseConfig();
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            // เปิดฟอร์ม Settings
            using (var settingsForm = new SystemSettingsForm(dbConfig))
            {
                if (settingsForm.ShowDialog() == DialogResult.OK)
                {
                    MessageBox.Show("Settings saved successfully!",
                        "Success",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
        }
    }
}