using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace final_project1
{
    public partial class ReportForm : Form
    {
        private string connectionString = "Data Source=HKL\\SQLEXPRESS03;Initial Catalog=finalProject;Integrated Security=True;";

        public ReportForm()
        {
            InitializeComponent();
        }

        private void ReportForm_Load(object sender, EventArgs e)
        {
            cmbStatus.Items.AddRange(new string[] { "All", "Present", "Absent" });
            cmbStatus.SelectedIndex = 0;
            dtpFrom.Value = DateTime.Now.AddDays(-7);
            dtpTo.Value = DateTime.Now;

            dgvReport.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvReport.RowHeadersVisible = false;
            dgvReport.AllowUserToAddRows = false;
            dgvReport.AllowUserToDeleteRows = false;
            dgvReport.ReadOnly = true;
            dgvReport.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvReport.BackgroundColor = Color.White;
            dgvReport.BorderStyle = BorderStyle.None;

            dgvReport.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 100, 200);
            dgvReport.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvReport.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            dgvReport.EnableHeadersVisualStyles = false;
            dgvReport.ColumnHeadersHeight = 38;
            dgvReport.CellFormatting += DgvReport_CellFormatting;

            LoadReport();
        }

        // ─── Search button ────────────────────────────────────────────
        private void btnSearch_Click(object sender, EventArgs e)
        {
            LoadReport();
        }

        // ─── Load data ────────────────────────────────────────────────
        private void LoadReport()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = @"SELECT studentId   AS [Student ID],
                                            studentName AS [Name],
                                            section     AS [Section],
                                            status      AS [Status],
                                            dateCreated AS [Date]
                                     FROM TableStudent
                                     WHERE CAST(dateCreated AS DATE) BETWEEN @from AND @to";

                    if (cmbStatus.SelectedItem.ToString() != "All")
                        query += " AND LOWER(status) = @status";

                    query += " ORDER BY dateCreated DESC";

                    SqlDataAdapter da = new SqlDataAdapter(query, con);
                    da.SelectCommand.Parameters.AddWithValue("@from", dtpFrom.Value.Date);
                    da.SelectCommand.Parameters.AddWithValue("@to", dtpTo.Value.Date);

                    if (cmbStatus.SelectedItem.ToString() != "All")
                        da.SelectCommand.Parameters.AddWithValue("@status",
                            cmbStatus.SelectedItem.ToString().ToLower());

                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgvReport.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ─── Color Present/Absent ─────────────────────────────────────
        private void DgvReport_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex < 0 || e.Value == null) return;

            if (dgvReport.Columns[e.ColumnIndex].HeaderText == "Status")
            {
                if (e.Value.ToString() == "Present")
                {
                    e.CellStyle.ForeColor = Color.FromArgb(40, 167, 69);
                    e.CellStyle.SelectionForeColor = Color.FromArgb(40, 167, 69);
                    e.CellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                }
                else if (e.Value.ToString() == "Absent")
                {
                    e.CellStyle.ForeColor = Color.Crimson;
                    e.CellStyle.SelectionForeColor = Color.Crimson;
                    e.CellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                }
            }
        }


        private void cmbStatus_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void btnSearch_Click_1(object sender, EventArgs e)
        {
            LoadReport();
        }

       

        private void btnExport_Click(object sender, EventArgs e)
        {
            if (dgvReport.Rows.Count == 0)
            {
                MessageBox.Show("No data to export.", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                FileName = $"AttendanceReport_{DateTime.Now:yyyyMMdd}.csv"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var sb = new System.Text.StringBuilder();
                    foreach (DataGridViewColumn col in dgvReport.Columns)
                        sb.Append(col.HeaderText + ",");
                    sb.AppendLine();

                    foreach (DataGridViewRow row in dgvReport.Rows)
                    {
                        foreach (DataGridViewCell cell in row.Cells)
                            sb.Append(cell.Value?.ToString() + ",");
                        sb.AppendLine();
                    }

                    System.IO.File.WriteAllText(sfd.FileName, sb.ToString());
                    MessageBox.Show("Exported successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Export error:\n" + ex.Message);
                }
            }
        }
    }
}