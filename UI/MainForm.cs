using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using ADAScanCenter;
using ADAScanCenter.Services;

namespace ADAScanCenter.UI
{
    public class MainForm : Form
    {
        private ConfigService _configService;
        private AppTrayContext? _trayContext;
        private Label lblStatus = null!;
        private Button btnOpenFolder = null!;
        private Button btnCheckNow = null!;
        private Button btnSettings = null!;
        private PictureBox picLogo = null!;
        private Label lblVersion = null!;

        public MainForm(ConfigService configService, AppTrayContext? trayContext)
        {
            _configService = configService;
            _trayContext = trayContext;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "ADAScanCenter - Panel de Control";
            this.Size = new Size(480, 450);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.Padding = new Padding(24, 24, 24, 24);
            this.MinimumSize = new Size(400, 380);

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                Padding = new Padding(0)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 130));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 168));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));

            // Logo ADA
            picLogo = new PictureBox
            {
                Size = new Size(120, 120),
                SizeMode = PictureBoxSizeMode.Zoom,
                Anchor = AnchorStyles.None
            };
            LoadLogoImage();
            var logoCell = new TableLayoutPanel();
            logoCell.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            logoCell.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            logoCell.Dock = DockStyle.Fill;
            logoCell.Controls.Add(picLogo, 0, 0);
            mainLayout.Controls.Add(logoCell, 0, 0);

            // Título
            var lblTitle = new Label
            {
                Text = "ADAScanCenter",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                AutoSize = true,
                Anchor = AnchorStyles.None
            };
            var titlePanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
            titlePanel.Controls.Add(lblTitle);
            mainLayout.Controls.Add(titlePanel, 0, 1);

            // Estado
            lblStatus = new Label
            {
                Text = "● Servicio en ejecución",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(46, 175, 80),
                AutoSize = true,
                Anchor = AnchorStyles.None
            };
            var statusPanel = new FlowLayoutPanel { Dock = DockStyle.Fill };
            statusPanel.Controls.Add(lblStatus);
            mainLayout.Controls.Add(statusPanel, 0, 2);

            // Botones
            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                Padding = new Padding(0, 12, 0, 0)
            };

            int btnW = 260;
            int btnH = 44;
            int gap = 12;

            btnOpenFolder = CreateStyledButton("📁 Abrir Carpeta de Escaneos", btnW, btnH, Color.FromArgb(70, 130, 180));
            btnOpenFolder.Click += BtnOpenFolder_Click;

            btnCheckNow = CreateStyledButton("🔄 Comprobar Escaneo", btnW, btnH, Color.FromArgb(52, 152, 219));
            btnCheckNow.Click += BtnCheckNow_Click;
            if (_trayContext == null)
            {
                btnCheckNow.Enabled = false;
                btnCheckNow.BackColor = Color.FromArgb(180, 180, 180);
            }

            btnSettings = CreateStyledButton("⚙️", 52, btnH, Color.FromArgb(149, 165, 166));
            btnSettings.Font = new Font("Segoe UI", 18, FontStyle.Regular);
            btnSettings.Click += BtnSettings_Click;
            var toolTip = new ToolTip();
            toolTip.SetToolTip(btnSettings, "Mis credenciales de correo");
            var gearWrap = new Panel { Width = btnW, Height = btnH + 4 };
            btnSettings.Location = new Point(40, 0);
            gearWrap.Controls.Add(btnSettings);

            buttonPanel.Controls.Add(btnOpenFolder);
            buttonPanel.Controls.Add(new Panel { Height = gap });
            buttonPanel.Controls.Add(btnCheckNow);
            buttonPanel.Controls.Add(new Panel { Height = gap });
            buttonPanel.Controls.Add(gearWrap);

            mainLayout.Controls.Add(buttonPanel, 0, 3);

            // Pie: versión y cerrar
            var footerTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Padding = new Padding(0)
            };
            footerTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            footerTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            footerTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
            lblVersion = new Label
            {
                Text = "Versión 1.0.0",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                AutoSize = true,
                Anchor = AnchorStyles.Left | AnchorStyles.Top
            };
            var btnClose = CreateStyledButton("Cerrar", 90, 36, Color.FromArgb(127, 140, 141));
            btnClose.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            btnClose.Click += (s, e) => this.Close();

            footerTable.Controls.Add(lblVersion, 0, 0);
            footerTable.Controls.Add(btnClose, 2, 0);

            mainLayout.Controls.Add(footerTable, 0, 4);

            this.Controls.Add(mainLayout);
        }

        private void LoadLogoImage()
        {
            try
            {
                string baseDir = Path.GetDirectoryName(Environment.ProcessPath ?? AppDomain.CurrentDomain.BaseDirectory) ?? "";
                string imagePath = Path.Combine(baseDir, "channels4_profile.jpg");
                if (File.Exists(imagePath))
                {
                    picLogo.Image = Image.FromFile(imagePath);
                }
                else
                {
                    picLogo.Image = CreateFallbackLogo();
                }
            }
            catch
            {
                picLogo.Image = CreateFallbackLogo();
            }
        }

        private Image CreateFallbackLogo()
        {
            using (var bmp = new Bitmap(120, 120))
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);
                using (var brush = new SolidBrush(Color.FromArgb(70, 130, 180)))
                {
                    g.FillRoundedRectangle(brush, new Rectangle(16, 40, 88, 50), 8);
                }
                using (var brush = new SolidBrush(Color.FromArgb(50, 100, 150)))
                {
                    g.FillRoundedRectangle(brush, new Rectangle(16, 32, 88, 16), 8);
                }
                using (var pen = new Pen(Color.FromArgb(0, 255, 100), 4))
                {
                    g.DrawLine(pen, 24, 64, 100, 64);
                }
                return (Image)bmp.Clone();
            }
        }

        private Button CreateStyledButton(string text, int width, int height, Color color)
        {
            var btn = new Button
            {
                Text = text,
                Size = new Size(width, height),
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.MouseEnter += (s, e) => btn.BackColor = ControlPaint.Light(color, 0.15f);
            btn.MouseLeave += (s, e) => btn.BackColor = color;
            return btn;
        }

        private void BtnOpenFolder_Click(object? sender, EventArgs e)
        {
            try
            {
                string path = _configService.CurrentConfig.OutputDirectory;
                if (Directory.Exists(path))
                {
                    System.Diagnostics.Process.Start("explorer.exe", path);
                }
                else
                {
                    MessageBox.Show($"La carpeta no existe: {path}\n\nPor favor, configura la ruta en Administración (icono bandeja).",
                        "Carpeta no encontrada", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir carpeta: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCheckNow_Click(object? sender, EventArgs e)
        {
            if (_trayContext != null)
            {
                _trayContext.ForceCheckNow();
                MessageBox.Show("Buscando escaneos nuevos...\n\nSe mostrará un aviso si hay escaneos pendientes.",
                    "Comprobar Escaneo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnSettings_Click(object? sender, EventArgs e)
        {
            using (var credForm = new UserCredentialsForm(_configService))
            {
                credForm.ShowDialog();
            }
        }
    }
}
