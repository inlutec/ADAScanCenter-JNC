using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ADAScanCenter.Models;
using ADAScanCenter.Services;
using ADAScanCenter.UI;

namespace ADAScanCenter
{
    public class AppTrayContext : ApplicationContext
    {
        private NotifyIcon _notifyIcon;
        private ConfigService _configService;
        private ImapService _imapService;
        private FileProcessor _fileProcessor;
        private Form _invokeControl;
        private bool _isProcessing = false;
        private bool _wasDisconnected;
        private System.Windows.Forms.Timer? _showPanelTimer;
        /// <summary>UIDs que el usuario dejó para "Revisar más tarde" - no mostrar popup hasta que use Comprobar</summary>
        private readonly HashSet<uint> _deferredScanUids = new();

        public AppTrayContext()
        {
            LogService.Info("=== Iniciando ADAScanCenter ===");
            LogService.Info($"Logs en: {LogService.LogPath}");
            
            _invokeControl = new Form
            {
                ShowInTaskbar = false,
                WindowState = FormWindowState.Minimized
            };
            _invokeControl.Load += (s, e) => { ((Form)s).Hide(); };
            // Forzar creación del handle
            var handle = _invokeControl.Handle;

            _configService = new ConfigService();
            _fileProcessor = new FileProcessor();
            _imapService = new ImapService(_configService);
            
            LogService.Info("Servicios inicializados correctamente");

            InitializeTrayIcon();

            _imapService.FileReceived += OnFileReceived;
            _imapService.MultipleScansAvailable += OnMultipleScansAvailable;
            _imapService.ConnectionStatusChanged += OnConnectionStatusChanged;

            // Verificar si es necesario establecer contraseña inicial
            if (string.IsNullOrEmpty(_configService.CurrentConfig.AdminPasswordHash))
            {
               using (var firstRun = new UI.SetPasswordForm())
               {
                   if (firstRun.ShowDialog() == DialogResult.OK)
                   {
                       _configService.SetAdminPassword(firstRun.NewPassword);
                       MessageBox.Show("Contraseña establecida correctamente.\nPuede acceder a la configuración desde el icono en la bandeja del sistema.", "Bienvenido", MessageBoxButtons.OK, MessageBoxIcon.Information);
                   }
                   else
                   {
                       // Si cancela la configuración inicial, salimos
                       Application.Exit();
                       return;
                   }
               }
            }

            LogService.Info("Iniciando servicio IMAP");
            _imapService.Start();

            _showPanelTimer = new System.Windows.Forms.Timer { Interval = 500 };
            _showPanelTimer.Tick += ShowPanelTimer_Tick;
            _showPanelTimer.Start();
        }

        private void ShowPanelTimer_Tick(object? sender, EventArgs e)
        {
            try
            {
                string path = Program.GetShowPanelTriggerPath();
                if (File.Exists(path))
                {
                    File.Delete(path);
                    _invokeControl.Invoke(new Action(() =>
                    {
                        var mainForm = new UI.MainForm(_configService, this);
                        mainForm.Show();
                        mainForm.BringToFront();
                    }));
                }
            }
            catch { }
        }

        private void InitializeTrayIcon()
        {
            _notifyIcon = new NotifyIcon();
            _notifyIcon.Icon = IconHelper.CreateScannerIcon();
            _notifyIcon.Text = "ADAScanCenter - Iniciando...";
            _notifyIcon.Visible = true;

            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Administración", null, OnConfigClick);
            contextMenu.Items.Add("Comprobar Ahora", null, OnCheckNowClick);
            contextMenu.Items.Add("Abrir Carpeta", null, OnOpenFolderClick);
            contextMenu.Items.Add("-");
            contextMenu.Items.Add("Salir", null, OnExitClick);

            _notifyIcon.ContextMenuStrip = contextMenu;
            _notifyIcon.DoubleClick += OnOpenFolderClick;
        }

        private void OnConnectionStatusChanged(object sender, bool isConnected)
        {
            LogService.Info($"[Tray] OnConnectionStatusChanged(isConnected={isConnected}), InvokeRequired={_invokeControl.InvokeRequired}");
            if (_invokeControl.InvokeRequired)
            {
                _invokeControl.BeginInvoke(new Action(() => OnConnectionStatusChanged(sender, isConnected)));
                return;
            }

            bool wasDown = _wasDisconnected;
            if (!isConnected) _wasDisconnected = true;
            else _wasDisconnected = false;

            LogService.Info($"Estado de conexión IMAP: {(isConnected ? "Conectado" : "Desconectado")}");
            _notifyIcon.Icon = isConnected ? IconHelper.CreateScannerIcon() : SystemIcons.Error;
            _notifyIcon.Text = isConnected ? "ADAScanCenter - Conectado" : "ADAScanCenter - Desconectado";
            if (isConnected && wasDown)
                _notifyIcon.ShowBalloonTip(3000, "Conexión restablecida", "El servicio IMAP se ha reconectado correctamente.", ToolTipIcon.Info);
            else if (!isConnected)
                _notifyIcon.ShowBalloonTip(5000, "Conexión perdida", "Clic derecho en el icono > Comprobar Ahora para reconectar.", ToolTipIcon.Warning);
        }

        private void OnMultipleScansAvailable(object? sender, MultipleScansEventArgs e)
        {
            if (_invokeControl.InvokeRequired)
            {
                _invokeControl.Invoke(new Action(() => OnMultipleScansAvailable(sender, e)));
                return;
            }

            // Filtrar escaneos diferidos (Revisar más tarde)
            var scansToShow = e.Scans.Where(s => !_deferredScanUids.Contains(s.MessageUid.Id)).ToList();
            if (scansToShow.Count == 0)
                return;

            if (_isProcessing) return;
            _isProcessing = true;

            Task.Run(() =>
            {
                _invokeControl.Invoke(new Action(() =>
                {
                    try
                    {
                        using (var selectForm = new UI.ScanSelectForm(scansToShow))
                        {
                            if (selectForm.ShowDialog() == DialogResult.OK && selectForm.SelectedScan != null)
                            {
                                ProcessSingleScan(selectForm.SelectedScan);
                            }
                            // Si solo había 1 y canceló, no añadir a diferidos (volverá a aparecer)
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        _isProcessing = false;
                    }
                }));
            });
        }

        private void OnFileReceived(object? sender, ScanReceivedEventArgs e)
        {
            if (_invokeControl.InvokeRequired)
            {
                _invokeControl.Invoke(new Action(() => OnFileReceived(sender, e)));
                return;
            }

            // Revisar más tarde: no mostrar popup hasta que el usuario use Comprobar
            if (_deferredScanUids.Contains(e.MessageUid.Id))
                return;

            if (_isProcessing) return;
            _isProcessing = true;

            Task.Run(() =>
            {
                _invokeControl.Invoke(new Action(() =>
                {
                    try
                    {
                        ProcessSingleScan(e);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error procesando: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        _isProcessing = false;
                    }
                }));
            });
        }

        private void ProcessSingleScan(ScanReceivedEventArgs e)
        {
            LogService.Info($"Procesando archivo: {e.OriginalFileName}");
            using (var form = new NewScanForm())
            {
                var result = form.ShowDialog();

                if (result == DialogResult.OK && form.ReviewLater)
                {
                    // Revisar más tarde: no borrar, no mostrar popup hasta que use Comprobar
                    _deferredScanUids.Add(e.MessageUid.Id);
                    return;
                }

                if (result == DialogResult.OK && form.ShouldSave)
                {
                    string outputDir = _configService.CurrentConfig.OutputDirectory;

                    if (form.SaveAsPdf)
                    {
                        // Guardar PDF: solo borrar de IMAP tras guardar correctamente en carpeta
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                await _fileProcessor.ProcessFileAsync(e.TempFilePath, outputDir, true);
                                await _imapService.DeleteMessage(e.MessageUid);
                                _invokeControl.BeginInvoke(new Action(() =>
                                    _notifyIcon.ShowBalloonTip(3000, "Éxito", "PDF guardado.", ToolTipIcon.Info)));
                            }
                            catch (Exception ex) { LogService.Error("Error guardando PDF", ex); }
                        });
                    }
                    else if (form.UserChoice == NewScanResult.SaveAsJpg)
                    {
                        // Abrir Editor Visual: solo borrar de IMAP cuando el usuario guarde en carpeta
                        using (var editor = new UI.ImageEditorForm(e.TempFilePath))
                        {
                            editor.ShowDialog();
                            if (editor.SavedAny)
                            {
                                _ = Task.Run(async () =>
                                {
                                    await _imapService.DeleteMessage(e.MessageUid);
                                    _invokeControl.BeginInvoke(new Action(() =>
                                        _notifyIcon.ShowBalloonTip(3000, "Éxito", "Imágenes guardadas.", ToolTipIcon.Info)));
                                });
                            }
                            else
                            {
                                // Usuario canceló: marcar como no leído para que recupere con Comprobar Escaneo
                                _ = Task.Run(async () =>
                                {
                                    await _imapService.MarkAsUnreadAsync(e.MessageUid);
                                    _invokeControl.BeginInvoke(new Action(() =>
                                        _notifyIcon.ShowBalloonTip(5000, "Escaneo no guardado", "El correo sigue en la bandeja. Use Comprobar Escaneo para procesarlo de nuevo.", ToolTipIcon.Info)));
                                });
                            }
                        }
                    }
                }
            }
        }

        private void OnConfigClick(object sender, EventArgs e)
        {
            using (var pass = new PasswordPrompt())
            {
                if (pass.ShowDialog() == DialogResult.OK)
                {
                    if (_configService.VerifyAdminPassword(pass.EnteredPassword))
                    {
                        using (var cfg = new ConfigForm(_configService))
                        {
                            if (cfg.ShowDialog() == DialogResult.OK)
                            {
                                _imapService.Stop();
                                _imapService.Start();
                                _notifyIcon.ShowBalloonTip(2000, "Configuración", "Servicio reiniciado.", ToolTipIcon.Info);
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Contraseña incorrecta", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }

        private void OnCheckNowClick(object sender, EventArgs e)
        {
            _deferredScanUids.Clear(); // Mostrar de nuevo los que estaban en "Revisar más tarde"
            _notifyIcon.ShowBalloonTip(1000, "ADAScanCenter", "Buscando correos...", ToolTipIcon.Info);
            _imapService.ForceCheckNow();
        }

        public void ForceCheckNow()
        {
            _deferredScanUids.Clear(); // Mostrar de nuevo los que estaban en "Revisar más tarde"
            _imapService.ForceCheckNow();
        }

        private void OnOpenFolderClick(object sender, EventArgs e)
        {
            string path = _configService.CurrentConfig.OutputDirectory;
            if (Directory.Exists(path))
            {
                System.Diagnostics.Process.Start("explorer.exe", path);
            }
            else
            {
                MessageBox.Show($"Ruta no encontrada: {path}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void OnExitClick(object sender, EventArgs e)
        {
            _showPanelTimer?.Stop();
            _showPanelTimer?.Dispose();
            _imapService.Stop();
            _notifyIcon.Visible = false;
            Application.Exit();
        }
    }
}
