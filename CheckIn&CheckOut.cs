// UI/TimeInOutForm.cs
// Standalone employee Time In / Time Out WinForms window
using System;
using System.Drawing;
using System.Windows.Forms;

namespace EAttendance.UI
{
    public class TimeInOutForm : Form
    {
        // ── Services ─────────────────────────────────────────────────
        private readonly AttendanceService _svc = new AttendanceService();
        private readonly AttendanceRepository _repo = new AttendanceRepository();
        private readonly User _user;

        // ── Live clock ────────────────────────────────────────────────
        private System.Windows.Forms.Timer _clockTimer;

        // ── Controls ─────────────────────────────────────────────────
        private Label lblClock;
        private Label lblDate;
        private Label lblStatusDot;
        private Label lblStatusVal;
        private Label lblCheckInVal;
        private Label lblCheckOutVal;
        private Label lblHoursVal;
        private Panel pnlHours;
        private ComboBox cboShift;
        private Button btnCheckIn;
        private Button btnCheckOut;
        private Label lblMsg;
        private DataGridView gridHistory;

        // ── Palette ───────────────────────────────────────────────────
        private readonly Color Navy = Color.FromArgb(26, 58, 92);
        private readonly Color NavyDark = Color.FromArgb(15, 42, 74);
        private readonly Color BgPage = Color.FromArgb(240, 242, 245);
        private readonly Color BgCard = Color.FromArgb(255, 255, 255);
        private readonly Color BgInput = Color.FromArgb(252, 252, 254);
        private readonly Color BgTimeBox = Color.FromArgb(247, 248, 251);
        private readonly Color BorderClr = Color.FromArgb(221, 226, 234);
        private readonly Color LabelClr = Color.FromArgb(128, 144, 168);
        private readonly Color TextPrim = Color.FromArgb(26, 58, 92);
        private readonly Color Green = Color.FromArgb(39, 174, 96);
        private readonly Color Red = Color.FromArgb(192, 57, 43);
        private readonly Color Orange = Color.FromArgb(230, 126, 34);

        public TimeInOutForm(User user)
        {
            _user = user;
            InitializeComponent();
            StartClock();
            RefreshTodayRecord();
            LoadHistory();
        }

        // ─────────────────────────────────────────────────────────────
        private void InitializeComponent()
        {
            this.Text = "E-Attendance — Time In / Out";
            this.Size = new Size(540, 720);
            this.MinimumSize = new Size(520, 680);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = BgPage;
            this.Font = new Font("Segoe UI", 9f);

            // ── Window chrome top bar ────────────────────────────────
            var pnlTopBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 46,
                BackColor = BgCard
            };
            var topBorder = new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = BorderClr };
            pnlTopBar.Controls.Add(topBorder);

            var lblTitle = new Label
            {
                Text = "Check In / Check Out",
                Font = new Font("Georgia", 12f, FontStyle.Bold),
                ForeColor = Navy,
                Location = new Point(16, 12),
                AutoSize = true
            };

            var lblRoleBadge = new Label
            {
                Text = "EMPLOYEE",
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(39, 128, 185),
                BackColor = Color.FromArgb(232, 242, 251),
                Location = new Point(370, 8),
                AutoSize = true,
                Padding = new Padding(5, 2, 5, 2)
            };
            var lblUserName = new Label
            {
                Text = _user.Username,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Navy,
                Location = new Point(370, 26),
                AutoSize = true
            };
            pnlTopBar.Controls.AddRange(new Control[] { lblTitle, lblRoleBadge, lblUserName });
            this.Controls.Add(pnlTopBar);

            // ── Scroll canvas ────────────────────────────────────────
            var scroll = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = BgPage,
                Padding = new Padding(16, 16, 16, 16)
            };

            int y = 16;

            // ── Live clock block ─────────────────────────────────────
            lblClock = new Label
            {
                Text = "00:00:00",
                Font = new Font("Georgia", 36f, FontStyle.Bold),
                ForeColor = Navy,
                Location = new Point(16, y),
                AutoSize = true
            };
            lblDate = new Label
            {
                Text = DateTime.Now.ToString("dddd, dd MMMM yyyy"),
                Font = new Font("Segoe UI", 9f),
                ForeColor = LabelClr,
                Location = new Point(20, y + 56),
                AutoSize = true
            };
            scroll.Controls.AddRange(new Control[] { lblClock, lblDate });
            y += 90;

            // ── Today record card ────────────────────────────────────
            var todayCard = MakeCard(16, y, 486, 200, "Today's Attendance Record");
            y += 216;

            // Status row
            lblStatusDot = new Label
            {
                Location = new Point(16, 46),
                Size = new Size(10, 10),
                BackColor = Color.FromArgb(192, 202, 216),
                Text = ""
            };
            lblStatusDot.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                e.Graphics.FillEllipse(new SolidBrush(lblStatusDot.BackColor), 0, 0, 10, 10);
            };

            var lblStatusLbl = MakeField("Status", 32, 42);
            lblStatusVal = new Label
            {
                Text = "Not recorded",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = LabelClr,
                Location = new Point(110, 42),
                AutoSize = true
            };

            // Time boxes
            var pnlCIBox = MakeTimeBox(16, 78, "Check In", out lblCheckInVal);
            var pnlCOBox = MakeTimeBox(252, 78, "Check Out", out lblCheckOutVal);

            // Hours pill
            pnlHours = new Panel
            {
                Location = new Point(16, 148),
                Size = new Size(200, 32),
                BackColor = Color.FromArgb(234, 247, 232),
                Visible = false
            };
            pnlHours.Paint += (s, e) =>
                e.Graphics.DrawRectangle(new Pen(Color.FromArgb(184, 221, 160), 1f), 0, 0, pnlHours.Width - 1, pnlHours.Height - 1);

            var lblHrsLbl = new Label
            {
                Text = "TOTAL HOURS",
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(58, 110, 26),
                Location = new Point(10, 5),
                AutoSize = true
            };
            lblHoursVal = new Label
            {
                Text = "0.0 hrs",
                Font = new Font("Georgia", 12f, FontStyle.Bold),
                ForeColor = Color.FromArgb(39, 130, 50),
                Location = new Point(10, 17),
                AutoSize = true
            };
            pnlHours.Controls.AddRange(new Control[] { lblHrsLbl, lblHoursVal });

            todayCard.Controls.AddRange(new Control[]
            {
                lblStatusDot, lblStatusLbl, lblStatusVal,
                pnlCIBox, pnlCOBox, pnlHours
            });
            scroll.Controls.Add(todayCard);

            // ── Action card ──────────────────────────────────────────
            var actionCard = MakeCard(16, y, 486, 176, "Mark Attendance");
            y += 192;

            // Shift selector
            var lblShiftLbl = MakeField("Shift", 16, 46);
            cboShift = new ComboBox
            {
                Location = new Point(60, 42),
                Size = new Size(400, 28),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = TextPrim,
                BackColor = BgInput,
                FlatStyle = FlatStyle.Flat
            };
            cboShift.Items.AddRange(new object[]
            {
                "Morning — 08:00 to 17:00",
                "Evening — 14:00 to 22:00",
                "Night   — 22:00 to 06:00"
            });
            cboShift.SelectedIndex = 0;

            // Check In button
            btnCheckIn = new Button
            {
                Text = "CHECK IN",
                Location = new Point(16, 86),
                Size = new Size(220, 52),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(30, 122, 64),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCheckIn.FlatAppearance.BorderColor = Color.FromArgb(20, 90, 48);
            btnCheckIn.FlatAppearance.MouseOverBackColor = Color.FromArgb(37, 150, 78);
            btnCheckIn.FlatAppearance.MouseDownBackColor = Color.FromArgb(20, 90, 48);
            btnCheckIn.Click += BtnCheckIn_Click;

            // Check Out button
            btnCheckOut = new Button
            {
                Text = "CHECK OUT",
                Location = new Point(250, 86),
                Size = new Size(220, 52),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(180, 48, 48),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Enabled = false
            };
            btnCheckOut.FlatAppearance.BorderColor = Color.FromArgb(140, 32, 32);
            btnCheckOut.FlatAppearance.MouseOverBackColor = Color.FromArgb(210, 60, 60);
            btnCheckOut.FlatAppearance.MouseDownBackColor = Color.FromArgb(140, 32, 32);
            btnCheckOut.Click += BtnCheckOut_Click;

            // Message bar
            lblMsg = new Label
            {
                Location = new Point(16, 148),
                Size = new Size(454, 20),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Text = "",
                ForeColor = Green
            };

            actionCard.Controls.AddRange(new Control[]
            {
                lblShiftLbl, cboShift,
                btnCheckIn, btnCheckOut,
                lblMsg
            });
            scroll.Controls.Add(actionCard);

            // ── History grid card ─────────────────────────────────────
            var histCard = MakeCard(16, y, 486, 200, "Recent Attendance History");
            y += 216;

            gridHistory = new DataGridView
            {
                Location = new Point(0, 34),
                Size = new Size(486, 162),
                BackgroundColor = BgCard,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = Color.FromArgb(240, 243, 248),
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font = new Font("Segoe UI", 8.5f)
            };
            gridHistory.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(245, 247, 250);
            gridHistory.ColumnHeadersDefaultCellStyle.ForeColor = LabelClr;
            gridHistory.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8f, FontStyle.Bold);
            gridHistory.ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 0, 0, 0);
            gridHistory.ColumnHeadersHeight = 32;
            gridHistory.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            gridHistory.DefaultCellStyle.BackColor = BgCard;
            gridHistory.DefaultCellStyle.ForeColor = TextPrim;
            gridHistory.DefaultCellStyle.SelectionBackColor = Color.FromArgb(225, 235, 248);
            gridHistory.DefaultCellStyle.SelectionForeColor = TextPrim;
            gridHistory.DefaultCellStyle.Padding = new Padding(6, 0, 0, 0);
            gridHistory.RowTemplate.Height = 30;
            gridHistory.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(249, 250, 252);

            gridHistory.Columns.Add("Date", "Date");
            gridHistory.Columns.Add("Status", "Status");
            gridHistory.Columns.Add("In", "Check In");
            gridHistory.Columns.Add("Out", "Check Out");
            gridHistory.Columns.Add("Hours", "Hours");

            gridHistory.Columns["Date"].FillWeight = 22;
            gridHistory.Columns["Status"].FillWeight = 16;
            gridHistory.Columns["In"].FillWeight = 18;
            gridHistory.Columns["Out"].FillWeight = 18;
            gridHistory.Columns["Hours"].FillWeight = 14;

            gridHistory.CellFormatting += (s, e) =>
            {
                if (gridHistory.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
                {
                    e.CellStyle.ForeColor = StatusColor(e.Value.ToString());
                    e.CellStyle.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
                }
            };
            histCard.Controls.Add(gridHistory);
            scroll.Controls.Add(histCard);

            this.Controls.Add(scroll);
        }

        // ─── Check In ─────────────────────────────────────────────────
        private void BtnCheckIn_Click(object sender, EventArgs e)
        {
            if (_user.EmployeeID == null)
            { ShowMsg("No employee record linked to this account.", false); return; }

            int shiftID = cboShift.SelectedIndex + 1;
            var (ok, msg) = _svc.CheckIn(_user.EmployeeID.Value, shiftID);
            ShowMsg(msg, ok);
            if (ok)
            {
                btnCheckIn.Enabled = false;
                btnCheckOut.Enabled = true;
                cboShift.Enabled = false;
                RefreshTodayRecord();
            }
        }

        // ─── Check Out ────────────────────────────────────────────────
        private void BtnCheckOut_Click(object sender, EventArgs e)
        {
            if (_user.EmployeeID == null) return;
            var (ok, msg) = _svc.CheckOut(_user.EmployeeID.Value);
            ShowMsg(msg, ok);
            if (ok)
            {
                btnCheckOut.Enabled = false;
                RefreshTodayRecord();
                LoadHistory();
            }
        }

        // ─── Refresh today's record ───────────────────────────────────
        private void RefreshTodayRecord()
        {
            if (_user.EmployeeID == null) return;
            var rec = _repo.GetTodayRecord(_user.EmployeeID.Value);

            if (rec == null)
            {
                lblStatusVal.Text = "Not recorded";
                lblStatusVal.ForeColor = LabelClr;
                lblStatusDot.BackColor = Color.FromArgb(192, 202, 216);
                lblCheckInVal.Text = "--:--";
                lblCheckInVal.ForeColor = LabelClr;
                lblCheckOutVal.Text = "--:--";
                lblCheckOutVal.ForeColor = LabelClr;
                pnlHours.Visible = false;
                btnCheckIn.Enabled = true;
                btnCheckOut.Enabled = false;
                cboShift.Enabled = true;
                return;
            }

            Color sc = StatusColor(rec.Status);
            lblStatusVal.Text = rec.Status;
            lblStatusVal.ForeColor = sc;
            lblStatusDot.BackColor = sc;
            lblStatusDot.Invalidate();

            if (rec.CheckIn.HasValue)
            {
                lblCheckInVal.Text = rec.CheckIn.Value.ToString("hh:mm tt");
                lblCheckInVal.ForeColor = TextPrim;
            }
            if (rec.CheckOut.HasValue)
            {
                lblCheckOutVal.Text = rec.CheckOut.Value.ToString("hh:mm tt");
                lblCheckOutVal.ForeColor = TextPrim;
                if (rec.WorkingHours.HasValue)
                {
                    lblHoursVal.Text = $"{rec.WorkingHours.Value:0.#} hrs";
                    pnlHours.Visible = true;
                }
            }

            btnCheckIn.Enabled = !rec.CheckIn.HasValue;
            btnCheckOut.Enabled = rec.CheckIn.HasValue && !rec.CheckOut.HasValue;
            cboShift.Enabled = !rec.CheckIn.HasValue;
        }

        // ─── Load history grid ────────────────────────────────────────
        private void LoadHistory()
        {
            if (_user.EmployeeID == null) return;
            gridHistory.Rows.Clear();

            int m = DateTime.Today.Month, yr = DateTime.Today.Year;
            var summary = _svc.GetSummary(_user.EmployeeID.Value, m, yr);
            var records = summary.Records;

            int start = Math.Max(0, records.Count - 7);
            for (int i = records.Count - 1; i >= start; i--)
            {
                var r = records[i];
                gridHistory.Rows.Add(
                    r.AttendanceDate.ToString("dd MMM yyyy"),
                    r.Status,
                    r.CheckIn.HasValue ? r.CheckIn.Value.ToString("hh:mm tt") : "--",
                    r.CheckOut.HasValue ? r.CheckOut.Value.ToString("hh:mm tt") : "--",
                    r.WorkingHours.HasValue ? $"{r.WorkingHours.Value:0.#} hrs" : "--"
                );
            }
        }

        // ─── Helpers ─────────────────────────────────────────────────
        private void StartClock()
        {
            lblClock.Text = DateTime.Now.ToString("HH:mm:ss");
            _clockTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            _clockTimer.Tick += (s, e) => lblClock.Text = DateTime.Now.ToString("HH:mm:ss");
            _clockTimer.Start();
        }

        private Panel MakeCard(int x, int y, int w, int h, string title)
        {
            var card = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(w, h),
                BackColor = BgCard
            };
            card.Paint += (s, e) =>
                e.Graphics.DrawRectangle(new Pen(BorderClr, 1f), 0, 0, card.Width - 1, card.Height - 1);

            var lblHdr = new Label
            {
                Text = title.ToUpper(),
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = LabelClr,
                Location = new Point(16, 12),
                AutoSize = true
            };
            var line = new Panel
            {
                Location = new Point(0, 32),
                Size = new Size(w, 1),
                BackColor = BorderClr
            };
            card.Controls.Add(lblHdr);
            card.Controls.Add(line);
            return card;
        }

        private Panel MakeTimeBox(int x, int y, string label, out Label valLabel)
        {
            var box = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(218, 62),
                BackColor = BgTimeBox
            };
            box.Paint += (s, e) =>
                e.Graphics.DrawRectangle(new Pen(BorderClr, 1f), 0, 0, box.Width - 1, box.Height - 1);

            var lbl = new Label
            {
                Text = label.ToUpper(),
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = LabelClr,
                Location = new Point(10, 8),
                AutoSize = true
            };
            var val = new Label
            {
                Text = "--:--",
                Font = new Font("Georgia", 16f, FontStyle.Bold),
                ForeColor = LabelClr,
                Location = new Point(10, 26),
                AutoSize = true
            };
            box.Controls.AddRange(new Control[] { lbl, val });
            valLabel = val;
            return box;
        }

        private Label MakeField(string text, int x, int y) => new Label
        {
            Text = text.ToUpper(),
            Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
            ForeColor = LabelClr,
            Location = new Point(x, y),
            AutoSize = true
        };

        private void ShowMsg(string msg, bool ok)
        {
            lblMsg.Text = (ok ? "✔  " : "✘  ") + msg;
            lblMsg.ForeColor = ok ? Green : Red;
        }

        private Color StatusColor(string status)
        {
            return status switch
            {
                "Present" => Green,
                "Late" => Orange,
                "Absent" => Red,
                "Leave" => Color.FromArgb(52, 152, 219),
                "Half-Day" => Color.FromArgb(155, 89, 182),
                _ => LabelClr
            };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _clockTimer?.Dispose();
            base.Dispose(disposing);
        }
    }
}