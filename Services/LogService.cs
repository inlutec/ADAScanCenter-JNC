using System;
using System.IO;

namespace ADAScanCenter.Services
{
    public class LogService
    {
        private static readonly object _lock = new object();
        private static string? _logPath;

        static LogService()
        {
            // Usar AppData en lugar de BaseDirectory: cuando la app está instalada en
            // Program Files, no tiene permisos de escritura. AppData es siempre escribible.
            string appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ADAScanCenter",
                "Logs");
            Directory.CreateDirectory(appDataPath);
            
            string logFile = $"ADAScanCenter_{DateTime.Now:yyyyMMdd}.log";
            _logPath = Path.Combine(appDataPath, logFile);
        }

        public static void Log(string message, string level = "INFO")
        {
            try
            {
                lock (_lock)
                {
                    string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
                    File.AppendAllText(_logPath, logEntry + Environment.NewLine);
                    
                    // También escribir en consola para debug
                    Console.WriteLine(logEntry);
                }
            }
            catch
            {
                // Silenciar errores de logging para no romper la app
            }
        }

        public static void Info(string message) => Log(message, "INFO");
        public static void Warning(string message) => Log(message, "WARN");
        public static void Error(string message) => Log(message, "ERROR");
        public static void Error(string message, Exception ex) => Log($"{message}: {ex.Message}\n{ex.StackTrace}", "ERROR");

        /// <summary>Ruta del archivo de log actual (para depuración).</summary>
        public static string LogPath => _logPath ?? "";
    }
}
