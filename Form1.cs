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
    public partial class Form1 : Form
    {
        // 🔧 Same connection string as SignUp
        string connectionString = "Data Source=HKL\\SQLEXPRESS03;Initial Catalog=finalProject;Integrated Security=True;";

        public Form1()
        {
            InitializeComponent();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            SignUp signUp = new SignUp();
            signUp.Show();
            this.Hide();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string username = txtUserName.Text.Trim();
            string password = txtPassword.Text.Trim();

            // --- Basic Validation ---
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter your Username and Password.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // --- Admin Check via TableAdmin ---
                    string adminQuery = @"SELECT COUNT(*) FROM TableAdmin 
                                  WHERE username = @username AND password = @password";

                    using (SqlCommand adminCmd = new SqlCommand(adminQuery, conn))
                    {
                        adminCmd.Parameters.AddWithValue("@username", username);
                        adminCmd.Parameters.AddWithValue("@password", password);

                        int adminCount = (int)adminCmd.ExecuteScalar();

                        if (adminCount > 0)
                        {
                            MessageBox.Show("Welcome, Admin!", "Success",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            MainDashboard mainDashboard = new MainDashboard();
                            mainDashboard.Show();
                            this.Hide();
                            return; // stop here, skip normal user check
                        }
                    }

                    // --- Normal User Check via TableSignUp ---
                    string userQuery = @"SELECT COUNT(*) FROM TableSignUp 
                                 WHERE username = @username AND password = @password";

                    using (SqlCommand userCmd = new SqlCommand(userQuery, conn))
                    {
                        userCmd.Parameters.AddWithValue("@username", username);
                        userCmd.Parameters.AddWithValue("@password", password);

                        int userCount = (int)userCmd.ExecuteScalar();

                        if (userCount > 0)
                        {
                            MessageBox.Show("Login successful!", "Success",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            Dashboard dashboard = new Dashboard();
                            dashboard.Show();
                            this.Hide();
                        }
                        else
                        {
                            MessageBox.Show("Invalid Username or Password. Please try again.", "Login Failed",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Database error: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}