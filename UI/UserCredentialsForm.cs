using System;
using System.Drawing;
using System.Windows.Forms;
using ADAScanCenter.Services;

namespace ADAScanCenter.UI
{
    /// <summary>
    /// Formulario para que el usuario actualice su email y contraseña de correo.
    /// Sin contraseña de administrador. Los datos IMAP se configuran en Administración (tray).
    /// </summary>
    public class UserCredentialsForm : Form
    {
        private ConfigService _configService;
        private TextBox txtEmail;
        private TextBox txtPassword;
        private Button btnTest;
        private Button btnSave;
        private Button btnCancel;

        public UserCredentialsForm(ConfigService configService)
        {
            _configService = configService;
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Mis credenciales de correo";
            this.Size = new Size(400, 280);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(248, 249, 250);

            var lblInfo = new Label
            {
                Text = "Si ha cambiado su contraseña de correo, actualícela aquí.\nLos datos del servidor IMAP se configuran en Administración (icono bandeja).",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(80, 80, 80),
                Location = new Point(20, 16),
                Size = new Size(350, 40),
                AutoSize = false
            };
            this.Controls.Add(lblInfo);

            int y = 70;
            int lblW = 90;
            int txtW = 250;

            var lblEmail = new Label { Text = "Email:", TextAlign = ContentAlignment.MiddleRight, Location = new Point(20, y + 3), Size = new Size(lblW, 20) };
            txtEmail = new TextBox { Location = new Point(lblW + 30, y), Size = new Size(txtW, 24) };
            this.Controls.Add(lblEmail);
            this.Controls.Add(txtEmail);
            y += 35;

            var lblPass = new Label { Text = "Contraseña:", TextAlign = ContentAlignment.MiddleRight, Location = new Point(20, y + 3), Size = new Size(lblW, 20) };
            txtPassword = new TextBox { Location = new Point(lblW + 30, y), Size = new Size(txtW, 24), PasswordChar = '*' };
            this.Controls.Add(lblPass);
            this.Controls.Add(txtPassword);
            y += 45;

            btnTest = new Button
            {
                Text = "Probar Conexión",
                Location = new Point(lblW + 30, y),
                Size = new Size(130, 32),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnTest.FlatAppearance.BorderSize = 0;
            btnTest.Click += BtnTest_Click;
            this.Controls.Add(btnTest);
            y += 50;

            btnSave = new Button { Text = "Guardar", Location = new Point(120, y), Size = new Size(90, 34), DialogResult = DialogResult.OK };
            btnSave.Click += BtnSave_Click;
            btnCancel = new Button { Text = "Cancelar", Location = new Point(220, y), Size = new Size(90, 34), DialogResult = DialogResult.Cancel };

            this.Controls.Add(btnSave);
            this.Controls.Add(btnCancel);
            this.AcceptButton = btnSave;
            this.CancelButton = btnCancel;
        }

        private void LoadData()
        {
            var config = _configService.CurrentConfig;
            txtEmail.Text = config.EmailUser;
            txtPassword.Text = _configService.DecryptPassword(config.EmailPasswordEncrypted);
        }

        private async void BtnTest_Click(object? sender, EventArgs e)
        {
            btnTest.Enabled = false;
            btnTest.Text = "Probando...";

            try
            {
                var config = _configService.CurrentConfig;
                if (!config.IsValid())
                {
                    MessageBox.Show("Configure primero el servidor IMAP en Administración (icono de la bandeja).", "Configuración incompleta", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                using (var svc = new ImapService(_configService))
                {
                    string result = await svc.TestConnectionAsync(config.ImapServer, config.ImapPort, config.UseSsl, txtEmail.Text, txtPassword.Text);
                    MessageBox.Show(result, "Resultado Prueba", MessageBoxButtons.OK, result.Contains("Exitosa") ? MessageBoxIcon.Information : MessageBoxIcon.Error);
                }
            }
            finally
            {
                btnTest.Enabled = true;
                btnTest.Text = "Probar Conexión";
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            try
            {
                var config = _configService.CurrentConfig;
                config.EmailUser = txtEmail.Text.Trim();
                config.EmailPasswordEncrypted = _configService.EncryptPassword(txtPassword.Text);
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
