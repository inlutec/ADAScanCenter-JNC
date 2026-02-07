using System;
using System.Drawing;
using System.Windows.Forms;

namespace ADAScanCenter.UI
{
    public enum NewScanResult
    {
        SaveAsPdf,
        SaveAsJpg,
        ReviewLater,
        Cancel
    }

    public class NewScanForm : Form
    {
        public bool SaveAsPdf => UserChoice == NewScanResult.SaveAsPdf;
        public bool ShouldSave => UserChoice == NewScanResult.SaveAsPdf || UserChoice == NewScanResult.SaveAsJpg;
        public bool ReviewLater => UserChoice == NewScanResult.ReviewLater;
        public NewScanResult UserChoice { get; private set; } = NewScanResult.Cancel;

        public NewScanForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Nuevo Escaneo Detectado";
            this.Size = new Size(500, 200);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.TopMost = true;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(248, 249, 250);

            var label = new Label()
            {
                Text = "Se ha recibido un nuevo documento escaneado.\n¿Cómo desea guardarlo?",
                Font = new Font("Segoe UI", 12, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleCenter,
                Top = 20,
                Left = 20,
                Width = 440,
                Height = 44
            };

            int btnW = 105;
            int btnH = 60;
            int startY = 80;
            int gap = 10;
            int startX = 20;

            var btnPdf = CreateOptionButton("Guardar PDF", Color.FromArgb(70, 130, 180), startX, startY, btnW, btnH);
            btnPdf.Click += (s, e) => { UserChoice = NewScanResult.SaveAsPdf; this.DialogResult = DialogResult.OK; this.Close(); };

            var btnJpg = CreateOptionButton("Guardar JPG", Color.FromArgb(46, 175, 80), startX + btnW + gap, startY, btnW, btnH);
            btnJpg.Click += (s, e) => { UserChoice = NewScanResult.SaveAsJpg; this.DialogResult = DialogResult.OK; this.Close(); };

            var btnLater = CreateOptionButton("Revisar más tarde", Color.FromArgb(149, 165, 166), startX + (btnW + gap) * 2, startY, btnW, btnH);
            btnLater.Click += (s, e) => { UserChoice = NewScanResult.ReviewLater; this.DialogResult = DialogResult.OK; this.Close(); };

            var btnCancel = CreateOptionButton("Cancelar", Color.FromArgb(127, 140, 141), startX + (btnW + gap) * 3, startY, btnW, btnH);
            btnCancel.Click += (s, e) => { UserChoice = NewScanResult.Cancel; this.DialogResult = DialogResult.Cancel; this.Close(); };

            this.Controls.Add(label);
            this.Controls.Add(btnPdf);
            this.Controls.Add(btnJpg);
            this.Controls.Add(btnLater);
            this.Controls.Add(btnCancel);
        }

        private Button CreateOptionButton(string text, Color color, int x, int y, int w, int h)
        {
            var btn = new Button
            {
                Text = text,
                Bounds = new Rectangle(x, y, w, h),
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.MouseEnter += (s, e) => btn.BackColor = ControlPaint.Light(color, 0.15f);
            btn.MouseLeave += (s, e) => btn.BackColor = color;
            return btn;
        }
    }
}
