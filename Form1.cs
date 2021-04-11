using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KWSalesOrderFormProject
{
    public partial class grpProducts : Form
    {
        int orderNumber,
            savedIndex;
        bool selectedFile = true,
            newCustomer = false;

        SqlConnection KWSalesConnection;
        SqlCommand ordersCommand;
        SqlDataAdapter ordersAdapter;
        DataTable ordersTable;

        SqlCommand customersCommand;
        SqlDataAdapter customersAdapter;
        DataTable customersTable;
        CurrencyManager customersManager;

        SqlCommand productsCommand;
        SqlDataAdapter productsAdapter;
        DataTable productsTable;

        SqlCommand purchasesCommand;
        SqlDataAdapter purchasesAdapter;
        DataTable purchasesTable;
        long customerID;
        public grpProducts()
        {
            InitializeComponent();
        }


        private void frmKWSales_Load(object sender, EventArgs e)
        {
            if (dlgOpen.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // connect to sales database
                    KWSalesConnection = new SqlConnection("Data Source =.\\SQLEXPRESS;" +
                        "AttachDbFilename=" + dlgOpen.FileName + "; " +
                        "Integrated Security=True;" +
                        "Connect Timeout=30;" +
                        "User Instance=True");
                    KWSalesConnection.Open();
                    // establish Orders command object
                    ordersCommand = new SqlCommand(
                        "SELECT * " +
                        "FROM Orders " +
                        "ORDER BY OrderID", KWSalesConnection);
                    // establish Orders data adapter/data table
                    ordersAdapter = new SqlDataAdapter();
                    ordersAdapter.SelectCommand = ordersCommand;
                    ordersTable = new DataTable();
                    ordersAdapter.Fill(ordersTable);
                    // establish Customers command object
                    customersCommand = new SqlCommand("" +
                        "SELECT * " +
                        "FROM Customers", KWSalesConnection);
                    // establish Customers data adapter/data table
                    customersAdapter = new SqlDataAdapter();
                    customersAdapter.SelectCommand = customersCommand;
                    customersTable = new DataTable();
                    customersAdapter.Fill(customersTable);
                    // bind controls to data table
                    txtFirstName.DataBindings.Add("Text", customersTable, "FirstName");
                    txtLastName.DataBindings.Add("Text", customersTable, "LastName");
                    txtAddress.DataBindings.Add("Text", customersTable, "Address");
                    txtCity.DataBindings.Add("Text", customersTable, "City");
                    txtState.DataBindings.Add("Text", customersTable, "State");
                    txtZip.DataBindings.Add("Text", customersTable, "Zip");
                    // establish currency manager
                    customersManager = (CurrencyManager)this.BindingContext[customersTable];
                    // establish Products command object
                    productsCommand = new SqlCommand(
                        "SELECT * " +
                        "FROM Products " +
                        "ORDER BY Description", KWSalesConnection);
                    // establish Products data adapter/data table
                    productsAdapter = new SqlDataAdapter();
                    productsAdapter.SelectCommand = productsCommand;
                    productsTable = new DataTable();
                    productsAdapter.Fill(productsTable);
                    // bind combobox to data table
                    cboProducts.DataSource = productsTable;
                    cboProducts.DisplayMember = "Description";
                    cboProducts.ValueMember = "ProductID";
                    // establish Purchases command object
                    purchasesCommand = new SqlCommand(
                        "SELECT * " +
                        "FROM Purchases " +
                        "ORDER BY OrderID", KWSalesConnection);
                    // establish Purchases data adapter/data table
                    purchasesAdapter = new SqlDataAdapter();
                    purchasesAdapter.SelectCommand = purchasesCommand;
                    purchasesTable = new DataTable();
                    purchasesAdapter.Fill(purchasesTable);
                    // Fill customers combo box
                    FillCustomers();
                    orderNumber = 0;
                    NewOrder();
                }
                catch (Exception ex)
                {
                    selectedFile = false;
                    MessageBox.Show(
                        ex.Message,
                        "Error establishing database file connection.",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }
            }
            else
            {
                selectedFile = false;
                MessageBox.Show(
                    "No file selected. Program will now exit.",
                    "Program stopping",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                this.Close();
            }
        }
        private void NewOrder()
        {
            string IDString;
            DateTime thisDay = DateTime.Now;
            lblDate.Text = thisDay.ToShortDateString();
            // Build order ID as string
            orderNumber++;
            IDString = thisDay.Year.ToString().Substring(2);
            if (thisDay.Month < 10)
                IDString += "0" + thisDay.Month.ToString();
            else
                IDString += thisDay.Month.ToString();
            if (thisDay.Day < 10)
                IDString += "0" + thisDay.Day.ToString();
            else
                IDString += thisDay.Day.ToString();
            if (orderNumber < 10)
                IDString += "00" + orderNumber.ToString();
            else if (orderNumber < 100)
                IDString += "0" + orderNumber.ToString();
            else
                IDString += orderNumber.ToString();
            lblOrderID.Text = IDString;

            if (cboCustomers.Items.Count != 0)
            {
                cboCustomers.SelectedIndex = 0;
            }
            // Clear purchase information
            cboProducts.SelectedIndex = -1;
            nudQuantity.Value = 1;
            lblTotal.Text = "0.00";
            listCart.Items.Clear();
        }
        private void FillCustomers()
        {
            if (customersTable.Rows.Count != 0)
            {
                for (int nRec = 0; nRec < customersTable.Rows.Count; nRec++)
                {
                    cboCustomers.Items.Add(CustomerListing(customersTable.Rows[nRec]["LastName"].ToString(),
                    customersTable.Rows[nRec]["FirstName"].ToString(),
                    customersTable.Rows[nRec]["CustomerID"].ToString()));
                }
            }
        }
        private string CustomerListing(string lastName, string firstName, string ID)
        {
            return (lastName + ", " + firstName + " (" + ID + ")");
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            // enable text boxes for editing and add row
            newCustomer = true;
            txtFirstName.ReadOnly = false;
            txtLastName.ReadOnly = false;
            txtAddress.ReadOnly = false;
            txtCity.ReadOnly = false;
            txtState.ReadOnly = false;
            txtZip.ReadOnly = false;
            btnNew.Enabled = false;
            btnSave.Enabled = true;
            btnCancel.Enabled = true;
            savedIndex = cboCustomers.SelectedIndex;
            cboCustomers.SelectedIndex = -1;
            cboCustomers.Enabled = false;
            customersManager.AddNew();
            txtFirstName.Focus();
        }

        private void txtFirstName_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((int)e.KeyChar == 13)
                txtLastName.Focus();
        }

        private void txtLastName_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((int)e.KeyChar == 13)
                txtAddress.Focus();
        }

        private void txtAddress_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((int)e.KeyChar == 13)
                txtCity.Focus();
        }

        private void txtCity_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((int)e.KeyChar == 13)
                txtState.Focus();
        }

        private void txtState_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((int)e.KeyChar == 13)
                txtZip.Focus();
        }

        private void txtZip_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((int)e.KeyChar == 13)
                btnSave.Focus();
        }

        private void btnSave_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((int)e.KeyChar == 13)
                btnSave.PerformClick();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            // return to previous customer
            newCustomer = false;
            txtFirstName.ReadOnly = true;
            txtLastName.ReadOnly = true;
            txtAddress.ReadOnly = true;
            txtCity.ReadOnly = true;
            txtState.ReadOnly = true;
            txtZip.ReadOnly = true;
            btnNew.Enabled = true;
            btnSave.Enabled = false;
            btnCancel.Enabled = false;
            customersManager.CancelCurrentEdit();
            cboCustomers.Enabled = true;
            cboCustomers.SelectedIndex = savedIndex;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            bool allOK = true;
            // make sure there are entries
            if (txtFirstName.Text.Equals(""))
                allOK = false;
            if (txtLastName.Text.Equals(""))
                allOK = false;
            if (txtAddress.Text.Equals(""))
                allOK = false;
            if (txtCity.Text.Equals(""))
                allOK = false;
            if (txtState.Text.Equals(""))
                allOK = false;
            if (txtZip.Text.Equals(""))
                allOK = false;
            if (!allOK)
            {
                MessageBox.Show(
                    "All text boxes require an entry.",
                    "Information Missing", 
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                txtFirstName.Focus();
                return;
            }
            customersManager.EndCurrentEdit();
            // save to database then reopen to retrieve assigned CustomerID
            string savedFirstName = txtFirstName.Text;
            string savedLastName = txtLastName.Text;
            SqlCommandBuilder customersAdapterCommands = new
            SqlCommandBuilder(customersAdapter);
            customersAdapter.Update(customersTable);
            KWSalesConnection.Close();
            // reconnect to sales database
            KWSalesConnection = new
            SqlConnection("Data Source=.\\SQLEXPRESS; " +
            "AttachDbFilename=" + Application.StartupPath + "\\SQLKWSalesDB.mdf;" + 
            "Integrated Security=True; " +
            "Connect Timeout = 30; " +
            "User Instance = True");
            customersCommand = new SqlCommand(
                "SELECT * " +
                "FROM Customers", KWSalesConnection);
            customersAdapter = new SqlDataAdapter();
            customersAdapter.SelectCommand = customersCommand;
            customersTable = new DataTable();
            customersAdapter.Fill(customersTable);
            // rebind controls to data table
            txtFirstName.DataBindings.Clear();
            txtLastName.DataBindings.Clear();
            txtLastName.DataBindings.Clear();
            txtAddress.DataBindings.Clear();
            txtCity.DataBindings.Clear();
            txtState.DataBindings.Clear();
            txtZip.DataBindings.Clear();
            txtFirstName.DataBindings.Add("Text", customersTable, "FirstName");
            txtLastName.DataBindings.Add("Text", customersTable, "LastName");
            txtAddress.DataBindings.Add("Text", customersTable, "Address");
            txtCity.DataBindings.Add("Text", customersTable, "City");
            txtState.DataBindings.Add("Text", customersTable, "State");
            txtZip.DataBindings.Add("Text", customersTable, "Zip");
            customersManager = (CurrencyManager)this.BindingContext[customersTable];
            /// Find added customer
            string ID = "";
            for (int i = 0; i < customersTable.Rows.Count; i++)
            {
                if
                (customersTable.Rows[i]["FirstName"].ToString().Equals(savedFirstName) &&
                customersTable.Rows[i]["LastName"].ToString().Equals(savedLastName))
                {
                    ID =
                    customersTable.Rows[i]["CustomerID"].ToString();
                    break;
                }
            }
            cboCustomers.Enabled = true;
            // refill customers combo box
            FillCustomers();
            // display new customer
            newCustomer = false;
            txtFirstName.ReadOnly = true;
            txtLastName.ReadOnly = true;
            txtAddress.ReadOnly = true;
            txtCity.ReadOnly = true;
            txtState.ReadOnly = true;
            txtZip.ReadOnly = true;
            btnNew.Enabled = true;
            btnSave.Enabled = false;
            btnCancel.Enabled = false;
            cboCustomers.SelectedItem =
            CustomerListing(savedLastName, savedFirstName, ID);
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            float unitPrice = 0.00F;
            if (cboProducts.SelectedIndex == -1)
            {
                MessageBox.Show("You must select a product.",
                "Purchase Error", MessageBoxButtons.OK,
                MessageBoxIcon.Information);
                return;
            }
            // Find unit price of selected product
            for (int nRec = 0; nRec < productsTable.Rows.Count; nRec++)
            {
                if
                (productsTable.Rows[nRec]["Description"].ToString().Equals
                (cboProducts.Text.ToString()))
                {
                    unitPrice =
                    Convert.ToSingle(productsTable.Rows[nRec]["Price"]);
                    break;
                }
            }
            listCart.Items.Add(nudQuantity.Value.ToString() + " " +
            cboProducts.SelectedValue.ToString() + "-" +
            cboProducts.Text.ToString() + " $" +
            unitPrice.ToString());
            // Adjust total price
            lblTotal.Text = string.Format("{0:f2}",
            Convert.ToSingle(lblTotal.Text) +
            Convert.ToSingle(nudQuantity.Value) * unitPrice);
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            int q, i;
            float p;
            if (listCart.SelectedIndex != -1)
            {
                // Adjust total before removing
                // find Q (quantity) and P (price)
                i = listCart.Text.IndexOf(" ");
                q = Convert.ToInt32(listCart.Text.Substring(0, i));
                i = listCart.Text.IndexOf("$");
                p = Convert.ToSingle(listCart.Text.Substring(i + 1, listCart.Text.Length - i - 1));
                lblTotal.Text = String.Format("{0:f2}", Convert.ToSingle(lblTotal.Text) - q * p);
                listCart.Items.RemoveAt(listCart.SelectedIndex);
            }
        }

        private void btnSubmitOrder_Click(object sender, EventArgs e)
        {
            int j, q;
            string ID;
            // Make sure there is customer information
            if (cboCustomers.SelectedIndex == -1)
            {
                MessageBox.Show("You need to select a customer.",
                "Submit Error", MessageBoxButtons.OK,
                MessageBoxIcon.Information);
                return;
            }
            if (listCart.Items.Count == 0)
            {
                MessageBox.Show("You need to select some items.",
                "Submit Error", MessageBoxButtons.OK,
                MessageBoxIcon.Information);
                return;
            }
            // Submit purchases to database
            DataRow newRow;
            newRow = ordersTable.NewRow();
            newRow["OrderID"] = lblOrderID.Text;
            newRow["CustomerID"] = customerID;
            newRow["OrderDate"] = lblDate.Text;
            ordersTable.Rows.Add(newRow);
            for (int i = 0; i < listCart.Items.Count; i++)
            {
                newRow = purchasesTable.NewRow();
                string s = listCart.Items[i].ToString();
                j = s.IndexOf(" ");
                q = Convert.ToInt32(s.Substring(0, j));
                ID = s.Substring(j + 1, 6);
                newRow["OrderID"] = lblOrderID.Text;
                newRow["ProductID"] = ID;
                newRow["Quantity"] = q;
                purchasesTable.Rows.Add(newRow);
                // Update number sold
                // find row with correct productid
                int pr;
                for (pr = 0; pr < productsTable.Rows.Count; pr++)
                {
                    if
                    (productsTable.Rows[pr]["ProductID"].ToString().Equals(ID))
                    {
                        break;
                    }
                }
                productsTable.Rows[pr]["NumberSold"] =
                Convert.ToInt32(productsTable.Rows[pr]["NumberSold"]) + q;
            }
            if (MessageBox.Show("Do you want a printed invoice?",
                "Print Inquiry", 
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes)
            {
                PrintInvoice();
            }
            NewOrder();
        }

        private void frmKWSales_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (newCustomer)
            {
                MessageBox.Show(
                    "You must finish the current edit before stopping.", 
                    "", 
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                e.Cancel = true;
            }
            else
            {
                try
                {
                    // save the tables to the database file
                    SqlCommandBuilder ordersAdapterCommands = new SqlCommandBuilder(ordersAdapter);
                    ordersAdapter.Update(ordersTable);
                    SqlCommandBuilder customersAdapterCommands = new SqlCommandBuilder(customersAdapter);
                    customersAdapter.Update(customersTable);
                    SqlCommandBuilder productsAdapterCommands = new
                        SqlCommandBuilder(productsAdapter);
                    productsAdapter.Update(productsTable);
                    SqlCommandBuilder purchasesAdapterCommands = new
                    SqlCommandBuilder(purchasesAdapter);
                    purchasesAdapter.Update(purchasesTable);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error saving database", "Save Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            if (selectedFile != false)
            {
                KWSalesConnection.Close();
                ordersCommand.Dispose();
                ordersAdapter.Dispose();
                ordersTable.Dispose();
                customersCommand.Dispose();
                customersAdapter.Dispose();
                customersTable.Dispose();
                productsCommand.Dispose();
                productsAdapter.Dispose();
                productsTable.Dispose();
                purchasesCommand.Dispose();
                purchasesAdapter.Dispose();
                purchasesTable.Dispose();
            }
        }

        private void cboCustomers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (newCustomer)
                return;

            string ID;
            int pl;
            string s = cboCustomers.SelectedItem.ToString();
            try
            {
                pl = s.IndexOf("(");
                if (pl == -1)
                    return;
                // extract ID from selected item
                ID = s.Substring(pl + 1, s.Length - pl - 2);
                customersTable.DefaultView.Sort = "CustomerID";
                customersManager.Position = customersTable.DefaultView.Find(ID);
                customerID = Convert.ToInt64(ID);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Could not find customer",
                    "Search Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }
        private void PrintInvoice()
        {
            // Declare the document
            PrintDocument recordDocument;
            // Create the document and name it
            recordDocument = new PrintDocument();
            recordDocument.DocumentName = "KWSales Invoice";
            // Add code handler
            recordDocument.PrintPage += new
            PrintPageEventHandler(this.PrintInvoicePage);
            // Print document
            recordDocument.Print();
            // Dispose of document when done printing
            recordDocument.Dispose();
        }
        private void PrintInvoicePage(object sender, PrintPageEventArgs e)
        {
            int y = 100;
            string s, ti, q, id, desc, unit, t;
            int j;
            Font myFont = new Font("Courier New", 14, FontStyle.Bold);
            // Print Heading
            e.Graphics.DrawString("KIDware Order " +
            lblOrderID.Text, myFont, Brushes.Black, 100, y);
            y += Convert.ToInt32(myFont.GetHeight(e.Graphics));
            e.Graphics.DrawString("Order Date " + lblDate.Text, myFont, Brushes.Black, 100, y);
            y += 2 * Convert.ToInt32(myFont.GetHeight(e.Graphics));
            // Print buyer address
            myFont = new Font("Courier new", 12, FontStyle.Regular);
            e.Graphics.DrawString(txtFirstName.Text + " " +
            txtLastName.Text, myFont, Brushes.Black, 100, y);
            y += Convert.ToInt32(myFont.GetHeight(e.Graphics));
            e.Graphics.DrawString(txtAddress.Text, myFont, Brushes.Black, 100, y);
            y += Convert.ToInt32(myFont.GetHeight(e.Graphics));
            e.Graphics.DrawString(txtCity.Text + ", " +
            txtState.Text + " " + txtZip.Text, myFont, Brushes.Black,
            100, y);
            y += 2 * Convert.ToInt32(myFont.GetHeight(e.Graphics));
            // Print items purchased and totals
            e.Graphics.DrawString("Qty ProductID DescriptionUnitTotal", myFont, Brushes.Black, 100, y);
            y += Convert.ToInt32(myFont.GetHeight(e.Graphics));
            e.Graphics.DrawString("--------------------------------------------------------", myFont, Brushes.Black, 100, y);
            y += Convert.ToInt32(myFont.GetHeight(e.Graphics));
            // Parse the shopping cart listings
            for (int i = 0; i < listCart.Items.Count; i++)
            {
                ti = listCart.Items[i].ToString();
                j = ti.IndexOf(" ");
                q = ti.Substring(0, j);
                id = ti.Substring(j + 1, 6);
                desc = ti.Substring(j + 8, ti.Length - (j + 8));
                j = desc.IndexOf("$");
                unit = desc.Substring(j + 1, desc.Length - (j + 1));
                desc = desc.Substring(0, j - 1);
                if (desc.Length > 25)
                    desc = desc.Substring(0, 25);
                s = BlankLine(56); s = MidLine(q, s, 3 - q.Length);
                s = MidLine(id, s, 7);
                s = MidLine(desc, s, 15);
                s = MidLine(unit, s, 47 - unit.Length);
                t = String.Format("{0:f2}", Convert.ToSingle(q) * Convert.ToSingle(unit));
                s = MidLine(t, s, 56 - t.Length);
                e.Graphics.DrawString(s, myFont, Brushes.Black, 100, y);
                y += Convert.ToInt32(myFont.GetHeight(e.Graphics));
            }
            e.Graphics.DrawString("--------------------------------------------------------", myFont, Brushes.Black, 100, y);
            y += Convert.ToInt32(myFont.GetHeight(e.Graphics));
            s = BlankLine(56);
            s = MidLine("Total", s, 41);
            s = MidLine("$" + lblTotal.Text, s, 55 - lblTotal.Text.Length);
            e.Graphics.DrawString(s, myFont, Brushes.Black, 100, y);
            e.HasMorePages = false;
        }
        public string BlankLine(int n)
        {
            string s = "";
            for (int i = 0; i < n; i++)
            {
                s += " ";
            }
            return (s);
        }
        public string MidLine(string string1, string string2, int p)
        {
            string s = "";
            // convert big string to character array
            char[] sArray = string2.ToCharArray();
            // put string1 in string2
            for (int i = p; i < p + string1.Length; i++)
            {
                sArray[i] = string1[i - p];
            }
            // put array in string variable
            for (int i = 0; i < string2.Length; i++)
            {
                s += sArray[i].ToString();
            }
            return (s);
        }
    }
}
