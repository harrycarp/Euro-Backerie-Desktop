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
using System.Printing;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;
using System.Windows.Xps.Serialization;
using System.Text.RegularExpressions;

namespace EuroBakerieAPp.Windows
{
    /// <summary>
    /// Interaction logic for Staff_Form.xaml
    /// </summary>
    /// 



    public partial class Staff_Form : Window
    {
        public string DB_Path = Properties.Settings.Default.DB_Source;
        public int ID = Properties.Settings.Default.ID;
        //public string datepicked;
        public int orderamount;

        //public string date_day;
        //public string date_mth;
        //public string date_yer;

        public string SelectedCell;
        public bool SelectedCellIsInt;
        public string ProductName;
        public int Glb_ProductID;
        public int Glb_BatchQuantity;
        OleDbConnection connection;
        public DateTime now = DateTime.Today;

        public System.Windows.Forms.DataGridViewRowCollection Rows { get; }

        public Staff_Form()
        {
            connection = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + DB_Path + "");
            InitializeComponent();
            StaffName();
            Init();
        }

        //
        //Base Functions
        //

        private void OpenConnection()
        {
            try { connection.Open(); } catch { connection.Close(); OpenConnection(); }
        }

        private void Init() { DP_Main.SelectedDate = now; ConstructTable(); grid_UpdateOrders.Visibility = Visibility.Hidden;  }
        //this function sets the datepickers date to the current day and then builds the table while setting the vis for 
        //the Update Orders pane to hidden.

        private void ConstructTable() { PopulateTable(); BuildCustMaintenance(); InitialiseCustomerOrders(); }
        //PopulateTable() runs the function to populate the Bakelist/Order Grid.
        //BuildCustomerMaintenance() populates the CBX for the customers maintenenace tab
        //InitialiseCustomerOrders builds the Orders form for the selected customers

        private void PopulateTable()
        {
            DataTable table = new DataTable();
            string qry_filltable = "SELECT Orders.CustomerID AS ID, Products.ProductName, Sum(Orderlines.Quantity) AS Quantity FROM Products INNER JOIN (Orders INNER JOIN Orderlines ON Orders.OrderID = Orderlines.OrderID) ON Products.ProductID = Orderlines.ProductID GROUP BY Orders.CustomerID, Products.ProductName, Orderlines.OrderlineID, Orders.DeliveryDate HAVING (((Orders.DeliveryDate)=#" + returnshortdate((DateTime)DP_Main.SelectedDate) + "#));";
            //removed Orders.DeliveryDate from qry_filltable because it's redundant since you've already selected the table
            OleDbCommand cmd = new OleDbCommand(qry_filltable, connection);
            OpenConnection();
            cmd.ExecuteNonQuery();
            OleDbDataAdapter adpater = new OleDbDataAdapter(cmd);
            adpater.Fill(table);
            dgmain.ItemsSource = table.DefaultView;
            connection.Close();
        }

        private void InitialiseCustomerOrders()
        {
            fillCBX();
        }

        private void StaffName() //this function builds the Welcome message for the staff member
        {
                string qry_name = "Select Fullname from Staff where StaffID=" + ID;
                //Selects the name of the Staffmember
                OleDbCommand cmd_name = new OleDbCommand(qry_name, connection);
                OpenConnection();
                string Name = (string)cmd_name.ExecuteScalar();
                lbl_welcome.Content = "Welcome, " + Name;
                //Builds the welcome message
                connection.Close();
        }

        private void ResetBakelist()
        {
            dg_ingred.Visibility = Visibility.Hidden;
            lbl_recipe.Content = "Recipe for: ";
            lbl_method.Text = null;
        }

        private void createlogfile(string logtext)
        {
            string filePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string now = DateTime.Now.ToString();
            string now1 = now.Replace(" ", String.Empty); string now2 = now1.Replace(":", String.Empty); string now3 = now2.Replace("/", String.Empty);
            filePath = (filePath + @"\EuroBakerie\STAFFCrashlog" + now3 + ".txt");
            string path = Convert.ToString(filePath);
            using (System.IO.StreamWriter file =
        new System.IO.StreamWriter(filePath, false))
            {
                file.WriteLine(logtext);
            }

        }


        //*//
        //BELOW HERE IS STUFF TO DO WITH THE MAIN BAKE LIST DATA GRID
        //ALL THESE LIST THE GRID, SHOW THE RECIPE OF THE SELECTED PRODUCT ETC.
        //ENJOY THIS MESS OF SPAGHETTI CODE
        //*//

        private void DP_Main_SelectedDateChanged(object sender, SelectionChangedEventArgs e){ ResetBakelist();  ConstructTable(); }
        //when the date is selected it resets the datagrid in reference to the date and blanks the selected products etc.

        private void Row_DoubleClick(object sender, MouseButtonEventArgs e){ 
            //this is redacted and redundant, use to show the recipe on doubleclick, now just show it on selection changed because easier.
            dgmainshowrec();
        }

        private void showRecipe()
        {
            //this function gets all the details for the recipe etc.
            Recipe_GetName();
            //gets the name of the product
            Recipe_FindProuctID();
            //get's the productID of the selected product for reference
            Recipe_FindMethod();
            //gets the method for the product
            Recipe_GetBatchQuantity();
            //gets the batch quantity
            PopulateRecipeTable();
            //populate the table for the ingredients needed 
        }

        private void dgmainshowrec()
        { 
            dg_ingred.Visibility = Visibility.Visible;
            //sets the ingredient datagrid to visible
            DataRowView dataRow = (DataRowView)dgmain.SelectedItem;
            int index = dgmain.CurrentCell.Column.DisplayIndex;
            string cellValue = dataRow.Row.ItemArray[1].ToString();
            //this grabs the value for the ProductName 
            SelectedCell = cellValue;
            ProductName = SelectedCell; 
            //assigns the Global Variable for product name so it can be referenced in other functions
            showRecipe(); 
            //shows the recipe related stuff
        }

        private void Recipe_GetName()
        {
            lbl_recipe.Content = "Recipe for " + ProductName;
            //since we already have the product name as a global variable
            //we can easily create the label for the recipe name 
        }

        private void Recipe_FindProuctID()
            //this function finds the product ID from the name of the product
            //this is for reference in other functions
        {
            try
            {
                string qry_prdname = "Select ProductID from Products where ProductName='" + ProductName + "'";
                OleDbCommand cmd_prdname = new OleDbCommand(qry_prdname, connection);
                OpenConnection();
                int ProductID = (int)cmd_prdname.ExecuteScalar();
                connection.Close();

                Glb_ProductID = ProductID;

            }
            catch (Exception ex) {
                System.Windows.MessageBox.Show("An Error Occured, and was logged in %appdata%", "Oops");
                createlogfile(ex.ToString());
            }
        }

        private void Recipe_FindMethod()
        {
            string qry_method = "Select Method from Recipes where ProductID=" + Glb_ProductID;
            OleDbCommand cmd_method = new OleDbCommand(qry_method, connection);
            OpenConnection();
            string Method = (string)cmd_method.ExecuteScalar();
            connection.Close();

            lbl_method.Text = Method;
        }
        
        private void Recipe_GetBatchQuantity()
        {
            string qry_method = "Select Count from Recipes where ProductID=" + Glb_ProductID;
            OleDbCommand cmd_method = new OleDbCommand(qry_method, connection);
            OpenConnection();
            int BatchQuantity = (int)cmd_method.ExecuteScalar();
            connection.Close();

            lbl_BatchQ.Content = "Batch Size: " + BatchQuantity;
        }

        private void PopulateRecipeTable()
        {
            DataTable recipetable = new DataTable();
            string qry_filltable = "SELECT Ingredients.Ingredient, [Recipe Ingredients].[Ingredient Quantity] AS Quantity FROM Products INNER JOIN (Recipes INNER JOIN (Ingredients INNER JOIN [Recipe Ingredients] ON Ingredients.IngredientID = [Recipe Ingredients].IngredientID) ON Recipes.ProductID = [Recipe Ingredients].ProductID) ON ([Recipe Ingredients].ProductID = Products.ProductID) AND (Products.ProductID = Recipes.ProductID) WHERE ((([Recipe Ingredients].ProductID)=" + Glb_ProductID +"))";
            //removed Orders.DeliveryDate from qry_filltable because it's redundant since you've already selected the date
            OleDbCommand cmd = new OleDbCommand(qry_filltable, connection);
            OpenConnection();
            cmd.ExecuteNonQuery();
            OleDbDataAdapter adpater = new OleDbDataAdapter(cmd);
            adpater.Fill(recipetable);
            dg_ingred.ItemsSource = recipetable.DefaultView;
            connection.Close();
        }

        private void cbx_StaffForm_SelectionChanged(object sender, SelectionChangedEventArgs e) //there is no option for going to bakelist from the drop down bc reasons only developers know
        {
            if (cbx_StaffForm.SelectedIndex == 0) { tc_Staff.SelectedIndex = 0; } //Bake list
            else if (cbx_StaffForm.SelectedIndex == 1) { tc_Staff.SelectedIndex = 2; } //Customer Maintenance
            //else if (cbx_StaffForm.SelectedIndex == 3) { tc_Staff.SelectedIndex = 3; cbx_StaffForm.SelectedValue = null; } //CANT ACCESS DELIVERY DOCKET FROM MENU DAWG  
            else if(cbx_StaffForm.SelectedIndex == 2) { logout(); } //Logout
        }

        private void logout()
        {
            if (System.Windows.Forms.MessageBox.Show("Are you sure you want to log out?", "Confirm", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                MainWindow LoginWindow = new MainWindow();
                LoginWindow.Show();
                this.Close();
            }
        }

        //Delivery Docket Stuff Below Here dawg

        private void btn_PrintDeliveryDocket_Click(object sender, RoutedEventArgs e)
        {
            resetDocket();
            //initially resets the docket, removing all the information from it so that
            //there is no duplicate printing
            tc_Staff.SelectedIndex = 1;
            //sets the tabindex to the delivery docket so the user is taken to it
            printdeliverydocket();
            //calls the function to print the docket
        }

        public string returnshortdate(DateTime inputDate)
        {
            string day = inputDate.Day.ToString();
            string month = inputDate.Month.ToString();
            string year = inputDate.Year.ToString();

            if(Convert.ToInt32(month) < 10) { month = "0" + month; }

            string shortdate = day + "/" + month + "/" + year;
            return shortdate;
        }

        private void printdeliverydocket()
        {
            OpenConnection();
            //set of variables for function
            string SelDate = returnshortdate((DateTime)DP_Main.SelectedDate);

            
            string qry_custid = "SELECT Orders.CustomerID FROM Customers INNER JOIN (Products INNER JOIN (Orders INNER JOIN Orderlines ON Orders.OrderID = Orderlines.OrderID) ON Products.ProductID = Orderlines.ProductID) ON Customers.CustomerID = Orders.CustomerID GROUP BY Orders.OrderID, Orders.CustomerID, Orders.DeliveryDate HAVING (((Orders.DeliveryDate)=#" + SelDate + "#));";
            OleDbCommand cmd_GetCustomers = new OleDbCommand(qry_custid, connection);
            OleDbDataReader dr1;
            //we are using a reader function because we are going to be returning multiple CustomerID's
            //this will allow us to fill out multiple orders for multiple customers
            dr1 = cmd_GetCustomers.ExecuteReader();
            List<Customers> customerlist = new List<Customers>();
            int counting = 0;
            while (dr1.Read())
            {
                customerlist.Add(new Customers()
                {
                    CustomerID = dr1.GetInt32(dr1.GetOrdinal("CustomerID"))
                    //GetOrdinal allows us to get the value within the column
                    
                });

                PrintDocket(customerlist[counting].CustomerID, SelDate);
                //calls the PrintDocket function in reference to the customerID
                counting++;
                //increments the CustomerID
            }
            counting = 0;
            // count resets once the docket has been printed

            connection.Close();
        }

        public class Customers
        {
            public int CustomerID;
        }

        private void PrintDocketOrders(int CustomerID, string SelectedDate)
        {
            
            string qry_listproducts = "SELECT Products.ProductName, Products.SalePrice, Orderlines.Quantity, [Products].[SalePrice]*[Orderlines].[Quantity] AS Total " +
                "FROM Customers INNER JOIN (Products INNER JOIN (Orders INNER JOIN Orderlines ON Orders.OrderID = Orderlines.OrderID) ON Products.ProductID = " +
                "Orderlines.ProductID) ON Customers.CustomerID = Orders.CustomerID GROUP BY Orders.OrderID, Orders.CustomerID, Orders.CustomerID, Orders.DeliveryDate, " +
                "Products.ProductName, Products.SalePrice, Orderlines.Quantity, [Products].[SalePrice]*[Orderlines].[Quantity] " +
                "HAVING (((Orders.CustomerID)=" + CustomerID + ") AND ((Orders.CustomerID)="+ CustomerID + ") AND ((Orders.DeliveryDate)=#" + SelectedDate + "#));";
            OleDbCommand cmd_GetCustomers = new OleDbCommand(qry_listproducts, connection);
            OleDbDataReader datareader;

            datareader = cmd_GetCustomers.ExecuteReader();

            List<CustomerOrders> orderlist = new List<CustomerOrders>();
            int counting = 0;

            while (datareader.Read())
            {

                orderlist.Add(new CustomerOrders()
                {
                    Quantity = datareader.GetInt32(datareader.GetOrdinal("Quantity")),
                    Price = datareader.GetValue(datareader.GetOrdinal("SalePrice")), 
                    ProdName = datareader.GetValue(datareader.GetOrdinal("ProductName"))
                });

                string PName = "Product: " + orderlist[counting].ProdName + " ";
                //this gets the product listed from the query at the certain customerID stage in the loop
                //it then assigns it to its own subheader
                string quant = null;
                //initialise the string as a null so that there are no value errors
                //
                if (Convert.ToInt32(orderlist[counting].Quantity) < 10) { quant = "Quantity: 0" + orderlist[counting].Quantity + " "; }
                //assigns the quantity to the variable, if the quantity is less than 10, it adds a 0 at the start
                //so that it aligns with the other columns, making it look pretty
                else { quant = "Quantity: " + orderlist[counting].Quantity + " "; }
                string price = "Sale Price: $" + orderlist[counting].Price;
                if(price.Length < 17) { price = price + "0"; }
                //if the price is in the single decimal range, it adds a zero for aesthetic sakes
                decimal TotPrice = Convert.ToDecimal(orderlist[counting].Price) * orderlist[counting].Quantity;
                //calculates the total price by getting the quantity and mulitplying it by the sale amount
                //assigns to a decimal because you know it will most like be xx.yy
                string TotalPrice = null; //initialise the string
                if (TotPrice < 10) { TotalPrice = "Total: $0" + TotPrice; } else { TotalPrice = "Total: $" + TotPrice; }
                //adds a 0 for aesthic if needs be
                if(TotalPrice.Length < 13) { TotalPrice = TotalPrice + "0"; } else if(TotalPrice.Length > 13) { TotalPrice.Substring(TotalPrice.Length - 5); }
                //if the string is too long (i.e. somehow gets triple decimal) it chops of the end for aesthetics
                int PName_Length = PName.Length; int Quant_length = quant.Length; int Price_Length = price.Length; int TotPrice_length = TotalPrice.Length;
                //assigns the length of each string to a variable for padding purposes, this is now redundant because of my stupidity
                //*Story time*//
                //I pulled my hair out for almost an hour because I couldn't get the padding to work, I did a whole bunch of math to
                //make sure all my padding was correct, which it was, I then realised that the width of each character
                //in my chosen font was inconsistent, then I changed it to Consolas and of course it works.

                int totalLength = 40;
                string pad_Pname = PName.PadRight(totalLength);
                string pad_Quant = quant.PadRight(15);
                string pad_Price = price.PadLeft(20);
                string pad_Total = TotalPrice.PadLeft(15);

                string OrderLine = pad_Pname.PadLeft(pad_Pname.Length + 2)  + pad_Quant + pad_Price + pad_Total; 
                //creates the orderline to add
                tb_docket.Inlines.Add(new Run { Text = OrderLine }); tb_docket.Inlines.Add(new LineBreak());
                //adds to orderline
                counting++;
            }
            counting = 0;
        }

            private void PrintDocket(int customerID, string Date)
            {
                //these Inlines.Add add in the customer details ABOVE their products to be 
                tb_docket.Inlines.Add(new Run { Text = "Company: " + GetCustomerOrderDetails(customerID)[0] }); tb_docket.Inlines.Add(new LineBreak());
                tb_docket.Inlines.Add(new Run { Text = "Contact: " + GetCustomerOrderDetails(customerID)[1] }); tb_docket.Inlines.Add(new LineBreak());
                tb_docket.Inlines.Add(new Run { Text = "Address: " + GetCustomerOrderDetails(customerID)[2] + ", " 
                    + GetCustomerOrderDetails(customerID)[3] + ", " + GetCustomerOrderDetails(customerID)[4]}); tb_docket.Inlines.Add(new LineBreak());
                PrintDocketOrders(customerID, Date);
                tb_docket.Inlines.Add(new LineBreak());
                tb_docket.Inlines.Add(new LineBreak());
            }

        private string[] GetCustomerOrderDetails(int CustomerID) //this function returns the details of the customers to be printed on the docket
        {
            string qry_getcompany = "Select Company from Customers where CustomerID = " + CustomerID;
            string qry_getcontact = "Select Contact from Customers where CustomerID = " + CustomerID;
            string qry_getaddress = "Select Address from Customers where CustomerID = " + CustomerID;
            string qry_gettown = "Select Town from Customers where CustomerID = " + CustomerID;
            string qry_getpc = "Select Postcode from Customers where CustomerID = " + CustomerID;
            //construct the queries which return the customer details
            OleDbCommand GetCompany = new OleDbCommand(qry_getcompany, connection);
            OleDbCommand GetContact = new OleDbCommand(qry_getcontact, connection);
            OleDbCommand GetAddress = new OleDbCommand(qry_getaddress, connection);
            OleDbCommand GetTown = new OleDbCommand(qry_gettown, connection);
            OleDbCommand GetPC = new OleDbCommand(qry_getpc, connection);
            //construct the OleDB commands
            string Company = (string)GetCompany.ExecuteScalar();
            string Contact = (string)GetContact.ExecuteScalar();
            string Address = (string)GetAddress.ExecuteScalar();
            string Town = (string)GetTown.ExecuteScalar();
            string Postcode = (string)GetPC.ExecuteScalar();
            //add the values to the array to be returned
            string[] customerdeets = { Company, Contact, Address, Town, Postcode };
            return customerdeets; //returns the array
        }

        public class CustomerOrders
        {
            public object ProdName;
            public int Quantity;
            public object Price;
        }

        private void btn_print_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.PrintDialog pd = new System.Windows.Controls.PrintDialog();
            Nullable<bool> printClicked = pd.ShowDialog();
            //creates the print dialog, if the user hits print it calles the function
            //to physically print the docket
            if(printClicked == true) {
                IRLPrintDocket(pd);
            }
        }

        private void IRLPrintDocket(System.Windows.Controls.PrintDialog pd)
        {
            PrintCapabilities printCapabilities = pd.PrintQueue.GetPrintCapabilities(pd.PrintTicket);
            Size pageAreaSize = new Size(printCapabilities.PageImageableArea.ExtentWidth, printCapabilities.PageImageableArea.ExtentHeight);

            VisualBrush vb = new VisualBrush(tb_docket);
            vb.Stretch = Stretch.Uniform;
            vb.ViewboxUnits = BrushMappingMode.Absolute;
            vb.Viewbox = new Rect(0, 0, tb_docket.ActualWidth, tb_docket.ActualHeight);

            Rectangle rect = new Rectangle();
            rect.Fill = vb;
            rect.Arrange(new Rect(new Point(0, 0), pageAreaSize));

            XpsDocumentWriter writer = PrintQueue.CreateXpsDocumentWriter(pd.PrintQueue);
            writer.Write(rect, pd.PrintTicket);
        }

        private void btn_TodayDocket_Click(object sender, RoutedEventArgs e)
        {
            resetDocket();
            printdeliverydocket();
        }

        private void resetDocket()
        {
            tb_docket.Text = null;
        }

        //CUSTOMER MAINTENACE

        private void BuildCustMaintenance()
        {
            cbx_UserID.Items.Clear();
            //clears the current set of customers so there's no duplicates
            OpenConnection();
            string qry_GetCustomerAmt = "SELECT Contact, Company From Customers";
            OleDbCommand cmd_GetCustAmt = new OleDbCommand(qry_GetCustomerAmt, connection);
            //selects the customers contact name AND company for the CBX
            OleDbDataReader cust_dr = cmd_GetCustAmt.ExecuteReader();
            while (cust_dr.Read())
            {
                cbx_UserID.Items.Add(cust_dr.GetString(cust_dr.GetOrdinal("Company")) + ", " + cust_dr.GetString(cust_dr.GetOrdinal("Contact")));
                //adds the Company and the Contact name to the CBX so the user can see their NAME and COMPANY
            }
            connection.Close();
        }

        private void cbx_UserID_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(cbx_UserID.SelectedValue != null)
            {
                OpenConnection();
                DisplaySelectedUser(cbx_UserID.SelectedIndex + 1);
                connection.Close();
            }
        }

        private void DisplaySelectedUser(int CustomerID)
        {
            //create queries
            string qry_findContact = "Select Contact from Customers where CustomerID=" + CustomerID;
            string qry_findCompany = "Select Company from Customers where CustomerID=" + CustomerID;
            string qry_findaddress = "Select Address from Customers where CustomerID=" + CustomerID;
            string qry_findtown = "Select Town from Customers where CustomerID=" + CustomerID;
            string qry_findpcd = "Select Postcode From Customers Where CustomerID=" + CustomerID;
            string qry_phonenum = "Select Telephone from Customers where CustomerID=" + CustomerID;
            string qry_findBilling = "Select Billing from Customers where CustomerID=" + CustomerID;
            //Construct OleDbCommands
            OleDbCommand cmd_findContact = new OleDbCommand(qry_findContact, connection);
            OleDbCommand cmd_findCompany = new OleDbCommand(qry_findCompany, connection);
            OleDbCommand cmd_findaddress = new OleDbCommand(qry_findaddress, connection);
            OleDbCommand cmd_findtown = new OleDbCommand(qry_findtown, connection);
            OleDbCommand cmd_findpcd = new OleDbCommand(qry_findpcd, connection);
            OleDbCommand cmd_findphonenum = new OleDbCommand(qry_phonenum, connection);
            OleDbCommand cmd_findbilling = new OleDbCommand(qry_findBilling, connection);
            //Set Textbox Values
            tbx_Company.Text = (string)cmd_findCompany.ExecuteScalar();
            tbx_address.Text = (string)cmd_findaddress.ExecuteScalar();
            tbx_town.Text = (string)cmd_findtown.ExecuteScalar();
            tbx_postcode.Text = (string)cmd_findpcd.ExecuteScalar();
            tbx_PhoneNum.Text = (string)cmd_findphonenum.ExecuteScalar();
            tbx_Contact.Text = (string)cmd_findContact.ExecuteScalar();
            cbx_Billing.Text = (string)cmd_findbilling.ExecuteScalar();
        }

        private void UpdateSelectedUser(int CustomerID)
        {
            //This function runs a bunch of SQL Update queries in order to
            //update the customers details given the entered values
            //*
            //construct variables
            string Sv_Company = tbx_Company.Text;
            string Sv_Contact = tbx_Contact.Text;
            string Sv_Address = tbx_address.Text;
            string Sv_Town = tbx_town.Text;
            string Sv_Postcode = tbx_postcode.Text;
            string Sv_Phonenum = tbx_PhoneNum.Text;
            string Sv_Billing = cbx_Billing.Text;
            //construct queries
            string qry_Save_Contact = "UPDATE Customers SET Contact='" + Sv_Contact + "' where CustomerID=" + CustomerID;
            string qry_Save_Company = "UPDATE Customers SET Company='" + Sv_Company + "' where CustomerID=" + CustomerID;
            string qry_Save_Address = "UPDATE Customers SET Address='" + Sv_Address + "' where CustomerID=" + CustomerID;
            string qry_Save_Town = "UPDATE Customers SET Town='" + Sv_Town + "' where CustomerID=" + CustomerID;
            string qry_Save_Postcode = "UPDATE Customers SET Postcode='" + Sv_Postcode + "' where CustomerID=" + CustomerID;
            string qry_Save_Phonenum = "UPDATE Customers SET Telephone='" + Sv_Phonenum + "' where CustomerID=" + CustomerID;
            string qry_Save_Billing = "UPDATE Customers SET Billing='" + Sv_Billing + "' where CustomerID=" + CustomerID;
            //create OleDB commands
            OleDbCommand cmd_Save_Contact = new OleDbCommand(qry_Save_Contact, connection);
            OleDbCommand cmd_Save_Company = new OleDbCommand(qry_Save_Company, connection);
            OleDbCommand cmd_Save_Address = new OleDbCommand(qry_Save_Address, connection);
            OleDbCommand cmd_Save_Town = new OleDbCommand(qry_Save_Address, connection);
            OleDbCommand cmd_Save_Postcode = new OleDbCommand(qry_Save_Postcode, connection);
            OleDbCommand cmd_Save_Phonenum = new OleDbCommand(qry_Save_Phonenum, connection);
            OleDbCommand cmd_Save_Billing = new OleDbCommand(qry_Save_Billing, connection);
            //execute Update 
            cmd_Save_Contact.ExecuteScalar();
            cmd_Save_Company.ExecuteScalar();
            cmd_Save_Address.ExecuteScalar();
            cmd_Save_Town.ExecuteScalar();
            cmd_Save_Postcode.ExecuteScalar();
            cmd_Save_Postcode.ExecuteScalar();
            //System.Windows.Forms.MessageBox.Show(Sv_Billing.ToString());
            cmd_Save_Billing.ExecuteScalar();
        }

        private void btn_UpdateUsers_Click(object sender, RoutedEventArgs e)
        {
            OpenConnection();
            try
            {
                int referenceselect = cbx_UserID.SelectedIndex;
                //creates a reference select so that when the CBX resets it can still be selected
                UpdateSelectedUser(cbx_UserID.SelectedIndex + 1);
                //calls the function to update the users, passing through the USER ID.
                //since the UserID is linear, that won't be changing any time soon hopefully
                BuildCustMaintenance();
                //we then rebuild the customer maintenance, filling in the appropriate details
                //and resetting the combobox
                cbx_UserID.SelectedIndex = referenceselect;
                //sets the selected combobox value/index to the original selection, making sure
                //the user doesn't need to change it back themselves
                lbl_UCResult.Content = "Successfully Updated"; lbl_UCResult.Foreground = new SolidColorBrush(Colors.ForestGreen);
                //gives a nice return message :)
            }
            catch(Exception ex) { lbl_UCResult.Content = "Error Updating User Details"; lbl_UCResult.Foreground = new SolidColorBrush(Colors.DarkRed);
                createlogfile(ex.ToString());
            }
            connection.Close();
        }

        private void btn_AddNewUser_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string qry_getcount = "SELECT Max(Customers.CustomerID) from Customers";
                OpenConnection();
                OleDbCommand cmd_getcount = new OleDbCommand(qry_getcount, connection);
                int count = (int)cmd_getcount.ExecuteScalar();
                InsertUser(count + 1);
                //gets the CustomerID to insert and passes it throught to the set of SQL Inserts
                connection.Close();
                BuildCustMaintenance();
                lbl_UCResult.Content = "Successfully Added New User"; lbl_UCResult.Foreground = new SolidColorBrush(Colors.ForestGreen);
                cbx_UserID.SelectedIndex = count;
            }
            catch(Exception ex) { lbl_UCResult.Content = "Error Adding New User";
                lbl_UCResult.Foreground = new SolidColorBrush(Colors.DarkRed);
                createlogfile(ex.ToString());
            }
        }

        private void InsertUser(int CustomerID)
        {
            string insCompany = tbx_Company.Text;
            string insContact = tbx_Contact.Text;
            string insAddress = tbx_address.Text;
            string insTown = tbx_town.Text;
            string insPostcode = tbx_postcode.Text;
            string insPhonenum = tbx_PhoneNum.Text;

            string qry_insertuser = null;

            //this creates a different query based off whether the data the user has inserted has already been found, if the company or address already exist, 
            //it creates a new blank entry, if they dont it creates a new entry based off of the input data.
            //if the user tries to add a new customer when the Slate one is already active, it won't let them because they haven't changed any details, and it would
            //then potentially allow for mutiple slates to be in play at once and that's no good if you ask me

            if (CheckForExistingCompany(tbx_Company.Text, insAddress) == 1)

            {
                qry_insertuser = "INSERT INTO Customers(CustomerID, Company, Contact, Address, Town, Postcode, Telephone, Billing, [Password]) VALUES(" + CustomerID + ", " + "'Company', 'Contact', 'Address', 'Town', '6000', 'Phone', 'COD', 'Password');";
            }
            else if (CheckForExistingCompany(tbx_Company.Text, insAddress) == 0)
            {
                string company_f = insCompany.IndexOf(" ") > -1 ? insCompany.Substring(0, insCompany.IndexOf(" ")) : insCompany;
                string contact_f = insContact.IndexOf(" ") > -1 ? insContact.Substring(0, insContact.IndexOf(" ")) : insCompany;
                string password = company_f + contact_f;
                qry_insertuser = "INSERT INTO Customers(CustomerID, Company, Contact, Address, Town, Postcode, Telephone, Billing, [Password]) VALUES(" + CustomerID + ", '" + insCompany + "','" + insContact + "','" + insAddress + "','" + insTown + "','" + insPostcode + "','" + insPhonenum + "', 'COD', '" + password + "');";
            }
            else if (CheckForExistingCompany(tbx_Company.Text, insAddress) == 0)
            { lbl_searchresults.Content = "Please finish editing new user"; lbl_searchresults.Foreground = new SolidColorBrush(Colors.DarkRed); }

                OleDbCommand cmd_addnewuser = new OleDbCommand(qry_insertuser, connection);
            cmd_addnewuser.ExecuteScalar();
            //System.Windows.Forms.MessageBox.Show(CustomerID.ToString()); --> was used for Debug
        }

        private int CheckForExistingCompany(string insCompany, string insAddress) 
        //this function checks if there is already a user with that existing company/address. If so returns the appropriate bool
        {
            System.Windows.MessageBox.Show(insCompany);
            string qry_check = "SELECT Count(*) FROM Customers WHERE Company = '" + insCompany + "' OR Address ='" + insAddress +"'";
            OleDbCommand cmd_checkexisting = new OleDbCommand(qry_check, connection);
            OpenConnection();
            int count;
            try { count = (int)cmd_checkexisting.ExecuteScalar(); } catch { count = 0; }
            System.Windows.MessageBox.Show(count.ToString());
            if (count > 0) return 1;
            else if ((tbx_Company.Text == "Company") || (tbx_address.Text == "Address")) return 3;
            else return 0;
        }

        

        private void tbx_postcode_previewtext(object sender, TextCompositionEventArgs e)
        {
            var AmountInput = sender as System.Windows.Forms.TextBox;
            e.Handled = Regex.IsMatch(e.Text, "[^0-9]+");
        }

        private void tbx_phonenum_previewtext(object sender, TextCompositionEventArgs e)
        {
            var AmountInput = sender as System.Windows.Forms.TextBox;
            e.Handled = Regex.IsMatch(e.Text, "[^0-9]+");
        }

        private void btn_find_click(object sender, RoutedEventArgs e)
        {
            findCompany(tbx_find.Text);
        }

        private void findCompany(string FindString)
        {
            string qry_find = "SELECT [Customers].[CustomerID] FROM Customers WHERE Customers.Company LIKE '%" + FindString + "%'";
            //System.Windows.Forms.MessageBox.Show(qry_find); --> was used for debug purposes
            OpenConnection();
            OleDbCommand cmd_find = new OleDbCommand(qry_find, connection);
            try {
                int ReturnCustID = (int)cmd_find.ExecuteScalar();
                DisplaySelectedUser(ReturnCustID);
                cbx_UserID.SelectedIndex = ReturnCustID - 1;
            }  catch { FindUsername(FindString);
                //if no customer is found from the inputted Company, it then checks for a customer
                //from an inputted name
            }
            connection.Close();
        }

        private void FindUsername(string findstring)
        {
            string qry_find = "SELECT [Customers].[CustomerID] FROM Customers WHERE Customers.Contact LIKE '%" + findstring + "%'";
            OpenConnection();
            OleDbCommand cmd_find = new OleDbCommand(qry_find, connection);
            try
            {
                int ReturnCustID = (int)cmd_find.ExecuteScalar();
                DisplaySelectedUser(ReturnCustID);
                cbx_UserID.SelectedIndex = ReturnCustID - 1;
            }
            catch { lbl_searchresults.Content="No Results Found"; lbl_searchresults.Foreground = new SolidColorBrush(Colors.DarkRed);
            /* if no customer is found it returns a message saying that theres no results*/ }
            connection.Close();
        }

        //
        //edit order tab
        //
        private void btn_editorder_click(object sender, RoutedEventArgs e)
        {
            
            DataRowView dgrow = (DataRowView)dgmain.SelectedValue;
            BuildEditOrder();
            try
            {
                int CustomerID = Convert.ToInt32(dgrow.Row.ItemArray[0].ToString());
                //this is the reason why we have the CustomerID visible on the DataGrid
                //we could remove it and NOT have this feature but that's no fun no is it
                string Product = Convert.ToString(dgrow.Row.ItemArray[1].ToString());
                cbx_Customers.SelectedIndex = CustomerID - 1;
                //gets the order of the selected customer
                DisplayOrderToEdit(CustomerID);
                tc_Staff.SelectedIndex = 3;
                lbl_EditOrderReq.Content = null;
            } catch {
                tc_Staff.SelectedIndex=3;
                //if there is no order selected, it still takes you to the Edit Order tab
            }
           
        }

        public void DisplayOrderToEdit(int customerid) //this displays the customers orders within the OrderGrid 
        {
            string Contact = null; //initially sets the Contact as null to allow for assignment
            string Address = null; // "
            string Company = null; // " 
            OpenConnection();
            string qry_sel = "SELECT Contact, Address, Company FROM Customers WHERE CustomerID=" + customerid; //creates a query which returns all the customer details
            OleDbCommand cmd = new OleDbCommand(qry_sel, connection);
            OleDbDataReader dr = cmd.ExecuteReader();
            while (dr.Read()) {
                Contact = dr.GetString(dr.GetOrdinal("Contact"));
                Address = dr.GetString(dr.GetOrdinal("Address"));
                Company = dr.GetString(dr.GetOrdinal("Company"));}
            lbl_custcontact.Content = Contact; lbl_custcompany.Content = Company;
            lbl_custaddress.Content = Address;
            fillOrderGrid(customerid);
        }

        public void fillOrderGrid(int customerid)
        {
            DataTable ordertable = new DataTable();
            string SelDate = returnshortdate((DateTime)dp_custorderdate.SelectedDate); 
            //returns the short date version of the selected date
            string qry_filltable = "SELECT Products.ProductName, Sum(Orderlines.Quantity) AS SumOfQuantity, Products.SalePrice, [Products].[SalePrice]*[Orderlines].[Quantity] AS Total FROM Products INNER JOIN (Orders INNER JOIN Orderlines ON Orders.OrderID = Orderlines.OrderID) ON Products.ProductID = Orderlines.ProductID GROUP BY Orders.DeliveryDate, Orders.CustomerID, Orderlines.OrderlineID, Products.ProductName, Products.SalePrice, [Products].[SalePrice]*[Orderlines].[Quantity] HAVING (((Orders.DeliveryDate)=#" + SelDate + "#) AND ((Orders.CustomerID)=" + customerid + "));";
            //removed Orders.DeliveryDate from qry_filltable because it's redundant since you've already selected the date
            OleDbCommand cmd = new OleDbCommand(qry_filltable, connection);
            OpenConnection();
            cmd.ExecuteNonQuery();
            OleDbDataAdapter adpater = new OleDbDataAdapter(cmd);
            adpater.Fill(ordertable);
            dg_custorders.ItemsSource = ordertable.DefaultView;
            connection.Close();
        }

        public void BuildEditOrder()
        {
            dp_custorderdate.SelectedDate = DP_Main.SelectedDate;
            int i = 0;
            //construct database stuff
            string qry_getcustomers = "SELECT CustomerID, Contact, Address, Company FROM Customers";
            OleDbCommand cmd_getcustomers = new OleDbCommand(qry_getcustomers, connection);
            OpenConnection();
            OleDbDataReader dr_getcust = cmd_getcustomers.ExecuteReader();

            //construct list
            List<Customerlist> cust_list = new List<Customerlist>();

            //Build Class Array from SQL Query
            while (dr_getcust.Read())
            {
                cust_list.Add(new Customerlist()
                {
                    CustomerID = dr_getcust.GetInt32(dr_getcust.GetOrdinal("CustomerID")),
                    Contact = dr_getcust.GetString(dr_getcust.GetOrdinal("Contact")),
                    Address = dr_getcust.GetString(dr_getcust.GetOrdinal("Address")),
                    Company = dr_getcust.GetString(dr_getcust.GetOrdinal("Company"))
                });

                string item = cust_list[i].Contact + ", " + cust_list[i].Company;
                
                cbx_Customers.Items.Add(item);
                i++;
            }
            i = 0;
        }

        public class Customerlist
        {
            public int CustomerID;
            public string Company;
            public string Contact;
            public string Address;
        }


        private void cbx_cust_selectionchanged(object sender, SelectionChangedEventArgs e)
        {
            DisplayOrderToEdit(cbx_Customers.SelectedIndex + 1);
            grid_UpdateOrders.Visibility = Visibility.Hidden;
        }


        private void btn_Add_Click(object sender, RoutedEventArgs e)
        {
            CheckValid();
        }

        private void CheckValid() { if (cbx_Products.SelectedValue == null) { lbl_Result.Content = "Please Select a Product"; lbl_Result.Foreground = new SolidColorBrush(Colors.DarkRed); ; } else { InsertOrders(); } }

        void fillCBX()
        {
            //get amount of products

            //this function is an exact clopy of the one from the Customers Form, it works so why change it
            OpenConnection();
            string qry_count = "Select COUNT(*) from Products";
            OleDbCommand cmd_CountProducts = new OleDbCommand(qry_count, connection);
            int amt = (int)cmd_CountProducts.ExecuteScalar();
            connection.Close();

            List<Products> products = new List<Products>();

            string query = "Select ProductName from Products";
            OleDbCommand cmd_ListProducts = new OleDbCommand(query, connection);
            OleDbDataReader dr;
            OpenConnection();
            dr = cmd_ListProducts.ExecuteReader();

            while (dr.Read())
            {
                int count = 0;
                products.Add(new Products()
                {
                    ProdName = dr.GetString(dr.GetOrdinal("ProductName")),
                    ProdID = count
                });
                count++;
            }

            while (amt > 0)
            {
                cbx_Products.Items.Add(products[amt - 1].ProdName); //yes
                cbx_UDProducts.Items.Add(products[amt - 1].ProdName);
                amt--;
            }
            connection.Close();

        }

        public class Products
        {
            public int ProdID;
            public string ProdName;
        }

        private void InsertOrders() //inserts orders from form
        {
            try
            {
                string selecteddate = returnshortdate((DateTime)dp_custorderdate.SelectedDate);
                string Product = cbx_Products.SelectedValue.ToString();
                string Quantity = tbx_Quantity.Text;
                int customerid = cbx_Customers.SelectedIndex + 1;
                int OrderID = GetOID(selecteddate, customerid);
                //gets the OrderID from an SQL command
                int OrderlineID = GetOLID();
                //gets the orderlineID from an SQL function
                int ProductID = GetProductID(Product);
                //gets the ProductID based off of the product to add
                InsertIntoTable(Product, Quantity, OrderID, customerid, (OrderlineID + 1), ProductID);
                //inserts the new order
                fillOrderGrid(customerid);
                //update sthe order grid
            }
            catch(Exception ex)
            {
                System.Windows.MessageBox.Show("There was an error inserting orders, it has been logged in %appdata%", "Oops");
                createlogfile(ex.ToString());
            }
        }
            
        
        private void InsertIntoTable(string product, string quantity, int OrderID, int CustomerID, int OrderlineID, int productID)
        {
            string qry_insertOLID = "INSERT INTO Orderlines(OrderlineID, OrderID, ProductID, Quantity) VALUES(" + OrderlineID + ", " + OrderID + ", " +  productID + ", " + quantity + ")";
            OleDbCommand cmd_InsertOrderline = new OleDbCommand(qry_insertOLID, connection);
            OpenConnection();
            cmd_InsertOrderline.ExecuteScalar();
        }

        private int GetProductID(string ProductName)
        {
            string qry_findPID = "SELECT ProductID FROM Products WHERE ProductName ='" + ProductName + "'";
            OleDbCommand cmd_findPID = new OleDbCommand(qry_findPID, connection);
            return (int)cmd_findPID.ExecuteScalar();
        }

        private int GetOLID()
        {
            string qry_getOLID = "SELECT Max(Orderlines.OrderlineID) FROM Orderlines";
            OleDbCommand cmd_getOLID = new OleDbCommand(qry_getOLID, connection);
            OpenConnection();
            return (int)cmd_getOLID.ExecuteScalar() /*+5*/; //at one stage it decided to end at OrderlineID value of 4460... so I have to improvise by adding 5 to the value... overflow limit?
            
        }

        private int GetOID(string date, int customerID)
        {
            string qry_GetOID = "SELECT Orderlines.OrderID FROM Products INNER JOIN(Orders INNER JOIN Orderlines ON Orders.OrderID = Orderlines.OrderID) ON Products.ProductID = Orderlines.ProductID GROUP BY Orderlines.OrderID, Orders.DeliveryDate, Orders.CustomerID HAVING(((Orders.DeliveryDate) =#" + date + "#) AND ((Orders.CustomerID)=" + customerID + "))";
            OleDbCommand cmd_getOrderID = new OleDbCommand(qry_GetOID, connection);
            OpenConnection();
            int OrderID = 0;
            try { OrderID = (int)cmd_getOrderID.ExecuteScalar(); }
            catch
            {
                try
                {
                    OleDbCommand GetNewOID = new OleDbCommand("SELECT Max(Orders.OrderID) from Orders", connection);
                    OpenConnection();
                    int OID = (int)GetNewOID.ExecuteScalar();
                    OrderID = OID + 1;
                    InsertNewOrder(OrderID, customerID, date);
                }
                catch(Exception ex)
                {
                    System.Windows.MessageBox.Show("There was an error adding Order", "Oops");
                    createlogfile(ex.ToString());
                }
            }
            return OrderID;
        }

        private void InsertNewOrder(int OrderID, int CustomerID, string DeliveryDate)
        {
            string OrderDate = returnshortdate(DateTime.Today);
            string qry_neworder = "INSERT INTO Orders(OrderID, CustomerID, OrderDate, DeliveryDate) VALUES(" + OrderID + ", " + CustomerID + ", #" + OrderDate + "#, #" + DeliveryDate + "#)";
            OpenConnection();
            OleDbCommand cmd_neworder = new OleDbCommand(qry_neworder, connection);
            cmd_neworder.ExecuteScalar();
        }

        private void tbx_Quantity_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var AmountInput = sender as System.Windows.Forms.TextBox;
            e.Handled = Regex.IsMatch(e.Text, "[^0-9]+");
        }

        private void btn_Updateuserorder(object sender, RoutedEventArgs e)
        {
            string UpdateProduct = cbx_UDProducts.SelectedValue.ToString();
            
            int ProductID = GetProductID(UpdateProduct);
            int UpdateQuantity = Convert.ToInt32(tbx_Quantity1.Text);
            string UpdateDate = returnshortdate((DateTime)dp_UDOrder.SelectedDate);
            string currentdate = returnshortdate((DateTime)dp_custorderdate.SelectedDate);
            int OrderID = GetOID(currentdate, (cbx_Customers.SelectedIndex + 1));
            int OrderlineID = GetOLID();

            UpdateUserOrder(ProductID, UpdateQuantity, UpdateDate, currentdate, OrderID, OrderlineID, (cbx_Customers.SelectedIndex + 1));
            
        }



        private void UpdateUserOrder(int ProductID, int UDQuantity, string UpdateDate, string CurrentDate, int OrderID, int OrderlineID, int CustomerID)
        {
            string qry_updateorder = "UPDATE Orders SET DeliveryDate=#" + UpdateDate + "# WHERE OrderID=" + OrderID;
            string qry_update_product = "UPDATE Orderlines SET ProductID=" + ProductID + " WHERE OrderlineID=" + OrderlineID;
            string qry_update_quantity = "UPDATE Orderlines SET Quantity=" + UDQuantity + " WHERE OrderlineID=" + OrderlineID;
            string qry_update_orderud = "UPDATE Orderlines SET OrderID=" + OrderID + " WHERE OrderlineID=" + OrderlineID;

            OleDbCommand cmd_updateorder = new OleDbCommand(qry_updateorder, connection);
            OleDbCommand cmd_updateproduct = new OleDbCommand(qry_update_product, connection);
            OleDbCommand cmd_updatequantity = new OleDbCommand(qry_update_quantity, connection);
            OleDbCommand cmd_updateOrderid = new OleDbCommand(qry_update_orderud, connection);

            OpenConnection();

            cmd_updateorder.ExecuteScalar(); cmd_updateproduct.ExecuteScalar(); cmd_updatequantity.ExecuteScalar(); cmd_updateOrderid.ExecuteScalar();
            fillOrderGrid(cbx_Customers.SelectedIndex + 1);
        }

       

        private void tbx_Quantity1_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var AmountInput = sender as System.Windows.Forms.TextBox;
            e.Handled = Regex.IsMatch(e.Text, "[^0-9]+");
        }

        private void dp_UDOrder_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
           
        }

        private void dg_custorders_selectionchanged(object sender, SelectionChangedEventArgs e)
        {
            int OrderID = GetOID((returnshortdate((DateTime)dp_custorderdate.SelectedDate)), (cbx_Customers.SelectedIndex+1));
            DataRowView dr_selected = null;
            try
            {
                dr_selected = (DataRowView)dg_custorders.SelectedItem;
            } catch { }
            try
            {
                string ProductName = dr_selected.Row.ItemArray[0].ToString();
                string Quantity = dr_selected.Row.ItemArray[1].ToString();
                DateTime Reference = DateTime.Now.AddDays(-1);
                DateTime SelectedDate = (DateTime)dp_custorderdate.SelectedDate;
                if(SelectedDate > Reference) { FillUpdate(ProductName, Quantity); grid_UpdateOrders.Visibility = Visibility.Visible;  }
                else { grid_UpdateOrders.Visibility = Visibility.Hidden; }  
            }
            catch { }
        }

        private void FillUpdate(string ProductName, string Quantity)
        {
            cbx_UDProducts.SelectedItem = ProductName;
            tbx_Quantity1.Text = Quantity;
            dp_UDOrder.SelectedDate = dp_custorderdate.SelectedDate;
            //fills the update pane with the product/order details
        }

        private void dp_customerorderdate_selchanged(object sender, SelectionChangedEventArgs e)
            //changes what's on the ordergrid when the date changes
        {
            fillOrderGrid(cbx_Customers.SelectedIndex + 1);
        }

        private void btn_removeorder_Click(object sender, RoutedEventArgs e)
        {
            DataRowView dr_selected = (DataRowView)dg_custorders.SelectedItem;
            int OrderID = GetOID((returnshortdate((DateTime)dp_custorderdate.SelectedDate)), (cbx_Customers.SelectedIndex + 1));
            //gets the Orderid
            int Quantity = Convert.ToInt32(dr_selected.Row.ItemArray[1].ToString());
            int ProductID = GetProductID(dr_selected.Row.ItemArray[0].ToString());
            int OrderlineID = SelectedOrderline(OrderID, ProductID, Quantity);
            //gets the OrderlineID based off of the ORderID, product and Quantity
            //this is a potentially dangerous method because theoretically a user COULD have
            //multiple of the same order, HOWEVER it wouldn't matter which got
            //deleted because they are the EXACT SAME!!
            RemoveOrder(OrderlineID);
        }

        private void RemoveOrder(int OrderlineID)
        {
            string qry_deleteselected = "DELETE FROM Orderlines WHERE OrderlineID=" + OrderlineID;
            OleDbCommand cmd_deleteSelected = new OleDbCommand(qry_deleteselected, connection);
            OpenConnection();
            cmd_deleteSelected.ExecuteScalar();
            fillOrderGrid(cbx_Customers.SelectedIndex + 1);
        }

        private int SelectedOrderline(int OrderID, int ProductID, int Quantity)
        {
            string qry_getselolid = "SELECT OrderlineID FROM Orderlines WHERE OrderID=" + OrderID + " AND ProductID=" + ProductID + " AND Quantity=" + Quantity;
            OleDbCommand cmd_getselectedOLID = new OleDbCommand(qry_getselolid, connection);
            int OrderlineID = (int)cmd_getselectedOLID.ExecuteScalar();
            return OrderlineID;
        }

        private void btn_DocketBack_Click(object sender, MouseWheelEventArgs e)
        {
            tc_Staff.SelectedIndex = 0;
        }

        private void tbx_find_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void btn_DocketBack_Click_1(object sender, RoutedEventArgs e)
        {
            tc_Staff.SelectedIndex = 0;
        }

        private void dgmain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //when the selection of the Product is changed, it shown the recipe for the product
            dgmainshowrec();
        }

        private void tbx_Find_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Enter)){
                btn_find_click(this, new RoutedEventArgs());
            }
        }
    }
}

