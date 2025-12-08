using Newtonsoft.Json;
using System;
using System.Configuration;
using System.IO;
using System.Text;

namespace Interface_pattaya.utils
{
    public class LogManager
    {
        private string _logFolder;
        public string LogFolder => _logFolder;

        private int _logRetentionDays = 30;
        public int LogRetentionDays
        {
            get => _logRetentionDays;
            set => _logRetentionDays = value > 0 ? value : 30;
        }

        private static bool _hasLoggedInit = false;

        public LogManager(string logFolder = "log", int logRetentionDays = 30)
        {
            try
            {
                var appFolder = AppDomain.CurrentDomain.BaseDirectory ?? Environment.CurrentDirectory;
                var desired = Path.Combine(appFolder, logFolder);

                if (!Directory.Exists(desired))
                {
                    Directory.CreateDirectory(desired);
                }

                _logFolder = desired;
                _logRetentionDays = LoadLogRetentionDaysFromConfig(logRetentionDays);

                if (!_hasLoggedInit)
                {
                    _hasLoggedInit = true;
                    LogInfo($"LogManager initialized - Folder: {_logFolder}, Retention: {_logRetentionDays} days");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error initializing LogManager: {ex.Message}");
            }
        }

        private int LoadLogRetentionDaysFromConfig(int defaultValue)
        {
            try
            {
                string configValue = ConfigurationManager.AppSettings["LogRetentionDays"];

                if (!string.IsNullOrEmpty(configValue) && int.TryParse(configValue, out int days))
                {
                    return days > 0 ? days : defaultValue;
                }

                return defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        public void ReloadLogRetentionDays()
        {
            try
            {
                ConfigurationManager.RefreshSection("appSettings");
                _logRetentionDays = LoadLogRetentionDaysFromConfig(30);
                LogInfo($"LogRetentionDays reloaded: {_logRetentionDays} days");
            }
            catch (Exception ex)
            {
                LogError("Error reloading LogRetentionDays", ex);
            }
        }

        public void LogToFile(string message, string logType = "INFO")
        {
            try
            {
                var logFileName = $"{DateTime.Now:yyyy-MM-dd}.log";
                var logPath = Path.Combine(_logFolder, logFileName);
                var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{logType}] {message}{Environment.NewLine}";

                File.AppendAllText(logPath, logEntry, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error writing to log: {ex.Message}");
            }
        }

        public void LogInfo(string message)
        {
            LogToFile(message, "INFO");
        }

        public void LogWarning(string message)
        {
            LogToFile(message, "WARN");
        }

        public void LogError(string message, Exception ex = null)
        {
            try
            {
                var fullMessage = ex != null
                    ? $"{message} | Exception: {ex.Message} | StackTrace: {ex.StackTrace}"
                    : message;

                LogToFile(fullMessage, "ERROR");
            }
            catch (Exception logEx)
            {
                Console.WriteLine($"❌ Critical logging error: {logEx.Message}");
            }
        }

        public void LogConnectDatabase(bool isConnected, DateTime? lastConnectedTime = null, DateTime? lastDisconnectedTime = null, string connectLogFolder = "Connection")
        {
            try
            {
                var appFolder = AppDomain.CurrentDomain.BaseDirectory ?? Environment.CurrentDirectory;
                var connectLogBaseDir = Path.Combine(appFolder, connectLogFolder);
                var dateFolder = DateTime.Now.ToString("yyyy-MM-dd");
                var connectLogDir = Path.Combine(connectLogBaseDir, dateFolder);

                if (!Directory.Exists(connectLogDir))
                {
                    Directory.CreateDirectory(connectLogDir);
                }

                CleanOldLogFolders(connectLogBaseDir);

                var connectLogPath = Path.Combine(connectLogDir, $"connection_{dateFolder}.log");
                var status = isConnected ? "✓ Connected" : "✗ Disconnected";
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                var logEntry = $"[{timestamp}] Database Status: {status}";

                if (isConnected && lastConnectedTime.HasValue)
                {
                    logEntry += $" | Last Connected: {lastConnectedTime.Value:yyyy-MM-dd HH:mm:ss}";
                }
                else if (!isConnected)
                {
                    if (lastDisconnectedTime.HasValue)
                        logEntry += $" | Disconnected at: {lastDisconnectedTime.Value:yyyy-MM-dd HH:mm:ss}";
                    if (lastConnectedTime.HasValue)
                        logEntry += $" | Last Connected: {lastConnectedTime.Value:yyyy-MM-dd HH:mm:ss}";
                }

                logEntry += Environment.NewLine;

                File.AppendAllText(connectLogPath, logEntry, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                LogError("Error logging database connection", ex);
            }
        }

        // ✅ Clean old log folders
        private void CleanOldLogFolders(string baseLogFolder)
        {
            try
            {
                if (!Directory.Exists(baseLogFolder))
                    return;

                var cutoffDate = DateTime.Now.AddDays(-_logRetentionDays);
                var directories = Directory.GetDirectories(baseLogFolder);

                foreach (var dir in directories)
                {
                    var folderName = Path.GetFileName(dir);

                    if (DateTime.TryParseExact(folderName, "yyyy-MM-dd",
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None,
                        out DateTime folderDate))
                    {
                        if (folderDate < cutoffDate)
                        {
                            try
                            {
                                Directory.Delete(dir, true);
                                LogInfo($"Deleted old log folder: {folderName}");
                            }
                            catch (Exception ex)
                            {
                                LogWarning($"Failed to delete old folder {folderName}: {ex.Message}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError($"Error cleaning old log folders", ex);
            }
        }

        // ✅ Clean old log files
        public void CleanOldLogs()
        {
            try
            {
                LogInfo($"Starting log cleanup. Retention: {_logRetentionDays} days");

                CleanOldLogFiles(_logFolder);
                CleanOldLogFolders(Path.Combine(AppDomain.CurrentDomain.BaseDirectory ?? Environment.CurrentDirectory, "Connection"));

                LogInfo("Log cleanup completed");
            }
            catch (Exception ex)
            {
                LogError("Error in CleanOldLogs", ex);
            }
        }

        private void CleanOldLogFiles(string logFolder)
        {
            try
            {
                if (!Directory.Exists(logFolder))
                    return;

                var cutoffDate = DateTime.Now.AddDays(-_logRetentionDays);
                var files = Directory.GetFiles(logFolder, "*.log");

                foreach (var file in files)
                {
                    var fileName = Path.GetFileNameWithoutExtension(file);

                    if (DateTime.TryParseExact(fileName, "yyyy-MM-dd",
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None,
                        out DateTime fileDate))
                    {
                        if (fileDate < cutoffDate)
                        {
                            try
                            {
                                File.Delete(file);
                                LogInfo($"Deleted old log file: {fileName}");
                            }
                            catch (Exception ex)
                            {
                                LogWarning($"Failed to delete file {fileName}: {ex.Message}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError($"Error cleaning log files", ex);
            }
        }
    }
}