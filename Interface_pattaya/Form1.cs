using Interface_pattaya.Configuration;
using Interface_pattaya.Models;
using Interface_pattaya.Services;
using Interface_pattaya.utils;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        private DateTime _lastConnectedTime = DateTime.MinValue;
        private bool _wasServiceRunningBeforeDisconnect = false;
        // ⭐ DataTable & DataView for better filtering/sorting
        private DataTable _processedDataTable;
        private DataView _filteredDataView;
        private string _currentStatusFilter = "All";

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

                // ⭐ Initialize DataTable
                InitializeDataTable();

                UpdateUIState();

                _connectionCheckTimer = new System.Windows.Forms.Timer();
                _connectionCheckTimer.Interval = 3000;
                _connectionCheckTimer.Tick += ConnectionCheckTimer_Tick;
                _connectionCheckTimer.Start();

                _logger.LogInfo("Connection check timer started");

                Task.Delay(500).ContinueWith(_ => CheckDatabaseConnection());

                _ = LoadInitialDataAsync();

                _logger.LogInfo("Application initialized successfully");
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error initializing application", ex);
                ShowAutoClosingMessageBox($"ข้อผิดพลาดการเริ่มต้น: {ex.Message}", "ข้อผิดพลาด");
            }
        }

        // ⭐ Initialize DataTable with columns
        private void InitializeDataTable()
        {
            try
            {
                _processedDataTable = new DataTable();
                _processedDataTable.Columns.Add("Time Check", typeof(string));
                _processedDataTable.Columns.Add("Transaction DateTime", typeof(string));
                _processedDataTable.Columns.Add("Order No", typeof(string));
                _processedDataTable.Columns.Add("HN", typeof(string));
                _processedDataTable.Columns.Add("Patient Name", typeof(string));
                _processedDataTable.Columns.Add("Status", typeof(string));

                _filteredDataView = new DataView(_processedDataTable);

                if (dataGridView != null)
                {
                    dataGridView.DataSource = _filteredDataView;

                    // Set column widths
                    dataGridView.Columns["Time Check"].Width = 165;
                    dataGridView.Columns["Transaction DateTime"].Width = 165;
                    dataGridView.Columns["Order No"].Width = 120;
                    dataGridView.Columns["HN"].Width = 90;
                    dataGridView.Columns["Patient Name"].Width = 180;
                    dataGridView.Columns["Status"].Width = 100;

                    // Setup cell formatting event
                    dataGridView.CellFormatting += DataGridView_CellFormatting;
                }

                // ⭐ Initialize Panel Click Filters
                InitializePanelFilters();

                _logger?.LogInfo("DataTable initialized successfully");
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error initializing DataTable", ex);
            }
        }

        // ⭐ Initialize Panel Click Events for Filtering
        private void InitializePanelFilters()
        {
            try
            {
                totalPanel.Click += TotalPanel_Click;
                successPanel.Click += SuccessPanel_Click;
                failedPanel.Click += FailedPanel_Click;

                foreach (Control ctrl in totalPanel.Controls)
                {
                    if (ctrl is Label)
                    {
                        ctrl.Click += TotalPanel_Click;
                        ctrl.Cursor = Cursors.Hand;
                    }
                }
                foreach (Control ctrl in successPanel.Controls)
                {
                    if (ctrl is Label)
                    {
                        ctrl.Click += SuccessPanel_Click;
                        ctrl.Cursor = Cursors.Hand;
                    }
                }
                foreach (Control ctrl in failedPanel.Controls)
                {
                    if (ctrl is Label)
                    {
                        ctrl.Click += FailedPanel_Click;
                        ctrl.Cursor = Cursors.Hand;
                    }
                }

                totalPanel.Cursor = Cursors.Hand;
                successPanel.Cursor = Cursors.Hand;
                failedPanel.Cursor = Cursors.Hand;

                _logger?.LogInfo("Panel filters initialized");
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error initializing panel filters", ex);
            }
        }

        // ⭐ Panel Click Handlers for Status Filtering
        private void TotalPanel_Click(object sender, EventArgs e)
        {
            _currentStatusFilter = "All";
            ApplyStatusFilter();
        }

        private void SuccessPanel_Click(object sender, EventArgs e)
        {
            _currentStatusFilter = "Success";
            ApplyStatusFilter();
        }

        private void FailedPanel_Click(object sender, EventArgs e)
        {
            _currentStatusFilter = "Failed";
            ApplyStatusFilter();
        }

        // ⭐ Apply Status Filter using DataView
        private void ApplyStatusFilter()
        {
            try
            {
                if (_filteredDataView == null) return;

                if (_currentStatusFilter == "All")
                {
                    _filteredDataView.RowFilter = string.Empty;
                }
                else
                {
                    _filteredDataView.RowFilter = $"[Status] = '{_currentStatusFilter}'";
                }

                UpdateStatusFilterUI();
                UpdateSummaryCounts();

                _logger?.LogInfo($"Filter applied: {_currentStatusFilter}");
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error applying status filter", ex);
            }
        }

        // ⭐ Update Panel Border Style to show selected filter
        private void UpdateStatusFilterUI()
        {
            try
            {
                totalPanel.BorderStyle = (_currentStatusFilter == "All")
                    ? BorderStyle.Fixed3D
                    : BorderStyle.FixedSingle;

                successPanel.BorderStyle = (_currentStatusFilter == "Success")
                    ? BorderStyle.Fixed3D
                    : BorderStyle.FixedSingle;

                failedPanel.BorderStyle = (_currentStatusFilter == "Failed")
                    ? BorderStyle.Fixed3D
                    : BorderStyle.FixedSingle;

                totalPanel.Invalidate();
                successPanel.Invalidate();
                failedPanel.Invalidate();
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error updating filter UI", ex);
            }
        }

        // ⭐ DataGridView Cell Formatting for Colors
        private void DataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0 && e.RowIndex < dataGridView.Rows.Count)
                {
                    var row = dataGridView.Rows[e.RowIndex];

                    if (row.Cells["Status"].Value != null)
                    {
                        string status = row.Cells["Status"].Value.ToString();

                        if (status == "Success")
                        {
                            row.DefaultCellStyle.BackColor = System.Drawing.Color.LightGreen;
                            row.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.Green;
                        }
                        else if (status == "Failed")
                        {
                            row.DefaultCellStyle.BackColor = System.Drawing.Color.LightCoral;
                            row.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.Red;
                        }
                        else
                        {
                            row.DefaultCellStyle.BackColor = System.Drawing.Color.White;
                            row.DefaultCellStyle.SelectionBackColor = System.Drawing.SystemColors.Highlight;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error in cell formatting", ex);
            }
        }

        private async Task LoadInitialDataAsync()
        {
            try
            {
                _logger?.LogInfo("⏳ Loading initial data...");
                await Task.Delay(2000);
                await LoadDataGridViewAsync(DateTime.Now.ToString("yyyy-MM-dd"), ""); // ส่ง "" เป็นค่าเริ่มต้น
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

                    // ✅ เปลี่ยนสถานะจาก Disconnected → Connected
                    if (!_isDatabaseConnected)
                    {
                        _isDatabaseConnected = true;
                        _logger?.LogInfo("✅ Database connected successfully");
                        _logger.LogConnectDatabase(true, DateTime.Now);

                        if (this.InvokeRequired)
                        {
                            this.Invoke((MethodInvoker)delegate
                            {
                                UpdateDatabaseConnectedUI(); // จะอัพเดทเวลาที่นี่
                            });
                        }
                        else
                        {
                            UpdateDatabaseConnectedUI();
                        }
                    }
                    // ⚠️ ถ้ายังคง Connected อยู่ ไม่ต้องทำอะไร (ไม่อัพเดทเวลา)

                    connection.Close();
                }
                catch (MySqlException mySqlEx)
                {
                    _logger?.LogWarning($"❌ Database connection failed: {mySqlEx.Message}");

                    if (_isDatabaseConnected)
                    {
                        _isDatabaseConnected = false;

                        // คำนวณเวลา disconnect
                        DateTime disconnectTime = DateTime.Now;

                        _logger.LogConnectDatabase(false, _lastConnectedTime, disconnectTime);

                        if (this.InvokeRequired)
                        {
                            this.Invoke((MethodInvoker)delegate
                            {
                                UpdateDatabaseDisconnectedUI(disconnectTime);
                            });
                        }
                        else
                        {
                            UpdateDatabaseDisconnectedUI(disconnectTime);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning($"❌ Unexpected error: {ex.Message}");

                    if (_isDatabaseConnected)
                    {
                        _isDatabaseConnected = false;

                        if (this.InvokeRequired)
                        {
                            this.Invoke((MethodInvoker)delegate
                            {
                                UpdateDatabaseDisconnectedUI(DateTime.Now);
                            });
                        }
                    }
                }
            }
        }


        private void UpdateDatabaseConnectedUI()
        {
            try
            {
                _lastConnectedTime = DateTime.Now;

                connectionStatusLabel.Text = $"Database: 🟢 Connected (Last Connected: {_lastConnectedTime:yyyy-MM-dd HH:mm:ss})";
                connectionStatusLabel.ForeColor = System.Drawing.Color.Green;
                startStopButton.Enabled = true;
                startStopButton.BackColor = System.Drawing.Color.FromArgb(52, 152, 219);

                // อัพเดท Status
                if (!_isServiceRunning)
                {
                    UpdateStatus("⏹ Stopped - Ready to start");
                }

                if (_wasServiceRunningBeforeDisconnect)
                {
                    _logger?.LogInfo("🔄 Auto-restarting service after database reconnection");
                    StartService();
                    _wasServiceRunningBeforeDisconnect = false;
                }

                _logger?.LogInfo($"UI updated - database connected at {_lastConnectedTime:yyyy-MM-dd HH:mm:ss}");
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error updating connected UI", ex);
            }
        }

        private void UpdateDatabaseDisconnectedUI(DateTime disconnectTime)
        {
            try
            {
                if (_isServiceRunning)
                {
                    _wasServiceRunningBeforeDisconnect = true;
                    _logger?.LogInfo("⚠️ Service was running before disconnect - will auto-restart when reconnected");
                    StopService();
                }
                else
                {
                    _wasServiceRunningBeforeDisconnect = false;
                }

                string lastConnectInfo = _lastConnectedTime != DateTime.MinValue
                    ? $" (Last Connected: {_lastConnectedTime:yyyy-MM-dd HH:mm:ss})"
                    : "";

                connectionStatusLabel.Text = $"Database: 🔴 Disconnected (Disconnected at: {disconnectTime:yyyy-MM-dd HH:mm:ss}){lastConnectInfo}";
                connectionStatusLabel.ForeColor = System.Drawing.Color.Red;
                startStopButton.Enabled = false;
                startStopButton.BackColor = System.Drawing.Color.Gray;

                // อัพเดท Status
                UpdateStatus("🔴 Database Disconnected - Service stopped");

                _logger?.LogInfo($"UI updated - database disconnected at {disconnectTime:yyyy-MM-dd HH:mm:ss}");
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error updating disconnected UI", ex);
            }
        }
        private void UpdateStatus(string status)
        {
            if (statusLabel.InvokeRequired)
            {
                statusLabel.Invoke(new Action<string>(UpdateStatus), status);
                return;
            }
            statusLabel.Text = $"Status: {status}";
            _logger?.LogInfo($"Status: {status}");
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

                _logger?.LogInfo($"UI State Updated - DB: {_isDatabaseConnected}, Running: {_isServiceRunning}");
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
                    ShowAutoClosingMessageBox("ไม่สามารถเชื่อมต่อฐานข้อมูล", "ข้อผิดพลาด");
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
                if (_isServiceRunning) return;
                if (!_isDatabaseConnected) return;
                if (_dataService == null) return;

                _isServiceRunning = true;
                _cancellationTokenSource = new CancellationTokenSource();

                this.Invoke((MethodInvoker)delegate
                {
                    startStopButton.Text = "⏹ Stop";
                    startStopButton.BackColor = System.Drawing.Color.FromArgb(231, 76, 60);
                    UpdateStatus("▶ Running - Waiting for data...");
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
                    UpdateStatus("⏹ Stopped");
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

                    // ⭐ แสดง Status กำลังตรวจสอบ
                    this.Invoke((MethodInvoker)delegate
                    {
                        UpdateStatus($"▶ Running - Checking for new data... (Loop #{loopCount})");
                    });

                    var (successCount, failedCount, errors) = await _dataService.ProcessAndSendDataAsync();

                    int totalFound = successCount + failedCount;

                    this.Invoke((MethodInvoker)delegate
                    {
                        lastCheckLabel.Text = $"Last Check: {DateTime.Now:HH:mm:ss}";

                        // ⭐ แสดง Status ตามผลลัพธ์
                        if (totalFound > 0)
                        {
                            UpdateStatus($"▶ Running - Processed {totalFound} items ({successCount} success, {failedCount} failed)");

                            if (successCount > 0)
                            {
                                lastSuccessLabel.Text = $"Last Success: {DateTime.Now:HH:mm:ss} ({successCount} items)";
                            }

                            lastFoundLabel.Text = $"Last Found: {totalFound} items";
                        }
                        else
                        {
                            UpdateStatus($"▶ Running - No new data found");
                        }

                        // โหลดข้อมูลใหม่
                        Task.Run(() => LoadDataGridViewAsync(DateTime.Now.ToString("yyyy-MM-dd")));
                    });

                    foreach (var error in errors)
                    {
                        _logger.LogWarning(error);
                    }

                    _logger.LogInfo($"Loop #{loopCount} Complete: {successCount} success, {failedCount} failed");

                    // ⭐ แสดง Status รอรอบถัดไป
                    int delaySeconds = _appConfig?.ProcessingIntervalSeconds ?? 5;

                    for (int i = delaySeconds; i > 0; i--)
                    {
                        if (cancellationToken.IsCancellationRequested) break;

                        this.Invoke((MethodInvoker)delegate
                        {
                            UpdateStatus($"▶ Running - Waiting {i}s for next check...");
                        });

                        await Task.Delay(1000, cancellationToken);
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger?.LogInfo("ProcessDataLoop cancelled");
                    break;
                }
                catch (Exception ex)
                {
                    _logger?.LogError("Error in process loop", ex);

                    this.Invoke((MethodInvoker)delegate
                    {
                        UpdateStatus($"⚠️ Error - Retrying in 5s...");
                    });

                    await Task.Delay(5000, cancellationToken);
                }
            }

            _logger?.LogInfo("ProcessDataLoop ended");
        }

        private async void SearchButton_Click(object sender, EventArgs e)
        {
            try
            {
                string selectedDate = dateTimePicker.Value.ToString("yyyy-MM-dd");
                string searchText = searchTextBox.Text.Trim();

                _logger?.LogInfo($"🔍 Search initiated - Date: {selectedDate}, Search: '{searchText}'");

                UpdateStatus($"🔍 Searching for '{searchText}' on {selectedDate}...");

                await DebugDatabaseQuery(selectedDate);
                await LoadDataGridViewAsync(selectedDate, searchText);

                UpdateStatus($"✅ Search completed");
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error in SearchButton_Click", ex);
                UpdateStatus("❌ Search failed");
                ShowAutoClosingMessageBox($"ข้อผิดพลาด: {ex.Message}", "ข้อผิดพลาด");
            }
        }

        private async void RefreshButton_Click(object sender, EventArgs e)
        {
            try
            {
                _logger?.LogInfo("Refresh button clicked");
                _currentStatusFilter = "All";

                searchTextBox.Clear();

                UpdateStatus("🔄 Refreshing data...");

                await LoadDataGridViewAsync(dateTimePicker.Value.ToString("yyyy-MM-dd"), "");

                UpdateStatus("✅ Data refreshed");
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error in RefreshButton_Click", ex);
                UpdateStatus("❌ Refresh failed");
                ShowAutoClosingMessageBox($"ข้อผิดพลาด: {ex.Message}", "ข้อผิดพลาด");
            }
        }

        private void SettingsButton_Click(object sender, EventArgs e)
        {
            try
            {
                _logger?.LogInfo("Settings button clicked");

                // เปิด Settings Form
                using (var settingsForm = new SettingsForm())
                {
                    var result = settingsForm.ShowDialog(this);

                    if (result == DialogResult.OK && settingsForm.SettingsChanged)
                    {
                        _logger?.LogInfo("Settings were changed, reloading configuration...");

                        // โหลดการตั้งค่าใหม่
                        _appConfig = new AppConfig();
                        if (_appConfig.LoadConfiguration())
                        {
                            _dataService = new DataService(_appConfig.ConnectionString, _appConfig.ApiEndpoint, _logger);
                            _logger?.LogInfo("Configuration reloaded successfully");

                            ShowAutoClosingMessageBox(
                                "✅ การตั้งค่าได้รับการอัพเดทแล้ว\nบางการตั้งค่าอาจต้อง Restart โปรแกรม",
                                "สำเร็จ",
                                3000
                            );

                            // ตรวจสอบการเชื่อมต่อฐานข้อมูลใหม่
                            CheckDatabaseConnection();
                        }
                        else
                        {
                            _logger?.LogError("Failed to reload configuration");
                            ShowAutoClosingMessageBox(
                                "⚠️ ไม่สามารถโหลดการตั้งค่าใหม่ได้\nกรุณาตรวจสอบไฟล์ config",
                                "คำเตือน"
                            );
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error in SettingsButton_Click", ex);
                ShowAutoClosingMessageBox($"ข้อผิดพลาด: {ex.Message}", "ข้อผิดพลาด");
            }
        }
        // ⭐ Load Data and Add to DataTable
        private async Task LoadDataGridViewAsync(string date = "", string searchText = "")
        {
            try
            {
                string queryDate = string.IsNullOrEmpty(date)
                    ? DateTime.Now.ToString("yyyyMMdd")
                    : date.Replace("-", "");

                _logger?.LogInfo($"🔍 [DEBUG] Loading grid data - Input date: '{date}', Query date: '{queryDate}', Search: '{searchText}'");

                if (_dataService == null)
                {
                    _logger?.LogWarning("⚠️ DataService is not initialized");
                    return;
                }

                // ⭐ อัพเดท Status ว่ากำลังโหลด
                if (this.InvokeRequired)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        if (!_isServiceRunning) // ถ้าไม่ได้ running ให้แสดง loading
                        {
                            UpdateStatus("⏳ Loading data...");
                        }
                    });
                }
                else
                {
                    if (!_isServiceRunning)
                    {
                        UpdateStatus("⏳ Loading data...");
                    }
                }

                // ⭐ ดึงข้อมูลจากฐานข้อมูล
                var data = await _dataService.GetPrescriptionDataAsync(queryDate, searchText);

                _logger?.LogInfo($"📊 [DEBUG] Retrieved {data.Count} records from database");

                // ⭐ ต้อง Invoke เสมอเมื่ออัพเดท UI
                if (this.InvokeRequired)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        UpdateGridView(data);

                        // ⭐ อัพเดท Status หลังโหลดเสร็จ
                        if (!_isServiceRunning)
                        {
                            UpdateStatus($"✅ Loaded {data.Count} records");
                        }
                    });
                }
                else
                {
                    UpdateGridView(data);

                    if (!_isServiceRunning)
                    {
                        UpdateStatus($"✅ Loaded {data.Count} records");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError("❌ Error loading DataGridView", ex);

                if (this.InvokeRequired)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        UpdateStatus($"❌ Error loading data");
                        ShowAutoClosingMessageBox($"ข้อผิดพลาด: {ex.Message}", "ข้อผิดพลาด");
                    });
                }
                else
                {
                    UpdateStatus($"❌ Error loading data");
                    ShowAutoClosingMessageBox($"ข้อผิดพลาด: {ex.Message}", "ข้อผิดพลาด");
                }
            }
        }

        private void UpdateGridView(List<GridViewDataModel> data)
        {
            try
            {
                _logger?.LogInfo($"📝 [DEBUG] Clearing DataTable, current rows: {_processedDataTable.Rows.Count}");

                _processedDataTable.Rows.Clear();

                _logger?.LogInfo($"➕ [DEBUG] Adding {data.Count} rows to DataTable");

                int addedCount = 0;
                foreach (var item in data)
                {
                    try
                    {
                        string displayStatus = item.Status == "1" ? "Success" :
                                              (item.Status == "3" ? "Failed" : "Pending");
                        string formattedPrescriptionDate = FormatPrescriptionDate(item.Prescriptiondate);
                        _processedDataTable.Rows.Add(
                            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            formattedPrescriptionDate,
                            item.PrescriptionNo,
                            item.HN,
                            item.PatientName,
                            displayStatus
                        );

                        addedCount++;

                        if (addedCount == 1)
                        {
                            _logger?.LogInfo($"📄 [DEBUG] First row: Rx={item.PrescriptionNo}, HN={item.HN}, Status={item.Status}→{displayStatus}");
                        }
                    }
                    catch (Exception rowEx)
                    {
                        _logger?.LogError($"❌ Error adding row: Rx={item.PrescriptionNo}", rowEx);
                    }
                }

                _logger?.LogInfo($"✅ [DEBUG] Added {addedCount}/{data.Count} rows successfully");

                _filteredDataView.Sort = "[Time Check] DESC";
                UpdateSummaryCounts();

                if (dataGridView.DataSource == null)
                {
                    _logger?.LogWarning("⚠️ [DEBUG] DataGridView.DataSource is NULL, setting it now");
                    dataGridView.DataSource = _filteredDataView;
                }
                else
                {
                    dataGridView.Refresh();
                }

                // ⭐ อัพเดท Status ให้กลับไปเป็นสถานะปกติ
                if (_isServiceRunning)
                {
                    UpdateStatus($"▶ Running - Grid updated with {addedCount} records");
                }
                else
                {
                    UpdateStatus($"⏹ Stopped - Showing {addedCount} records");
                }

                _logger?.LogInfo($"✅ Grid loaded with {addedCount} rows, Total rows in table: {_processedDataTable.Rows.Count}");
            }
            catch (Exception ex)
            {
                _logger?.LogError("❌ Error in UpdateGridView", ex);
                UpdateStatus("❌ Error updating grid");
            }
        }
        // ⭐ Update Summary Counts from DataTable
        private void UpdateSummaryCounts()
        {
            try
            {
                int totalCount = _processedDataTable.Rows.Count;
                int successCount = 0;
                int failedCount = 0;

                foreach (DataRow row in _processedDataTable.Rows)
                {
                    string status = row["Status"]?.ToString() ?? "";
                    if (status == "Success") successCount++;
                    else if (status == "Failed") failedCount++;
                }

                totalCountLabel.Text = totalCount.ToString();
                successCountLabel.Text = successCount.ToString();
                failedCountLabel.Text = failedCount.ToString();

                _logger?.LogInfo($"Summary: Total={totalCount}, Success={successCount}, Failed={failedCount}");
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error updating summary counts", ex);
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

        private async Task DebugDatabaseQuery(string date)
        {
            try
            {
                string queryDate = date.Replace("-", "");
                _logger?.LogInfo($"🔍 [DEBUG CHECK] Checking database for date: {queryDate}");

                // ตรวจสอบว่ามีข้อมูลวันนี้ไหม (ไม่สนใจ status)
                string debugQuery = @"
            SELECT COUNT(*) as total_count
            FROM tb_thaneshosp_middle
            WHERE SUBSTRING(f_prescriptiondate, 1, 8) = @QueryDate";

                using (var connection = new MySqlConnection(_appConfig.ConnectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new MySqlCommand(debugQuery, connection))
                    {
                        command.Parameters.AddWithValue("@QueryDate", queryDate);
                        var totalCount = await command.ExecuteScalarAsync();
                        _logger?.LogInfo($"📊 [DEBUG] Total records for date {queryDate}: {totalCount}");
                    }

                    // ตรวจสอบว่ามีข้อมูลที่ status = 1 หรือ 3 ไหม
                    string statusQuery = @"
                SELECT 
                    COUNT(*) as count,
                    f_dispensestatus_conhis as status
                FROM tb_thaneshosp_middle
                WHERE SUBSTRING(f_prescriptiondate, 1, 8) = @QueryDate
                GROUP BY f_dispensestatus_conhis";

                    using (var command = new MySqlCommand(statusQuery, connection))
                    {
                        command.Parameters.AddWithValue("@QueryDate", queryDate);
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            _logger?.LogInfo($"📊 [DEBUG] Status breakdown:");
                            while (await reader.ReadAsync())
                            {
                                var status = reader["status"]?.ToString() ?? "NULL";
                                var count = reader["count"]?.ToString() ?? "0";
                                _logger?.LogInfo($"   Status '{status}': {count} records");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError("❌ Error in debug query", ex);
            }
        }
        private string FormatPrescriptionDate(string dateStr)
        {
            try
            {
                if (string.IsNullOrEmpty(dateStr))
                    return "";

                // ถ้ามีความยาว >= 14 ตัวอักษร (yyyyMMddHHmmss)
                if (dateStr.Length >= 14)
                {
                    string year = dateStr.Substring(0, 4);
                    string month = dateStr.Substring(4, 2);
                    string day = dateStr.Substring(6, 2);
                    string hour = dateStr.Substring(8, 2);
                    string minute = dateStr.Substring(10, 2);
                    string second = dateStr.Substring(12, 2);

                    return $"{year}-{month}-{day} {hour}:{minute}:{second}";
                }
                // ถ้ามีความยาว >= 12 ตัวอักษร (yyyyMMddHHmm)
                else if (dateStr.Length >= 12)
                {
                    string year = dateStr.Substring(0, 4);
                    string month = dateStr.Substring(4, 2);
                    string day = dateStr.Substring(6, 2);
                    string hour = dateStr.Substring(8, 2);
                    string minute = dateStr.Substring(10, 2);
                   

                    return $"{year}-{month}-{day} {hour}:{minute}:00";
                }
                // ถ้ามีความยาว >= 8 ตัวอักษร (yyyyMMdd)
                else if (dateStr.Length >= 8)
                {
                    string year = dateStr.Substring(0, 4);
                    string month = dateStr.Substring(4, 2);
                    string day = dateStr.Substring(6, 2);

                    return $"{year}-{month}-{day} 00:00:00";
                }

                return dateStr; // คืนค่าเดิมถ้าไม่ตรงรูปแบบ
            }
            catch (Exception ex)
            {
                _logger?.LogWarning($"⚠️ Error formatting date '{dateStr}': {ex.Message}");
                return dateStr;
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

        private async void ExportButton_Click(object sender, EventArgs e)
        {
            try
            {
                _logger?.LogInfo("Export button clicked");

                // ตรวจสอบว่ามีข้อมูลใน DataGridView หรือไม่
                if (dataGridView.Rows.Count == 0)
                {
                    ShowAutoClosingMessageBox("ไม่มีข้อมูลให้ Export", "แจ้งเตือน");
                    return;
                }

                // ตรวจสอบว่ามีการเลือกแถวหรือไม่
                if (dataGridView.SelectedRows.Count == 0)
                {
                    ShowAutoClosingMessageBox("กรุณาเลือกข้อมูลที่ต้องการ Export ก่อน", "แจ้งเตือน");
                    return;
                }

                // Export เฉพาะที่เลือก (ดึงข้อมูลแบบเต็มจาก database)
                await ExportSelectedRows();
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error in ExportButton_Click", ex);
                ShowAutoClosingMessageBox($"ข้อผิดพลาด: {ex.Message}", "ข้อผิดพลาด");
            }
        }

        private void ExportSelectedRows()
        {
            try
            {
                if (dataGridView.SelectedRows.Count == 0)
                {
                    ShowAutoClosingMessageBox("กรุณาเลือกข้อมูลที่ต้องการ Export", "แจ้งเตือน");
                    return;
                }

                _logger?.LogInfo($"Exporting {dataGridView.SelectedRows.Count} selected rows");

                // เปิด SaveFileDialog
                using (var saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "CSV Files (*.csv)|*.csv|Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*";
                    saveFileDialog.DefaultExt = "csv";
                    saveFileDialog.FileName = $"Export_Selected_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        string filePath = saveFileDialog.FileName;

                        // สร้าง CSV content
                        var csvContent = new StringBuilder();

                        // Header
                        var headers = new List<string>();
                        foreach (DataGridViewColumn column in dataGridView.Columns)
                        {
                            headers.Add($"\"{column.HeaderText}\"");
                        }
                        csvContent.AppendLine(string.Join(",", headers));

                        // Rows (เฉพาะที่เลือก)
                        foreach (DataGridViewRow row in dataGridView.SelectedRows)
                        {
                            var rowData = new List<string>();
                            foreach (DataGridViewCell cell in row.Cells)
                            {
                                string cellValue = cell.Value?.ToString() ?? "";
                                rowData.Add($"\"{cellValue.Replace("\"", "\"\"")}\"");
                            }
                            csvContent.AppendLine(string.Join(",", rowData));
                        }

                        // บันทึกไฟล์
                        File.WriteAllText(filePath, csvContent.ToString(), Encoding.UTF8);

                        _logger?.LogInfo($"✅ Export completed: {filePath}");
                        ShowAutoClosingMessageBox(
                            $"✅ Export สำเร็จ!\n\nจำนวน: {dataGridView.SelectedRows.Count} รายการ\nบันทึกที่: {filePath}",
                            "สำเร็จ",
                            3000
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error exporting selected rows", ex);
                ShowAutoClosingMessageBox($"ข้อผิดพลาดในการ Export: {ex.Message}", "ข้อผิดพลาด");
            }
        }
    }
}