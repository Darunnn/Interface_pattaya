using System;
using System.IO;
using System.Windows.Forms;
using Interface_pattaya.Configuration;

namespace Interface_pattaya
{
    public partial class SystemSettingsForm : Form
    {
        private DatabaseConfig dbConfig;

        public SystemSettingsForm(DatabaseConfig config)
        {
            InitializeComponent();
            dbConfig = config;
            LoadCurrentConfiguration();
        }

        private void LoadCurrentConfiguration()
        {
            try
            {
                dbConfig.LoadConfiguration();

                cmbDbType.SelectedItem = dbConfig.Provider;
                txtServer.Text = dbConfig.Server;
                txtDatabase.Text = dbConfig.Database;
                txtUserId.Text = dbConfig.UserId;
                txtPassword.Text = dbConfig.Password;
            }
            catch
            {
                // ถ้าโหลดไม่ได้ ใช้ค่า default
            }
        }

        private void btnTestConnection_Click(object sender, EventArgs e)
        {
            if (ValidateDatabaseInputs())
            {
                SaveDatabaseConfiguration();

                try
                {
                    dbConfig.ReloadConfiguration();
                    MessageBox.Show("✅ Connection test successful!\n\n" + dbConfig.GetConfigurationSummary(),
                        "Connection Test",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"❌ Connection test failed!\n\nError: {ex.Message}",
                        "Connection Test",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        private void btnBrowseLog_Click(object sender, EventArgs e)
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Select Log Folder";
                folderDialog.SelectedPath = txtLogPath.Text;

                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    txtLogPath.Text = folderDialog.SelectedPath;
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (ValidateAllInputs())
            {
                SaveAllConfiguration();
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private bool ValidateDatabaseInputs()
        {
            if (string.IsNullOrWhiteSpace(txtServer.Text))
            {
                MessageBox.Show("Please enter server name/IP.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tabControl.SelectedTab = tabDatabase;
                txtServer.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtDatabase.Text))
            {
                MessageBox.Show("Please enter database name.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tabControl.SelectedTab = tabDatabase;
                txtDatabase.Focus();
                return false;
            }

            return true;
        }

        private bool ValidateAllInputs()
        {
            if (!ValidateDatabaseInputs())
                return false;

            if (string.IsNullOrWhiteSpace(txtApiUrl.Text))
            {
                MessageBox.Show("Please enter API URL.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tabControl.SelectedTab = tabAPI;
                txtApiUrl.Focus();
                return false;
            }

            return true;
        }

        private void SaveDatabaseConfiguration()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Connection", "database.ini");
            var directory = Path.GetDirectoryName(path);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var config = $@"# ===== DATABASE CONFIGURATION =====
# Provider: SqlServer, MySQL, PostgreSQL
Provider={cmbDbType.SelectedItem}

# Server Configuration
Server={txtServer.Text}
Port=

# Database Name
Database={txtDatabase.Text}

# Authentication
UserId={txtUserId.Text}
Password={txtPassword.Text}

# Connection Settings
ConnectionTimeout=30

# Encoding
Encoding={cmbEncoding.SelectedItem}
";

            File.WriteAllText(path, config);
        }

        private void SaveAllConfiguration()
        {
            SaveDatabaseConfiguration();

            var apiPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Connection", "api.ini");
            var apiConfig = $@"# ===== API CONFIGURATION =====
ApiUrl={txtApiUrl.Text}
ApiKey={txtApiKey.Text}
Timeout={numApiTimeout.Value}
";
            File.WriteAllText(apiPath, apiConfig);

            var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Connection", "log.ini");
            var logConfig = $@"# ===== LOG CONFIGURATION =====
EnableLog={chkEnableLog.Checked}
LogPath={txtLogPath.Text}
";
            File.WriteAllText(logPath, logConfig);
        }
    }
}
