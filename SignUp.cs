using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace final_project1
{
    public partial class SignUp : Form
    {
        // 🔧 Change this to match your SQL Server connection
        string connectionString = "Data Source=HKL\\SQLEXPRESS03;Initial Catalog=finalProject;Integrated Security=True;";

        public SignUp()
        {
            InitializeComponent();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Form1 form = new Form1();
            form.Show();
            this.Hide();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Get values from form controls
            string username = txtUsername.Text.Trim();       // Username field
            string gender = txtGender.Text.Trim();       // Gender field
            DateTime dob = dateTimePicker1.Value;      // Date of Birth
            string gmail = txtGmail.Text.Trim();       // Gmail field
            string password = txtPassword.Text.Trim();       // Password field
            string repass = txtRepassword.Text.Trim();       // Re-password field

            // --- Basic Validation ---
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(gmail) ||
                string.IsNullOrEmpty(password) || string.IsNullOrEmpty(repass))
            {
                MessageBox.Show("Please fill in all required fields.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (password != repass)
            {
                MessageBox.Show("Passwords do not match.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // --- Save to Database ---
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = @"INSERT INTO TableSignUp (username, dob, gmail, password)
                                     VALUES (@username, @dob, @gmail, @password)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        cmd.Parameters.AddWithValue("@dob", dob.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@gmail", gmail);
                        cmd.Parameters.AddWithValue("@password", password); // ⚠️ See note below

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Account created successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            catch (SqlException ex)
            {
                MessageBox.Show("Database error: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}