using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace final_project1
{
    public partial class StudentForm : Form
    {
        private string connectionString = "Data Source=HKL\\SQLEXPRESS03;Initial Catalog=finalProject;Integrated Security=True;";

        public StudentForm()
        {
            InitializeComponent();
            SetPlaceholder();
            LoadStudents();
        }

        // ─── Placeholder ────────────────────────────────────────────
        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, string lParam);
        private const uint EM_SETCUEBANNER = 0x1501;

        private void SetPlaceholder()
        {
            SendMessage(txtFilter.Handle, EM_SETCUEBANNER, IntPtr.Zero, "Search by name or ID...");
        }

        // ─── Load Students ───────────────────────────────────────────
        private void LoadStudents(string filter = "")
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = @"SELECT studentId   AS [Student ID],
                                            studentName AS [Name],
                                            section     AS [Section],
                                            status      AS [Status],
                                            dateCreated AS [Date Created]
                                     FROM TableStudent";

                    if (!string.IsNullOrWhiteSpace(filter))
                        query += " WHERE studentName LIKE @filter OR CAST(studentId AS VARCHAR) LIKE @filter";

                    query += " ORDER BY studentId";

                    SqlDataAdapter da = new SqlDataAdapter(query, con);

                    if (!string.IsNullOrWhiteSpace(filter))
                        da.SelectCommand.Parameters.AddWithValue("@filter", "%" + filter + "%");

                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dataGridView1.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading students:\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ─── txtFilter: Search on Enter key ─────────────────────────
        private void txtFilter_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                LoadStudents(txtFilter.Text.Trim());
        }

        private void txtFilter_TextChanged(object sender, EventArgs e)
        {
            // Optional: live search as user types
            //LoadStudents(txtFilter.Text.Trim());
        }

        // ─── btnFilter ───────────────────────────────────────────────
        private void btnFillter_Click(object sender, EventArgs e)
        {
            LoadStudents(txtFilter.Text.Trim());
        }

      

        // ─── Duplicate stubs from Designer — leave empty ─────────────
        private void btnAddStudent_Click_1(object sender, EventArgs e) {
            AddStudentForm addForm = new AddStudentForm(connectionString);
            addForm.ShowDialog();
            LoadStudents();
        }
        private void btnUpdate_Click_1(object sender, EventArgs e) {
            if (dataGridView1.CurrentRow == null)
            {
                MessageBox.Show("Please select a student to update.", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Check if it's the empty new row at the bottom
            if (dataGridView1.CurrentRow.IsNewRow)
            {
                MessageBox.Show("Please select a valid student row.", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Safe null checks on every cell
            int studentId = 0;
            if (dataGridView1.CurrentRow.Cells["Student ID"].Value != null)
                studentId = Convert.ToInt32(dataGridView1.CurrentRow.Cells["Student ID"].Value);

            string studentName = dataGridView1.CurrentRow.Cells["Name"].Value?.ToString() ?? "";
            string section = dataGridView1.CurrentRow.Cells["Section"].Value?.ToString() ?? "";
            string status = dataGridView1.CurrentRow.Cells["Status"].Value?.ToString() ?? "Present";

            UpdateStudentForm updateForm = new UpdateStudentForm(connectionString, studentId, studentName, section, status);
            updateForm.ShowDialog();
            LoadStudents();
        }
        private void btnDelete_Click_1(object sender, EventArgs e) {
            if (dataGridView1.CurrentRow == null)
            {
                MessageBox.Show("Please select a student to delete.", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int studentId = Convert.ToInt32(dataGridView1.CurrentRow.Cells["Student ID"].Value);
            string studentName = dataGridView1.CurrentRow.Cells["Name"].Value?.ToString();

            DialogResult confirm = MessageBox.Show(
                $"Are you sure you want to delete:\n[{studentId}] {studentName}?",
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirm == DialogResult.Yes)
            {
                try
                {
                    using (SqlConnection con = new SqlConnection(connectionString))
                    {
                        con.Open();
                        SqlCommand cmd = new SqlCommand("DELETE FROM TableStudent WHERE studentId = @id", con);
                        cmd.Parameters.AddWithValue("@id", studentId);
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Student deleted successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadStudents();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting student:\n" + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }
    }
}