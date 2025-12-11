using MySql.Data.MySqlClient;
using Org.BouncyCastle.Tls;
using System;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace Interface_pattaya
{
    public partial class SettingsForm : System.Windows.Forms.Form
    {
        private const string ConnFolder = "Connection";
        private const string ConnFile = "connectdatabase.ini";
        private const string ConfigFolder = "Config";
        private const string ConfigFile = "appsettings.ini";

        public bool SettingsChanged { get; private set; }

        public SettingsForm()
        {
            InitializeComponent();
            LoadCurrentSettings();
        }

        private void LoadCurrentSettings()
        {
            try
            {
                LoadDatabaseSettings();
                LoadAPISettings();
                LoadLogSettings();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"เกิดข้อผิดพลาดในการโหลดการตั้งค่า:\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void LoadDatabaseSettings()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConnFolder, ConnFile);

         

            if (File.Exists(path))
            {
                var lines = File.ReadAllLines(path);
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    if (line.StartsWith("Server="))
                        txtServer.Text = line.Replace("Server=", "").Trim().TrimEnd(';');
                    else if (line.StartsWith("Database="))
                        txtDatabase.Text = line.Replace("Database=", "").Trim().TrimEnd(';');
                    else if (line.StartsWith("User Id="))
                        txtUserId.Text = line.Replace("User Id=", "").Trim().TrimEnd(';');
                    else if (line.StartsWith("Password="))
                        txtPassword.Text = line.Replace("Password=", "").Trim().TrimEnd(';');
                 
                }
            }
        }

        private void LoadAPISettings()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFolder, ConfigFile);

            if (File.Exists(path))
            {
                var lines = File.ReadAllLines(path);
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;

                    var parts = line.Split('=');
                    if (parts.Length != 2) continue;

                    var key = parts[0].Trim();
                    var value = parts[1].Trim();

                    switch (key.ToUpper())
                    {
                        case "APIENDPOINT":
                            txtApiEndpoint.Text = value;
                            break;
                        case "APITIMEOUTSECONDS":
                            if (int.TryParse(value, out int timeout))
                                numApiTimeout.Value = timeout;
                            break;
                        case "APIRETRYATTEMPTS":
                            if (int.TryParse(value, out int retry))
                                numApiRetry.Value = retry;
                            break;
                        case "APIRETRYDELAYSECONDS":
                            if (int.TryParse(value, out int delay))
                                numApiRetryDelay.Value = delay;
                            break;
                    }
                }
            }
        }

        private void LoadLogSettings()
        {
            var logDays = ConfigurationManager.AppSettings["LogRetentionDays"];
            if (!string.IsNullOrEmpty(logDays) && int.TryParse(logDays, out int days))
            {
                numLogRetention.Value = days;
            }
        }

        // ⭐ Test Database Connection
        private async void BtnTestConnection_Click(object sender, EventArgs e)
        {
            if (!ValidateDatabaseInputs())
                return;

            btnTestConnection.Enabled = false;
            lblConnectionStatus.Text = "⏳ Testing connection...";
            lblConnectionStatus.ForeColor = Color.Orange;

            try
            {
                // Get charset based on selected encoding


                var connectionString = $"Server={txtServer.Text.Trim()};" +
                                     $"Database={txtDatabase.Text.Trim()};" +
                                     $"User Id={txtUserId.Text.Trim()};" +
                                     $"Password={txtPassword.Text.Trim()};";
                                     

                await Task.Run(() =>
                {
                    using (var connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();

                        // Test query
                        using (var cmd = new MySqlCommand("SELECT VERSION()", connection))
                        {
                            var version = cmd.ExecuteScalar()?.ToString() ?? "Unknown";

                            this.Invoke(new Action(() =>
                            {
                                lblConnectionStatus.Text = $"✅ Connected successfully!\nMySQL Version: {version}";
                                lblConnectionStatus.ForeColor = Color.Green;

                                MessageBox.Show(
                                    $"✅ การเชื่อมต่อสำเร็จ!\n\n" +
                                    $"Server: {txtServer.Text}\n" +
                                    $"Database: {txtDatabase.Text}\n" +
                                    $"MySQL Version: {version}",
                                    "Connection Successful",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Information
                                );
                            }));
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                lblConnectionStatus.Text = $"❌ Connection failed: {ex.Message}";
                lblConnectionStatus.ForeColor = Color.Red;

                MessageBox.Show(
                    $"❌ การเชื่อมต่อล้มเหลว!\n\n{ex.Message}\n\n" +
                    $"กรุณาตรวจสอบ:\n" +
                    $"• Server address และ port\n" +
                    $"• Database name\n" +
                    $"• Username และ Password\n" +
                    $"• Encoding setting\n" +
                    $"• Network connectivity",
                    "Connection Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            finally
            {
                btnTestConnection.Enabled = true;
            }
        }



        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (!ValidateInputs())
                    return;

                var result = MessageBox.Show(
                    "คุณต้องการบันทึกการตั้งค่าทั้งหมดหรือไม่?\n\n" +
                    "📁 ไฟล์ที่จะถูกแก้ไข:\n" +
                    "   • Connection\\connectdatabase.ini\n" +
                    "   • Config\\appsettings.ini\n" +
                    "   • App.config\n\n" +
                    "⚠️ หมายเหตุ: การตั้งค่าบางอย่างอาจต้อง Restart โปรแกรม",
                    "Confirm Save",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result != DialogResult.Yes)
                    return;

                SaveDatabaseSettings();
                SaveAPISettings();
                SaveLogSettings();

                SettingsChanged = true;

                MessageBox.Show(
                    "✅ บันทึกการตั้งค่าทั้งหมดสำเร็จ!\n\n" +
                    "ไฟล์ที่ถูกอัพเดท:\n" +
                    $"✓ {Path.Combine(ConnFolder, ConnFile)}\n" +
                    $"✓ {Path.Combine(ConfigFolder, ConfigFile)}\n" +
                    "✓ App.config\n\n" +
                    "💡 การตั้งค่าจะมีผลในครั้งถัดไปที่โปรแกรมโหลดข้อมูล",
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"❌ เกิดข้อผิดพลาดในการบันทึกการตั้งค่า:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private bool ValidateDatabaseInputs()
        {
            if (string.IsNullOrWhiteSpace(txtServer.Text))
            {
                MessageBox.Show("กรุณาระบุ Server", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tabControl.SelectedTab = tabDatabase;
                txtServer.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtDatabase.Text))
            {
                MessageBox.Show("กรุณาระบุ Database", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tabControl.SelectedTab = tabDatabase;
                txtDatabase.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtUserId.Text))
            {
                MessageBox.Show("กรุณาระบุ User ID", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tabControl.SelectedTab = tabDatabase;
                txtUserId.Focus();
                return false;
            }

           

            return true;
        }

        private bool ValidateInputs()
        {
            // Validate Database Settings
            if (!ValidateDatabaseInputs())
                return false;

            // Validate API Settings
            if (string.IsNullOrWhiteSpace(txtApiEndpoint.Text))
            {
                MessageBox.Show("กรุณาระบุ API Endpoint", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tabControl.SelectedTab = tabAPI;
                txtApiEndpoint.Focus();
                return false;
            }

            if (!Uri.TryCreate(txtApiEndpoint.Text, UriKind.Absolute, out _))
            {
                MessageBox.Show("API Endpoint ไม่ถูกต้อง\nกรุณาระบุ URL ที่ถูกต้อง (เช่น https://example.com/api)",
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tabControl.SelectedTab = tabAPI;
                txtApiEndpoint.Focus();
                return false;
            }

            return true;
        }

        private void SaveDatabaseSettings()
        {
            var directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConnFolder);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var path = Path.Combine(directory, ConnFile);

            // Determine charset based on selected encoding
            

            var content = new StringBuilder();
            content.AppendLine($"Server={txtServer.Text.Trim()};");
            content.AppendLine($"Database={txtDatabase.Text.Trim()};");
            content.AppendLine($"User Id={txtUserId.Text.Trim()};");
            content.AppendLine($"Password={txtPassword.Text.Trim()};");
           

            File.WriteAllText(path, content.ToString());
        }

        private void SaveAPISettings()
        {
            var directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFolder);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var path = Path.Combine(directory, ConfigFile);

            var content = new StringBuilder();
            content.AppendLine("# ===== API SETTINGS =====");
            content.AppendLine("# URL endpoint สำหรับส่งข้อมูลไป Drug Dispenser API");
            content.AppendLine("# Format: http://{HOST}/api/conHIS/insertPrescription");
            content.AppendLine($"ApiEndpoint={txtApiEndpoint.Text.Trim()}");
            content.AppendLine($"# API timeout in seconds (default: 30)");
            content.AppendLine($"ApiTimeoutSeconds={numApiTimeout.Value}");
            content.AppendLine($"# API retry attempts when failed (default: 3)");
            content.AppendLine($"ApiRetryAttempts={numApiRetry.Value}");
            content.AppendLine($"# API retry delay in seconds (default: 5)");
            content.AppendLine($"ApiRetryDelaySeconds={numApiRetryDelay.Value}");
            content.AppendLine();
            content.AppendLine("# ===== PROCESSING SETTINGS =====");
            content.AppendLine("# ระยะเวลาในการตรวจสอบข้อมูลใหม่ (วินาที)");
            content.AppendLine("# ค่าที่แนะนำ: 15-60 วินาที");
            content.AppendLine("ProcessingIntervalSeconds=30");
            content.AppendLine("# จำนวนสูงสุดของ records ที่ประมวลผลในแต่ละรอบ");
            content.AppendLine("MaxProcessingBatchSize=50");
            content.AppendLine("# เริ่มการประมวลผลอัตโนมัติเมื่อเปิดโปรแกรม (true/false)");
            content.AppendLine("AutoStart=true");

            File.WriteAllText(path, content.ToString());
        }

        private void SaveLogSettings()
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            if (config.AppSettings.Settings["LogRetentionDays"] == null)
            {
                config.AppSettings.Settings.Add("LogRetentionDays", numLogRetention.Value.ToString());
            }
            else
            {
                config.AppSettings.Settings["LogRetentionDays"].Value = numLogRetention.Value.ToString();
            }

            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "คุณต้องการยกเลิกการเปลี่ยนแปลงหรือไม่?\n\nการตั้งค่าที่แก้ไขจะไม่ถูกบันทึก",
                "Confirm Cancel",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }
    }
}