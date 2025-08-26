using System.Drawing;
using System.Windows.Forms;

namespace AVCS
{
    partial class Form1
    {
        private System.ComponentModel.IContainer _components = null;

        private ToolTip _toolTip;
        private Label _lblTitle;
        private Label _lblBody;
        private Button _btnChecksum;
        private Button _btnContinue;
        private Button _btnCancel;
        private CheckBox _chkSelectAll;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (_components != null))
            {
                _components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            _toolTip = new ToolTip();
            _lblTitle = new Label();
            _lblBody = new Label();
            _panel = new Panel();
            _layout = new TableLayoutPanel();
            _chkSelectAll = new CheckBox();
            _btnChecksum = new Button();
            _checkRow = new FlowLayoutPanel();
            _buttonRow = new FlowLayoutPanel();
            _btnContinue = new Button();
            _btnCancel = new Button();
            _panel.SuspendLayout();
            _layout.SuspendLayout();
            SuspendLayout();
            // 
            // _lblTitle
            // 
            _lblTitle.Dock = DockStyle.Top;
            _lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            _lblTitle.ForeColor = Color.White;
            _lblTitle.Location = new Point(0, 0);
            _lblTitle.Name = "_lblTitle";
            _lblTitle.Size = new Size(560, 54);
            _lblTitle.TabIndex = 2;
            _lblTitle.Text = "AVCS CORE DEV TOOLKIT";
            _lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // _lblBody
            // 
            _lblBody.Dock = DockStyle.Top;
            _lblBody.Font = new Font("Segoe UI", 10F);
            _lblBody.ForeColor = Color.Gainsboro;
            _lblBody.Location = new Point(0, 54);
            _lblBody.Name = "_lblBody";
            _lblBody.Size = new Size(560, 80);
            _lblBody.TabIndex = 1;
            _lblBody.Text = "You may use this tool to validate compiled inline functions\nagainst a supplied hashtable, or to run specialized tests.\nChoose a button to proceed";
            _lblBody.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // _panel
            // 
            _panel.BackColor = Color.FromArgb(32, 32, 32);
            _panel.Controls.Add(_layout);
            _panel.Dock = DockStyle.Fill;
            _panel.Location = new Point(0, 134);
            _panel.Name = "_panel";
            _panel.Padding = new Padding(20, 10, 20, 20);
            _panel.Size = new Size(560, 166);
            _panel.TabIndex = 0;
            // 
            // _layout
            // 
            _layout.BackColor = Color.FromArgb(32, 32, 32);
            _layout.ColumnCount = 3;
            _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34F));
            _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            _layout.Controls.Add(_btnChecksum, 0, 1);
            _layout.Controls.Add(_checkRow, 1, 2);
            _layout.Controls.Add(_btnContinue, 1, 1);
            _layout.Controls.Add(_btnCancel, 2, 1);
            _layout.Controls.Add(_buttonRow, 1, 2);
            _layout.Controls.Add(_chkSelectAll, 0, 2);
            _layout.Dock = DockStyle.Fill;
            _layout.Location = new Point(20, 10);
            _layout.Name = "_layout";
            _layout.RowCount = 3;
            _layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46F));
            _layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            _layout.Size = new Size(520, 136);
            _layout.TabIndex = 0;
            // 
            // _chkSelectAll
            // 
            _chkSelectAll.Anchor = AnchorStyles.Top;
            _chkSelectAll.AutoSize = true;
            _chkSelectAll.ForeColor = Color.Gainsboro;
            _chkSelectAll.Location = new Point(8, 94);
            _chkSelectAll.Name = "_chkSelectAll";
            _chkSelectAll.Size = new Size(155, 19);
            _chkSelectAll.TabIndex = 0;
            _toolTip.SetToolTip(_chkSelectAll, "Select folder instead of one file to validate all against supplied hashtable.");
            _chkSelectAll.Text = "Select all from hashtable";
            // 
            // _btnChecksum
            // 
            _btnChecksum.BackColor = Color.FromArgb(64, 64, 64);
            _btnChecksum.Cursor = Cursors.Hand;
            _btnChecksum.FlatAppearance.BorderSize = 0;
            _btnChecksum.FlatStyle = FlatStyle.Flat;
            _btnChecksum.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            _btnChecksum.ForeColor = Color.White;
            _btnChecksum.Location = new Point(3, 48);
            _btnChecksum.Name = "_btnChecksum";
            _btnChecksum.Size = new Size(165, 40);
            _btnChecksum.TabIndex = 0;
            _btnChecksum.Text = "Checksum";
            _btnChecksum.UseVisualStyleBackColor = false;
            _toolTip.SetToolTip(_btnChecksum, "Supply a hashtable, then select a compiled inline function file to run checksum validation against the hashtable.");
            _btnChecksum.Click += btnChecksum_Click;
            // 
            // _checkRow
            // 
            _checkRow.Anchor = AnchorStyles.Top;
            _checkRow.AutoSize = true;
            _checkRow.Location = new Point(259, 94);
            _checkRow.Name = "_checkRow";
            _checkRow.Padding = new Padding(0, 6, 0, 0);
            _checkRow.Size = new Size(0, 6);
            _checkRow.TabIndex = 3;
            // 
            // _buttonRow
            // 
            _buttonRow.Anchor = AnchorStyles.None;
            _buttonRow.AutoSize = true;
            _buttonRow.BackColor = Color.FromArgb(32, 32, 32);
            _buttonRow.Location = new Point(433, 113);
            _buttonRow.Name = "_buttonRow";
            _buttonRow.Size = new Size(0, 0);
            _buttonRow.TabIndex = 1;
            _buttonRow.WrapContents = false;
            // 
            // _btnContinue
            // 
            _btnContinue.BackColor = Color.FromArgb(64, 64, 64);
            _btnContinue.Cursor = Cursors.Hand;
            _btnContinue.FlatAppearance.BorderSize = 0;
            _btnContinue.FlatStyle = FlatStyle.Flat;
            _btnContinue.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            _btnContinue.ForeColor = Color.White;
            _btnContinue.Location = new Point(174, 48);
            _btnContinue.Name = "_btnContinue";
            _btnContinue.Size = new Size(167, 40);
            _btnContinue.TabIndex = 2;
            _btnContinue.Text = "Run Tests";
            _btnContinue.UseVisualStyleBackColor = false;
            _toolTip.SetToolTip(_btnContinue, "Run any specialized tests set up the in Program.cs test method.");
            _btnContinue.Click += btnContinue_Click;
            // 
            // _btnCancel
            // 
            _btnCancel.BackColor = Color.FromArgb(64, 64, 64);
            _btnCancel.Cursor = Cursors.Hand;
            _btnCancel.FlatAppearance.BorderSize = 0;
            _btnCancel.FlatStyle = FlatStyle.Flat;
            _btnCancel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            _btnCancel.ForeColor = Color.White;
            _btnCancel.Location = new Point(350, 48);
            _btnCancel.Name = "_btnCancel";
            _btnCancel.Size = new Size(167, 40);
            _btnCancel.TabIndex = 2;
            _btnCancel.Text = "Cancel / Exit";
            _btnCancel.UseVisualStyleBackColor = false;
            _toolTip.SetToolTip(_btnCancel, "Cancel / Exit");
            _btnCancel.Click += btnCancel_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(32, 32, 32);
            ClientSize = new Size(560, 300);
            Controls.Add(_panel);
            Controls.Add(_lblBody);
            Controls.Add(_lblTitle);
            ForeColor = Color.White;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "AVCS CORE DEV TOOLKIT";
            _panel.ResumeLayout(false);
            _layout.ResumeLayout(false);
            _layout.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel _panel;
        private TableLayoutPanel _layout;
        private FlowLayoutPanel _buttonRow;
        private FlowLayoutPanel _checkRow;
    }
}
