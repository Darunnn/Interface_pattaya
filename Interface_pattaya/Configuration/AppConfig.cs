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
        public static string ApiEndpoint { get; private set; }
        public int ApiTimeoutSeconds { get; private set; } = 30;
        public int ApiRetryAttempts { get; private set; } = 3;
        public int ApiRetryDelaySeconds { get; private set; } = 5;

        // Processing Settings
        public int ProcessingIntervalSeconds { get; private set; } = 5;
        public int MaxProcessingBatchSize { get; private set; } = 20;
        public bool AutoStart { get; private set; } = true;

        public bool LoadConfiguration()
        {
            try
            {
                LoadConnectionString();
                LoadAppSettings();
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

            // Read and parse the connection file
            var lines = File.ReadAllLines(path);
            var connBuilder = new System.Text.StringBuilder();

            foreach (var line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    connBuilder.Append(line.Trim());
                    if (!line.Trim().EndsWith(";"))
                        connBuilder.Append(";");
                }
            }

            ConnectionString = connBuilder.ToString();

            if (string.IsNullOrWhiteSpace(ConnectionString))
            {
                throw new Exception("Connection string is empty");
            }
        }

        private void LoadAppSettings()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFolder, ConfigFile);

            // Create default config if not exists
            if (!File.Exists(path))
            {
                CreateDefaultConfig(path);
            }

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
                        ApiEndpoint = value;
                        break;
                    case "APITIMEOUTSECONDS":
                        if (int.TryParse(value, out int timeout))
                            ApiTimeoutSeconds = timeout;
                        break;
                    case "APIRETRYATTEMPTS":
                        if (int.TryParse(value, out int retry))
                            ApiRetryAttempts = retry;
                        break;
                    case "APIRETRYDELAYSECONDS":
                        if (int.TryParse(value, out int delay))
                            ApiRetryDelaySeconds = delay;
                        break;
                    case "PROCESSINGINTERVALSECONDS":
                        if (int.TryParse(value, out int interval))
                            ProcessingIntervalSeconds = interval;
                        break;
                    case "MAXPROCESSINGBATCHSIZE":
                        if (int.TryParse(value, out int batchSize))
                            MaxProcessingBatchSize = batchSize;
                        break;
                    case "AUTOSTART":
                        if (bool.TryParse(value, out bool autoStart))
                            AutoStart = autoStart;
                        break;
                }
            }

            // Validate required settings
            if (string.IsNullOrWhiteSpace(ApiEndpoint))
            {
                throw new Exception("ApiEndpoint is not configured in appsettings.ini");
            }
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
ApiEndpoint=https://localhost:8080/api/conHIS/insertPrescription
#ApiEndpoint=https://api-conhissystem.thanespgm.com/api/conHIS/insertPrescription
#ApiEndpoint=http://192.168.0.47:3001/api/conHIS/insertPrescription
ApiTimeoutSeconds=30
ApiRetryAttempts=3
ApiRetryDelaySeconds=5
# ===== PROCESSING SETTINGS =====
ProcessingIntervalSeconds=5
MaxProcessingBatchSize=20
AutoStart=true
";

            File.WriteAllText(path, defaultConfig);
        }

        // Helper method to get configuration summary
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
                    if (part.Trim().StartsWith("Server=", StringComparison.OrdinalIgnoreCase))
                        return part.Split('=')[1].Trim();
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
                    if (part.Trim().StartsWith("Database=", StringComparison.OrdinalIgnoreCase))
                        return part.Split('=')[1].Trim();
                }
            }
            catch { }
            return "Unknown";
        }
    }
}