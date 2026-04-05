using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace final_project1
{
    public partial class DashboardFrom : Form
    {
        public DashboardFrom()
        {
            InitializeComponent();
        }
        private void DashboardFrom_Load(object sender, EventArgs e)
        {
            LoadDashboardStats();
            LoadWeeklyAttendance();
        }

        private void LoadDashboardStats()
        {
            string connectionString = "Data Source=HKL\\SQLEXPRESS03;Initial Catalog=finalProject;Integrated Security=True;";

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                try
                {
                    con.Open();

                    // Total Students
                    SqlCommand cmd1 = new SqlCommand("SELECT COUNT(*) FROM TableStudent", con);
                    lblTotalStudents.Text = cmd1.ExecuteScalar().ToString();

                    // Present Today
                    SqlCommand cmd2 = new SqlCommand(
                        "SELECT COUNT(*) FROM TableStudent WHERE status = 'Present' AND CAST(dateCreated AS DATE) = CAST(GETDATE() AS DATE)", con);
                    lblPresentToday.Text = cmd2.ExecuteScalar().ToString();

                    // Absent Today
                    SqlCommand cmd3 = new SqlCommand(
                        "SELECT COUNT(*) FROM TableStudent WHERE status = 'Absent' AND CAST(dateCreated AS DATE) = CAST(GETDATE() AS DATE)", con);
                    lblAbsentToday.Text = cmd3.ExecuteScalar().ToString();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        // ── Shared helper ──────────────────────────────────────────────────
        private GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void PaintCard(Panel panel, Color accentColor, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new Rectangle(0, 0, panel.Width - 1, panel.Height - 1);
            var path = RoundedRect(rect, 12);

            // 1. White rounded background
            g.FillPath(Brushes.White, path);

            // 2. Light gray border
            var borderPen = new Pen(Color.FromArgb(220, 225, 235), 1);
            g.DrawPath(borderPen, path);

            // 3. Colored left accent bar
            var accentPen = new Pen(accentColor, 4);
            g.DrawLine(accentPen, 2, 12, 2, panel.Height - 12);

            // 4. Clip to rounded shape
            panel.Region = new Region(RoundedRect(
                new Rectangle(0, 0, panel.Width, panel.Height), 12));
        }

        // ── Card 1 – Total Students (Blue) ─────────────────────────────────
        private void panel2_Paint(object sender, PaintEventArgs e)
        {
            PaintCard(panel2, Color.FromArgb(30, 100, 200), e);
        }
        private void panel3_Paint_1(object sender, PaintEventArgs e)
        {
            PaintCard(panel3, Color.FromArgb(40, 140, 60), e);
        }

        private void panel4_Paint_1(object sender, PaintEventArgs e)
        {
            PaintCard(panel4, Color.FromArgb(190, 40, 40), e);
        }

        

        private void label5_Click(object sender, EventArgs e)
        {
        }

        private void chartWeekly_Click(object sender, EventArgs e)
        {

        }
        private void LoadWeeklyAttendance()
        {
            string connectionString = "Data Source=HKL\\SQLEXPRESS03;Initial Catalog=finalProject;Integrated Security=True;";

            // Show last 7 days instead of Mon-Fri so Sunday is included
            string[] days = new string[7];
            int[] presentCounts = new int[7];
            int[] absentCounts = new int[7];

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                try
                {
                    con.Open();

                    for (int i = 6; i >= 0; i--)
                    {
                        DateTime targetDay = DateTime.Now.AddDays(-i);
                        days[6 - i] = targetDay.ToString("ddd");        // Mon, Tue ... Sun
                        string dateStr = targetDay.ToString("yyyy-MM-dd");

                        SqlCommand cmd1 = new SqlCommand(
                            "SELECT COUNT(*) FROM TableStudent WHERE LOWER(status) = 'present' AND CAST(dateCreated AS DATE) = @date", con);
                        cmd1.Parameters.AddWithValue("@date", dateStr);
                        presentCounts[6 - i] = (int)cmd1.ExecuteScalar();

                        SqlCommand cmd2 = new SqlCommand(
                            "SELECT COUNT(*) FROM TableStudent WHERE LOWER(status) = 'absent' AND CAST(dateCreated AS DATE) = @date", con);
                        cmd2.Parameters.AddWithValue("@date", dateStr);
                        absentCounts[6 - i] = (int)cmd2.ExecuteScalar();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }

            // Setup chart
            chartWeekly.Series.Clear();
            chartWeekly.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
            chartWeekly.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;
            chartWeekly.ChartAreas[0].AxisY.Interval = 1;              // show only whole numbers
            chartWeekly.ChartAreas[0].AxisY.Minimum = 0;
            chartWeekly.ChartAreas[0].BackColor = Color.White;
            chartWeekly.BackColor = Color.White;
            chartWeekly.Legends[0].Enabled = true;

            var presentSeries = new Series("Present");
            presentSeries.ChartType = SeriesChartType.Column;
            presentSeries.Color = Color.Green;
            presentSeries.IsValueShownAsLabel = true;                   // show number on top of bar

            var absentSeries = new Series("Absent");
            absentSeries.ChartType = SeriesChartType.Column;
            absentSeries.Color = Color.Crimson;
            absentSeries.IsValueShownAsLabel = true;

            for (int i = 0; i < 7; i++)
            {
                presentSeries.Points.AddXY(days[i], presentCounts[i]);
                absentSeries.Points.AddXY(days[i], absentCounts[i]);
            }

            chartWeekly.Series.Add(presentSeries);
            chartWeekly.Series.Add(absentSeries);

            chartWeekly.Titles.Clear();
            chartWeekly.Titles.Add("Last 7 Days Attendance");
            chartWeekly.Titles[0].Font = new Font("Segoe UI", 11, FontStyle.Bold);
        }

        // ── Call this from StudentForm after add/delete ────────────────
        public void RefreshDashboard()
        {
            LoadDashboardStats();
            LoadWeeklyAttendance();
        }
    }
}