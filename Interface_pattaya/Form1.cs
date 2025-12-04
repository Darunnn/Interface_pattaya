using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using Interface_pattaya.Configuration;
using Interface_pattaya.Services;
using Interface_pattaya.utils;

namespace Interface_pattaya
{
    public partial class Form1 : Form
    {
        private AppConfig _appConfig;
        private LogManager _logger;
        private DataService _dataService;
        private bool _isServiceRunning = false;
        private bool _isDatabaseConnected = false;
        private CancellationTokenSource _cancellationTokenSource;
        private System.Windows.Forms.Timer _connectionCheckTimer;
        private System.Windows.Forms.Timer _autoMessageBoxTimer;

        public Form1()
        {
            InitializeComponent();
            InitializeApplication();
        }

        private void InitializeApplication()
        {
            try
            {
                // Load configuration
                _appConfig = new AppConfig();
                if (!_appConfig.LoadConfiguration())
                {
                    ShowAutoClosingMessageBox("ล้มเหลวในการโหลดการกำหนดค่า", "ข้อผิดพลาด");
                    _appConfig = new AppConfig();
                }

                // Initialize logger
                _logger = new LogManager();
                _logger.LogInfo("=== Application Starting ===");

                // Initialize data service
                if (_appConfig != null && !string.IsNullOrEmpty(_appConfig.ConnectionString))
                {
                    _dataService = new DataService(_appConfig.ConnectionString, AppConfig.ApiEndpoint);
                    _logger.LogInfo($"DataService initialized with connection string");
                }
                else
                {
                    _logger.LogWarning("Connection string is empty or null");
                }

                // Set initial UI state
                UpdateUIState();

                if (_appConfig != null)
                {
                    _logger.LogInfo(_appConfig.GetConfigurationSummary());
                }

                // Start connection check timer (check every 3 seconds for debugging)
                _connectionCheckTimer = new System.Windows.Forms.Timer();
                _connectionCheckTimer.Interval = 3000;  // ✅ ลดจาก 5000 เป็น 3000 เพื่อตรวจสอบเร็วขึ้น
                _connectionCheckTimer.Tick += ConnectionCheckTimer_Tick;
                _connectionCheckTimer.Start();

                _logger.LogInfo("Connection check timer started - checking every 3 seconds");

                // ตรวจสอบการเชื่อมต่อทันที
                Task.Delay(500).ContinueWith(_ =>
                {
                    CheckDatabaseConnection();
                });

                _logger.LogInfo("Application initialized successfully");
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error initializing application", ex);
                ShowAutoClosingMessageBox($"ข้อผิดพลาดการเริ่มต้น: {ex.Message}", "ข้อผิดพลาด");
            }
        }

        private void ConnectionCheckTimer_Tick(object sender, EventArgs e)
        {
            CheckDatabaseConnection();
        }

        private void CheckDatabaseConnection()
        {
            try
            {
                if (_appConfig == null || string.IsNullOrEmpty(_appConfig.ConnectionString))
                {
                    _logger?.LogWarning("Connection string is null or empty");
                    return;
                }

                using (var connection = new MySqlConnection(_appConfig.ConnectionString))
                {
                    try
                    {
                        _logger?.LogInfo("Attempting to connect to database...");
                        connection.Open();
                        _logger?.LogInfo("✅ Database connection successful!");

                        if (!_isDatabaseConnected)
                        {
                            _isDatabaseConnected = true;
                            _logger.LogConnectDatabase(true, DateTime.Now);
                            _logger.LogInfo("Database connected successfully - updating UI");

                            // Update UI on UI thread
                            if (this.InvokeRequired)
                            {
                                this.Invoke((MethodInvoker)delegate
                                {
                                    UpdateDatabaseConnectedUI();
                                });
                            }
                            else
                            {
                                UpdateDatabaseConnectedUI();
                            }
                        }

                        connection.Close();
                    }
                    catch (MySqlException mySqlEx)
                    {
                        _logger?.LogError($"❌ MySQL Connection Error: {mySqlEx.Message}", mySqlEx);

                        if (_isDatabaseConnected)
                        {
                            _isDatabaseConnected = false;
                            _logger.LogConnectDatabase(false, lastConnectedTime: DateTime.Now, lastDisconnectedTime: DateTime.Now);

                            // Update UI on UI thread
                            if (this.InvokeRequired)
                            {
                                this.Invoke((MethodInvoker)delegate
                                {
                                    UpdateDatabaseDisconnectedUI();
                                });
                            }
                            else
                            {
                                UpdateDatabaseDisconnectedUI();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError($"❌ Connection Error: {ex.Message}", ex);

                        if (_isDatabaseConnected)
                        {
                            _isDatabaseConnected = false;

                            if (this.InvokeRequired)
                            {
                                this.Invoke((MethodInvoker)delegate
                                {
                                    UpdateDatabaseDisconnectedUI();
                                });
                            }
                            else
                            {
                                UpdateDatabaseDisconnectedUI();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error in CheckDatabaseConnection", ex);
            }
        }

        private void UpdateDatabaseConnectedUI()
        {
            try
            {
                connectionStatusLabel.Text = "Database: 🟢 Connected";
                connectionStatusLabel.ForeColor = System.Drawing.Color.Green;

                // ✅ เปิดใช้ปุ่ม Start
                startStopButton.Enabled = true;
                startStopButton.BackColor = System.Drawing.Color.FromArgb(52, 152, 219);

                _logger?.LogInfo("UI updated - database connected, Start button enabled");
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error updating connected UI", ex);
            }
        }

        private void UpdateDatabaseDisconnectedUI()
        {
            try
            {
                connectionStatusLabel.Text = "Database: 🔴 Disconnected";
                connectionStatusLabel.ForeColor = System.Drawing.Color.Red;

                // ❌ ปิดใช้ปุ่ม Start
                startStopButton.Enabled = false;
                startStopButton.BackColor = System.Drawing.Color.Gray;

                // Stop service if running
                if (_isServiceRunning)
                {
                    _logger?.LogInfo("Stopping service due to database disconnection");
                    StopService();
                }

                _logger?.LogInfo("UI updated - database disconnected, Start button disabled");
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error updating disconnected UI", ex);
            }
        }

        private void UpdateUIState()
        {
            try
            {
                // Enable/Disable start button based on connection status
                startStopButton.Enabled = _isDatabaseConnected;
                startStopButton.BackColor = _isDatabaseConnected
                    ? System.Drawing.Color.FromArgb(52, 152, 219)
                    : System.Drawing.Color.Gray;

                // Update status label
                statusLabel.Text = _isServiceRunning
                    ? "Status: ▶ Running"
                    : "Status: ⏹ Stopped";

                _logger?.LogInfo($"UI State Updated - DB Connected: {_isDatabaseConnected}, Service Running: {_isServiceRunning}");
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error updating UI state", ex);
            }
        }

        private void StartStopButton_Click(object sender, EventArgs e)
        {
            try
            {
                _logger?.LogInfo("Start/Stop button clicked");
                _logger?.LogInfo($"Current state - isServiceRunning: {_isServiceRunning}, isDatabaseConnected: {_isDatabaseConnected}");

                if (!_isDatabaseConnected)
                {
                    _logger?.LogWarning("Cannot start - database not connected");
                    ShowAutoClosingMessageBox("ไม่สามารถเชื่อมต่อฐานข้อมูล กรุณารอสักครู่", "ข้อผิดพลาด");
                    return;
                }

                if (_isServiceRunning)
                {
                    _logger?.LogInfo("Stopping service");
                    StopService();
                }
                else
                {
                    _logger?.LogInfo("Starting service");
                    StartService();
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error in StartStopButton_Click", ex);
                ShowAutoClosingMessageBox($"ข้อผิดพลาด: {ex.Message}", "ข้อผิดพลาด");
            }
        }

        private void StartService()
        {
            try
            {
                if (_isServiceRunning)
                {
                    _logger?.LogWarning("Service is already running");
                    return;
                }

                if (!_isDatabaseConnected)
                {
                    _logger?.LogWarning("Cannot start service - database not connected");
                    ShowAutoClosingMessageBox("ฐานข้อมูลไม่เชื่อมต่อ", "ข้อผิดพลาด");
                    return;
                }

                if (_dataService == null)
                {
                    _logger?.LogError("DataService is not initialized");
                    ShowAutoClosingMessageBox("บริการข้อมูลไม่ได้รับการเริ่มต้น", "ข้อผิดพลาด");
                    return;
                }

                // ตั้งค่า flag
                _isServiceRunning = true;
                _cancellationTokenSource = new CancellationTokenSource();

                // อัปเดต UI ทันที
                this.Invoke((MethodInvoker)delegate
                {
                    startStopButton.Text = "⏹ Stop";
                    startStopButton.BackColor = System.Drawing.Color.FromArgb(231, 76, 60);
                    statusLabel.Text = "Status: ▶ Running";
                    lastCheckLabel.Text = $"Last Check: {DateTime.Now:HH:mm:ss}";
                });

                _logger.LogInfo("Service started successfully");

                // เริ่ม background process
                Task.Run(() => ProcessDataLoop(_cancellationTokenSource.Token));
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error starting service", ex);
                _isServiceRunning = false;
                this.Invoke((MethodInvoker)delegate
                {
                    UpdateUIState();
                });
                ShowAutoClosingMessageBox($"ข้อผิดพลาดในการเริ่มบริการ: {ex.Message}", "ข้อผิดพลาด");
            }
        }

        private void StopService()
        {
            try
            {
                if (!_isServiceRunning)
                {
                    _logger?.LogWarning("Service is not running");
                    return;
                }

                // ตั้งค่า flag ทันที
                _isServiceRunning = false;

                // ยกเลิก task
                if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
                {
                    _cancellationTokenSource.Cancel();
                }

                // อัปเดต UI
                this.Invoke((MethodInvoker)delegate
                {
                    startStopButton.Text = "▶ Start";
                    startStopButton.BackColor = System.Drawing.Color.FromArgb(52, 152, 219);
                    statusLabel.Text = "Status: ⏹ Stopped";
                });

                _logger.LogInfo("Service stopped successfully");
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error stopping service", ex);
            }
        }

        private async Task ProcessDataLoop(CancellationToken cancellationToken)
        {
            int loopCount = 0;
            while (!cancellationToken.IsCancellationRequested && _isServiceRunning)
            {
                try
                {
                    loopCount++;
                    _logger?.LogInfo($"ProcessDataLoop iteration {loopCount}");

                    // Process data
                    var (successCount, failedCount, errors) = await _dataService.ProcessAndSendDataAsync();

                    // Update UI
                    this.Invoke((MethodInvoker)delegate
                    {
                        lastCheckLabel.Text = $"Last Check: {DateTime.Now:HH:mm:ss}";
                        if (successCount > 0)
                        {
                            lastSuccessLabel.Text = $"Last Success: {DateTime.Now:HH:mm:ss} ({successCount} items)";
                            lastFoundLabel.Text = $"Last Found: {successCount + failedCount} items";
                        }

                        // Update summary labels
                        totalCountLabel.Text = (successCount + failedCount).ToString();
                        successCountLabel.Text = successCount.ToString();
                        failedCountLabel.Text = failedCount.ToString();
                    });

                    // Log errors if any
                    foreach (var error in errors)
                    {
                        _logger.LogWarning(error);
                    }

                    _logger.LogInfo($"Processing complete: {successCount} success, {failedCount} failed");

                    // Wait for processing interval
                    int delayMs = (_appConfig?.ProcessingIntervalSeconds ?? 30) * 1000;
                    await Task.Delay(delayMs, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    _logger?.LogInfo("ProcessDataLoop cancelled");
                    break;
                }
                catch (Exception ex)
                {
                    _logger?.LogError("Error in process loop", ex);
                    await Task.Delay(5000, cancellationToken);
                }
            }

            _logger?.LogInfo("ProcessDataLoop ended");
        }

        private void SearchButton_Click(object sender, EventArgs e)
        {
            try
            {
                string searchValue = searchTextBox.Text.Trim();
                if (string.IsNullOrEmpty(searchValue))
                {
                    ShowAutoClosingMessageBox("กรุณาป้อนเลขที่ใบสั่งหรือ HN", "ข้อมูลไม่ครบ");
                    return;
                }

                _logger?.LogInfo($"Search initiated for: {searchValue}");
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error in SearchButton_Click", ex);
            }
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            try
            {
                _logger?.LogInfo("Refresh button clicked");
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error in RefreshButton_Click", ex);
            }
        }

        private void SettingsButton_Click(object sender, EventArgs e)
        {
            try
            {
                _logger?.LogInfo("Settings button clicked");
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error in SettingsButton_Click", ex);
            }
        }

        private void ShowAutoClosingMessageBox(string message, string title = "แจ้งเตือน", int delayMs = 10000)
        {
            try
            {
                Form messageForm = new Form
                {
                    Text = title,
                    StartPosition = FormStartPosition.CenterParent,
                    AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F),
                    AutoScaleMode = AutoScaleMode.Font,
                    Width = 400,
                    Height = 200,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    MaximizeBox = false,
                    MinimizeBox = false,
                    ShowInTaskbar = false,
                    TopMost = true
                };

                Label messageLabel = new Label
                {
                    Text = message,
                    Dock = DockStyle.Fill,
                    TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                    Padding = new Padding(10)
                };

                Button okButton = new Button
                {
                    Text = "ตกลง",
                    DialogResult = DialogResult.OK,
                    Dock = DockStyle.Bottom,
                    Height = 40
                };

                messageForm.Controls.Add(messageLabel);
                messageForm.Controls.Add(okButton);
                messageForm.AcceptButton = okButton;

                _autoMessageBoxTimer = new System.Windows.Forms.Timer();
                _autoMessageBoxTimer.Interval = delayMs;
                _autoMessageBoxTimer.Tick += (s, e) =>
                {
                    _autoMessageBoxTimer.Stop();
                    messageForm.Close();
                };
                _autoMessageBoxTimer.Start();

                messageForm.ShowDialog(this);
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error showing auto-closing message box", ex);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                _logger?.LogInfo("=== Application Closing ===");

                if (_isServiceRunning)
                {
                    _logger?.LogInfo("Stopping service before close");
                    StopService();
                }

                _connectionCheckTimer?.Stop();
                _connectionCheckTimer?.Dispose();
                _autoMessageBoxTimer?.Stop();
                _autoMessageBoxTimer?.Dispose();

                _cancellationTokenSource?.Dispose();

                _logger?.LogInfo("Application closed successfully");
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error during form closing", ex);
            }

            base.OnFormClosing(e);
        }
    }
}