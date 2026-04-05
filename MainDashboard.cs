using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace final_project1
{
    public partial class MainDashboard : Form
    {
        public MainDashboard()
        {
            InitializeComponent();
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel6_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void btnDashboard_Click(object sender, EventArgs e)
        {
                DashboardFrom dashboardFrom = new DashboardFrom();
                dashboardFrom.TopLevel = false;
                pnlContent.Controls.Add(dashboardFrom);
                dashboardFrom.BringToFront();
                dashboardFrom.Show();
        }

        private void MainDashboard_Load(object sender, EventArgs e)
        {

        }

        

        private void btnReport_Click(object sender, EventArgs e)
        {
            StudentForm studentForm = new StudentForm();
            studentForm.TopLevel = false;
            pnlContent.Controls.Add(studentForm);
            studentForm.BringToFront();
            studentForm.Show();
        }

        private void btnSetting_Click(object sender, EventArgs e)
        {
            ReportForm reportForm = new ReportForm();
            reportForm.TopLevel = false;
            pnlContent.Controls.Add(reportForm);
            reportForm.BringToFront();
            reportForm.Show();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            SettingForm settingForm = new SettingForm();
            settingForm.TopLevel = false;
            pnlContent.Controls.Add(settingForm);
            settingForm.BringToFront();
            settingForm.Show();
            
        }
    }
}
