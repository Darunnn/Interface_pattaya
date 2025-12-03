using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interface_pattaya.Configuration
{
    public class DatabaseConfig
    {
        private const string ConnFolder = "Connection";
        private const string ConnFile = "database.ini";

        // Database Connection Properties
        public string Server { get; private set; }
        public string Port { get; private set; }
        public string Database { get; private set; }
        public string UserId { get; private set; }
        public string Password { get; private set; }
        public string Provider { get; private set; } = "SqlServer";
        public int ConnectionTimeout { get; private set; } = 30;

        // Full Connection String
        public string ConnectionString { get; private set; }

        public bool LoadConfiguration()
        {
            try
            {
                LoadDatabaseConfig();
                BuildConnectionString();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to load database configuration: {ex.Message}", ex);
            }
        }

        public void ReloadConfiguration()
        {
            LoadConfiguration();
        }

        private void LoadDatabaseConfig()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConnFolder, ConnFile);

            if (!File.Exists(path))
            {
                CreateDefaultConfig(path);
            }

            var lines = File.ReadAllLines(path);

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#") || line.StartsWith("["))
                    continue;

                var parts = line.Split('=');
                if (parts.Length != 2) continue;

                var key = parts[0].Trim();
                var value = parts[1].Trim();

                switch (key.ToUpper())
                {
                    case "SERVER":
                        Server = value;
                        break;
                    case "PORT":
                        Port = value;
                        break;
                    case "DATABASE":
                        Database = value;
                        break;
                    case "USERID":
                    case "USERNAME":
                        UserId = value;
                        break;
                    case "PASSWORD":
                        Password = value;
                        break;
                    case "PROVIDER":
                        Provider = value;
                        break;
                    case "CONNECTIONTIMEOUT":
                    case "TIMEOUT":
                        if (int.TryParse(value, out int timeout))
                            ConnectionTimeout = timeout;
                        break;
                }
            }

            // Validate required settings
            if (string.IsNullOrWhiteSpace(Server))
                throw new Exception("Server is not configured in database.ini");

            if (string.IsNullOrWhiteSpace(Database))
                throw new Exception("Database is not configured in database.ini");
        }

        private void BuildConnectionString()
        {
            var connBuilder = new StringBuilder();

            switch (Provider.ToLower())
            {
                case "sqlserver":
                    connBuilder.Append($"Server={Server}");
                    if (!string.IsNullOrWhiteSpace(Port))
                        connBuilder.Append($",{Port}");
                    connBuilder.Append($";Database={Database}");

                    if (!string.IsNullOrWhiteSpace(UserId))
                    {
                        connBuilder.Append($";User Id={UserId}");
                        connBuilder.Append($";Password={Password}");
                    }
                    else
                    {
                        connBuilder.Append(";Integrated Security=True");
                    }

                    connBuilder.Append($";Connection Timeout={ConnectionTimeout}");
                    break;

                case "mysql":
                    connBuilder.Append($"Server={Server}");
                    if (!string.IsNullOrWhiteSpace(Port))
                        connBuilder.Append($";Port={Port}");
                    connBuilder.Append($";Database={Database}");
                    connBuilder.Append($";Uid={UserId}");
                    connBuilder.Append($";Pwd={Password}");
                    connBuilder.Append($";Connection Timeout={ConnectionTimeout}");
                    break;

                case "postgresql":
                    connBuilder.Append($"Host={Server}");
                    if (!string.IsNullOrWhiteSpace(Port))
                        connBuilder.Append($";Port={Port}");
                    connBuilder.Append($";Database={Database}");
                    connBuilder.Append($";Username={UserId}");
                    connBuilder.Append($";Password={Password}");
                    connBuilder.Append($";Timeout={ConnectionTimeout}");
                    break;

                default:
                    throw new Exception($"Unsupported database provider: {Provider}");
            }

            ConnectionString = connBuilder.ToString();
        }

        private void CreateDefaultConfig(string path)
        {
            var directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var defaultConfig = @"# ===== DATABASE CONFIGURATION =====
# Provider: SqlServer, MySQL, PostgreSQL
Provider=SqlServer

# Server Configuration
Server=localhost
Port=1433

# Database Name
Database=MyDatabase

# Authentication (leave UserId empty for Windows Authentication in SQL Server)
UserId=sa
Password=YourPassword123

# Connection Settings
ConnectionTimeout=30
";

            File.WriteAllText(path, defaultConfig);
        }

        public string GetConfigurationSummary()
        {
            return $@"Database Configuration Summary:
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Provider: {Provider}
Server: {Server}{(string.IsNullOrWhiteSpace(Port) ? "" : ":" + Port)}
Database: {Database}
User: {(string.IsNullOrWhiteSpace(UserId) ? "Windows Authentication" : UserId)}
Timeout: {ConnectionTimeout} seconds

Connection String:
{MaskPassword(ConnectionString)}
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━";
        }

        private string MaskPassword(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                return "";

            var masked = connectionString;
            var patterns = new[] { "Password=", "Pwd=", "password=" };

            foreach (var pattern in patterns)
            {
                var startIndex = masked.IndexOf(pattern, StringComparison.OrdinalIgnoreCase);
                if (startIndex >= 0)
                {
                    var valueStart = startIndex + pattern.Length;
                    var endIndex = masked.IndexOf(';', valueStart);
                    if (endIndex < 0) endIndex = masked.Length;

                    var passwordLength = endIndex - valueStart;
                    masked = masked.Remove(valueStart, passwordLength)
                                   .Insert(valueStart, new string('*', Math.Min(passwordLength, 8)));
                }
            }

            return masked;
        }
    }

   
   
}

/*
ตัวอย่างไฟล์ database.ini ที่จะถูกสร้างที่ Connection/database.ini

# ===== DATABASE CONFIGURATION =====
# Provider: SqlServer, MySQL, PostgreSQL
Provider=SqlServer

# Server Configuration
Server=localhost
Port=1433

# Database Name
Database=MyDatabase

# Authentication (leave UserId empty for Windows Authentication in SQL Server)
UserId=sa
Password=YourPassword123

# Connection Settings
ConnectionTimeout=30
*/

