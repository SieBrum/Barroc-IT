﻿using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace Barroc_IT
{
    public partial class frm_Development : Form
    {
        bool showallProjects = false;
        bool showallAppointments = false;
        bool showallNotifications = false;
        DatabaseHandler dbh;

        public frm_Development()
        {
            InitializeComponent();
            dbh = new DatabaseHandler();
            cbox_Project_Status.SelectedIndex = 0;
            cbox_Maintenance_Contract.SelectedIndex = 0;
            //ShowProjects();
            ShowAppointments();
            ShowNotifications();
            tcp_Main.SelectedIndex = 0;

            ToolStripControlHost[] arrayControl = MenuItems.DTPGenerator(this);
            ToolStripControlHost[] arrayControl1 = MenuItems.DTPGenerator(this);
            HideFilters(true,false,false);

            for (int i = 0; i < arrayControl.Length; i++)
            {
                mnfltr_Overview_Date.DropDownItems.Add(arrayControl[i]);
                mnfltr_Appointments_Date.DropDownItems.Add(arrayControl1[i]);
            }
        }

        private void MenuHandler(object sender, EventArgs e)
        {
            ToolStripMenuItem toolstrip;
            toolstrip = sender as ToolStripMenuItem;

            switch (toolstrip.Name)
            { 
                case "mnitem_Overview":
                    tcp_Main.SelectedIndex = 0;
                    HideFilters(true, false, false);
                    break;
                case "mnitem_Projects":
                    tcp_Main.SelectedIndex = 1;
                    HideFilters(false, true, false);
                    break;
                case "mnitem_Appointments":
                    tcp_Main.SelectedIndex = 2;
                    HideFilters(false, false, true);
                    break;
                default:
                    tcp_Main.SelectedIndex = 0;
                    break;
            }
        }

        private void HideFilters(bool overview, bool projects, bool appointments)
        {
            Font bold = new Font(mnitem_Overview.Font, FontStyle.Bold);
            Font regular = new Font(mnitem_Overview.Font, FontStyle.Regular);

            if (overview)
                mnitem_Overview.Font = bold;
            else
                mnitem_Overview.Font = regular;
            if (projects)
                mnitem_Projects.Font = bold;
            else
                mnitem_Projects.Font = regular;
            if (appointments)
                mnitem_Appointments.Font = bold;
            else
                mnitem_Appointments.Font = regular;

            mnfltr_Overview_Department.Visible = overview;
            mnfltr_Overview_Type.Visible = overview;
            mnfltr_Overview_Date.Visible = overview;

            mnfltr_Projects_CuName.Visible = projects;
            mnfltr_Projects_ID.Visible = projects;
            mnfltr_Projects_Name.Visible = projects;

            mnfltr_Appointments_CuName.Visible = appointments;
            mnfltr_Appointments_CoName.Visible = appointments;
            mnfltr_Appointments_Residence.Visible = appointments;
            mnfltr_Appointments_Summary.Visible = appointments;
            mnfltr_Appointments_Date.Visible = appointments;
        }

        private void mnitem_Logout_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btn_Add_Project_Click(object sender, EventArgs e)
        {
            tcp_Main.SelectedIndex = 4;
        }

        private void AddProject(object sender, EventArgs e)
        {
            int result;
            if (txtb_Amount_Invoices.Text == "" || txtb_Contact_Person.Text == "" || txtb_Operating_System.Text == "" || txtb_Project_Name.Text == "" || txtb_Software.Text == "" || txtb_Hardware.Text == "" || !int.TryParse(txtb_Amount_Invoices.Text, out result))
            {
                MessageBox.Show("Please make sure all the fields are filled in correctly.");
            }
            else if(dtp_Deadline.Value <= DateTime.Now)
            {
                MessageBox.Show("Date cannot be today or in the past.");
            }
            else
            {
                dbh.OpenConnection();
                string date = DateHandler.GetDate(dtp_Deadline);

                if (dbh.AddProject(cb_Select_Customer.SelectedValue.ToString(), txtb_Project_Name.Text, cbox_Project_Status.SelectedIndex, txtb_Operating_System.Text, txtb_Software.Text, txtb_Amount_Invoices.Text, txtb_Contact_Person.Text, cbox_Maintenance_Contract.SelectedIndex, date,txtb_Hardware.Text))
                    MessageBox.Show("Succesfully added a project!");
                else 
                    MessageBox.Show("An error occcured while adding a project.");

                dbh.CloseConnection();
            }
        }

        private void txtb_Amount_Invoices_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void tc_Main_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tcp_Main.SelectedIndex == 0)
            {
                try
                {
                    showallNotifications = false;
                    ShowNotifications();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occured: \n" + ex);
                }
                finally
                {
                    dbh.CloseConnection();
                }
            }
            if (tcp_Main.SelectedIndex == 1)
            {
                try
                {
                    showallProjects = false;
                    ShowProjects();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occured: \n" + ex);
                }
                finally
                {
                    dbh.CloseConnection();
                }
            }

            if (tcp_Main.SelectedIndex == 2)
            {
                try
                {
                    showallAppointments = false;
                    ShowAppointments();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occured: \n" + ex);
                }
                finally
                {
                    dbh.CloseConnection();
                }
            }

            if (tcp_Main.SelectedIndex == 4)
            {
                try
                {
                    dbh.OpenConnection();
                    DataTable dt = dbh.GetCustomerCB();
                    cb_Select_Customer.ValueMember = "customer_id";
                    cb_Select_Customer.DisplayMember = "full_name";

                    cb_Select_Customer.DataSource = dt;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occured: \n" + ex);
                }
                finally
                {
                    dbh.CloseConnection();
                }
            }
        }

        private void ShowAppointments()
        {
            dbh.OpenConnection();
            DataTable dt = dbh.GetAppointments();
            appointmentsPanel.Controls.Clear();
            int amount = dt.Rows.Count;
            if (!showallAppointments && amount > 5)
            {
                amount = 5;

                Button btn_showAllAppointments = new Button();
                btn_showAllAppointments.Text = "Show all appointments";
                btn_showAllAppointments.Dock = DockStyle.Bottom;
                btn_showAllAppointments.Click += new System.EventHandler(this.ShowAllAppointments);
                appointmentsPanel.Controls.Add(btn_showAllAppointments);
                dbh.CloseConnection();

            }

            AppointmentPanel[] appointmentInfoPanel = new AppointmentPanel[amount];

            for (int i = 0; i < appointmentInfoPanel.Length; i++)
            {
                appointmentInfoPanel[i] = new AppointmentPanel(i, dt);
                appointmentInfoPanel[i].BorderStyle = BorderStyle.FixedSingle;
                appointmentInfoPanel[i].Dock = DockStyle.Top;
                appointmentsPanel.Controls.Add(appointmentInfoPanel[i]);
                appointmentInfoPanel[i].btn_Edit.Dispose();
            }
            dbh.CloseConnection();
        }

        private void ShowAllAppointments(object sender, EventArgs e)
        {
            showallAppointments = true;
            appointmentsPanel.Controls.Clear();
            ShowAppointments();
        }

        private void ShowProjects()
        {
            try
            {
                dbh.OpenConnection();
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occured: \n\n" + ex);
            }

            projectsPanel.Controls.Clear();
            DataTable dt = dbh.GetProjects();
            int amount = dt.Rows.Count;
            if (!showallProjects && amount > 5)
            {
                amount = 5;

                Button btn_showAllProjects = new Button();
                btn_showAllProjects.Text = "Show all projects";
                btn_showAllProjects.Dock = DockStyle.Bottom;
                btn_showAllProjects.Click += new System.EventHandler(this.showAllProjects);
                projectsPanel.Controls.Add(btn_showAllProjects);
                dbh.CloseConnection();
            }

            ProjectPanel[] projectInfoPanel = new ProjectPanel[amount];

            for (int i = 0; i < projectInfoPanel.Length; i++ )
            {
                projectInfoPanel[i] = new ProjectPanel(i,dt);
                projectInfoPanel[i].BorderStyle = BorderStyle.FixedSingle;
                projectInfoPanel[i].Dock = DockStyle.Top;
                projectInfoPanel[i].btn_Edit.Click += new System.EventHandler(this.FillEditProjectItems);
                projectInfoPanel[i].btn_Edit.AccessibleName = projectInfoPanel[i].lbl_Project_Id.Text;
                projectInfoPanel[i].lbl_Customer_Name.AccessibleName = dt.Rows[i]["customer_id"].ToString();
                projectInfoPanel[i].lbl_Customer_Name.Click += new System.EventHandler(this.FillCustomerData);
                projectsPanel.Controls.Add(projectInfoPanel[i]);
            }
            dbh.CloseConnection();
        }

        private void FillEditProjectItems(object sender, EventArgs e)
        {
            dbh.OpenConnection();

            Button button = (Button)sender;
            DataTable dt = dbh.GetProject(button.AccessibleName);

            txtb_Edit_Project_P_Name.Text = dt.Rows[0]["project_name"].ToString();
            txtb_Edit_Project_OS.Text = dt.Rows[0]["operating_system"].ToString();
            txtb_Edit_Project_Software.Text = dt.Rows[0]["software"].ToString();
            txtb_Edit_Project_Hardware.Text = dt.Rows[0]["hardware"].ToString();
            lbl_Edit_Project_C_P.Text = dt.Rows[0]["contact_person"].ToString();
            txtb_Edit_Project_AOI.Text = dt.Rows[0]["amount_invoice"].ToString();
            cb_Edit_Project_M_C.SelectedIndex = Convert.ToInt32(dt.Rows[0]["maintenance_contract"].ToString());
            cb_Edit_Project_P_Status.SelectedIndex = Convert.ToInt32(dt.Rows[0]["project_status"].ToString());
            lbl_Edit_Project_C_ID.Text = dt.Rows[0]["full_name"].ToString();
            dtp_Edit_Project_Deadline.Value = Convert.ToDateTime(dt.Rows[0]["deadline_date"]);
            lbl_Edit_Project_P_Id.Text = button.AccessibleName;

            dbh.CloseConnection();
            tcp_Main.SelectedIndex = 5;
        }

        private void showAllProjects(object sender, EventArgs e)
        {
            showallProjects = true;
            projectsPanel.Controls.Clear();
            ShowProjects();
        }

        private void FillCustomerData(object sender, EventArgs e)
        {
            dbh.OpenConnection();

            Label label = (Label)sender;
            DataTable dt = dbh.GetCustomer(label.AccessibleName);
            lbl_Customer_Name.Text = dt.Rows[0]["customer_name"].ToString();
            lbl_Residence.Text = dt.Rows[0]["residence"].ToString();
            lbl_Address.Text = dt.Rows[0]["address"].ToString();
            lbl_Zip_Code.Text = dt.Rows[0]["zip_code"].ToString();
            lbl_Email.Text = dt.Rows[0]["email"].ToString();
            lbl_Phonenumber.Text = dt.Rows[0]["phone_number"].ToString();
            lbl_Fax.Text = dt.Rows[0]["fax"].ToString();
            lbl_Company_Name.Text = dt.Rows[0]["company_name"].ToString();
            lbl_Residence2.Text = dt.Rows[0]["residence_2"].ToString();
            lbl_Address2.Text = dt.Rows[0]["address_2"].ToString();
            lbl_Zip_Code2.Text = dt.Rows[0]["zip_code_2"].ToString();
            lbl_Phonenumber2.Text = dt.Rows[0]["phone_number_2"].ToString();

            foreach (Control control in panel2.Controls)
            {
                if (control.AccessibleName == "Controls" && (control.Text == "" || control.Text == "0" || control.Text == " 0"))
                {
                    control.Text = "N/A";
                }
            }

            dbh.CloseConnection();
            tcp_Main.SelectedIndex = 6;

        }

        private void btn_Project_Show_All_Click(object sender, EventArgs e)
        {
            tcp_Main.SelectedIndex = 6;

            showallProjects = true;
            projectsPanel.Controls.Clear();
            ShowProjects();
        }

        private void EditProject(object sender, EventArgs e)
        {
            string date = DateHandler.GetDate(dtp_Edit_Project_Deadline);
            int result;
            if (!int.TryParse(txtb_Edit_Project_AOI.Text, out result))
            {
                MessageBox.Show("Amount of Invoices not a number!");
            }
            else if(txtb_Edit_Project_P_Name.Text == "" || txtb_Edit_Project_OS.Text == "" || txtb_Edit_Project_Software.Text == "" || txtb_Edit_Project_Hardware.Text == "")
            {
                MessageBox.Show("Make sure all fields are filled in correctly!");
            }
            else if(dtp_Edit_Project_Deadline.Value <= DateTime.Now)
            {
                MessageBox.Show("Deadline date cannot be today or in the past!");
            }
            else
            { 
                dbh.OpenConnection();

                if(dbh.EditProject(lbl_Edit_Project_P_Id.Text, txtb_Edit_Project_P_Name.Text, cb_Edit_Project_P_Status.SelectedIndex.ToString(), cb_Edit_Project_M_C.SelectedIndex.ToString(), txtb_Edit_Project_OS.Text, txtb_Edit_Project_Hardware.Text, txtb_Edit_Project_Software.Text, txtb_Edit_Project_AOI.Text, date))
                    MessageBox.Show("Succesfully added a project!");
                else
                    MessageBox.Show("An error occcured while adding a project.");

                dbh.CloseConnection();
            }
        }

        private void btn_showallAppointments_Click(object sender, EventArgs e)
        {
            showallAppointments = true;
            appointmentsPanel.Controls.Clear();
            ShowAppointments();
        }

        private void btn_GoTo_Appointment(object sender, EventArgs e)
        {
            tcp_Main.SelectedIndex = 8;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (e.CloseReason == CloseReason.WindowsShutDown) return;

            switch (MessageBox.Show(this, "Are you sure you want to exit? Any unsaved changes will be lost.", "Barroc-IT - Software for real - Development", MessageBoxButtons.YesNo, MessageBoxIcon.Warning))
            {
                case DialogResult.No:
                    e.Cancel = true;
                    break;
                default:
                    Frm_Login frmlogin = new Frm_Login();
                    frmlogin.Show();
                    break;
            }
        }


        private void SearchProjectOnP_Name(object sender, KeyEventArgs e)
        {
            string filter = tstxtb_Projects_PName.Text;
            if (e.KeyCode == Keys.Return)
            {
                dbh.OpenConnection();
                projectsPanel.Controls.Clear();
                DataTable dt = dbh.FilterProjects(filter, "project_name");
                int amount = dt.Rows.Count;
                if (!showallProjects && amount > 5)
                    amount = 5;

                ProjectPanel[] projectInfoPanel = new ProjectPanel[amount];

                for (int i = 0; i < projectInfoPanel.Length; i++)
                {
                    projectInfoPanel[i] = new ProjectPanel(i, dt);
                    projectInfoPanel[i].BorderStyle = BorderStyle.FixedSingle;
                    projectInfoPanel[i].Dock = DockStyle.Top;
                    projectInfoPanel[i].btn_Edit.Click += new System.EventHandler(this.FillEditProjectItems);
                    projectInfoPanel[i].btn_Edit.AccessibleName = projectInfoPanel[i].lbl_Project_Id.Text;
                    projectInfoPanel[i].lbl_Customer_Name.AccessibleName = dt.Rows[i]["customer_id"].ToString();
                    projectInfoPanel[i].lbl_Customer_Name.Click += new System.EventHandler(this.FillCustomerData);
                    projectsPanel.Controls.Add(projectInfoPanel[i]);
                }
                dbh.CloseConnection();
                tcp_Main.SelectedIndex = 1;
            }
        }

        private void SearchProjectOnP_ID(object sender, KeyEventArgs e)
        {
            string filter = tstxtb_Projects_PID.Text;
            if (e.KeyCode == Keys.Return)
            {
                dbh.OpenConnection();
                projectsPanel.Controls.Clear();
                DataTable dt = dbh.FilterProjects(filter, "project_id");
                int amount = dt.Rows.Count;
                if (!showallProjects && amount > 5)
                    amount = 5;

                ProjectPanel[] projectInfoPanel = new ProjectPanel[amount];

                for (int i = 0; i < projectInfoPanel.Length; i++)
                {
                    projectInfoPanel[i] = new ProjectPanel(i, dt);
                    projectInfoPanel[i].BorderStyle = BorderStyle.FixedSingle;
                    projectInfoPanel[i].Dock = DockStyle.Top;
                    projectInfoPanel[i].btn_Edit.Click += new System.EventHandler(this.FillEditProjectItems);
                    projectInfoPanel[i].btn_Edit.AccessibleName = projectInfoPanel[i].lbl_Project_Id.Text;
                    projectInfoPanel[i].lbl_Customer_Name.AccessibleName = dt.Rows[i]["customer_id"].ToString();
                    projectInfoPanel[i].lbl_Customer_Name.Click += new System.EventHandler(this.FillCustomerData);
                    projectsPanel.Controls.Add(projectInfoPanel[i]);
                }
                dbh.CloseConnection();
                tcp_Main.SelectedIndex = 1;
            }
        }


        private void SearchProjectOnC_Name(object sender, KeyEventArgs e)
        {
            string filter = tstxtb_Projects_CuName.Text;
            if (e.KeyCode == Keys.Return)
            {
                dbh.OpenConnection();
                projectsPanel.Controls.Clear();
                DataTable dt = dbh.FilterProjects(filter, "tbl_customers.first_name", "tbl_customers.last_name");
                int amount = dt.Rows.Count;

                ProjectPanel[] projectInfoPanel = new ProjectPanel[amount];

                for (int i = 0; i < projectInfoPanel.Length; i++)
                {
                    projectInfoPanel[i] = new ProjectPanel(i, dt);
                    projectInfoPanel[i].BorderStyle = BorderStyle.FixedSingle;
                    projectInfoPanel[i].Dock = DockStyle.Top;
                    projectInfoPanel[i].btn_Edit.Click += new System.EventHandler(this.FillEditProjectItems);
                    projectInfoPanel[i].btn_Edit.AccessibleName = projectInfoPanel[i].lbl_Project_Id.Text;
                    projectInfoPanel[i].lbl_Customer_Name.AccessibleName = dt.Rows[i]["customer_id"].ToString();
                    projectInfoPanel[i].lbl_Customer_Name.Click += new System.EventHandler(this.FillCustomerData);
                    projectsPanel.Controls.Add(projectInfoPanel[i]);
                }
                dbh.CloseConnection();
                tcp_Main.SelectedIndex = 1;
            }
        }

        private void SearchAppointmentOnCu_Name(object sender, KeyEventArgs e)
        {
            string filter = tstxtb_Appointments_CuName.Text;
            if (e.KeyCode == Keys.Return)
            {
                dbh.OpenConnection();
                appointmentsPanel.Controls.Clear();
                DataTable dt = dbh.FilterAppointments(filter, "tbl_customers.first_name", "tbl_customers.last_name");
                int amount = dt.Rows.Count;
                if (!showallAppointments && amount > 5)
                    amount = 5;

                AppointmentPanel[] appointmentInfoPanel = new AppointmentPanel[amount];

                for (int i = 0; i < appointmentInfoPanel.Length; i++)
                {
                    appointmentInfoPanel[i] = new AppointmentPanel(i, dt);
                    appointmentInfoPanel[i].BorderStyle = BorderStyle.FixedSingle;
                    appointmentInfoPanel[i].Dock = DockStyle.Top;
                    appointmentsPanel.Controls.Add(appointmentInfoPanel[i]);
                    appointmentInfoPanel[i].btn_Edit.Dispose();
                }
                dbh.CloseConnection();
            }
        }

        private void SearchAppointmentOnCo_Name(object sender, KeyEventArgs e)
        {
            string filter = tstxtb_Appointments_CoName.Text;
            if (e.KeyCode == Keys.Return)
            {
                dbh.OpenConnection();
                appointmentsPanel.Controls.Clear();
                DataTable dt = dbh.FilterAppointments(filter, "tbl_customers.company_name");
                int amount = dt.Rows.Count;
                if (!showallAppointments && amount > 5)
                    amount = 5;

                AppointmentPanel[] appointmentInfoPanel = new AppointmentPanel[amount];

                for (int i = 0; i < appointmentInfoPanel.Length; i++)
                {
                    appointmentInfoPanel[i] = new AppointmentPanel(i, dt);
                    appointmentInfoPanel[i].BorderStyle = BorderStyle.FixedSingle;
                    appointmentInfoPanel[i].Dock = DockStyle.Top;
                    appointmentsPanel.Controls.Add(appointmentInfoPanel[i]);
                    appointmentInfoPanel[i].btn_Edit.Dispose();
                }
                dbh.CloseConnection();
            }
        }

        private void SearchAppointmentOnResidence(object sender, KeyEventArgs e)
        {
            string filter = tstxtb_Appointments_Residence.Text;
            if (e.KeyCode == Keys.Return)
            {
                dbh.OpenConnection();
                appointmentsPanel.Controls.Clear();
                DataTable dt = dbh.FilterAppointments(filter, "tbl_customers.residence");
                int amount = dt.Rows.Count;
                if (!showallAppointments && amount > 5)
                    amount = 5;

                AppointmentPanel[] appointmentInfoPanel = new AppointmentPanel[amount];

                for (int i = 0; i < appointmentInfoPanel.Length; i++)
                {
                    appointmentInfoPanel[i] = new AppointmentPanel(i, dt);
                    appointmentInfoPanel[i].BorderStyle = BorderStyle.FixedSingle;
                    appointmentInfoPanel[i].Dock = DockStyle.Top;
                    appointmentsPanel.Controls.Add(appointmentInfoPanel[i]);
                    appointmentInfoPanel[i].btn_Edit.Dispose();
                }
                dbh.CloseConnection();
            }
        }

        private void SearchAppointmentsOnSummary(object sender, EventArgs e)
        {
            if (tscmb_Appointments_Summary.Text == "All")
            {
                appointmentsPanel.Controls.Clear();

                dbh.OpenConnection();
                DataTable dt = dbh.GetAppointments();
                int amount = dt.Rows.Count;

                AppointmentPanel[] appointmentInfoPanel = new AppointmentPanel[amount];

                for (int i = 0; i < appointmentInfoPanel.Length; i++)
                {
                    appointmentInfoPanel[i] = new AppointmentPanel(i, dt);
                    appointmentInfoPanel[i].BorderStyle = BorderStyle.FixedSingle;
                    appointmentInfoPanel[i].Dock = DockStyle.Top;
                    appointmentsPanel.Controls.Add(appointmentInfoPanel[i]);
                    appointmentInfoPanel[i].btn_Edit.AccessibleName = dt.Rows[i]["appointment_id"].ToString();
                    appointmentInfoPanel[i].btn_Edit.Dispose();
                }
                dbh.CloseConnection();
            }
            else if (tscmb_Appointments_Summary.Text == "Has summary")
            {
                dbh.OpenConnection();
                appointmentsPanel.Controls.Clear();
                DataTable dt = dbh.FilterAppointmentsHasSummary();
                int amount = dt.Rows.Count;

                AppointmentPanel[] appointmentInfoPanel = new AppointmentPanel[amount];

                for (int i = 0; i < appointmentInfoPanel.Length; i++)
                {
                    appointmentInfoPanel[i] = new AppointmentPanel(i, dt);
                    appointmentInfoPanel[i].BorderStyle = BorderStyle.FixedSingle;
                    appointmentInfoPanel[i].Dock = DockStyle.Top;
                    appointmentsPanel.Controls.Add(appointmentInfoPanel[i]);
                    appointmentInfoPanel[i].btn_Edit.AccessibleName = dt.Rows[i]["appointment_id"].ToString();
                    appointmentInfoPanel[i].btn_Edit.Dispose();
                }
                dbh.CloseConnection();
            }
            else
            {
                dbh.OpenConnection();
                appointmentsPanel.Controls.Clear();
                DataTable dt = dbh.FilterAppointmentsHasNoSummary();
                int amount = dt.Rows.Count;

                AppointmentPanel[] appointmentInfoPanel = new AppointmentPanel[amount];

                for (int i = 0; i < appointmentInfoPanel.Length; i++)
                {
                    appointmentInfoPanel[i] = new AppointmentPanel(i, dt);
                    appointmentInfoPanel[i].BorderStyle = BorderStyle.FixedSingle;
                    appointmentInfoPanel[i].Dock = DockStyle.Top;
                    appointmentsPanel.Controls.Add(appointmentInfoPanel[i]);
                    appointmentInfoPanel[i].btn_Edit.AccessibleName = dt.Rows[i]["appointment_id"].ToString();
                    appointmentInfoPanel[i].btn_Edit.Dispose();
                }
                dbh.CloseConnection();
            }
        }

        private void SearchNotificationOnDepartment(object sender, EventArgs e)
        {
            if (tscmb_Overview_Department.Text == "All")
            {
                dbh.OpenConnection();
                notificationsPanel.Controls.Clear();
                DataTable dt = dbh.GetNotifications();
                int amount = dt.Rows.Count;
                if (!showallNotifications && amount > 5)
                    amount = 5;

                OverviewPanel[] overviewInfoPanel = new OverviewPanel[amount];

                for (int i = 0; i < overviewInfoPanel.Length; i++)
                {
                    overviewInfoPanel[i] = new OverviewPanel(i, dt);
                    overviewInfoPanel[i].BorderStyle = BorderStyle.FixedSingle;
                    overviewInfoPanel[i].Dock = DockStyle.Top;
                    overviewInfoPanel[i].btn_Resolved.AccessibleName = dt.Rows[i]["notification_ID"].ToString();
                    overviewInfoPanel[i].btn_Resolved.Click += new System.EventHandler(this.ResolveNotification);
                    notificationsPanel.Controls.Add(overviewInfoPanel[i]);
                }
                dbh.CloseConnection();
            }
            else
            {
                string filter = tscmb_Overview_Department.Text;
                dbh.OpenConnection();
                notificationsPanel.Controls.Clear();
                DataTable dt = dbh.FilterNotifications(filter, "notification_department");
                int amount = dt.Rows.Count;

                OverviewPanel[] overviewInfoPanel = new OverviewPanel[amount];

                for (int i = 0; i < overviewInfoPanel.Length; i++)
                {
                    overviewInfoPanel[i] = new OverviewPanel(i, dt);
                    overviewInfoPanel[i].BorderStyle = BorderStyle.FixedSingle;
                    overviewInfoPanel[i].Dock = DockStyle.Top;
                    overviewInfoPanel[i].btn_Resolved.AccessibleName = dt.Rows[i]["notification_ID"].ToString();
                    overviewInfoPanel[i].btn_Resolved.Click += new System.EventHandler(this.ResolveNotification);
                    notificationsPanel.Controls.Add(overviewInfoPanel[i]);
                }
                dbh.CloseConnection();
            }
        }

        private void SearchNotificationOnType(object sender, EventArgs e)
        {
            if (tscmb_Overview_Type.Text == "All")
            {
                dbh.OpenConnection();
                notificationsPanel.Controls.Clear();
                DataTable dt = dbh.GetNotifications();
                int amount = dt.Rows.Count;
                if (!showallNotifications && amount > 5)
                    amount = 5;

                OverviewPanel[] overviewInfoPanel = new OverviewPanel[amount];

                for (int i = 0; i < overviewInfoPanel.Length; i++)
                {
                    overviewInfoPanel[i] = new OverviewPanel(i, dt);
                    overviewInfoPanel[i].BorderStyle = BorderStyle.FixedSingle;
                    overviewInfoPanel[i].Dock = DockStyle.Top;
                    overviewInfoPanel[i].btn_Resolved.AccessibleName = dt.Rows[i]["notification_ID"].ToString();
                    overviewInfoPanel[i].btn_Resolved.Click += new System.EventHandler(this.ResolveNotification);
                    notificationsPanel.Controls.Add(overviewInfoPanel[i]);
                }
                dbh.CloseConnection();
            }
            else
            {
                string filter = tscmb_Overview_Type.Text;
                dbh.OpenConnection();
                notificationsPanel.Controls.Clear();
                DataTable dt = dbh.FilterNotifications(filter, "notification_type");
                int amount = dt.Rows.Count;

                OverviewPanel[] overviewInfoPanel = new OverviewPanel[amount];

                for (int i = 0; i < overviewInfoPanel.Length; i++)
                {
                    overviewInfoPanel[i] = new OverviewPanel(i, dt);
                    overviewInfoPanel[i].BorderStyle = BorderStyle.FixedSingle;
                    overviewInfoPanel[i].Dock = DockStyle.Top;
                    overviewInfoPanel[i].btn_Resolved.AccessibleName = dt.Rows[i]["notification_ID"].ToString();
                    overviewInfoPanel[i].btn_Resolved.Click += new System.EventHandler(this.ResolveNotification);
                    notificationsPanel.Controls.Add(overviewInfoPanel[i]);
                }
                dbh.CloseConnection();
            }
        }

        private void SearchNotificationOnHasSummary(object sender, EventArgs e)
        {
            // WORK IN PROGRESS
        }

        private void ShowNotifications()
        {
            dbh.OpenConnection();
            DataTable dt = dbh.GetNotifications();
            notificationsPanel.Controls.Clear();
            int amount = dt.Rows.Count;
            if (!showallNotifications && amount > 5)
            {
                amount = 5;

                Button btn_showAllNotifications = new Button();
                btn_showAllNotifications.Text = "Show all projects";
                btn_showAllNotifications.Dock = DockStyle.Bottom;
                btn_showAllNotifications.Click += new System.EventHandler(this.ShowAllNotifications);
                notificationsPanel.Controls.Add(btn_showAllNotifications);
                dbh.CloseConnection();
            }

            OverviewPanel[] overviewInfoPanel = new OverviewPanel[amount];

            for (int i = 0; i < overviewInfoPanel.Length; i++)
            {
                overviewInfoPanel[i] = new OverviewPanel(i, dt);
                overviewInfoPanel[i].BorderStyle = BorderStyle.FixedSingle;
                overviewInfoPanel[i].Dock = DockStyle.Top;
                overviewInfoPanel[i].btn_Resolved.AccessibleName = dt.Rows[i]["notification_ID"].ToString();
                overviewInfoPanel[i].btn_Resolved.Click += new System.EventHandler(this.ResolveNotification);
                notificationsPanel.Controls.Add(overviewInfoPanel[i]);
            }
            dbh.CloseConnection();
        }

        private void ShowAllNotifications(object sender, EventArgs e)
        {
            showallNotifications = true;
            notificationsPanel.Controls.Clear();
            ShowNotifications();
        }

        private void ResolveNotification(object sender, EventArgs e)
        {
            dbh.OpenConnection();
            Button button = (Button)sender;
            dbh.ResolveNotification(button.AccessibleName);
            dbh.CloseConnection();
        }

        private void btn_Show_All_Notifications_Click(object sender, EventArgs e)
        {
            notificationsPanel.Controls.Clear();
            showallNotifications = true;
            ShowNotifications();
        }

        private void mnitem_Help_Click(object sender, EventArgs e)
        {
            tcp_Main.SelectedIndex = 3;
        }
    }
}
