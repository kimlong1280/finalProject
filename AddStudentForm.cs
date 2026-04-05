using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace final_project1
{
    public partial class AddStudentForm : Form
    {
        private string connectionString;

        public AddStudentForm(string connStr)
        {
            InitializeComponent();
            connectionString = connStr;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // Validate
            if (string.IsNullOrWhiteSpace(txtStudentId.Text) ||
                string.IsNullOrWhiteSpace(txtStudentName.Text))
            {
                MessageBox.Show("Student ID and Name are required.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = @"INSERT INTO TableStudent (studentId, studentName, section, status, dateCreated)
                                     VALUES (@id, @name, @section, @status, GETDATE())";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@id", Convert.ToInt32(txtStudentId.Text.Trim()));
                    cmd.Parameters.AddWithValue("@name", txtStudentName.Text.Trim());
                    cmd.Parameters.AddWithValue("@section", txtSection.Text.Trim());
                    cmd.Parameters.AddWithValue("@status", cmbStatus.SelectedItem?.ToString() ?? "Present");

                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Student added successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding student:\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddStudentForm_Load(object sender, EventArgs e)
        {
            // Populate status dropdown
            cmbStatus.Items.AddRange(new string[] { "Present", "Absent" });
            cmbStatus.SelectedIndex = 0;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}