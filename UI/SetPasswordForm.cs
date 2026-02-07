using System;
using System.Drawing;
using System.Windows.Forms;

namespace ADAScanCenter.UI
{
    public class SetPasswordForm : Form
    {
        public string NewPassword { get; private set; }
        private TextBox txtPass1, txtPass2;
        private Button btnOk;

        public SetPasswordForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Configurar Contraseña de Admin";
            this.Size = new Size(350, 220);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var lblInfo = new Label() { 
                Text = "Bienvenido a ADAScanCenter.\nPor favor, establezca una contraseña para acceder a la configuración.", 
                Top = 15, Left = 20, Width = 300, Height = 40 
            };
            
            var lbl1 = new Label() { Text = "Contraseña:", Top = 65, Left = 20, Width = 100 };
            txtPass1 = new TextBox() { Top = 62, Left = 120, Width = 180, PasswordChar = '*' };

            var lbl2 = new Label() { Text = "Repetir:", Top = 95, Left = 20, Width = 100 };
            txtPass2 = new TextBox() { Top = 92, Left = 120, Width = 180, PasswordChar = '*' };

            btnOk = new Button() { Text = "Guardar", Top = 130, Left = 120, Width = 100, DialogResult = DialogResult.OK };
            btnOk.Click += BtnOk_Click;

            this.Controls.Add(lblInfo);
            this.Controls.Add(lbl1);
            this.Controls.Add(txtPass1);
            this.Controls.Add(lbl2);
            this.Controls.Add(txtPass2);
            this.Controls.Add(btnOk);
            
            this.AcceptButton = btnOk;
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPass1.Text))
            {
                MessageBox.Show("La contraseña no puede estar vacía.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (txtPass1.Text != txtPass2.Text)
            {
                MessageBox.Show("Las contraseñas no coinciden.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            NewPassword = txtPass1.Text;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
