using System;
using System.IO;

namespace Interface_pattaya.Configuration
{
    public class AppConfig
    {
        private const string ConnFolder = "Connection";
        private const string ConnFile = "connectdatabase.ini";
        private const string ConfigFolder = "Config";
        private const string ConfigFile = "appsettings.ini";

        // Database Settings
        public string ConnectionString { get; private set; }

        // API Settings
        public string ApiEndpoint { get; private set; } // ✅ Fixed: Changed from static to instance property
        public int ApiTimeoutSeconds { get; private set; } = 30;
        public int ApiRetryAttempts { get; private set; } = 3;
        public int ApiRetryDelaySeconds { get; private set; } = 5;

        // Processing Settings
        public int ProcessingIntervalSeconds { get; private set; } = 5;
        public int MaxProcessingBatchSize { get; private set; } = 20;
        public bool AutoStart { get; private set; } = false; // ✅ Changed default to false for safety

        public bool LoadConfiguration()
        {
            try
            {
                LoadConnectionString();
                LoadAppSettings();
                ValidateConfiguration();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to load configuration: {ex.Message}", ex);
            }
        }

        public void ReloadConfiguration()
        {
            LoadConfiguration();
        }

        private void LoadConnectionString()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConnFolder, ConnFile);

            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Connection file not found: {path}");
            }

            var lines = File.ReadAllLines(path);
            var connBuilder = new System.Text.StringBuilder();

            foreach (var line in lines)
            {
                // ✅ Skip comments and empty lines
                if (string.IsNullOrWhiteSpace(line) || line.Trim().StartsWith("#"))
                    continue;

                connBuilder.Append(line.Trim());
                if (!line.Trim().EndsWith(";"))
                    connBuilder.Append(";");
            }

            ConnectionString = connBuilder.ToString();

            if (string.IsNullOrWhiteSpace(ConnectionString))
            {
                throw new Exception("Connection string is empty after parsing");
            }
        }

        private void LoadAppSettings()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFolder, ConfigFile);

            if (!File.Exists(path))
            {
                CreateDefaultConfig(path);
            }

            var lines = File.ReadAllLines(path);

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                    continue;

                var parts = line.Split('=');
                if (parts.Length != 2)
                    continue;

                var key = parts[0].Trim();
                var value = parts[1].Trim();

                switch (key.ToUpper())
                {
                    case "APIENDPOINT":
                        ApiEndpoint = value;
                        break;
                    case "APITIMEOUTSECONDS":
                        if (int.TryParse(value, out int timeout) && timeout > 0)
                            ApiTimeoutSeconds = timeout;
                        break;
                    case "APIRETRYATTEMPTS":
                        if (int.TryParse(value, out int retry) && retry > 0)
                            ApiRetryAttempts = retry;
                        break;
                    case "APIRETRYDELAYSECONDS":
                        if (int.TryParse(value, out int delay) && delay > 0)
                            ApiRetryDelaySeconds = delay;
                        break;
                    case "PROCESSINGINTERVALSECONDS":
                        if (int.TryParse(value, out int interval) && interval > 0)
                            ProcessingIntervalSeconds = interval;
                        break;
                    case "MAXPROCESSINGBATCHSIZE":
                        if (int.TryParse(value, out int batchSize) && batchSize > 0)
                            MaxProcessingBatchSize = batchSize;
                        break;
                    case "AUTOSTART":
                        if (bool.TryParse(value, out bool autoStart))
                            AutoStart = autoStart;
                        break;
                }
            }
        }

        // ✅ Added validation method
        private void ValidateConfiguration()
        {
            if (string.IsNullOrWhiteSpace(ConnectionString))
                throw new Exception("ConnectionString is not configured");

            if (string.IsNullOrWhiteSpace(ApiEndpoint))
                throw new Exception("ApiEndpoint is not configured in appsettings.ini");

            if (ApiTimeoutSeconds <= 0)
                throw new Exception("ApiTimeoutSeconds must be greater than 0");

            if (ApiRetryAttempts < 0)
                throw new Exception("ApiRetryAttempts must be 0 or greater");

            if (ProcessingIntervalSeconds <= 0)
                throw new Exception("ProcessingIntervalSeconds must be greater than 0");

            if (MaxProcessingBatchSize <= 0)
                throw new Exception("MaxProcessingBatchSize must be greater than 0");
        }

        private void CreateDefaultConfig(string path)
        {
            var directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var defaultConfig = @"# ===== API SETTINGS =====
# URL endpoint สำหรับส่งข้อมูลไป Drug Dispenser API
# Format: http://{HOST}/api/conHIS/insertPrescription
ApiEndpoint=http://localhost:3001/api/conHIS/insertPrescription
# ApiEndpoint=https://api-conhissystem.thanespgm.com/api/conHIS/insertPrescription
# ApiEndpoint=http://192.168.0.47:3001/api/conHIS/insertPrescription

# API timeout in seconds (default: 30)
ApiTimeoutSeconds=30

# Number of retry attempts (default: 3)
ApiRetryAttempts=3

# Delay between retries in seconds (default: 5)
ApiRetryDelaySeconds=5

# ===== PROCESSING SETTINGS =====
# Processing interval in seconds (default: 5)
ProcessingIntervalSeconds=5

# Max records to process per batch (default: 20)
MaxProcessingBatchSize=20

# Auto start service when app launches (default: false)
AutoStart=false
";

            File.WriteAllText(path, defaultConfig);
        }

        public string GetConfigurationSummary()
        {
            return $@"Configuration Summary:
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Database Connection:
  Server: {GetServerFromConnectionString()}
  Database: {GetDatabaseFromConnectionString()}

API Settings:
  Endpoint: {ApiEndpoint}
  Timeout: {ApiTimeoutSeconds} seconds
  Retry Attempts: {ApiRetryAttempts}
  Retry Delay: {ApiRetryDelaySeconds} seconds

Processing Settings:
  Interval: {ProcessingIntervalSeconds} seconds
  Batch Size: {MaxProcessingBatchSize} records
  Auto Start: {AutoStart}
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━";
        }

        private string GetServerFromConnectionString()
        {
            try
            {
                var parts = ConnectionString.Split(';');
                foreach (var part in parts)
                {
                    var trimmed = part.Trim();
                    if (trimmed.StartsWith("Server=", StringComparison.OrdinalIgnoreCase))
                        return trimmed.Substring(7);
                }
            }
            catch { }
            return "Unknown";
        }

        private string GetDatabaseFromConnectionString()
        {
            try
            {
                var parts = ConnectionString.Split(';');
                foreach (var part in parts)
                {
                    var trimmed = part.Trim();
                    if (trimmed.StartsWith("Database=", StringComparison.OrdinalIgnoreCase))
                        return trimmed.Substring(9);
                }
            }
            catch { }
            return "Unknown";
        }
    }
}