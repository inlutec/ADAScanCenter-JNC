using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace ADAScanCenter
{
    public static class IconHelper
    {
        public static Icon CreateScannerIcon()
        {
            // Crear un bitmap de 64x64 para el icono
            using (Bitmap bmp = new Bitmap(64, 64))
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                // Dibujar un escáner estilizado
                // Base del escáner (rectángulo principal)
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(70, 130, 180))) // Steel Blue
                {
                    g.FillRoundedRectangle(brush, new Rectangle(8, 20, 48, 28), 4);
                }

                // Tapa superior (más oscura)
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(50, 100, 150)))
                {
                    g.FillRoundedRectangle(brush, new Rectangle(8, 16, 48, 8), 4);
                }

                // Línea de escaneo (verde brillante)
                using (Pen pen = new Pen(Color.FromArgb(0, 255, 100), 2))
                {
                    g.DrawLine(pen, 12, 32, 52, 32);
                }

                // Documento saliendo
                using (SolidBrush brush = new SolidBrush(Color.White))
                {
                    g.FillRectangle(brush, new Rectangle(20, 8, 24, 12));
                }
                using (Pen pen = new Pen(Color.Gray, 1))
                {
                    g.DrawRectangle(pen, new Rectangle(20, 8, 24, 12));
                    // Líneas del documento
                    g.DrawLine(pen, 22, 11, 40, 11);
                    g.DrawLine(pen, 22, 14, 40, 14);
                    g.DrawLine(pen, 22, 17, 35, 17);
                }

                // Botón de encendido
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(100, 200, 100)))
                {
                    g.FillEllipse(brush, new Rectangle(50, 22, 6, 6));
                }

                return Icon.FromHandle(bmp.GetHicon());
            }
        }
    }

    // Extensión para dibujar rectángulos redondeados
    public static class GraphicsExtensions
    {
        public static void FillRoundedRectangle(this Graphics g, Brush brush, Rectangle bounds, int radius)
        {
            using (GraphicsPath path = GetRoundedRectPath(bounds, radius))
            {
                g.FillPath(brush, path);
            }
        }

        private static GraphicsPath GetRoundedRectPath(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            Size size = new Size(diameter, diameter);
            Rectangle arc = new Rectangle(bounds.Location, size);
            GraphicsPath path = new GraphicsPath();

            if (radius == 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            // Esquina superior izquierda
            path.AddArc(arc, 180, 90);

            // Esquina superior derecha
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);

            // Esquina inferior derecha
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // Esquina inferior izquierda
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }
    }
}
