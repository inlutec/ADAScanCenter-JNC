using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using PdfiumViewer;

namespace ADAScanCenter.UI
{
    public class ImageEditorForm : Form
    {
        private string _pdfPath;
        private PdfDocument _document;
        private int _currentPageIndex = 0;
        private PictureBox _pictureBox;
        private FlowLayoutPanel _thumbnailsPanel;
        private Rectangle _cropRect;
        private bool _isSelecting = false;
        private Point _startPoint;
        private Label _lblHint;
        private List<Button> _thumbnailButtons = new List<Button>();

        private static readonly Color PrimaryColor = Color.FromArgb(70, 130, 180);
        private static readonly Color accentGreen = Color.FromArgb(46, 175, 80);
        private static readonly Color accentBlue = Color.FromArgb(52, 152, 219);
        private static readonly Color accentGray = Color.FromArgb(149, 165, 166);

        public bool SavedAny { get; private set; } = false;

        public ImageEditorForm(string pdfPath)
        {
            _pdfPath = pdfPath;
            InitializeComponent();
            LoadPdf();
        }

        private void InitializeComponent()
        {
            this.Text = "Editor de Escaneo - ADAScanCenter";
            this.Size = new Size(1100, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.Font = new Font("Segoe UI", 9);

            var mainLayout = new TableLayoutPanel();
            mainLayout.Dock = DockStyle.Fill;
            mainLayout.Padding = new Padding(0);
            mainLayout.ColumnCount = 2;
            mainLayout.RowCount = 3;
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));

            // Barra superior
            var headerPanel = CreateHeaderPanel();
            mainLayout.Controls.Add(headerPanel, 0, 0);
            mainLayout.SetColumnSpan(headerPanel, 2);

            // Panel lateral de páginas
            var sidebarTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1,
                BackColor = Color.White,
                Padding = new Padding(0)
            };
            sidebarTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            sidebarTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            var lblPages = new Label
            {
                Text = "Páginas",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                AutoSize = true,
                Margin = new Padding(16, 12, 0, 0)
            };
            _thumbnailsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(12, 4, 12, 12),
                BackColor = Color.White
            };
            sidebarTable.Controls.Add(lblPages, 0, 0);
            sidebarTable.Controls.Add(_thumbnailsPanel, 0, 1);
            mainLayout.Controls.Add(sidebarTable, 0, 1);

            // Área de visualización
            var scrollBox = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.FromArgb(44, 62, 80),
                Padding = new Padding(16)
            };
            _pictureBox = new PictureBox();
            _pictureBox.SizeMode = PictureBoxSizeMode.AutoSize;
            _pictureBox.Cursor = Cursors.Cross;
            _pictureBox.BackColor = Color.White;
            _pictureBox.MouseDown += PictureBox_MouseDown;
            _pictureBox.MouseMove += PictureBox_MouseMove;
            _pictureBox.MouseUp += PictureBox_MouseUp;
            _pictureBox.Paint += PictureBox_Paint;
            scrollBox.Controls.Add(_pictureBox);
            mainLayout.Controls.Add(scrollBox, 1, 1);

            // Barra de herramientas inferior
            var toolbarPanel = CreateToolbarPanel();
            mainLayout.Controls.Add(toolbarPanel, 0, 2);
            mainLayout.SetColumnSpan(toolbarPanel, 2);

            this.Controls.Add(mainLayout);
        }

        private Panel CreateHeaderPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                Height = 52,
                BackColor = PrimaryColor,
                Padding = new Padding(16, 0, 16, 0)
            };

            var lblTitle = new Label
            {
                Text = "Editor de Escaneo",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 14)
            };

            _lblHint = new Label
            {
                Text = "Arrastra el mouse para seleccionar el área a recortar",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(230, 230, 230),
                AutoSize = true,
                Location = new Point(220, 17)
            };

            panel.Controls.Add(lblTitle);
            panel.Controls.Add(_lblHint);
            return panel;
        }

        private Panel CreateToolbarPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                Height = 72,
                BackColor = Color.White,
                Padding = new Padding(20, 12, 20, 12)
            };

            var toolLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 5,
                RowCount = 1,
                Padding = new Padding(0)
            };
            toolLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200));
            toolLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200));
            toolLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));
            toolLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            toolLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));

            int btnH = 44;

            var btnSaveAll = CreateToolButton("Guardar Todas las Páginas", accentGreen, 200, btnH);
            btnSaveAll.Click += BtnSaveAll_Click;
            toolLayout.Controls.Add(btnSaveAll, 0, 0);

            var btnSaveCurrent = CreateToolButton("Guardar Página Actual", accentBlue, 200, btnH);
            btnSaveCurrent.Click += BtnSaveCurrent_Click;
            toolLayout.Controls.Add(btnSaveCurrent, 1, 0);

            var btnSaveCrop = CreateToolButton("Guardar Recorte", accentGreen, 180, btnH);
            btnSaveCrop.Click += BtnSaveCrop_Click;
            toolLayout.Controls.Add(btnSaveCrop, 2, 0);

            var btnClose = CreateToolButton("Cerrar", accentGray, 100, btnH);
            btnClose.Click += (s, e) => this.Close();
            toolLayout.Controls.Add(btnClose, 4, 0);

            panel.Controls.Add(toolLayout);
            return panel;
        }

        private Button CreateToolButton(string text, Color baseColor, int width, int height)
        {
            var btn = new Button
            {
                Text = text,
                Size = new Size(width, height),
                BackColor = baseColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = ControlPaint.Light(baseColor, 0.15f);
            btn.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(baseColor, 0.1f);

            btn.MouseEnter += (s, e) => btn.BackColor = ControlPaint.Light(baseColor, 0.15f);
            btn.MouseLeave += (s, e) => btn.BackColor = baseColor;

            return btn;
        }

        private void LoadPdf()
        {
            try
            {
                _document = PdfDocument.Load(_pdfPath);
                _thumbnailButtons.Clear();
                _thumbnailsPanel.Controls.Clear();

                for (int i = 0; i < _document.PageCount; i++)
                {
                    int index = i;
                    var btn = CreateThumbnailButton($"Página {i + 1}", index);
                    _thumbnailButtons.Add(btn);
                    _thumbnailsPanel.Controls.Add(btn);
                }

                if (_document.PageCount > 0) ShowPage(0);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error cargando PDF: {ex.Message}");
            }
        }

        private Button CreateThumbnailButton(string text, int index)
        {
            var btn = new Button
            {
                Text = text,
                Size = new Size(196, 42),
                Margin = new Padding(0, 4, 0, 4),
                BackColor = Color.FromArgb(248, 249, 250),
                ForeColor = Color.FromArgb(52, 73, 94),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10),
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 0, 0)
            };
            btn.FlatAppearance.BorderColor = Color.FromArgb(227, 229, 232);
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(236, 240, 241);
            btn.Click += (s, e) => ShowPage(index);
            return btn;
        }

        private void UpdateThumbnailSelection()
        {
            for (int i = 0; i < _thumbnailButtons.Count; i++)
            {
                var btn = _thumbnailButtons[i];
                if (i == _currentPageIndex)
                {
                    btn.BackColor = PrimaryColor;
                    btn.ForeColor = Color.White;
                    btn.FlatAppearance.BorderColor = Color.FromArgb(50, 110, 160);
                }
                else
                {
                    btn.BackColor = Color.FromArgb(248, 249, 250);
                    btn.ForeColor = Color.FromArgb(52, 73, 94);
                    btn.FlatAppearance.BorderColor = Color.FromArgb(227, 229, 232);
                }
            }
        }

        private void ShowPage(int index)
        {
            _currentPageIndex = index;
            UpdateThumbnailSelection();

            var size = _document.PageSizes[index];
            int width = (int)(size.Width / 72.0f * 150);
            int height = (int)(size.Height / 72.0f * 150);

            var image = _document.Render(index, width, height, 150, 150, false);
            _pictureBox.Image = image;
            _cropRect = Rectangle.Empty;
            _pictureBox.Invalidate();
        }

        // --- Lógica de Recorte ---

        private void PictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isSelecting = true;
                _startPoint = e.Location;
                _cropRect = Rectangle.Empty;
                _pictureBox.Invalidate();
            }
        }

        private void PictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isSelecting)
            {
                int x = Math.Min(_startPoint.X, e.X);
                int y = Math.Min(_startPoint.Y, e.Y);
                int w = Math.Abs(_startPoint.X - e.X);
                int h = Math.Abs(_startPoint.Y - e.Y);
                _cropRect = new Rectangle(x, y, w, h);
                _pictureBox.Invalidate();
            }
        }

        private void PictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            _isSelecting = false;
            if (_lblHint != null)
                _lblHint.Text = _cropRect.Width > 0 && _cropRect.Height > 0
                    ? "Área seleccionada. Haz clic en «Guardar Recorte» para guardar."
                    : "Arrastra el mouse para seleccionar el área a recortar";
        }

        private void PictureBox_Paint(object sender, PaintEventArgs e)
        {
            if (_cropRect != Rectangle.Empty && _cropRect.Width > 0 && _cropRect.Height > 0)
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                // Overlay oscuro en el área exterior (más profesional)
                using (var region = new Region(_pictureBox.ClientRectangle))
                {
                    region.Exclude(_cropRect);
                    using (Brush brush = new SolidBrush(Color.FromArgb(120, 0, 0, 0)))
                        e.Graphics.FillRegion(brush, region);
                }

                // Borde del área de recorte - estilo limpio
                using (Pen pen = new Pen(Color.FromArgb(46, 175, 80), 3))
                {
                    pen.DashStyle = DashStyle.Dash;
                    pen.DashCap = DashCap.Round;
                    e.Graphics.DrawRectangle(pen, _cropRect);
                }

                // Borde interior blanco para contraste
                using (Pen innerPen = new Pen(Color.White, 1))
                {
                    innerPen.DashStyle = DashStyle.Dash;
                    var inner = Rectangle.Inflate(_cropRect, -2, -2);
                    e.Graphics.DrawRectangle(innerPen, inner);
                }
            }
        }

        // --- Guardado ---

        private void SaveImage(Image img, string suffix)
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "Imagen JPG|*.jpg";
                sfd.FileName = Path.GetFileNameWithoutExtension(_pdfPath) + suffix + ".jpg";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    img.Save(sfd.FileName, ImageFormat.Jpeg);
                    SavedAny = true;
                    MessageBox.Show("Guardado.");
                }
            }
        }

        private void BtnSaveCurrent_Click(object sender, EventArgs e)
        {
            if (_pictureBox.Image != null)
                SaveImage(_pictureBox.Image, $"_pag{_currentPageIndex + 1}");
        }

        private void BtnSaveCrop_Click(object sender, EventArgs e)
        {
            if (_cropRect.Width > 0 && _cropRect.Height > 0 && _pictureBox.Image != null)
            {
                try
                {
                    Bitmap src = (Bitmap)_pictureBox.Image;
                    Bitmap target = new Bitmap(_cropRect.Width, _cropRect.Height);
                    using (Graphics g = Graphics.FromImage(target))
                    {
                        g.DrawImage(src, new Rectangle(0, 0, target.Width, target.Height), 
                                    _cropRect, 
                                    GraphicsUnit.Pixel);
                    }
                    SaveImage(target, $"_pag{_currentPageIndex + 1}_crop");
                }
                catch (Exception ex) 
                {
                    MessageBox.Show("Error al recortar: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Por favor, seleccione un área primero arrastrando el mouse sobre la imagen.");
            }
        }

        private void BtnSaveAll_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    for (int i = 0; i < _document.PageCount; i++)
                    {
                        var size = _document.PageSizes[i];
                        int width = (int)(size.Width / 72.0f * 150);
                        int height = (int)(size.Height / 72.0f * 150);
                        
                        using (var img = _document.Render(i, width, height, 150, 150, false))
                        {
                            string fname = Path.Combine(fbd.SelectedPath, $"{Path.GetFileNameWithoutExtension(_pdfPath)}_pag{i + 1}.jpg");
                            img.Save(fname, ImageFormat.Jpeg);
                        }
                    }
                    SavedAny = true;
                    MessageBox.Show("Todas las páginas guardadas.");
                    this.Close();
                }
            }
        }
    }
}
