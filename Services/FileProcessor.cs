using System;
using System.IO;
using System.Threading.Tasks;

namespace ADAScanCenter.Services
{
    public class FileProcessor
    {
        public async Task ProcessFileAsync(string tempFilePath, string outputDir, bool saveAsPdf)
        {
            if (!File.Exists(tempFilePath)) throw new FileNotFoundException("Temp file not found", tempFilePath);
            if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);

            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(tempFilePath);
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string destFileNameBase = $"{fileNameWithoutExt}_{timestamp}";
            string destPath = Path.Combine(outputDir, destFileNameBase);

            if (saveAsPdf)
            {
                // Mover PDF original
                string destFile = destPath + ".pdf";
                
                // Asegurar unicidad
                int counter = 1;
                while (File.Exists(destFile))
                {
                    destFile = Path.Combine(outputDir, $"{destFileNameBase}_{counter}.pdf");
                    counter++;
                }

                File.Move(tempFilePath, destFile);
            }
            // La lógica de JPG ya no se maneja aquí, sino en ImageEditorForm
        }
    }
}
