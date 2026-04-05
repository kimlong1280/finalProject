using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace final_project1
{
    public partial class SettingForm : Form
    {
        private string connectionString = "Data Source=HKL\\SQLEXPRESS03;Initial Catalog=finalProject;Integrated Security=True;";

        // Store original values for reset
        private string originalName = "";
        private string originalEmail = "";
        private string originalPassword = "";
        private DateTime originalDob = DateTime.Now;

        public SettingForm()
        {
            InitializeComponent();
        }

        // ── Load admin data when form opens ──
        private void SettingForm_Load(object sender, EventArgs e)
        {
            LoadAdminProfile();
        }

        private void LoadAdminProfile()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT username, gmail, password, dob FROM TableAdmin WHERE id = 1";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                originalName = reader["username"].ToString();
                                originalEmail = reader["gmail"].ToString();
                                originalPassword = reader["password"].ToString();
                                originalDob = DateTime.Parse(reader["dob"].ToString());

                                // Fill the controls
                                txtName.Text = originalName;
                                txtEmail.Text = originalEmail;
                                txtPassword.Text = originalPassword;
                                dateTimePicker1.Value = originalDob;
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Failed to load profile: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        

        // ── Unused event stubs (keep to avoid designer errors) ──
        private void label2_Click(object sender, EventArgs e) { }
        private void textBox1_TextChanged(object sender, EventArgs e) { }
        private void textBox2_TextChanged(object sender, EventArgs e) { }
        private void textBox3_TextChanged(object sender, EventArgs e) { }
        private void textBox4_TextChanged(object sender, EventArgs e) { }
        private void dateTimePicker1_ValueChanged(object sender, EventArgs e) { }

        private void btnReset_Click(object sender, EventArgs e)
        {
            txtName.Text = originalName;
            txtEmail.Text = originalEmail;
            txtPassword.Text = originalPassword;
            dateTimePicker1.Value = originalDob;
        }

        private void btnSave_Click_1(object sender, EventArgs e)
        {
            string newName = txtName.Text.Trim();
            string newEmail = txtEmail.Text.Trim();
            string newPassword = txtPassword.Text.Trim();
            DateTime newDob = dateTimePicker1.Value.Date;

            // Basic validation
            if (string.IsNullOrEmpty(newName) || string.IsNullOrEmpty(newEmail) || string.IsNullOrEmpty(newPassword))
            {
                MessageBox.Show("Please fill in all fields.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"UPDATE TableAdmin 
                                     SET username = @username, 
                                         gmail    = @gmail, 
                                         password = @password, 
                                         dob      = @dob 
                                     WHERE id = 1";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", newName);
                        cmd.Parameters.AddWithValue("@gmail", newEmail);
                        cmd.Parameters.AddWithValue("@password", newPassword);
                        cmd.Parameters.AddWithValue("@dob", newDob.ToString("yyyy-MM-dd"));

                        int rows = cmd.ExecuteNonQuery();

                        if (rows > 0)
                        {
                            // Update stored originals so Reset reflects the new saved values
                            originalName = newName;
                            originalEmail = newEmail;
                            originalPassword = newPassword;
                            originalDob = newDob;

                            MessageBox.Show("Profile updated successfully!", "Success",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("No record was updated.", "Warning",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Failed to save profile: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}