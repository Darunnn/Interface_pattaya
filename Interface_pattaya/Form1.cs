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
                    // ให้ UI ยังคงทำงานได้แม้ config ล้มเหลว
                    _appConfig = new AppConfig(); // สร้าง config default
                }

                // Initialize logger
                _logger = new LogManager();
                _logger.LogInfo("=== Application Starting ===");

                // Initialize data service
                if (_appConfig != null && !string.IsNullOrEmpty(_appConfig.ConnectionString))
                {
                    _dataService = new DataService(_appConfig.ConnectionString, AppConfig.ApiEndpoint);
                }

                // Set initial UI state
                UpdateUIState();

                // Display configuration summary
                if (_appConfig != null)
                {
                    _logger.LogInfo(_appConfig.GetConfigurationSummary());
                }

                // ลงทะเบียน Button Click Events ก่อนที่จะทำอะไร
                startStopButton.Click += StartStopButton_Click;
                settingsButton.Click += SettingsButton_Click;
                searchButton.Click += SearchButton_Click;
                refreshButton.Click += RefreshButton_Click;

                // Start connection check timer (check every 5 seconds)
                _connectionCheckTimer = new System.Windows.Forms.Timer();
                _connectionCheckTimer.Interval = 5000;
                _connectionCheckTimer.Tick += ConnectionCheckTimer_Tick;
                _connectionCheckTimer.Start();

                _logger.LogInfo("Connection check timer started");

                // ตรวจสอบการเชื่อมต่อครั้งแรกทันที
                CheckDatabaseConnection();

                // If AutoStart is enabled and database is connected, start service
                if (_appConfig != null && _appConfig.AutoStart && _isDatabaseConnected)
                {
                    Task.Delay(2000).ContinueWith(_ =>
                    {
                        this.Invoke((MethodInvoker)delegate
                        {
                            StartService();
                        });
                    });
                }

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
                        connection.Open();

                        if (!_isDatabaseConnected)
                        {
                            _isDatabaseConnected = true;
                            _logger.LogConnectDatabase(true, DateTime.Now);
                            _logger.LogInfo("Database connected successfully");

                            // Update UI on UI thread
                            this.Invoke((MethodInvoker)delegate
                            {
                                connectionStatusLabel.Text = "Database: 🟢 Connected";
                                connectionStatusLabel.ForeColor = System.Drawing.Color.Green;
                                UpdateUIState();

                                _logger.LogInfo("UI updated - database connected");

                                // If service was running or AutoStart is enabled, start it
                                if (_appConfig.AutoStart && !_isServiceRunning)
                                {
                                    _logger.LogInfo("AutoStart enabled - starting service");
                                    StartService();
                                }
                            });
                        }

                        connection.Close();
                    }
                    catch (MySqlException mySqlEx)
                    {
                        if (_isDatabaseConnected)
                        {
                            _isDatabaseConnected = false;
                            _logger.LogConnectDatabase(false, lastConnectedTime: DateTime.Now, lastDisconnectedTime: DateTime.Now);
                            _logger.LogError("MySQL connection error", mySqlEx);

                            // Update UI on UI thread
                            this.Invoke((MethodInvoker)delegate
                            {
                                connectionStatusLabel.Text = "Database: 🔴 Disconnected";
                                connectionStatusLabel.ForeColor = System.Drawing.Color.Red;
                                UpdateUIState();

                                _logger.LogInfo("UI updated - database disconnected");

                                // Stop service if running
                                if (_isServiceRunning)
                                {
                                    _logger.LogInfo("Stopping service due to database disconnection");
                                    StopService();
                                }
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error checking database connection", ex);
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

                _isServiceRunning = true;
                _cancellationTokenSource = new CancellationTokenSource();

                startStopButton.Text = "⏹ Stop";
                startStopButton.BackColor = System.Drawing.Color.FromArgb(231, 76, 60);

                _logger.LogInfo("Service started successfully");
                statusLabel.Text = "Status: ▶ Running";
                lastCheckLabel.Text = $"Last Check: {DateTime.Now:HH:mm:ss}";

                // Start background process
                Task.Run(() => ProcessDataLoop(_cancellationTokenSource.Token));
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error starting service", ex);
                ShowAutoClosingMessageBox($"ข้อผิดพลาดในการเริ่มบริการ: {ex.Message}", "ข้อผิดพลาด");
                _isServiceRunning = false;
                UpdateUIState();
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

                _isServiceRunning = false;
                _cancellationTokenSource?.Cancel();

                startStopButton.Text = "▶ Start";
                startStopButton.BackColor = System.Drawing.Color.FromArgb(52, 152, 219);

                _logger.LogInfo("Service stopped successfully");
                statusLabel.Text = "Status: ⏹ Stopped";
                UpdateUIState();
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
                // Implement search logic here
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
                // Implement refresh logic here
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
                // Implement settings dialog here
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error in SettingsButton_Click", ex);
            }
        }

        /// <summary>
        /// แสดง MessageBox ที่จะปิดอัตโนมัติหลังจาก 10 วินาที
        /// </summary>
        private void ShowAutoClosingMessageBox(string message, string title = "แจ้งเตือน", int delayMs = 10000)
        {
            try
            {
                // Create and show the message box
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

                // Set timer to auto-close
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

                // Stop service
                if (_isServiceRunning)
                {
                    _logger?.LogInfo("Stopping service before close");
                    StopService();
                }

                // Clean up timers
                _connectionCheckTimer?.Stop();
                _connectionCheckTimer?.Dispose();
                _autoMessageBoxTimer?.Stop();
                _autoMessageBoxTimer?.Dispose();

                // Clean up services
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