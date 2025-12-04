using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using Interface_pattaya.Configuration;
using Interface_pattaya.Services;
using Interface_pattaya.utils;
using System.Windows.Forms;

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
                    return;
                }

                // Initialize logger
                _logger = new LogManager();

                // Initialize data service
                _dataService = new DataService(_appConfig.ConnectionString, AppConfig.ApiEndpoint);

                // Set initial UI state
                UpdateUIState();

                // Display configuration summary
                _logger.LogInfo(_appConfig.GetConfigurationSummary());

                // Start connection check timer (check every 5 seconds)
                _connectionCheckTimer = new System.Windows.Forms.Timer();
                _connectionCheckTimer.Interval = 5000;
                _connectionCheckTimer.Tick += ConnectionCheckTimer_Tick;
                _connectionCheckTimer.Start();

                // Set initial button state
                startStopButton.Click += StartStopButton_Click;
                settingsButton.Click += SettingsButton_Click;
                searchButton.Click += SearchButton_Click;
                refreshButton.Click += RefreshButton_Click;

                // If AutoStart is enabled and database is connected, start service
                if (_appConfig.AutoStart)
                {
                    Task.Delay(1000).ContinueWith(_ =>
                    {
                        if (_isDatabaseConnected)
                        {
                            StartService();
                        }
                    });
                }
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
                using (var connection = new MySqlConnection(_appConfig.ConnectionString))
                {
                    try
                    {
                        connection.Open();

                        if (!_isDatabaseConnected)
                        {
                            _isDatabaseConnected = true;
                            _logger.LogConnectDatabase(true, DateTime.Now);

                            // Update UI
                            this.Invoke((MethodInvoker)delegate
                            {
                                connectionStatusLabel.Text = "Database: 🟢 Connected";
                                connectionStatusLabel.ForeColor = System.Drawing.Color.Green;
                                UpdateUIState();

                                // If service was running or AutoStart is enabled, start it
                                if (_appConfig.AutoStart && !_isServiceRunning)
                                {
                                    StartService();
                                }
                            });
                        }
                    }
                    catch (Exception)
                    {
                        if (_isDatabaseConnected)
                        {
                            _isDatabaseConnected = false;
                            _logger.LogConnectDatabase(false, lastConnectedTime: DateTime.Now, lastDisconnectedTime: DateTime.Now);

                            // Update UI
                            this.Invoke((MethodInvoker)delegate
                            {
                                connectionStatusLabel.Text = "Database: 🔴 Disconnected";
                                connectionStatusLabel.ForeColor = System.Drawing.Color.Red;
                                UpdateUIState();

                                // Stop service if running
                                if (_isServiceRunning)
                                {
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
            // Enable/Disable start button based on connection status
            startStopButton.Enabled = _isDatabaseConnected;
            startStopButton.BackColor = _isDatabaseConnected
                ? System.Drawing.Color.FromArgb(52, 152, 219)
                : System.Drawing.Color.Gray;

            // Update status label
            statusLabel.Text = _isServiceRunning
                ? "Status: ▶ Running"
                : "Status: ⏹ Stopped";
        }

        private void StartStopButton_Click(object sender, EventArgs e)
        {
            if (!_isDatabaseConnected)
            {
                ShowAutoClosingMessageBox("ไม่สามารถเชื่อมต่อฐานข้อมูล", "ข้อผิดพลาด");
                return;
            }

            if (_isServiceRunning)
            {
                StopService();
            }
            else
            {
                StartService();
            }
        }

        private void StartService()
        {
            if (_isServiceRunning || !_isDatabaseConnected)
                return;

            try
            {
                _isServiceRunning = true;
                _cancellationTokenSource = new CancellationTokenSource();

                startStopButton.Text = "⏹ Stop";
                startStopButton.BackColor = System.Drawing.Color.FromArgb(231, 76, 60);

                _logger.LogInfo("Service started");
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
            if (!_isServiceRunning)
                return;

            try
            {
                _isServiceRunning = false;
                _cancellationTokenSource?.Cancel();

                startStopButton.Text = "▶ Start";
                startStopButton.BackColor = System.Drawing.Color.FromArgb(52, 152, 219);

                _logger.LogInfo("Service stopped");
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
            while (!cancellationToken.IsCancellationRequested && _isServiceRunning)
            {
                try
                {
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
                    await Task.Delay(_appConfig.ProcessingIntervalSeconds * 1000, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger?.LogError("Error in process loop", ex);
                    await Task.Delay(5000, cancellationToken);
                }
            }
        }

        private void SearchButton_Click(object sender, EventArgs e)
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

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            _logger?.LogInfo("Refresh button clicked");
            // Implement refresh logic here
        }

        private void SettingsButton_Click(object sender, EventArgs e)
        {
            _logger?.LogInfo("Settings button clicked");
            // Implement settings dialog here
        }

        /// <summary>
        /// แสดง MessageBox ที่จะปิดอัตโนมัติหลังจาก 10 วินาที
        /// </summary>
        private void ShowAutoClosingMessageBox(string message, string title = "แจ้งเตือน", int delayMs = 10000)
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

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                // Stop service
                if (_isServiceRunning)
                {
                    StopService();
                }

                // Clean up timers
                _connectionCheckTimer?.Stop();
                _connectionCheckTimer?.Dispose();
                _autoMessageBoxTimer?.Stop();
                _autoMessageBoxTimer?.Dispose();

                // Clean up services
                _cancellationTokenSource?.Dispose();

                _logger?.LogInfo("Application closing");
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error during form closing", ex);
            }

            base.OnFormClosing(e);
        }
    }
}