using System;
using System.Drawing;
using System.Windows.Forms;
using ADAScanCenter.Services;

namespace ADAScanCenter.UI
{
    public class ConfigForm : Form
    {
        private ConfigService _configService;
        private TextBox txtServer, txtPort, txtEmail, txtPassword, txtFilter, txtPath, txtInterval, txtIdleTimeout;
        private CheckBox chkSsl, chkUseIdle;
        private Button btnCheckIdle;
        private Button btnBrowse, btnSave, btnCancel, btnTest;

        public ConfigForm(ConfigService configService)
        {
            _configService = configService;
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Administración ADAScanCenter";
            this.Size = new Size(520, 540);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;

            int y = 20;
            int gap = 35;
            int lblW = 120;
            int txtW = 250;

            AddControl("Servidor IMAP:", txtServer = new TextBox(), ref y, lblW, txtW);
            AddControl("Puerto:", txtPort = new TextBox(), ref y, lblW, 60);
            
            chkSsl = new CheckBox() { Text = "Usar SSL/TLS", Top = y - gap, Left = lblW + 90, Checked = true };
            this.Controls.Add(chkSsl);

            AddControl("Email Usuario:", txtEmail = new TextBox(), ref y, lblW, txtW);
            AddControl("Contraseña:", txtPassword = new TextBox() { PasswordChar = '*' }, ref y, lblW, txtW);
            
            btnTest = new Button() { Text = "Probar Conexión", Top = y, Left = lblW + 20, Width = 150 };
            btnTest.Click += BtnTest_Click;
            this.Controls.Add(btnTest);
            y += gap;

            AddControl("Remitente (Filtro):", txtFilter = new TextBox(), ref y, lblW, txtW);

            chkUseIdle = new CheckBox()
            {
                Text = "Usar IMAP IDLE (menos carga en servidor)",
                Top = y,
                Left = lblW + 20,
                Width = 260,
                Checked = true
            };
            this.Controls.Add(chkUseIdle);
            btnCheckIdle = new Button()
            {
                Text = "Comprobar soporte",
                Top = y - 2,
                Left = lblW + 290,
                Width = 120,
                Height = 24
            };
            btnCheckIdle.Click += BtnCheckIdle_Click;
            this.Controls.Add(btnCheckIdle);
            y += 30;

            AddControl("Timeout IDLE (seg):", txtIdleTimeout = new TextBox(), ref y, lblW, 60);
            var lblIdleHint = new Label()
            {
                Text = "(60 = recomendado en W11; menor = más respuestas)",
                Top = txtIdleTimeout.Top + 2,
                Left = lblW + 85,
                Width = 250,
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 8)
            };
            this.Controls.Add(lblIdleHint);

            AddControl("Intervalo polling (seg):", txtInterval = new TextBox(), ref y, lblW, 60);
            var lblIntervalHint = new Label()
            {
                Text = "(solo si IDLE no está activo)",
                Top = txtInterval.Top + 2,
                Left = lblW + 85,
                Width = 180,
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 8)
            };
            this.Controls.Add(lblIntervalHint);

            var lblPath = new Label() { Text = "Ruta Destino:", Top = y, Left = 20, Width = lblW };
            txtPath = new TextBox() { Top = y, Left = lblW + 20, Width = 230, ReadOnly = true };
            btnBrowse = new Button() { Text = "...", Top = y - 1, Left = lblW + 260, Width = 30 };
            btnBrowse.Click += BtnBrowse_Click;
            
            this.Controls.Add(lblPath);
            this.Controls.Add(txtPath);
            this.Controls.Add(btnBrowse);
            y += gap + 20;

            btnSave = new Button() { Text = "Guardar", Top = y, Left = 120, Width = 100, DialogResult = DialogResult.OK };
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button() { Text = "Cancelar", Top = y, Left = 240, Width = 100, DialogResult = DialogResult.Cancel };

            this.Controls.Add(btnSave);
            this.Controls.Add(btnCancel);
            
            this.AcceptButton = btnSave;
            this.CancelButton = btnCancel;
        }

        private void AddControl(string labelText, Control inputControl, ref int y, int lblW, int inputW)
        {
            var lbl = new Label() { Text = labelText, Top = y + 3, Left = 20, Width = lblW };
            inputControl.Top = y;
            inputControl.Left = lblW + 20;
            inputControl.Width = inputW;
            
            this.Controls.Add(lbl);
            this.Controls.Add(inputControl);
            y += 35;
        }

        private async void BtnTest_Click(object sender, EventArgs e)
        {
            btnTest.Enabled = false;
            btnTest.Text = "Probando...";
            
            try
            {
                int p = 993;
                int.TryParse(txtPort.Text, out p);
                
                using (var svc = new ImapService(_configService))
                {
                    string result = await svc.TestConnectionAsync(txtServer.Text, p, chkSsl.Checked, txtEmail.Text, txtPassword.Text);
                    MessageBox.Show(result, "Resultado Prueba", MessageBoxButtons.OK, result.Contains("Exitosa") ? MessageBoxIcon.Information : MessageBoxIcon.Error);
                }
            }
            finally
            {
                btnTest.Enabled = true;
                btnTest.Text = "Probar Conexión";
            }
        }

        private async void BtnCheckIdle_Click(object? sender, EventArgs e)
        {
            btnCheckIdle.Enabled = false;
            btnCheckIdle.Text = "Comprobando...";

            try
            {
                int p = 993;
                int.TryParse(txtPort.Text, out p);

                using (var svc = new ImapService(_configService))
                {
                    string result = await svc.CheckIdleSupportAsync(txtServer.Text, p, chkSsl.Checked, txtEmail.Text, txtPassword.Text);
                    bool ok = result.StartsWith("✓");
                    MessageBox.Show(result, "Soporte IMAP IDLE", MessageBoxButtons.OK, ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
                }
            }
            finally
            {
                btnCheckIdle.Enabled = true;
                btnCheckIdle.Text = "Comprobar soporte";
            }
        }

        private void LoadData()
        {
            var config = _configService.CurrentConfig;
            txtServer.Text = config.ImapServer;
            txtPort.Text = config.ImapPort.ToString();
            chkSsl.Checked = config.UseSsl;
            txtEmail.Text = config.EmailUser;
            txtPassword.Text = _configService.DecryptPassword(config.EmailPasswordEncrypted);
            txtFilter.Text = config.SenderEmailFilter;
            txtPath.Text = config.OutputDirectory;
            chkUseIdle.Checked = config.UseImapIdle;
            txtIdleTimeout.Text = config.IdleTimeoutSeconds > 0 ? config.IdleTimeoutSeconds.ToString() : "60";
            txtInterval.Text = config.PollingIntervalSeconds.ToString();
        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    txtPath.Text = fbd.SelectedPath;
                }
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                var config = _configService.CurrentConfig;
                config.ImapServer = txtServer.Text.Trim();
                if (int.TryParse(txtPort.Text, out int p)) config.ImapPort = p;
                config.UseSsl = chkSsl.Checked;
                config.EmailUser = txtEmail.Text.Trim();
                config.EmailPasswordEncrypted = _configService.EncryptPassword(txtPassword.Text);
                config.SenderEmailFilter = txtFilter.Text.Trim();
                config.OutputDirectory = txtPath.Text.Trim();
                config.UseImapIdle = chkUseIdle.Checked;

                if (int.TryParse(txtIdleTimeout.Text, out int idleSec) && idleSec > 0)
                    config.IdleTimeoutSeconds = Math.Clamp(idleSec, 30, 120);

                if (int.TryParse(txtInterval.Text, out int interval) && interval > 0)
                    config.PollingIntervalSeconds = interval;

                _configService.SaveConfig();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
