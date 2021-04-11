using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KWSalesOrderFormProject
{
    public partial class frmKWSales : Form
    {
        int orderNumber;
        SqlConnection KWSalesConnection;
        SqlCommand ordersCommand;
        SqlCommandBuilder ordersCommandBuilder;
        SqlDataAdapter ordersAdapter;
        DataTable ordersTable;
        public frmKWSales()
        {
            InitializeComponent();
        }
        

        private void frmKWSales_Load(object sender, EventArgs e)
        {
            // connect to sales database
            KWSalesConnection = new SqlConnection("Data Source =.\\SQLEXPRESS; " +
                "AttachDbFilename = " + Application.StartupPath + "\\SQLKWSalesDB.mdf" +
                "Integrated Security = True; " +
                "Connect Timeout = 30; " +
                "User Instance = True");
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
            orderNumber = 0;
            NewOrder();
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
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
