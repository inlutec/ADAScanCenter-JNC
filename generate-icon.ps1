# Script para generar el icono antes de compilar
Write-Host "Generando icono de aplicación..." -ForegroundColor Yellow

# Crear un programa temporal para generar el icono
$iconGenCode = @'
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;

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

        path.AddArc(arc, 180, 90);
        arc.X = bounds.Right - diameter;
        path.AddArc(arc, 270, 90);
        arc.Y = bounds.Bottom - diameter;
        path.AddArc(arc, 0, 90);
        arc.X = bounds.Left;
        path.AddArc(arc, 90, 90);
        path.CloseFigure();
        return path;
    }
}

public class IconGen
{
    public static void Main()
    {
        int[] sizes = { 16, 32, 48, 256 };
        
        using (var ms = new MemoryStream())
        using (var bw = new BinaryWriter(ms))
        {
            bw.Write((short)0);
            bw.Write((short)1);
            bw.Write((short)sizes.Length);

            var images = new MemoryStream[sizes.Length];
            
            for (int i = 0; i < sizes.Length; i++)
            {
                int size = sizes[i];
                images[i] = new MemoryStream();
                
                using (Bitmap bmp = CreateIcon(size))
                {
                    bmp.Save(images[i], ImageFormat.Png);
                }
                
                bw.Write((byte)(size == 256 ? 0 : size));
                bw.Write((byte)(size == 256 ? 0 : size));
                bw.Write((byte)0);
                bw.Write((byte)0);
                bw.Write((short)1);
                bw.Write((short)32);
                bw.Write((int)images[i].Length);
                bw.Write((int)(6 + sizes.Length * 16 + images.Take(i).Sum(img => (int)img.Length)));
            }
            
            foreach (var img in images)
            {
                img.Position = 0;
                img.CopyTo(ms);
                img.Dispose();
            }
            
            File.WriteAllBytes("app.ico", ms.ToArray());
        }
        Console.WriteLine("Icono generado: app.ico");
    }

    static Bitmap CreateIcon(int size)
    {
        Bitmap bmp = new Bitmap(size, size);
        using (Graphics g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.Transparent);

            float s = size / 64f;
            
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(70, 130, 180)))
                g.FillRoundedRectangle(brush, new Rectangle((int)(8*s), (int)(20*s), (int)(48*s), (int)(28*s)), (int)(4*s));

            using (SolidBrush brush = new SolidBrush(Color.FromArgb(50, 100, 150)))
                g.FillRoundedRectangle(brush, new Rectangle((int)(8*s), (int)(16*s), (int)(48*s), (int)(8*s)), (int)(4*s));

            using (Pen pen = new Pen(Color.FromArgb(0, 255, 100), Math.Max(1, (int)(2*s))))
                g.DrawLine(pen, (int)(12*s), (int)(32*s), (int)(52*s), (int)(32*s));

            using (SolidBrush brush = new SolidBrush(Color.White))
                g.FillRectangle(brush, new Rectangle((int)(20*s), (int)(8*s), (int)(24*s), (int)(12*s)));
            
            using (Pen pen = new Pen(Color.Gray, Math.Max(1, (int)(1*s))))
            {
                g.DrawRectangle(pen, new Rectangle((int)(20*s), (int)(8*s), (int)(24*s), (int)(12*s)));
                if (size >= 32)
                {
                    g.DrawLine(pen, (int)(22*s), (int)(11*s), (int)(40*s), (int)(11*s));
                    g.DrawLine(pen, (int)(22*s), (int)(14*s), (int)(40*s), (int)(14*s));
                    g.DrawLine(pen, (int)(22*s), (int)(17*s), (int)(35*s), (int)(17*s));
                }
            }

            if (size >= 32)
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(100, 200, 100)))
                    g.FillEllipse(brush, new Rectangle((int)(50*s), (int)(22*s), (int)(6*s), (int)(6*s)));
        }
        return bmp;
    }
}
'@

# Compilar y ejecutar el generador de iconos
Add-Type -TypeDefinition $iconGenCode -ReferencedAssemblies System.Drawing, System.Windows.Forms
[IconGen]::Main()

Write-Host "Icono generado exitosamente!" -ForegroundColor Green
