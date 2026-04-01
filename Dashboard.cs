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
    public partial class Dashboard : Form
    {
        public Dashboard()
        {
            InitializeComponent();
        }
        SqlConnection conn = new SqlConnection(@"Data Source=HKL\SQLEXPRESS03;Initial Catalog=finalProject;Integrated Security=True;Encrypt=False;");

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            dateTimePicker1.CustomFormat = "dd/MM/yyyy";
        }

        private void dateTimePicker1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Back) {
                dateTimePicker1.CustomFormat = "";
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtStudentID.Text))
            {
                MessageBox.Show("Please enter a Student ID.");
                return;
            }

            conn.Open();

            // Check if studentId already exists
            SqlCommand checkCmd = new SqlCommand("SELECT COUNT(*) FROM TableStudent WHERE studentId = @studentid", conn);
            checkCmd.Parameters.AddWithValue("@studentid", int.Parse(txtStudentID.Text));

            int count = (int)checkCmd.ExecuteScalar();

            if (count > 0)
            {
                // ID already exists
                MessageBox.Show("This Student ID already exists. Please use a different ID.");
            }
            else
            {
                //Insert new record
                SqlCommand cmd = new SqlCommand("INSERT INTO TableStudent(studentId,studentName,section,status,dateCreated) VALUES(@studentid,@studentname,@section,@status,@datecreated)", conn);
                cmd.Parameters.AddWithValue("@studentid", int.Parse(txtStudentID.Text));
                cmd.Parameters.AddWithValue("@studentname", txtStudentName.Text);
                cmd.Parameters.AddWithValue("@section", txtSection.Text);
                cmd.Parameters.AddWithValue("@status", txtStatus.Text);
                cmd.Parameters.AddWithValue("@datecreated", dateTimePicker1.Value);

                cmd.ExecuteNonQuery();
                MessageBox.Show("Record Added Successfully");
                BindDate();
            }

            conn.Close();


        }
        void BindDate()
        {
            SqlCommand cmd = new SqlCommand("select * from TableStudent", conn);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable table = new DataTable();
            da.Fill(table);
            dataGridView1.DataSource = table;
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            conn.Open();
            SqlCommand cmd = new SqlCommand("delete TableStudent where studentId=@studentid", conn);
            cmd.Parameters.AddWithValue("@studentid", int.Parse(txtStudentID.Text));
            cmd.ExecuteNonQuery();
            conn.Close();
            MessageBox.Show("Record Deleted Successfully");
            BindDate();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtStudentID.Text = "";
            txtStudentName.Text = "";
            txtSection.Text = "";
            txtStatus.Text = "";

        }

        private void Dashboard_Load(object sender, EventArgs e)
        {
            BindDate();
        }

        private void btnExist_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtStudentID.Text))
            {
                MessageBox.Show("Please enter a Student ID to search.");
                return;
            }

            conn.Open();
            SqlCommand cmd = new SqlCommand("SELECT * FROM TableStudent WHERE studentId = @studentid", conn);
            cmd.Parameters.AddWithValue("@studentid", int.Parse(txtStudentID.Text));

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable table = new DataTable();
            da.Fill(table);

            if (table.Rows.Count > 0)
            {
                dataGridView1.DataSource = table;

                txtStudentName.Text = table.Rows[0]["studentName"].ToString();
                txtSection.Text = table.Rows[0]["section"].ToString();
                txtStatus.Text = table.Rows[0]["status"].ToString();
                dateTimePicker1.Value = Convert.ToDateTime(table.Rows[0]["dateCreated"]);
            }
            else
            {
                MessageBox.Show("No student found with this ID.");
            }

            conn.Close();

        }
    }
}
