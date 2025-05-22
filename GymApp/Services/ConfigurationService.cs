using Newtonsoft.Json;
using System;
using System.IO;

namespace GymApp.Services
{
    public class ConfigurationService
    {
        private readonly dynamic _config;
        private static ConfigurationService _instance;
        private static readonly object _lock = new object();

        private ConfigurationService()
        {
            try
            {
                string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
                if (File.Exists(configPath))
                {
                    string json = File.ReadAllText(configPath);
                    _config = JsonConvert.DeserializeObject(json);
                }
                else
                {
                    // Default config if file doesn't exist
                    _config = new
                    {
                        ConnectionStrings = new
                        {
                            OracleConnection = "Data Source=localhost:1521/FREE;User Id=C##GymApp;Password=a123;Pooling=true;Connection Timeout=60;"
                        },
                        AppSettings = new
                        {
                            ApplicationName = "Gym Management System",
                            Version = "1.0.0",
                            DatabaseTimeout = 30
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading configuration: {ex.Message}");
            }
        }

        public static ConfigurationService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                            _instance = new ConfigurationService();
                    }
                }
                return _instance;
            }
        }

        public string GetConnectionString(string name = "OracleConnection")
        {
            try
            {
                return _config.ConnectionStrings[name].ToString();
            }
            catch
            {
                return "Data Source=localhost:1521/FREE;User Id=C##GymApp;Password=a123;Pooling=true;Connection Timeout=60;";
            }
        }

        public T GetAppSetting<T>(string key, T defaultValue = default(T))
        {
            try
            {
                var value = _config.AppSettings[key];
                if (value == null) return defaultValue;

                if (typeof(T) == typeof(string))
                    return (T)(object)value.ToString();

                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }

        public string GetAppName() => GetAppSetting("ApplicationName", "Gym Management System");
        public string GetVersion() => GetAppSetting("Version", "1.0.0");
        public int GetDatabaseTimeout() => GetAppSetting("DatabaseTimeout", 30);
    }
}