using System;
using System.Drawing;
using System.Windows.Forms;

namespace ADAScanCenter.UI
{
    public class PasswordPrompt : Form
    {
        private TextBox txtPassword;
        private Button btnOk;
        private Button btnCancel;
        public string EnteredPassword => txtPassword.Text;

        public PasswordPrompt()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Acceso Administrativo";
            this.Size = new Size(300, 150);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var label = new Label() { Text = "Ingrese contraseña de administrador:", Top = 20, Left = 20, Width = 250 };
            
            txtPassword = new TextBox() { Top = 45, Left = 20, Width = 240, PasswordChar = '*' };
            
            btnOk = new Button() { Text = "Aceptar", Top = 80, Left = 100, DialogResult = DialogResult.OK };
            btnCancel = new Button() { Text = "Cancelar", Top = 80, Left = 180, DialogResult = DialogResult.Cancel };

            btnOk.Click += (s, e) => { this.DialogResult = DialogResult.OK; this.Close(); };
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            this.Controls.Add(label);
            this.Controls.Add(txtPassword);
            this.Controls.Add(btnOk);
            this.Controls.Add(btnCancel);
            this.AcceptButton = btnOk;
            this.CancelButton = btnCancel;
        }
    }
}
