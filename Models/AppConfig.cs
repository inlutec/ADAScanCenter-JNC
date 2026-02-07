using System;

namespace ADAScanCenter.Models
{
    public class AppConfig
    {
        public string ImapServer { get; set; } = "imap.example.com";
        public int ImapPort { get; set; } = 993;
        public bool UseSsl { get; set; } = true;
        public string EmailUser { get; set; } = "";
        
        // Esta contraseña se guardará encriptada en el JSON
        public string EmailPasswordEncrypted { get; set; } = "";

        // Remitente a filtrar (ej: scanner@empresa.com)
        public string SenderEmailFilter { get; set; } = "";

        // Ruta local o de red donde se guardarán los archivos
        public string OutputDirectory { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        // Hash de la contraseña de administrador (SHA256)
        // Se establecerá en el primer inicio
        public string AdminPasswordHash { get; set; } = "";

        // Intervalo de comprobación en segundos (solo si no usa IDLE)
        public int PollingIntervalSeconds { get; set; } = 30;

        // Usar IMAP IDLE cuando el servidor lo soporte (menos carga en el servidor)
        public bool UseImapIdle { get; set; } = true;

        // Timeout de IDLE en segundos (60 = más fiable en Windows 11/sleep; menor = más rápida detección de conexión caída)
        public int IdleTimeoutSeconds { get; set; } = 60;

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(ImapServer) &&
                   !string.IsNullOrEmpty(EmailUser) &&
                   !string.IsNullOrEmpty(EmailPasswordEncrypted);
        }
    }
}
