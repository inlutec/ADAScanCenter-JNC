using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MimeKit;

namespace ADAScanCenter.Services
{
    public class ScanReceivedEventArgs : EventArgs
    {
        public string TempFilePath { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public UniqueId MessageUid { get; set; }
        public string? Subject { get; set; }
        public DateTimeOffset? Date { get; set; }
    }

    public class MultipleScansEventArgs : EventArgs
    {
        public List<ScanReceivedEventArgs> Scans { get; set; } = new();
    }

    public class ImapService : IDisposable
    {
        private readonly ConfigService _configService;
        private CancellationTokenSource _cts;
        private Task _monitorTask;

        public event EventHandler<ScanReceivedEventArgs> FileReceived;
        public event EventHandler<MultipleScansEventArgs> MultipleScansAvailable;
        public event EventHandler<bool> ConnectionStatusChanged;

        public ImapService(ConfigService configService)
        {
            _configService = configService;
        }

        public void Start()
        {
            LogService.Info("[IMAP] Start() llamado");
            if (_monitorTask != null)
            {
                LogService.Info("[IMAP] Start() ignorado: _monitorTask ya existe");
                return;
            }
            _cts = new CancellationTokenSource();
            _monitorTask = Task.Run(() => MonitorLoop(_cts.Token));
            LogService.Info("[IMAP] MonitorLoop iniciado");
        }

        public void Stop()
        {
            LogService.Info("[IMAP] Stop() llamado");
            _cts?.Cancel();
            try { _monitorTask?.Wait(5000); } catch (Exception ex) { LogService.Warning($"[IMAP] Stop Wait: {ex.Message}"); }
            _monitorTask = null;
            _cts?.Dispose();
            LogService.Info("[IMAP] Stop() completado");
        }

        private async Task MonitorLoop(CancellationToken token)
        {
            int ciclo = 0;
            LogService.Info("[IMAP] MonitorLoop ENTRADA");
            while (!token.IsCancellationRequested)
            {
                ciclo++;
                if (ciclo > 1)
                {
                    LogService.Info($"[IMAP] MonitorLoop ciclo {ciclo} - pausa 2s antes de reconectar");
                    await Task.Delay(2000, token);
                }
                LogService.Info($"[IMAP] MonitorLoop ciclo {ciclo} - inicio");
                var config = _configService.CurrentConfig;
                if (!config.IsValid())
                {
                    LogService.Warning("[IMAP] Config inválida, esperando 5s");
                    await Task.Delay(5000, token);
                    continue;
                }

                ImapClient client = new ImapClient();
                try
                {
                    LogService.Info($"[IMAP] Conectando a {config.ImapServer}:{config.ImapPort}...");
                    await client.ConnectAsync(config.ImapServer, config.ImapPort, config.UseSsl, token);
                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    
                    string password = _configService.DecryptPassword(config.EmailPasswordEncrypted);
                    await client.AuthenticateAsync(config.EmailUser, password, token);

                    var inbox = client.Inbox;
                    await inbox.OpenAsync(FolderAccess.ReadWrite, token);

                    LogService.Info("[IMAP] Conectado y autenticado OK");
                    try { ConnectionStatusChanged?.Invoke(this, true); } catch (Exception ex) { LogService.Error("[IMAP] Error en ConnectionStatusChanged(true)", ex); }

                    while (!token.IsCancellationRequested)
                    {
                        if (!client.IsConnected || !client.IsAuthenticated)
                        {
                            LogService.Warning($"[IMAP] Conexión caída (IsConnected={client.IsConnected}, IsAuth={client.IsAuthenticated}) - reintentando");
                            try { ConnectionStatusChanged?.Invoke(this, false); } catch (Exception ex) { LogService.Error("[IMAP] Error en ConnectionStatusChanged(false)", ex); }
                            break; // Reiniciar conexión si cae
                        }

                        // Buscar correos no leídos del remitente
                        SearchQuery query;
                        if (!string.IsNullOrEmpty(config.SenderEmailFilter))
                        {
                            query = SearchQuery.NotSeen.And(SearchQuery.FromContains(config.SenderEmailFilter));
                        }
                        else
                        {
                            query = SearchQuery.NotSeen;
                        }

                        var uids = await inbox.SearchAsync(query, token);
                        var scansWithAttachments = new List<ScanReceivedEventArgs>();

                        foreach (var uid in uids)
                        {
                            var message = await inbox.GetMessageAsync(uid, token);

                            foreach (var attachment in message.Attachments)
                            {
                                if (attachment is MimePart part && !string.IsNullOrEmpty(part.FileName))
                                {
                                    string ext = Path.GetExtension(part.FileName).ToLower();
                                    if (ext == ".pdf" || ext == ".jpg" || ext == ".jpeg" || ext == ".png")
                                    {
                                        // Nombre único para evitar "file in use" cuando el mismo correo se procesa varias veces
                                        string uniqueName = $"{Path.GetFileNameWithoutExtension(part.FileName)}_{Guid.NewGuid():N}{Path.GetExtension(part.FileName)}";
                                        string tempFile = Path.Combine(Path.GetTempPath(), uniqueName);
                                        using (var stream = File.Create(tempFile))
                                        {
                                            await part.Content.DecodeToAsync(stream, token);
                                        }

                                        scansWithAttachments.Add(new ScanReceivedEventArgs
                                        {
                                            TempFilePath = tempFile,
                                            OriginalFileName = part.FileName,
                                            MessageUid = uid,
                                            Subject = message.Subject,
                                            Date = message.Date
                                        });
                                    }
                                }
                            }
                        }

                        if (scansWithAttachments.Count > 1)
                        {
                            MultipleScansAvailable?.Invoke(this, new MultipleScansEventArgs { Scans = scansWithAttachments });
                        }
                        else if (scansWithAttachments.Count == 1)
                        {
                            FileReceived?.Invoke(this, scansWithAttachments[0]);
                        }

                        // IDLE (menos carga) o polling según configuración y soporte del servidor
                        if (config.UseImapIdle && client.Capabilities.HasFlag(ImapCapabilities.Idle))
                        {
                            // NoOp antes de IDLE: verifica conexión viva (importante en Windows 11/sleep)
                            try { await client.NoOpAsync(token); } catch (Exception ex) { LogService.Warning($"[IMAP] NoOp pre-IDLE: {ex.Message}"); break; }

                            // IDLE con timeout corto: más fiable en W11 (detecta conexión caída antes)
                            int idleSec = config.IdleTimeoutSeconds > 0 ? config.IdleTimeoutSeconds : 60;
                            int idleTimeoutMs = Math.Clamp(idleSec * 1000, 30000, 120000); // 30s-2min
                            using (var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(token))
                            {
                                timeoutCts.CancelAfter(idleTimeoutMs);
                                try { await client.IdleAsync(timeoutCts.Token); } catch (OperationCanceledException) { /* timeout o notificación */ }
                            }
                        }
                        else
                        {
                            // Polling: intervalos configurables
                            int interval = config.PollingIntervalSeconds > 0 ? config.PollingIntervalSeconds * 1000 : 30000;
                            await Task.Delay(interval, token);
                            await client.NoOpAsync(token);
                        }
                    }
                    LogService.Info("[IMAP] Salida del bucle interior (normal o conexión caída)");
                }
                catch (OperationCanceledException)
                {
                    LogService.Info("[IMAP] OperationCanceledException - saliendo del MonitorLoop");
                    break;
                }
                catch (IOException ex) when (ex.Message.Contains("being used by another process"))
                {
                    // Error de archivo bloqueado: no es fallo de conexión IMAP, no mostrar Desconectado
                    LogService.Warning($"[IMAP] Archivo en uso, reintentando: {ex.Message}");
                    try { await Task.Delay(5000, token); } catch (OperationCanceledException) { break; }
                }
                catch (Exception ex)
                {
                    LogService.Error($"[IMAP] Excepción: {ex.Message}", ex);
                    try { ConnectionStatusChanged?.Invoke(this, false); } catch (Exception ex2) { LogService.Error("[IMAP] Error en ConnectionStatusChanged(false)", ex2); }
                    LogService.Info("[IMAP] Esperando 5s antes de reintentar...");
                    try { await Task.Delay(5000, token); } catch (OperationCanceledException) { break; }
                    LogService.Info("[IMAP] Reintento tras excepción");
                }
                finally
                {
                    LogService.Info("[IMAP] Finally: desconectando cliente");
                    try { await client.DisconnectAsync(true, token); } catch (Exception ex) { LogService.Warning($"[IMAP] DisconnectAsync: {ex.Message}"); }
                    try { client.Dispose(); } catch (Exception ex) { LogService.Warning($"[IMAP] Dispose: {ex.Message}"); }
                    LogService.Info("[IMAP] Finally completado - volviendo al inicio del ciclo");
                }
            }
            LogService.Info($"[IMAP] MonitorLoop SALIDA (ciclos: {ciclo})");
        }

        public void ForceCheckNow()
        {
            LogService.Info("[IMAP] ForceCheckNow() llamado");
            Stop();
            Thread.Sleep(500);
            _cts = new CancellationTokenSource();
            _monitorTask = Task.Run(() => MonitorLoop(_cts.Token));
            LogService.Info("[IMAP] ForceCheckNow() - nuevo MonitorLoop iniciado");
        }

        public async Task<string> TestConnectionAsync(string server, int port, bool ssl, string user, string password)
        {
            using (var client = new ImapClient())
            {
                try
                {
                    await client.ConnectAsync(server, port, ssl);
                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    await client.AuthenticateAsync(user, password);
                    await client.DisconnectAsync(true);
                    return "Conexión Exitosa";
                }
                catch (Exception ex)
                {
                    return $"Error: {ex.Message}";
                }
            }
        }

        public async Task<string> CheckIdleSupportAsync(string server, int port, bool ssl, string user, string password)
        {
            using (var client = new ImapClient())
            {
                try
                {
                    await client.ConnectAsync(server, port, ssl);
                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    await client.AuthenticateAsync(user, password);

                    bool supportsIdle = client.Capabilities.HasFlag(ImapCapabilities.Idle);
                    await client.DisconnectAsync(true);

                    return supportsIdle
                        ? "✓ El servidor soporta IMAP IDLE. Puede activar la opción para reducir la carga."
                        : "✗ El servidor NO soporta IMAP IDLE. Use el modo polling.";
                }
                catch (Exception ex)
                {
                    return $"Error al comprobar: {ex.Message}";
                }
            }
        }

        public async Task DeleteMessage(UniqueId uid)
        {
             var config = _configService.CurrentConfig;
             using (var client = new ImapClient())
             {
                 try
                 {
                    await client.ConnectAsync(config.ImapServer, config.ImapPort, config.UseSsl);
                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    string password = _configService.DecryptPassword(config.EmailPasswordEncrypted);
                    await client.AuthenticateAsync(config.EmailUser, password);
                    
                    var inbox = client.Inbox;
                    await inbox.OpenAsync(FolderAccess.ReadWrite);
                    
                    await inbox.AddFlagsAsync(uid, MessageFlags.Deleted, true);
                    await inbox.ExpungeAsync();
                    
                    await client.DisconnectAsync(true);
                 }
                 catch { /* Log error */ }
             }
        }

        /// <summary>Marca el mensaje como no leído para que vuelva a aparecer en Comprobar Escaneo.</summary>
        public async Task MarkAsUnreadAsync(UniqueId uid)
        {
             var config = _configService.CurrentConfig;
             using (var client = new ImapClient())
             {
                 try
                 {
                    await client.ConnectAsync(config.ImapServer, config.ImapPort, config.UseSsl);
                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    string password = _configService.DecryptPassword(config.EmailPasswordEncrypted);
                    await client.AuthenticateAsync(config.EmailUser, password);
                    
                    var inbox = client.Inbox;
                    await inbox.OpenAsync(FolderAccess.ReadWrite);
                    await inbox.RemoveFlagsAsync(uid, MessageFlags.Seen, true);
                    await client.DisconnectAsync(true);
                 }
                 catch { /* Log error */ }
             }
        }

        public void Dispose()
        {
            Stop();
            _cts?.Dispose();
        }
    }
}
