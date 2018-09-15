using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Data.OleDb;
using System.Data.Common;
using System.Data;
using System.Windows.Forms;
using System.IO;
using System.Data.SqlClient;
using System.Data.Sql;

namespace EuroBakerieAPp.Windows
{
    /// <summary>
    /// Interaction logic for Data_Test.xaml
    /// </summary>
    /// 



    public partial class Data_Test : Window
    {
        public string DB_Path = Properties.Settings.Default.DB_Source;
        public int ID = Properties.Settings.Default.ID;
        public bool isStaff = Properties.Settings.Default.IsStaff;
        
        
        public Data_Test()
        {
            InitializeComponent();
            OleDbConnection connection = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + DB_Path + "");
            try
            {

                if (isStaff == true) { FindStaffName(); } else { FindCustName(); }
              
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Error" + ex);
            }
        }

        private void FindStaffName()
        {
            OleDbConnection connection = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + DB_Path + "");
            string qry_name = "Select Fullname from Staff where StaffID=" + ID;
            OleDbCommand cmd_name = new OleDbCommand(qry_name, connection);
            connection.Open();
            string Name = (string)cmd_name.ExecuteScalar();
            lbl_test.Content = "Welcome, " + Name;
            connection.Close();
        }

        private void FindCustName()
        {
            OleDbConnection connection = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + DB_Path + "");
            string qry_name = "Select Contact from Customers where CustomerID=" + ID;
            OleDbCommand cmd_name = new OleDbCommand(qry_name, connection);
            connection.Open();
            string Name = (string)cmd_name.ExecuteScalar();
            lbl_test.Content = "Welcome, " + Name;
            connection.Close();
        }

    }

   

    public class dataset{

        public string CustomerID { get; set; }
        public string Company { get; set; }

        }
}
