using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace final_project1
{
    public partial class UpdateStudentForm : Form
    {
        private string connectionString;
        private int studentId;
        private string currentStatus; // ← store status here

        public UpdateStudentForm(string connStr, int id, string name, string section, string status)
        {
            InitializeComponent();
            connectionString = connStr;
            studentId = id;
            currentStatus = status; // ← save it before Load fires

            txtStudentName.Text = name;
            txtSection.Text = section;
        }

        private void UpdateStudentForm_Load(object sender, EventArgs e)
        {
            // Add items first
            cmbStatus.Items.Clear();
            cmbStatus.Items.Add("Present");
            cmbStatus.Items.Add("Absent");

            // Then set selected using the saved value
            cmbStatus.SelectedItem = currentStatus ?? "Present";
        }


        

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtStudentName.Text))
            {
                MessageBox.Show("Student Name is required.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand(
                        @"UPDATE TableStudent
                          SET studentName = @name,
                              section     = @section,
                              status      = @status
                          WHERE studentId = @id", con);

                    cmd.Parameters.AddWithValue("@name", txtStudentName.Text.Trim());
                    cmd.Parameters.AddWithValue("@section", txtSection.Text.Trim());
                    cmd.Parameters.AddWithValue("@status", cmbStatus.SelectedItem?.ToString() ?? "Present");
                    cmd.Parameters.AddWithValue("@id", studentId);

                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Student updated successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}