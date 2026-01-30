using System;
using System.Data;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DentalClinic.Properties;
using Microsoft.Data.SqlClient;
using Microsoft.VisualBasic.ApplicationServices;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace DentalClinic
{
    public partial class Patients : Form
    {
        //Database Connection
        Functions Con;
        private string connectionString = "Data Source=.;Initial Catalog=CMS" +
            "                               ;Integrated Security=True;Encrypt=False";

        //Database Queries
        string QuerySelect = "SELECT PatientID, FirstName, LastName, DateOfBirth, " +
                       "Gender, ContactNumber, EmergencyContact, Address FROM PatientsTbl";

        string QueryInsert = "INSERT INTO PatientsTbl (FirstName, LastName, DateOfBirth," +
                            "Gender, ContactNumber, EmergencyContact, Address) " +

            "VALUES (@FirstName, @LastName, @DateOfBirth, @Gender, " +
            "@ContactNumber, @EmergencyContact, @Address)";

        string QueryUpdate = "UPDATE PatientsTbl SET FirstName = @FirstName, " +
            "LastName = @LastName, DateOfBirth = @DateOfBirth, " +
            "Gender = @Gender, ContactNumber = @ContactNumber, " +
            "EmergencyContact = @EmergencyContact, Address = @Address " +

            "WHERE PatientID = @PatientID";

        string QueryDelete = "DELETE FROM PatientsTbl WHERE PatientID = @PatientID";

        private bool isDragging = false;
        private Point startPoint = new Point(0, 0);

        //Constructors
        public Patients()
        {
            InitializeComponent();
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));
            // Enable dragging only for panel1
            panel1.MouseDown += Panel1_MouseDown;
            panel1.MouseMove += Panel1_MouseMove;
            panel1.MouseUp += Panel1_MouseUp;
            patientEmegencyContactNumber.KeyPress += NumericOnly_KeyPress;
            patientContactNumber.KeyPress += NumericOnly_KeyPress;
            patientAddress.KeyDown += patientAddress_KeyDown;
            Con = new Functions();
            showPatients();
        }

        // Mouse Down Event for panel1: Start dragging
        private void Panel1_MouseDown(object sender, MouseEventArgs e)
        {
            isDragging = true;
            startPoint = new Point(e.X, e.Y);
        }

        // Mouse Move Event for panel1: Move form
        private void Panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                Point p = PointToScreen(e.Location);
                this.Location = new Point(p.X - startPoint.X, p.Y - startPoint.Y);
            }
        }

        // Mouse Up Event for panel1: Stop dragging
        private void Panel1_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
        }

        //Show Patients
        private void showPatients()
        {
            try
            {
                string Query = QuerySelect;
                patientDataView.DataSource = Con.GetData(Query);


                if (patientDataView.Columns["PatientID"] != null)
                    patientDataView.Columns["PatientID"].HeaderText = "Patient ID";

                if (patientDataView.Columns["FirstName"] != null)
                    patientDataView.Columns["FirstName"].HeaderText = "First Name";

                if (patientDataView.Columns["LastName"] != null)
                    patientDataView.Columns["LastName"].HeaderText = "Last Name";

                if (patientDataView.Columns["DateOfBirth"] != null)
                    patientDataView.Columns["DateOfBirth"].HeaderText = "Date Of Birth";

                if (patientDataView.Columns["ContactNumber"] != null)
                    patientDataView.Columns["ContactNumber"].HeaderText = "Contact Number";

                if (patientDataView.Columns["EmergencyContact"] != null)
                    patientDataView.Columns["EmergencyContact"].HeaderText = "Emergency Contact";
            }
            catch (Exception Ex)
            {
                MessageBox.Show("An error occurred while fetching patients: " + Ex.Message);
            }
        }



        // Select Patient ID
        int Key = 0;
        private void patientDataView_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {
            // Check if a valid row is selected
            if (e.RowIndex >= 0 && e.RowIndex < patientDataView.Rows.Count)
            {
                // Get the selected row
                DataGridViewRow selectedRow = patientDataView.Rows[e.RowIndex];

                // Safely set the text fields, checking for null values and we used "??" operator to check if the value is null
                patientFirstName.Text = selectedRow.Cells[1].Value?.ToString() ?? string.Empty;
                patientLastName.Text = selectedRow.Cells[2].Value?.ToString() ?? string.Empty;
                patientDOB.Text = selectedRow.Cells[3].Value?.ToString() ?? string.Empty;
                patientGender.Text = selectedRow.Cells[4].Value?.ToString() ?? string.Empty;
                patientContactNumber.Text = selectedRow.Cells[5].Value?.ToString() ?? string.Empty;
                patientEmegencyContactNumber.Text = selectedRow.Cells[6].Value?.ToString() ?? string.Empty;
                patientAddress.Text = selectedRow.Cells[7].Value?.ToString() ?? string.Empty;

                // If the first name is empty, reset the Key
                if (string.IsNullOrWhiteSpace(patientFirstName.Text))
                {
                    Key = 0;
                }
                else
                {
                    Key = Convert.ToInt32(selectedRow.Cells[0].Value?.ToString() ?? "0");
                }
            }
            else
            {
                MessageBox.Show("Please select a valid patient row.");
            }
        }



        // Update Patient
        private void updateBtn_Click(object sender, EventArgs e)
        {
            // Confirmation Message
            DialogResult result = MessageBox.Show("Are you sure you want to update this Patient's info?", "Confirmation", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                try
                {
                    // Check for empty fields
                    if (string.IsNullOrWhiteSpace(patientFirstName.Text) ||
                        string.IsNullOrWhiteSpace(patientLastName.Text) ||
                        string.IsNullOrWhiteSpace(patientDOB.Text) ||
                        string.IsNullOrWhiteSpace(patientGender.Text) ||
                        string.IsNullOrWhiteSpace(patientContactNumber.Text) ||
                        string.IsNullOrWhiteSpace(patientEmegencyContactNumber.Text) ||
                        string.IsNullOrWhiteSpace(patientAddress.Text))
                    {
                        MessageBox.Show("Please Fill All the Details");
                        return;
                    }
                    // Check if a patient is selected
                    if (Key == 0)
                    {
                        MessageBox.Show("Please select a patient to update.");
                        return;
                    }
                    // Sql Query
                    string Query = QueryUpdate;

                    //Connection to Database and Execute Query
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        using (SqlCommand cmd = new SqlCommand(Query, conn))
                        {
                            cmd.Parameters.AddWithValue("@FirstName", patientFirstName.Text);
                            cmd.Parameters.AddWithValue("@LastName", patientLastName.Text);
                            cmd.Parameters.AddWithValue("@DateOfBirth", DateTime.Parse(patientDOB.Text));
                            cmd.Parameters.AddWithValue("@Gender", patientGender.Text);
                            cmd.Parameters.AddWithValue("@ContactNumber", patientContactNumber.Text);
                            cmd.Parameters.AddWithValue("@EmergencyContact", patientEmegencyContactNumber.Text);
                            cmd.Parameters.AddWithValue("@Address", patientAddress.Text);
                            cmd.Parameters.AddWithValue("@PatientID", Key);

                            conn.Open();
                            cmd.ExecuteNonQuery();
                        }
                    }
                    // Refresh the data grid view
                    showPatients();
                    MessageBox.Show("Patient Updated Successfully");

                    // Clear the input fields
                    patientFirstName.Text = "";
                    patientLastName.Text = "";
                    patientDOB.Text = "";
                    patientGender.Text = "";
                    patientContactNumber.Text = "";
                    patientEmegencyContactNumber.Text = "";
                    patientAddress.Text = "";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while updating the patient: " + ex.Message);
                }
            }
        }
        // Delete Patient
        private void deleteBtn_Click(object sender, EventArgs e)
        {
            // Confirmation Message
            DialogResult result = MessageBox.Show("Are you sure you want to Delele this Patient?", "Confirmation", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {

                try
                {
                    // Check if a patient is selected
                    if (Key == 0)
                    {
                        MessageBox.Show("Please select a patient to delete.");
                        return;
                    }

                    // Sql Query
                    string Query = QueryDelete;

                    //Connection to Database and Execute Query
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        using (SqlCommand cmd = new SqlCommand(Query, conn))
                        {
                            cmd.Parameters.AddWithValue("@PatientID", Key);

                            conn.Open();
                            cmd.ExecuteNonQuery();
                        }
                    }

                    showPatients();
                    MessageBox.Show("Patient Deleted Successfully");

                    // Clear the input fields
                    patientFirstName.Text = "";
                    patientLastName.Text = "";
                    patientDOB.Text = "";
                    patientGender.Text = "";
                    patientContactNumber.Text = "";
                    patientEmegencyContactNumber.Text = "";
                    patientAddress.Text = "";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while deleting the patient: " + ex.Message);
                }
            }
        }

        // Logout
        private void logoutBtn_Click(object sender, EventArgs e)
        {
            // Confirmation Message
            DialogResult result = MessageBox.Show("Are you sure you want to Logout?"
                , "Confirmation", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                MessageBox.Show("Logout successful.");
                this.Hide();
                Login loginForm = new Login();
                loginForm.Show();
            }
        }
        // Navigation
        private void treatmentBtn_Click(object sender, EventArgs e)
        {
            this.Close();
            Treatments obj = new Treatments();
            obj.Show();
        }
        private void appointmentBtn_Click(object sender, EventArgs e)
        {
            this.Close();
            Appointments obj = new Appointments();
            obj.Show();
        }
        private void prescriptionsBtn_Click(object sender, EventArgs e)
        {
            this.Close();
            Prescriptions obj = new Prescriptions();
            obj.Show();
        }
        // Close System
        private void closeSystemBtn_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        //Code Snippet for Rounded Corners
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
       (
           int nLeftRect,     // x-coordinate of upper-left corner
           int nTopRect,      // y-coordinate of upper-left corner
           int nRightRect,    // x-coordinate of lower-right corner
           int nBottomRect,   // y-coordinate of lower-right corner
           int nWidthEllipse, // height of ellipse
           int nHeightEllipse // width of ellipse
       );

        // Add Patient
        private void addBtn_Click__Click(object sender, EventArgs e)
        {
            // Confirmation Message
            DialogResult result = MessageBox.Show("Are you sure you want to insert the patient?", "Confirmation", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                // Try to insert the patient
                try
                {
                    // Check for empty fields
                    if (string.IsNullOrWhiteSpace(patientFirstName.Text) ||
                        string.IsNullOrWhiteSpace(patientLastName.Text) ||
                        string.IsNullOrWhiteSpace(patientDOB.Text) ||
                        string.IsNullOrWhiteSpace(patientGender.Text) ||
                        string.IsNullOrWhiteSpace(patientContactNumber.Text) ||
                        string.IsNullOrWhiteSpace(patientEmegencyContactNumber.Text) ||
                        string.IsNullOrWhiteSpace(patientAddress.Text))
                    {
                        MessageBox.Show("Please Fill All the Details");
                        return;
                    }

                    // Sql Query
                    string Query = QueryInsert;

                    //Connection to Database and Execute Query
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        using (SqlCommand cmd = new SqlCommand(Query, conn))
                        {
                            cmd.Parameters.AddWithValue("@FirstName", patientFirstName.Text);
                            cmd.Parameters.AddWithValue("@LastName", patientLastName.Text);
                            cmd.Parameters.AddWithValue("@DateOfBirth", patientDOB.Value.Date);
                            cmd.Parameters.AddWithValue("@Gender", patientGender.Text);
                            cmd.Parameters.AddWithValue("@ContactNumber", patientContactNumber.Text);
                            cmd.Parameters.AddWithValue("@EmergencyContact", patientEmegencyContactNumber.Text);
                            cmd.Parameters.AddWithValue("@Address", patientAddress.Text);

                            conn.Open();
                            cmd.ExecuteNonQuery();
                        }
                    }
                    // Refresh the data grid view
                    showPatients();
                    MessageBox.Show("Patient Added Successfully");
                    patientFirstName.Text = "";
                    patientLastName.Text = "";
                    patientDOB.Text = "";
                    patientGender.Text = "";
                    patientContactNumber.Text = "";
                    patientEmegencyContactNumber.Text = "";
                    patientAddress.Text = "";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while adding the patient: " + ex.Message);
                }
            }
        }
        private void NumericOnly_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow control characters (e.g., backspace) and digits only
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;

                // Display a warning message
                MessageBox.Show("You can only use Numbers here", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void patientAddress_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                // Check if the last character is a comma
                if (!string.IsNullOrEmpty(patientAddress.Text) && patientAddress.Text[^1] == ',')
                {
                    // Replace the last comma with a space
                    patientAddress.Text = patientAddress.Text.Remove(patientAddress.Text.Length - 1) + " ";
                    patientAddress.SelectionStart = patientAddress.Text.Length; // Move the cursor to the end
                }
                else
                {
                 
                    patientAddress.AppendText(",");
                }
                // Mark the event as handled so that the space doesn't get inserted
                e.SuppressKeyPress = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            PatientsVisted obj = new PatientsVisted();
            obj.Show();
        }
    }
}
