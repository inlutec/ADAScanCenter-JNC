using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using ADAScanCenter.Models;
using Newtonsoft.Json;

namespace ADAScanCenter.Services
{
    public class ConfigService
    {
        private static readonly string ConfigPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ADAScanCenter",
            "config.json");

        public AppConfig CurrentConfig { get; private set; }

        public ConfigService()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath)!);
            LoadConfig();
        }

        public void LoadConfig()
        {
            if (!File.Exists(ConfigPath))
            {
                CurrentConfig = new AppConfig();
                SaveConfig();
            }
            else
            {
                try
                {
                    var json = File.ReadAllText(ConfigPath);
                    CurrentConfig = JsonConvert.DeserializeObject<AppConfig>(json) ?? new AppConfig();
                }
                catch
                {
                    CurrentConfig = new AppConfig();
                }
            }
        }

        public void SaveConfig()
        {
            var json = JsonConvert.SerializeObject(CurrentConfig, Formatting.Indented);
            File.WriteAllText(ConfigPath, json);
        }

        public string DecryptPassword(string encryptedBase64)
        {
            if (string.IsNullOrEmpty(encryptedBase64)) return string.Empty;
            try
            {
                byte[] protectedData = Convert.FromBase64String(encryptedBase64);
                byte[] entropy = null; // Opcional: añadir entropía adicional
                byte[] userData = ProtectedData.Unprotect(protectedData, entropy, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(userData);
            }
            catch
            {
                return string.Empty;
            }
        }

        public string EncryptPassword(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return string.Empty;
            try
            {
                byte[] userData = Encoding.UTF8.GetBytes(plainText);
                byte[] entropy = null; 
                byte[] protectedData = ProtectedData.Protect(userData, entropy, DataProtectionScope.CurrentUser);
                return Convert.ToBase64String(protectedData);
            }
            catch
            {
                return string.Empty;
            }
        }
        
        public bool VerifyAdminPassword(string inputPassword)
        {
             if (string.IsNullOrEmpty(inputPassword)) return false;

             // Hash por defecto de "admin1234" para fallback seguro
             string defaultHash = "8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918";
             string targetHash = !string.IsNullOrEmpty(CurrentConfig?.AdminPasswordHash) 
                 ? CurrentConfig.AdminPasswordHash 
                 : defaultHash;

             using (SHA256 sha256 = SHA256.Create())
             {
                 byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(inputPassword));
                 StringBuilder builder = new StringBuilder();
                 for (int i = 0; i < bytes.Length; i++)
                 {
                     builder.Append(bytes[i].ToString("x2"));
                 }
                 return string.Equals(builder.ToString(), targetHash, StringComparison.OrdinalIgnoreCase);
             }
        }

        public void SetAdminPassword(string newPassword)
        {
            if (string.IsNullOrEmpty(newPassword)) return;
            using (SHA256 sha256 = SHA256.Create())
            {
                 byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(newPassword));
                 StringBuilder builder = new StringBuilder();
                 for (int i = 0; i < bytes.Length; i++)
                 {
                     builder.Append(bytes[i].ToString("x2"));
                 }
                 CurrentConfig.AdminPasswordHash = builder.ToString();
                 SaveConfig();
            }
        }
    }
}
