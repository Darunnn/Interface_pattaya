using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using Interface_pattaya.Configuration;
using Interface_pattaya.Services;
using Interface_pattaya.utils;
using Interface_pattaya.Models;

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
                _logger = new LogManager();
                _logger.LogInfo("=== Application Starting ===");

                _appConfig = new AppConfig();
                if (!_appConfig.LoadConfiguration())
                {
                    _logger.LogError("Failed to load configuration");
                    ShowAutoClosingMessageBox("ล้มเหลวในการโหลดการกำหนดค่า", "ข้อผิดพลาด");
                    return;
                }

                if (_appConfig != null && !string.IsNullOrEmpty(_appConfig.ConnectionString))
                {
                    _dataService = new DataService(_appConfig.ConnectionString, _appConfig.ApiEndpoint, _logger);
                    _logger.LogInfo($"DataService initialized");
                }
                else
                {
                    _logger.LogWarning("Connection string is empty or null");
                    ShowAutoClosingMessageBox("Connection string is empty", "ข้อผิดพลาด");
                    return;
                }

                if (_appConfig != null)
                {
                    _logger.LogInfo(_appConfig.GetConfigurationSummary());
                }

                UpdateUIState();

                // ✅ START CONNECTION CHECK TIMER
                _connectionCheckTimer = new System.Windows.Forms.Timer();
                _connectionCheckTimer.Interval = 3000;
                _connectionCheckTimer.Tick += ConnectionCheckTimer_Tick;
                _connectionCheckTimer.Start();

                _logger.LogInfo("Connection check timer started");

                // ✅ CHECK DATABASE IMMEDIATELY
                Task.Delay(500).ContinueWith(_ => CheckDatabaseConnection());

                // ✅ FIX: LOAD INITIAL DATA PROPERLY WITH AWAIT
                _ = LoadInitialDataAsync();

                _logger.LogInfo("Application initialized successfully");
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error initializing application", ex);
                ShowAutoClosingMessageBox($"ข้อผิดพลาดการเริ่มต้น: {ex.Message}", "ข้อผิดพลาด");
            }
        }

        // ✅ NEW METHOD: LOAD INITIAL DATA WITH PROPER ASYNC/AWAIT
        private async Task LoadInitialDataAsync()
        {
            try
            {
                _logger?.LogInfo("⏳ Loading initial data...");
                await Task.Delay(2000); // รอให้ connection และ database พร้อม
                await LoadDataGridViewAsync(DateTime.Now.ToString("yyyy-MM-dd"));
                _logger?.LogInfo("✅ Initial data loaded successfully");
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error loading initial data", ex);
            }
        }

        private void ConnectionCheckTimer_Tick(object sender, EventArgs e)
        {
            CheckDatabaseConnection();
        }

        private void CheckDatabaseConnection()
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
                        _logger?.LogInfo("✅ Database connected successfully");
                        _logger.LogConnectDatabase(true, DateTime.Now);

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
                    _logger?.LogWarning($"❌ Database connection failed: {mySqlEx.Message}");

                    if (_isDatabaseConnected)
                    {
                        _isDatabaseConnected = false;
                        _logger.LogConnectDatabase(false, DateTime.Now, DateTime.Now);

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
                    _logger?.LogWarning($"❌ Unexpected error in CheckDatabaseConnection: {ex.Message}");

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

        private void UpdateDatabaseConnectedUI()
        {
            try
            {
                connectionStatusLabel.Text = "Database: 🟢 Connected";
                connectionStatusLabel.ForeColor = System.Drawing.Color.Green;
                startStopButton.Enabled = true;
                startStopButton.BackColor = System.Drawing.Color.FromArgb(52, 152, 219);

                _logger?.LogInfo("UI updated - database connected");
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
                startStopButton.Enabled = false;
                startStopButton.BackColor = System.Drawing.Color.Gray;

                if (_isServiceRunning)
                {
                    _logger?.LogInfo("Stopping service due to database disconnection");
                    StopService();
                }

                _logger?.LogInfo("UI updated - database disconnected");
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
                startStopButton.Enabled = _isDatabaseConnected;
                startStopButton.BackColor = _isDatabaseConnected
                    ? System.Drawing.Color.FromArgb(52, 152, 219)
                    : System.Drawing.Color.Gray;

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
                    return;
                }

                if (_dataService == null)
                {
                    _logger?.LogError("DataService is not initialized");
                    return;
                }

                _isServiceRunning = true;
                _cancellationTokenSource = new CancellationTokenSource();

                this.Invoke((MethodInvoker)delegate
                {
                    startStopButton.Text = "⏹ Stop";
                    startStopButton.BackColor = System.Drawing.Color.FromArgb(231, 76, 60);
                    statusLabel.Text = "Status: ▶ Running";
                });

                _logger.LogInfo("Service started");
                Task.Run(() => ProcessDataLoop(_cancellationTokenSource.Token));
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error starting service", ex);
                _isServiceRunning = false;
                this.Invoke((MethodInvoker)delegate { UpdateUIState(); });
            }
        }

        private void StopService()
        {
            try
            {
                _isServiceRunning = false;

                if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
                {
                    _cancellationTokenSource.Cancel();
                }

                this.Invoke((MethodInvoker)delegate
                {
                    startStopButton.Text = "▶ Start";
                    startStopButton.BackColor = System.Drawing.Color.FromArgb(52, 152, 219);
                    statusLabel.Text = "Status: ⏹ Stopped";
                });

                _logger.LogInfo("Service stopped");
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
                    _logger?.LogInfo($"Processing Loop #{loopCount}");

                    var (successCount, failedCount, errors) = await _dataService.ProcessAndSendDataAsync();

                    this.Invoke((MethodInvoker)delegate
                    {
                        lastCheckLabel.Text = $"Last Check: {DateTime.Now:HH:mm:ss}";

                        if (successCount > 0)
                        {
                            lastSuccessLabel.Text = $"Last Success: {DateTime.Now:HH:mm:ss} ({successCount} items)";
                        }

                        if (successCount > 0 || failedCount > 0)
                        {
                            lastFoundLabel.Text = $"Last Found: {successCount + failedCount} items";
                        }

                        totalCountLabel.Text = (successCount + failedCount).ToString();
                        successCountLabel.Text = successCount.ToString();
                        failedCountLabel.Text = failedCount.ToString();

                        Task.Run(() => LoadDataGridViewAsync(DateTime.Now.ToString("yyyy-MM-dd")));
                    });

                    foreach (var error in errors)
                    {
                        _logger.LogWarning(error);
                    }

                    _logger.LogInfo($"Loop #{loopCount} Complete: {successCount} success, {failedCount} failed");

                    int delayMs = (_appConfig?.ProcessingIntervalSeconds ?? 5) * 1000;
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

        private async void SearchButton_Click(object sender, EventArgs e)
        {
            try
            {
                string searchValue = searchTextBox.Text.Trim();
                string selectedDate = dateTimePicker.Value.ToString("yyyy-MM-dd");

                if (string.IsNullOrEmpty(searchValue))
                {
                    _logger?.LogWarning("Search: Empty search value");
                    ShowAutoClosingMessageBox("กรุณาป้อนเลขที่ใบสั่งหรือ HN", "ข้อมูลไม่ครบ");
                    return;
                }

                _logger?.LogInfo($"Search initiated for: {searchValue}, Date: {selectedDate}");
                await LoadDataGridViewAsync(selectedDate, searchValue);
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error in SearchButton_Click", ex);
                ShowAutoClosingMessageBox($"ข้อผิดพลาด: {ex.Message}", "ข้อผิดพลาด");
            }
        }

        private async void RefreshButton_Click(object sender, EventArgs e)
        {
            try
            {
                _logger?.LogInfo("Refresh button clicked");
                await LoadDataGridViewAsync(dateTimePicker.Value.ToString("yyyy-MM-dd"));
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error in RefreshButton_Click", ex);
                ShowAutoClosingMessageBox($"ข้อผิดพลาด: {ex.Message}", "ข้อผิดพลาด");
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

        private async Task LoadDataGridViewAsync(string date = "", string searchValue = "")
        {
            try
            {
                string queryDate = string.IsNullOrEmpty(date)
          ? DateTime.Now.ToString("yyyyMMdd")
          : date.Replace("-", "");
                _logger?.LogInfo($"queryDate{queryDate}");
                _logger?.LogInfo($"Loading grid data - Date: {date}, Search: {searchValue}");

                if (_dataService == null)
                {
                    _logger?.LogWarning("DataService is not initialized");
                    return;
                }

                if (this.InvokeRequired)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        dataGridView.DataSource = null;
                        statusLabel.Text = "Status: ⏳ Loading data...";
                    });
                }

     

                var data = await _dataService.GetPrescriptionDataAsync(queryDate);

                if (!string.IsNullOrEmpty(searchValue))
                {
                    data = data.Where(d =>
                        (!string.IsNullOrEmpty(d.PrescriptionNo) && d.PrescriptionNo.Contains(searchValue)) ||
                        (!string.IsNullOrEmpty(d.HN) && d.HN.Contains(searchValue))
                    ).ToList();

                    _logger?.LogInfo($"Filter applied: Found {data.Count} matching records");
                }

                if (this.InvokeRequired)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        dataGridView.DataSource = null;
                        dataGridView.Columns.Clear();
                        dataGridView.DataSource = data;

                        ConfigureDataGridViewColumns();
                        ApplyDataGridViewFormatting(data);

                        statusLabel.Text = _isServiceRunning ? "Status: ▶ Running" : "Status: ⏹ Stopped";

                        _logger?.LogInfo($"Grid loaded with {data.Count} rows");
                    });
                }
                else
                {
                    dataGridView.DataSource = null;
                    dataGridView.Columns.Clear();
                    dataGridView.DataSource = data;

                    ConfigureDataGridViewColumns();
                    ApplyDataGridViewFormatting(data);

                    _logger?.LogInfo($"Grid loaded with {data.Count} rows");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error loading DataGridView", ex);

                if (this.InvokeRequired)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        ShowAutoClosingMessageBox($"ข้อผิดพลาดในการโหลดข้อมูล: {ex.Message}", "ข้อผิดพลาด");
                    });
                }
                else
                {
                    ShowAutoClosingMessageBox($"ข้อผิดพลาดในการโหลดข้อมูล: {ex.Message}", "ข้อผิดพลาด");
                }
            }
        }

        private void ConfigureDataGridViewColumns()
        {
            try
            {
                if (dataGridView.Columns.Count == 0) return;

                var columnMappings = new Dictionary<string, (string HeaderText, int Width)>
                {
                    { "PrescriptionNo", ("เลขที่ใบสั่ง", 120) },
                    { "Seq", ("ที่", 40) },
                    { "SeqMax", ("Max", 40) },
                    { "PatientName", ("ชื่อผู้ป่วย", 150) },
                    { "HN", ("HN", 100) },
                    { "ItemNameTH", ("ชื่อยา", 200) },
                    { "OrderQty", ("จำนวน", 60) },
                    { "OrderUnit", ("หน่วย", 70) },
                    { "Dosage", ("วิธีใช้", 100) },
                    { "Status", ("สถานะ", 80) },
                    { "Remark", ("หมายเหตุ", 150) }
                };

                foreach (DataGridViewColumn col in dataGridView.Columns)
                {
                    if (columnMappings.TryGetValue(col.Name, out var mapping))
                    {
                        col.HeaderText = mapping.HeaderText;
                        col.Width = mapping.Width;
                    }

                    if (col.Name == "Status")
                    {
                        col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    }
                }

                dataGridView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.DisplayedCells);
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error configuring columns", ex);
            }
        }

        private void ApplyDataGridViewFormatting(List<GridViewDataModel> data)
        {
            try
            {
                foreach (DataGridViewRow row in dataGridView.Rows)
                {
                    try
                    {
                        if (row.DataBoundItem is GridViewDataModel item)
                        {
                            var statusCell = row.Cells["Status"];

                            if (statusCell != null)
                            {
                                string status = item?.Status?.ToString() ?? "";

                                if (status == "1")
                                {
                                    statusCell.Style.BackColor = System.Drawing.Color.LightGreen;
                                    statusCell.Style.ForeColor = System.Drawing.Color.DarkGreen;
                                    statusCell.Value = "✅ สำเร็จ";
                                    statusCell.Style.Font = new System.Drawing.Font(
                                        dataGridView.Font.FontFamily,
                                        dataGridView.Font.Size,
                                        System.Drawing.FontStyle.Bold
                                    );
                                    row.Visible = true;
                                }
                                else if (status == "3")
                                {
                                    statusCell.Style.BackColor = System.Drawing.Color.LightCoral;
                                    statusCell.Style.ForeColor = System.Drawing.Color.DarkRed;
                                    statusCell.Value = "❌ ล้มเหลว";
                                    statusCell.Style.Font = new System.Drawing.Font(
                                        dataGridView.Font.FontFamily,
                                        dataGridView.Font.Size,
                                        System.Drawing.FontStyle.Bold
                                    );
                                    row.Visible = true;
                                }
                                else
                                {
                                    row.Visible = false;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError($"Error formatting row {row.Index}: {ex.Message}", ex);
                    }
                }

                dataGridView.Refresh();
                _logger?.LogInfo("DataGridView formatting applied successfully");
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error applying formatting", ex);
            }
        }

        private void ShowAutoClosingMessageBox(string message, string title = "แจ้งเตือน", int delayMs = 5000)
        {
            try
            {
                var messageForm = new Form
                {
                    Text = title,
                    StartPosition = FormStartPosition.CenterParent,
                    Width = 400,
                    Height = 150,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    MaximizeBox = false,
                    MinimizeBox = false,
                    ShowInTaskbar = false,
                    TopMost = true
                };

                var messageLabel = new Label
                {
                    Text = message,
                    Dock = DockStyle.Fill,
                    TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                    Padding = new Padding(10)
                };

                var okButton = new Button
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
                _logger?.LogError("Error showing message box", ex);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                _logger?.LogInfo("=== Application Closing ===");

                if (_isServiceRunning)
                {
                    _logger?.LogInfo("Stopping service");
                    StopService();
                    Thread.Sleep(1000);
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