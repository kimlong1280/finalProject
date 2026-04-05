using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace final_project1
{
    public partial class Dashboard : Form
    {
        public Dashboard()
        {
            InitializeComponent();
        }

        SqlConnection conn = new SqlConnection(@"Data Source=HKL\SQLEXPRESS03;Initial Catalog=finalProject;Integrated Security=True;Encrypt=False;");

        private void Dashboard_Load(object sender, EventArgs e)
        {
            BindData();
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            dateTimePicker1.CustomFormat = "dd/MM/yyyy";
        }

        private void dateTimePicker1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Back)
                dateTimePicker1.CustomFormat = "";
        }

        // ─── Bind DataGridView ───────────────────────────────────────
        void BindData(string filter = "")
        {
            string query = "SELECT * FROM TableStudent";

            if (!string.IsNullOrWhiteSpace(filter))
                query += " WHERE studentId = @filter OR studentName LIKE @namef";

            query += " ORDER BY dateCreated DESC";

            SqlDataAdapter da = new SqlDataAdapter(query, conn);

            if (!string.IsNullOrWhiteSpace(filter))
            {
                // try parse as int for ID search
                if (int.TryParse(filter, out int id))
                    da.SelectCommand.Parameters.AddWithValue("@filter", id);
                else
                    da.SelectCommand.Parameters.AddWithValue("@filter", -1);

                da.SelectCommand.Parameters.AddWithValue("@namef", "%" + filter + "%");
            }

            DataTable table = new DataTable();
            da.Fill(table);
            dataGridView1.DataSource = table;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        // ─── Add / Save Attendance ───────────────────────────────────
        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtStudentID.Text))
            {
                MessageBox.Show("Please enter a Student ID.");
                return;
            }

            if (!int.TryParse(txtStudentID.Text, out int studentId))
            {
                MessageBox.Show("Student ID must be a number.");
                return;
            }

            DateTime selectedDate = dateTimePicker1.Value.Date;

            try
            {
                conn.Open();

                // Check if this student already has a record on the SAME date
                SqlCommand checkCmd = new SqlCommand(
                    @"SELECT COUNT(*) FROM TableStudent 
                      WHERE studentId = @studentid 
                      AND CAST(dateCreated AS DATE) = @date", conn);
                checkCmd.Parameters.AddWithValue("@studentid", studentId);
                checkCmd.Parameters.AddWithValue("@date", selectedDate);

                int count = (int)checkCmd.ExecuteScalar();

                if (count > 0)
                {
                    // Already has attendance for this date — ask to update instead
                    DialogResult result = MessageBox.Show(
                        $"Student ID {studentId} already has attendance on {selectedDate:dd/MM/yyyy}.\n\nDo you want to UPDATE it instead?",
                        "Duplicate Date", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        SqlCommand updateCmd = new SqlCommand(
                            @"UPDATE TableStudent 
                              SET studentName = @studentname,
                                  section     = @section,
                                  status      = @status
                              WHERE studentId = @studentid 
                              AND CAST(dateCreated AS DATE) = @date", conn);
                        updateCmd.Parameters.AddWithValue("@studentname", txtStudentName.Text);
                        updateCmd.Parameters.AddWithValue("@section", txtSection.Text);
                        updateCmd.Parameters.AddWithValue("@status", txtStatus.Text);
                        updateCmd.Parameters.AddWithValue("@studentid", studentId);
                        updateCmd.Parameters.AddWithValue("@date", selectedDate);
                        updateCmd.ExecuteNonQuery();
                        MessageBox.Show("Attendance updated successfully!");
                    }
                }
                else
                {
                    // New date — insert new attendance record
                    SqlCommand cmd = new SqlCommand(
                        @"INSERT INTO TableStudent (studentId, studentName, section, status, dateCreated)
                          VALUES (@studentid, @studentname, @section, @status, @datecreated)", conn);
                    cmd.Parameters.AddWithValue("@studentid", studentId);
                    cmd.Parameters.AddWithValue("@studentname", txtStudentName.Text);
                    cmd.Parameters.AddWithValue("@section", txtSection.Text);
                    cmd.Parameters.AddWithValue("@status", txtStatus.Text);
                    cmd.Parameters.AddWithValue("@datecreated", selectedDate);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Attendance recorded successfully!");
                }

                BindData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }

        // ─── Delete ──────────────────────────────────────────────────
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtStudentID.Text))
            {
                MessageBox.Show("Please enter a Student ID.");
                return;
            }

            DateTime selectedDate = dateTimePicker1.Value.Date;

            DialogResult confirm = MessageBox.Show(
                $"Delete attendance for Student ID {txtStudentID.Text} on {selectedDate:dd/MM/yyyy}?",
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirm == DialogResult.Yes)
            {
                try
                {
                    conn.Open();

                    // Delete only the record for that specific date
                    SqlCommand cmd = new SqlCommand(
                        @"DELETE FROM TableStudent 
                          WHERE studentId = @studentid 
                          AND CAST(dateCreated AS DATE) = @date", conn);
                    cmd.Parameters.AddWithValue("@studentid", int.Parse(txtStudentID.Text));
                    cmd.Parameters.AddWithValue("@date", selectedDate);
                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Record deleted successfully!");
                    BindData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        // ─── Search ──────────────────────────────────────────────────
        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtStudentID.Text))
            {
                MessageBox.Show("Please enter a Student ID to search.");
                return;
            }

            try
            {
                conn.Open();

                // Show ALL attendance records for this student ID
                SqlCommand cmd = new SqlCommand(
                    "SELECT * FROM TableStudent WHERE studentId = @studentid ORDER BY dateCreated DESC", conn);
                cmd.Parameters.AddWithValue("@studentid", int.Parse(txtStudentID.Text));

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable table = new DataTable();
                da.Fill(table);

                if (table.Rows.Count > 0)
                {
                    dataGridView1.DataSource = table;
                    dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                    // Fill textboxes with the most recent record
                    txtStudentName.Text = table.Rows[0]["studentName"].ToString();
                    txtSection.Text = table.Rows[0]["section"].ToString();
                    txtStatus.Text = table.Rows[0]["status"].ToString();
                    dateTimePicker1.Value = Convert.ToDateTime(table.Rows[0]["dateCreated"]);

                    MessageBox.Show($"Found {table.Rows.Count} attendance record(s) for this student.");
                }
                else
                {
                    MessageBox.Show("No student found with this ID.");
                    BindData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }

        // ─── Clear ───────────────────────────────────────────────────
        private void btnClear_Click(object sender, EventArgs e)
        {
            txtStudentID.Text = "";
            txtStudentName.Text = "";
            txtSection.Text = "";
            txtStatus.Text = "";
            dateTimePicker1.Value = DateTime.Now;
            BindData(); // reload all records
        }

        // ─── Click row in grid → fill textboxes ─────────────────────
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
                txtStudentID.Text = row.Cells["studentId"].Value?.ToString();
                txtStudentName.Text = row.Cells["studentName"].Value?.ToString();
                txtSection.Text = row.Cells["section"].Value?.ToString();
                txtStatus.Text = row.Cells["status"].Value?.ToString();

                if (row.Cells["dateCreated"].Value != null)
                    dateTimePicker1.Value = Convert.ToDateTime(row.Cells["dateCreated"].Value);
            }
        }

        private void btnExist_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void label2_Click(object sender, EventArgs e) { }
    }
}