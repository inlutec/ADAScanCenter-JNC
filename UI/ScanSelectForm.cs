using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ADAScanCenter.Services;

namespace ADAScanCenter.UI
{
    public class ScanSelectForm : Form
    {
        public ScanReceivedEventArgs? SelectedScan { get; private set; }

        public ScanSelectForm(List<ScanReceivedEventArgs> scans)
        {
            InitializeComponent(scans);
        }

        private void InitializeComponent(List<ScanReceivedEventArgs> scans)
        {
            this.Text = "Varios escaneos detectados - Seleccione uno";
            this.Size = new Size(500, 200 + scans.Count * 50);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.TopMost = true;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(248, 249, 250);

            var label = new Label
            {
                Text = "Se han detectado varios correos con adjuntos.\nSeleccione el que desea procesar:",
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                Location = new Point(20, 16),
                AutoSize = true
            };
            this.Controls.Add(label);

            int y = 60;
            foreach (var scan in scans)
            {
                string displayText = $"{scan.OriginalFileName}";
                if (!string.IsNullOrEmpty(scan.Subject))
                    displayText += $" - {scan.Subject}";
                if (scan.Date.HasValue)
                    displayText += $" ({scan.Date:dd/MM/yyyy HH:mm})";

                var btn = new Button
                {
                    Text = displayText.Length > 70 ? displayText.Substring(0, 67) + "..." : displayText,
                    Tag = scan,
                    Location = new Point(20, y),
                    Size = new Size(440, 40),
                    BackColor = Color.FromArgb(70, 130, 180),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(12, 0, 0, 0),
                    Cursor = Cursors.Hand
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.MouseEnter += (s, e) => btn.BackColor = Color.FromArgb(90, 150, 200);
                btn.MouseLeave += (s, e) => btn.BackColor = Color.FromArgb(70, 130, 180);
                btn.Click += (s, e) =>
                {
                    SelectedScan = (ScanReceivedEventArgs)((Button)s!).Tag!;
                    DialogResult = DialogResult.OK;
                    Close();
                };
                this.Controls.Add(btn);
                y += 48;
            }

            var btnCancel = new Button
            {
                Text = "Cancelar",
                Location = new Point(20, y + 12),
                Size = new Size(100, 36),
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            this.Controls.Add(btnCancel);
        }
    }
}
