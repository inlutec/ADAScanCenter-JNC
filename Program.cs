using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ADAScanCenter.Services;
using PdfiumViewer;

namespace ADAScanCenter
{
    static class Program
    {
        private static readonly string ShowPanelTriggerPath = Path.Combine(Path.GetTempPath(), "ADAScanCenter_ShowPanel.trigger");

        [STAThread]
        static void Main(string[] args)
        {
            // .NET 10+ con PublishSingleFile ya no busca DLLs nativas en el directorio del exe.
            // Configurar explícitamente la ruta de pdfium.dll para PdfiumViewer.
            ConfigurePdfiumNativePath();

            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Si se ejecuta con parámetro --service es inicio automático
            bool serviceMode = args.Contains("--service");

            if (serviceMode)
            {
                // Modo servicio (inicio automático de Windows)
                LogService.Info("Iniciando en modo servicio (background)");
                Application.Run(new AppTrayContext());
            }
            else
            {
                // Modo UI (ejecutado por el usuario desde acceso directo)
                LogService.Info("Iniciando en modo UI");
                
                // Verificar si ya hay una instancia corriendo
                var currentProcess = System.Diagnostics.Process.GetCurrentProcess();
                var processes = System.Diagnostics.Process.GetProcessesByName(currentProcess.ProcessName);
                
                if (processes.Length > 1)
                {
                    // Otra instancia ya corre: señalarla para que muestre el panel (Comprobar Escaneo funcionará)
                    try { File.WriteAllText(ShowPanelTriggerPath, ""); } catch { }
                    return;
                }
                else
                {
                    // Primera instancia, iniciar servicio en background y mostrar UI
                    var context = new AppTrayContext();
                    var configService = new ConfigService();
                    var mainForm = new UI.MainForm(configService, context);
                    mainForm.Show();
                    Application.Run(context);
                }
            }
        }

        public static string GetShowPanelTriggerPath() => ShowPanelTriggerPath;

        private static void ConfigurePdfiumNativePath()
        {
            try
            {
                // Obtener directorio del ejecutable (donde están x64/pdfium.dll y x86/pdfium.dll)
                string exeDir = Path.GetDirectoryName(
                    Environment.ProcessPath ?? AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar));
                if (string.IsNullOrEmpty(exeDir)) return;

                string arch = Environment.Is64BitProcess ? "x64" : "x86";
                string pdfiumPath = Path.Combine(exeDir, arch, "pdfium.dll");
                if (!File.Exists(pdfiumPath)) return;

                // Registrar resolver para que PdfiumViewer encuentre pdfium.dll
                var pdfiumViewerAssembly = typeof(PdfDocument).Assembly;
                NativeLibrary.SetDllImportResolver(pdfiumViewerAssembly, (libraryName, assembly, searchPath) =>
                {
                    if (libraryName.Equals("pdfium", StringComparison.OrdinalIgnoreCase) ||
                        libraryName.Equals("pdfium.dll", StringComparison.OrdinalIgnoreCase))
                    {
                        return NativeLibrary.Load(pdfiumPath);
                    }
                    return IntPtr.Zero;
                });
            }
            catch
            {
                // Si falla, PdfiumViewer usará la búsqueda por defecto
            }
        }
    }
}
