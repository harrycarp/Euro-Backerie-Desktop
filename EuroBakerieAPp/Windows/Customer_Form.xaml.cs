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
using Microsoft.VisualBasic;
using System.Text.RegularExpressions;

namespace EuroBakerieAPp.Windows
{
    /// <summary>
    /// Interaction logic for Customer_Form.xaml
    /// </summary>
    /// 

    

    public partial class Customer_Form : Window
    {
        public string DB_Path = Properties.Settings.Default.DB_Source;
        public int CustomerID = Properties.Settings.Default.ID;
        public OleDbConnection connection;
        public SqlConnection sqlConnection;
        public string CustomerName;
        public string Company;
        public string datetoday;
        public int LatestOrderlineID;
        public bool UDRES;

        public Customer_Form()
        {
            InitializeComponent();
            InitializeLayout();
            InitializeVariables();
            FindCustName();
            FindCustDetails();
            GetCustomerOrders(false);
            fillCBX();
        }

        private void InitializeVariables() {
            connection = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + DB_Path + "");
        }

        private void InitializeLayout()
        {
            grid_pwreset.Visibility = Visibility.Hidden;
            grid_UpdateOrders.Visibility = Visibility.Hidden;
            btn_RemoveOrder.Visibility = Visibility.Hidden;
        }

        private void OpenConnection()
        {
            try { connection.Open();  } catch { connection.Close(); connection.Open(); }
        }

        private void createlogfile(string logtext)
        {
            string filePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string now = DateTime.Now.ToString();
            string now1 = now.Replace(" ", String.Empty); string now2 = now1.Replace(":", String.Empty); string now3 = now2.Replace("/", String.Empty);
            filePath = (filePath + @"\EuroBakerie\CUSTOMERSCrashlog" + now3 + ".txt");
            string path = Convert.ToString(filePath);
            using (System.IO.StreamWriter file =
        new System.IO.StreamWriter(filePath, false))
            {
                file.WriteLine(logtext);
            }

        }

        //
        //Ability for Customers to edit their own details --> located in CustDetails TAB.
        //

        private void FindCustName()
        {
            string qry_name = "Select Contact from Customers where CustomerID=" + CustomerID;
            OleDbCommand cmd_name = new OleDbCommand(qry_name, connection);
            OpenConnection();
            string Name = (string)cmd_name.ExecuteScalar();
            lbl_welcome.Content = "Welcome, " + Name;
            lbl_Welcome.Content = "Welcome, " + Name;
            connection.Close();
            CustomerName = Name;
        }

        private void FindCustDetails()
        {

            try
            {
                //create Query strings
                string qry_findCompany = "Select Company from Customers where CustomerID=" + CustomerID;
                string qry_findaddress = "Select Address from Customers where CustomerID=" + CustomerID;
                string qry_findtown = "Select Town from Customers where CustomerID=" + CustomerID;
                string qry_findpcd = "Select Postcode From Customers Where CustomerID=" + CustomerID;
                string qry_phonenum = "Select Telephone from Customers where CustomerID=" + CustomerID;

                //Construct OleDbCommands
                OleDbCommand cmd_findCompany = new OleDbCommand(qry_findCompany, connection);
                //OleDbCommand cmd_findCompany = new OleDbCommand(qry_findCompany, connection);
                OleDbCommand cmd_findaddress = new OleDbCommand(qry_findaddress, connection);
                OleDbCommand cmd_findtown = new OleDbCommand(qry_findtown, connection);
                OleDbCommand cmd_findpcd = new OleDbCommand(qry_findpcd, connection);
                OleDbCommand cmd_findphonenum = new OleDbCommand(qry_phonenum, connection);

                OpenConnection();

                //Set Textbox Values
                tbx_Company.Text = (string)cmd_findCompany.ExecuteScalar();
                //tbx_email.Text = blah blah blah <<-- do this later
                tbx_address.Text = (string)cmd_findaddress.ExecuteScalar();
                tbx_town.Text = (string)cmd_findtown.ExecuteScalar();
                tbx_postcode.Text = (string)cmd_findpcd.ExecuteScalar();
                tbx_PhoneNum.Text = (string)cmd_findphonenum.ExecuteScalar();
                tbx_Contact.Text = CustomerName;

                Company = (string)cmd_findCompany.ExecuteScalar();
                connection.Close();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("There was an error. Error has been logged in %appdata%", "An Error Occured");
                createlogfile(ex.ToString());
            }
            


        }

        private void btn_SaveCUstDetails_Click(object sender, RoutedEventArgs e)
        {
            try {  writetodb(); } catch { }
        }

        private void writetodb()
        {
            try
            {
                OpenConnection();

                string Sv_Company = tbx_Company.Text;
                string Sv_Contact = tbx_Contact.Text;
                string Sv_Address = tbx_address.Text;
                string Sv_Town = tbx_town.Text;
                string Sv_Postcode = tbx_postcode.Text;
                string Sv_Phonenum = tbx_PhoneNum.Text;

                string qry_Save_Contact = "UPDATE Customers SET Contact='" + Sv_Contact + "' where CustomerID=" + CustomerID;
                string qry_Save_Company = "UPDATE Customers SET Company='" + Sv_Company + "' where CustomerID=" + CustomerID;
                string qry_Save_Address = "UPDATE Customers SET Address='" + Sv_Address + "' where CustomerID=" + CustomerID;
                string qry_Save_Town = "UPDATE Customers SET Town='" + Sv_Town + "' where CustomerID=" + CustomerID;
                string qry_Save_Postcode = "UPDATE Customers SET Postcode='" + Sv_Postcode + "' where CustomerID=" + CustomerID;
                string qry_Save_Phonenum = "UPDATE Customers SET Telephone='" + Sv_Phonenum + "' where CustomerID=" + CustomerID;

                OleDbCommand cmd_Save_Contact = new OleDbCommand(qry_Save_Contact, connection);
                OleDbCommand cmd_Save_Company = new OleDbCommand(qry_Save_Company, connection);
                OleDbCommand cmd_Save_Address = new OleDbCommand(qry_Save_Address, connection);
                OleDbCommand cmd_Save_Town = new OleDbCommand(qry_Save_Address, connection);
                OleDbCommand cmd_Save_Postcode = new OleDbCommand(qry_Save_Postcode, connection);
                OleDbCommand cmd_Save_Phonenum = new OleDbCommand(qry_Save_Phonenum, connection);

                cmd_Save_Contact.ExecuteScalar();
                cmd_Save_Company.ExecuteScalar();
                cmd_Save_Address.ExecuteScalar();
                cmd_Save_Town.ExecuteScalar();
                cmd_Save_Postcode.ExecuteScalar();
                cmd_Save_Postcode.ExecuteScalar();

                connection.Close();

                lbl_UsertabResult.Content = "Successfully updated your details"; lbl_UsertabResult.Foreground = new SolidColorBrush(Colors.Green);
            }
            catch (Exception ex)
            {
                lbl_UsertabResult.Content = "There was an error, please try again"; lbl_UsertabResult.Foreground = new SolidColorBrush(Colors.DarkRed);
                createlogfile(ex.ToString());
            }
        }

        //Everything to do with the orders below here dawg

        private void GetCustomerOrders(bool isReset)
        {
            bool isDateSelected = false;
            bool reset = isReset;

            string date;;
            if (dp_CustOrders.SelectedDate != null){
                isDateSelected = true;
                if (dp_CustOrders.SelectedDate.Value.Month < 10)
                {
                    date = dp_CustOrders.SelectedDate.Value.Day + "/0" + dp_CustOrders.SelectedDate.Value.Month + "/" + dp_CustOrders.SelectedDate.Value.Year;
                }
                else
                {
                    date = dp_CustOrders.SelectedDate.Value.Day + "/" + dp_CustOrders.SelectedDate.Value.Month + "/" + dp_CustOrders.SelectedDate.Value.Year;
                }
            }
            else { isDateSelected = false; date = ""; }

            string qry_fillCOtable;
            if(isDateSelected == true) { qry_fillCOtable = "SELECT Orderlines.OrderlineID, Orders.DeliveryDate, Products.ProductName, Sum(Orderlines.Quantity) AS Quantity FROM Products INNER JOIN(Orders INNER JOIN Orderlines ON Orders.OrderID = Orderlines.OrderID) ON Products.ProductID = Orderlines.ProductID GROUP BY Orderlines.OrderlineID, Orders.DeliveryDate, Products.ProductName, Orders.CustomerID HAVING (((Orders.DeliveryDate)=#" + date + "#) AND ((Orders.CustomerID)=" + CustomerID + "));"; }
            else {
                qry_fillCOtable = "SELECT Orderlines.OrderlineID, Orders.DeliveryDate, Products.ProductName, Sum(Orderlines.Quantity) AS Quantity FROM Products INNER JOIN(Orders INNER JOIN Orderlines ON Orders.OrderID = Orderlines.OrderID) ON Products.ProductID = Orderlines.ProductID GROUP BY Orderlines.OrderlineID, Orders.DeliveryDate, Products.ProductName, Orders.CustomerID HAVING (((Orders.CustomerID)=" + CustomerID + "));";
            }
            if (isReset == true) { qry_fillCOtable = "SELECT Orderlines.OrderlineID, Orders.DeliveryDate, Products.ProductName, Sum(Orderlines.Quantity) AS Quantity FROM Products INNER JOIN(Orders INNER JOIN Orderlines ON Orders.OrderID = Orderlines.OrderID) ON Products.ProductID = Orderlines.ProductID GROUP BY Orderlines.OrderlineID, Orders.DeliveryDate, Products.ProductName, Orders.CustomerID HAVING(((Orders.CustomerID)= " + CustomerID + "));"; }
            //removed Orders.DeliveryDate from qry_filltable because it's redundant since you've already selected the date
            Execute(qry_fillCOtable); //this fills the DataTable with the results spat out from the query
        }

         private void Execute(string query)
        {
            DataTable recipetable = new DataTable();
            OleDbCommand cmd = new OleDbCommand(query, connection);
            OpenConnection();
            cmd.ExecuteNonQuery();
            connection.Close();
            OleDbDataAdapter adpater = new OleDbDataAdapter(cmd);
            adpater.Fill(recipetable);
            dg_CustomerOrders.ItemsSource = recipetable.DefaultView;
        }

        

        private bool CheckUpdate()
        {
            bool result;

            if(tbx_Quantity1.Text == null) { lbl_UOResult.Content = "Please choose an amount"; result = false; }
            else if (Convert.ToInt32(tbx_Quantity1.Text) > 999) { lbl_UOResult.Content = "No orders over 999 lmao"; result = false; }
            else if (cbx_UDProducts.SelectedValue == null) { lbl_UOResult.Content = "Please select a product"; result = false; }
            else { result = true; }

            return result;
        }

        private void dp_CustOrders_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            GetCustomerOrders(false);
            lbl_SelDate.Content = "Delivery Date: " + getDateSelected();
        }

        private void btn_Reset_Click(object sender, RoutedEventArgs e)
        {
            resetCustOrders(true); //sets the parameter to true so that when the resetCustOrders function runs, it will fully reset the table, removing the date criteria. 
        }


        void fillCBX()
        {
            //get amount of products
            OpenConnection();
            string qry_count = "Select COUNT(*) from Products";
            OleDbCommand cmd_CountProducts = new OleDbCommand(qry_count, connection);
            int amt = (int)cmd_CountProducts.ExecuteScalar();
            connection.Close();
            //The list below references a public class which contains the details for the product (i.e. ProductID and ProductName)
            List<Products> products = new List<Products>(); 

            //get the products that you need to add
            string query = "Select ProductName from Products";
            OleDbCommand cmd_ListProducts = new OleDbCommand(query, connection);
            OleDbDataReader dr; //create an DataReader so that multiple values/columns from the DB can be read
            OpenConnection();
            dr = cmd_ListProducts.ExecuteReader();

            while (dr.Read()) //does everything within for each set of items read from the query
            {
                int count = 0; //initialise count variable
                products.Add(new Products() //write the values found from the query to the list class
                {
                    ProdName = dr.GetString(dr.GetOrdinal("ProductName")), 
                    //using GetOrdinal returns the value within the defined column, we're using GetString to convert it to a string
                    ProdID = count
                });
                count++; //increment the count variable
            }

            while (amt > 0) {
                cbx_Products.Items.Add(products[amt-1].ProdName); //this adds the product name to the Add Items CBX referencing the index to the class array
                cbx_UDProducts.Items.Add(products[amt - 1].ProdName); //this adds the product name to the Update Items CBX referencing the index to the class array
                amt--;
            }
            connection.Close();

        }

        public class Products
        {
            public int ProdID;
            public string ProdName;
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var AmountInput = sender as System.Windows.Forms.TextBox;
            e.Handled = Regex.IsMatch(e.Text, "[^0-9]+");
        }

        private void btn_Add_Click(object sender, RoutedEventArgs e) //on Add Order button press -> for adding an order for the logged in user
        {
            if (dp_CustOrders.SelectedDate > DateTime.Today.AddDays(-1)) { CheckValid(); } else
            { lbl_Result.Content = "Can't order in the past" ;lbl_Result.Foreground = new SolidColorBrush(Colors.DarkRed); }
            //this checks to see if the selected date is today or later for adding an order, 
            //realistically you shouldn't be able to add one for Today because that'd be a rush job
            //but oh well nobody every gets what they truley want
        }

        private void CheckValid() { if (cbx_Products.SelectedValue == null) { lbl_Result.Content = "Please Select a Product"; lbl_Result.Foreground = new SolidColorBrush(Colors.DarkRed); ; } else { InsertOrders(); } }

        private void InsertOrders() //inserts orders from form
        {

            string Productname = cbx_Products.SelectedValue.ToString(); //assign product name
            int quantity; //initialise the Quantity variable
            if (tbx_Quantity.Text == "") { quantity = 0; } else { quantity = Convert.ToInt32(tbx_Quantity.Text); }
            //if the quantity textbox isn't filled, the default quantity value is set to 0, we don't want a null return now do we
            if (quantity == 0 || tbx_Quantity.Text == null) { lbl_Result.Content = "Please Input a Quantity"; lbl_Result.Foreground = new SolidColorBrush(Colors.DarkRed); }
            //if there is no quantity, it returns a validity error message
            else if (quantity > 999) { System.Windows.Forms.MessageBox.Show("Please contact support for individual orders over 999"); }
            //this is to make sure you don't try and order too much ya know
            else
            {
                if (getDateSelected() != null)
                {
                    //this query and command selects the Maximum OrderlineID value and then creates a new OrderlineID
                    //that will be added for the inserted product
                    string Qry_GetOrderlineCount = "Select Max(Orderlines.OrderlineID) AS MaxOrderlineID from Orderlines";
                    OleDbCommand cmd_CountOrderline = new OleDbCommand(Qry_GetOrderlineCount, connection);
                    OpenConnection();
                    int orderlinecount = (int)cmd_CountOrderline.ExecuteScalar();
                    LatestOrderlineID = orderlinecount + 1;
                   
                    //this query and command set selects the productid from the product name so it can be
                    //added to the database effectively
                    string Qry_GetProductID = "Select ProductID from Products where ProductName='" + Productname + "'";
                    OleDbCommand cmd_GetPID = new OleDbCommand(Qry_GetProductID, connection);
                    int ProductID = (int)cmd_GetPID.ExecuteScalar();

                    //this gets the max orderID for inserting thr order. Originally I had this as a COUNT(*) query
                    //but then I realised that when you remove an order, COUNT() doesn't match the top orderID, so that wasn't going to work now was it.
                    string Qry_GetNextOrderID = "Select Max(Orderlines.OrderID) AS MaxOrderID from Orderlines"; 
                    OleDbCommand cmd_GetMaxOID = new OleDbCommand(Qry_GetNextOrderID, connection);
                    int NextOrderID = (int)cmd_GetMaxOID.ExecuteScalar() + 1;
                 
                    //this query/command set adds the values for the new order into the Order table, this needs to happen BEFORE adding the Orderlines
                    //values because the Orderlines references the Orders Table
                    string Qry_AddOrderID = "INSERT INTO Orders (OrderID, CustomerID, OrderDate, DeliveryDate) VALUES(" + (NextOrderID) + ", " + CustomerID + ", #" + today() + "#, #" + getDateSelected() + "#);";
                    OleDbCommand cmd_AOID = new OleDbCommand(Qry_AddOrderID, connection);
                    cmd_AOID.ExecuteScalar();

                    //inserts the order into the Orderlines Table
                    string Qry_AddOrder = "INSERT INTO Orderlines (OrderlineID, OrderID, ProductID, Quantity) VALUES("+ LatestOrderlineID + ", " + (NextOrderID) + ", " + ProductID + ", " + quantity + ");" ;
                    OleDbCommand cmd_AddOrder = new OleDbCommand(Qry_AddOrder, connection);
                    cmd_AddOrder.ExecuteScalar();
                    connection.Close();

                    //gives the user a message of successfully adding the order - a bit of confirmation feedback is always nice :)
                    lbl_Result.Content = "Successfully Added Order"; lbl_Result.Foreground = new SolidColorBrush(Colors.ForestGreen);

                    GetCustomerOrders(false); //it then rebuilds the table dependant on the selected date, this is so the user can see the updated version of it
                } else { lbl_Result.Content = "No Date Selected"; lbl_Result.Foreground = new SolidColorBrush(Colors.DarkRed); }
                //if there's no selected date it peaces out 
            }
        }

        private string today() //returns TODAYS date, used for validity checks and OrderDate values
        {
            string datetoday;
            if (DateTime.Today.Month < 10)
            {
                datetoday = DateTime.Today.Day + "/0" + DateTime.Today.Month + "/" + DateTime.Today.Year;
            }
            else
            {
                datetoday = DateTime.Today.Day + "/" + DateTime.Today.Month + "/" + DateTime.Today.Year;
            }
            return datetoday;
        }

        private string getDateSelected()
        {
            string datesel = null;
            if (dp_CustOrders.SelectedDate != null)
            {
                if (dp_CustOrders.SelectedDate.Value.Month < 10)
                {
                    datesel = dp_CustOrders.SelectedDate.Value.Day + "/0" + dp_CustOrders.SelectedDate.Value.Month + "/" + dp_CustOrders.SelectedDate.Value.Year;
                }
                else
                {
                    datesel = dp_CustOrders.SelectedDate.Value.Day + "/" + dp_CustOrders.SelectedDate.Value.Month + "/" + dp_CustOrders.SelectedDate.Value.Year;
                }
            } else { lbl_Result.Content = "Please select a date"; datesel = null; }
            return datesel;
        }

        private void btn_Today_Click(object sender, RoutedEventArgs e)
        {
            dp_CustOrders.SelectedDate = DateTime.Today;
            GetCustomerOrders(false);

            lbl_SelDate.Content = "Delivery Date: Today";
        }



        private void dg_CustomerOrders_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            try
            {
                grid_UpdateOrders.Visibility = Visibility.Hidden;
                DataRowView dataRow = (DataRowView)dg_CustomerOrders.SelectedItem; 
                //gets the row that is currently selected
                string SelectedOrderlineID= dataRow.Row.ItemArray[0].ToString(); 
                //gets the values from the row in reference to the columns, in this case it is the OrderlineID
                string DeliveryDate = dataRow.Row.ItemArray[1].ToString(); 
                //gets the values from the row in reference to the columns, in this case it is the DeliveryDate
                string ProductName = dataRow.Row.ItemArray[2].ToString(); 
                //gets the values from the row in reference to the columns, in this case it is the Product Name
                string Quantity = dataRow.Row.ItemArray[3].ToString(); 
                //gets the values from the row in reference to the columns, in this case it is the Quantity

                if (Convert.ToDateTime(DeliveryDate) < DateTime.Today.AddDays(-1)) { //checks to see if the date of the selected item is less than today
                    lbl_Output.Content = "You can't change past orders!"; lbl_Output.Foreground = new SolidColorBrush(Colors.DarkRed); //spits out an error output
                    grid_UpdateOrders.Visibility = Visibility.Hidden; //hides the Update Order option
                    btn_RemoveOrder.Visibility = Visibility.Hidden; 
                    //hides the option to remove the selected order because it's in the past and you shouldn't be able to remove past orders
                }
                else { grid_UpdateOrders.Visibility = Visibility.Visible; btn_RemoveOrder.Visibility = Visibility.Visible; lbl_Output.Content = ""; }
            {
                fillUpdateTab(SelectedOrderlineID, DeliveryDate, ProductName, Quantity);
            }
            }
            catch (Exception ex) { Console.WriteLine(ex); } //this is so it doesn't crash when you select a date that doesn't have an order - look man it works idk
        }

        private void fillUpdateTab(string OLID, string DelDate, string ProdName, string Quant)
        {
            lbl_OrderlineID.Content = OLID;
            dp_UDOrder.SelectedDate = Convert.ToDateTime(DelDate);
            tbx_Quantity1.Text = Quant;
            cbx_UDProducts.SelectedValue = ProdName;

        }

        private void btn_Save_Click(object sender, RoutedEventArgs e)
        {
            try{
                if (dp_UDOrder.SelectedDate >= DateTime.Today && Convert.ToInt32(tbx_Quantity1.Text) <= 999) //checks to see if the inputted values are valid
                {
                    updateDB(); //calls the function which updates the database
                } else if (Convert.ToInt32(tbx_Quantity1.Text) > 999) { lbl_UOResult.Content = "Please contact sales rep for order over 999"; lbl_UOResult.Foreground = new SolidColorBrush(Colors.DarkRed); }
            }
            catch(Exception ex) //catches the exception
            {
                System.Windows.Forms.MessageBox.Show("There was an error. Error has been logged in %appdata%", "An Error Occured"); //Shows an error message box so the user knows what's up
                createlogfile(ex.ToString()); //writes the exception to the log file (as shown at top of Report Document)
            }
        }

        private void UpdateResult(bool Success)
        {
            if(Success == true)
            {
                lbl_UOResult.Content = "Successfully Updated"; lbl_UOResult.Foreground = new SolidColorBrush(Colors.ForestGreen);
            }
            else
            {
                lbl_UOResult.Content = "Failed to Update"; lbl_UOResult.Foreground = new SolidColorBrush(Colors.DarkRed);
            }
        }

        private void updateDB()
        {
            DataRowView dataRow = (DataRowView)dg_CustomerOrders.SelectedItem; //creates a row reference to the selected values

            string SelectedOrderlineID = dataRow.Row.ItemArray[0].ToString(); //assign the orderlineID to the appropriate column in the table
            DateTime DeliveryDate = Convert.ToDateTime(dataRow.Row.ItemArray[1]); //same for this and below of the same struct
            string ProductName = dataRow.Row.ItemArray[2].ToString();
            string Quantity = dataRow.Row.ItemArray[3].ToString();

            DateTime UDDateTime = dp_UDOrder.SelectedDate.Value; //Assigns the DatePicker to a variable for easy use

            string UDQuantity = tbx_Quantity1.Text; 
            string DelDate = UDDate(UDDateTime); //runs the selected date through a ReturnShortDate function so it can interact with SQL
            string SelProdName = ""; //nullifies the currently selected product name

            int OrdID = 0; //initially sets the OrderID

            SelProdName = cbx_UDProducts.SelectedValue.ToString(); //gets the selected product name.

            OpenConnection();
            string qry_SelectOIDFromOLID = "SELECT OrderID from Orderlines where Orderlines.OrderlineID = " + SelectedOrderlineID;
            //creates the query to find the OrderID
            OleDbCommand SelectOID = new OleDbCommand(qry_SelectOIDFromOLID, connection);
            //creates the appropriate OleDbCommand
            OrdID = (int)SelectOID.ExecuteScalar();
            //Assigns the OrderID variable to the result returned. This should NEVER return a NULL value otherwise something is seriously wrong

            string qry_UpdateDateFromOID = "UPDATE Orders SET DeliveryDate=#" + DelDate + "# where OrderID=" + OrdID + ""; 
            //This query updates the DeliveryDate in reference to the OrderID
            OleDbCommand UpdateDate = new OleDbCommand(qry_UpdateDateFromOID, connection);
            UpdateDate.ExecuteScalar(); //executes the SQL, updating the order

            string qry_UpdateQtyFromOID = "UPDATE Orderlines SET Quantity=" + UDQuantity + " where OrderlineID=" +  SelectedOrderlineID; //update quantity
            OleDbCommand cmd_UpdateQnt = new OleDbCommand(qry_UpdateQtyFromOID, connection);
            cmd_UpdateQnt.ExecuteScalar(); //executes the SQL, updating the order

            string qry_SelectProductID = "SELECT ProductID from Products where ProductName = '" + SelProdName+ "'";
            //this selects the the ProductID based off of the selected product name so that it can be updated in the database
            OleDbCommand cmd_SelectPID = new OleDbCommand(qry_SelectProductID, connection);
            int PID = (int)cmd_SelectPID.ExecuteScalar(); 

            string qry_UpdateProductfromOID = "Update Orderlines SET ProductID=" + PID + " WHERE OrderlineID=" + SelectedOrderlineID; //update ProductID & thus ProdName
            OleDbCommand cmd_UpdateProd = new OleDbCommand(qry_UpdateProductfromOID, connection);
            cmd_UpdateProd.ExecuteScalar(); //executes the SQL, updating the order

            connection.Close();
            resetCustOrders(false); //resets the layout of the form
            UpdateResult(true); //spits out the appropriate output message - didn't really need to put this in a separate function but oh well I did
        }

        private void resetCustOrders(bool ALL)
        {
            if(ALL == true) //executes the below if you are resetting ALL of the layout
            {
                GetCustomerOrders(ALL);
                tbx_Quantity1.Text = "";
                cbx_UDProducts.SelectedValue = null;
                dp_UDOrder.SelectedDate = null;
                lbl_Result.Content = null;
                dp_CustOrders.SelectedDate = null;

                cbx_Products.SelectedValue = null;
                tbx_Quantity.Text = "";
                lbl_SelDate.Content = "";

                grid_UpdateOrders.Visibility = Visibility.Hidden;
            }
            else
                //executes below if your are NOT resetting everything.
                //Not the most efficient method but it works none the less
            {
                GetCustomerOrders(ALL);
                tbx_Quantity1.Text = "";
                cbx_UDProducts.SelectedValue = null;
                dp_UDOrder.SelectedDate = null;
                
                cbx_Products.SelectedValue = null;
                tbx_Quantity.Text = "";
                lbl_SelDate.Content = "";

                grid_UpdateOrders.Visibility = Visibility.Hidden;
            }
        }

        private string UDDate(DateTime UDSelDate)
        {
            string selDay = UDSelDate.Day.ToString(); int IntSelMth = UDSelDate.Month;  string SelYer = UDSelDate.Year.ToString(); string SelMth;
            if(IntSelMth < 10) { SelMth = "0" + IntSelMth; } else { SelMth = IntSelMth.ToString(); }
            string SelDate = selDay + "/" + SelMth + "/" + SelYer;
            return SelDate;
        }

        private void dp_UDOrders_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dp_UDOrder.SelectedDate < DateTime.Today) { lbl_UOResult.Content = "Can't set an order to the past!"; lbl_UOResult.Foreground = new SolidColorBrush(Colors.DarkRed); }
            else { lbl_UOResult.Content = null; }
        }


        private void btnResetPassword_Click(object sender, RoutedEventArgs e)
        {
            grid_pwreset.Visibility = Visibility.Visible;
        }

        private void btn_ConfResetPass_Click(object sender, RoutedEventArgs e)
        {

            if(InitPasswordMatch() == true)
            {
                if(pwd_NewPass1.Password == pwd_NewPassRep.Password) {
                    WriteNewPasswordtoDB();
                }
                else { lbl_PWResult.Content = "Passwords Don't match"; lbl_PWResult.Foreground = new SolidColorBrush(Colors.DarkRed); }
            }
            else { lbl_PWResult.Content = "Password Incorrect"; lbl_PWResult.Foreground = new SolidColorBrush(Colors.DarkRed); }
        }

        private void WriteNewPasswordtoDB()
        {
            OpenConnection();
            try
            {
                string password = pwd_NewPassRep.Password;
                lbl_PWResult.Content = password;
                string qry_UpdatePassword = "UPDATE Customers SET [Password] = '" + password + "' where CustomerID = " + CustomerID;
                OleDbCommand cmd_UpdatePassword = new OleDbCommand(qry_UpdatePassword, connection);
                cmd_UpdatePassword.ExecuteScalar();
                lbl_PWResult.Content = "Succesfully changed password!"; lbl_PWResult.Foreground = new SolidColorBrush(Colors.ForestGreen);
            }
            catch(Exception ex) {
                lbl_PWResult.Content = "Something went wrong! Please try again."; lbl_PWResult.Foreground = new SolidColorBrush(Colors.DarkRed);
                createlogfile(ex.ToString());
            }
            connection.Close();
        }

        bool InitPasswordMatch()
        {
            OpenConnection();
            OleDbConnection connection = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + DB_Path + "");
            string cmdText = "select count(*) from Customers where Password=? AND CustomerID=" + CustomerID;
            OleDbCommand cmd = new OleDbCommand(cmdText, connection);
            {
                OpenConnection();
                bool PasswordMatchresult = false;
                cmd.Parameters.AddWithValue("@p1", pwb_OldPassword.Password);
                connection.Open();
                int result = (int)cmd.ExecuteScalar();
                if (result > 0)
                {
                    PasswordMatchresult = true;
                   
                } else { PasswordMatchresult = false; }

                return PasswordMatchresult;
            }

        }

        private void cbx_Menu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(cbx_Menu.SelectedIndex == 0) { tc_customer.SelectedIndex = 1; } 
            else if(cbx_Menu.SelectedIndex == 1){ logout(); } else { /* they aint doing nothing bruv */ }
            cbx_Menu.SelectedValue = null;
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

        private void btn_MyProfile_Back_Click(object sender, RoutedEventArgs e)
        {
            tc_customer.SelectedIndex = 0;
        }

        private void tbx_Quantity1_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var AmountInput = sender as System.Windows.Forms.TextBox;
            e.Handled = Regex.IsMatch(e.Text, "[^0-9]+");
        }

        

        private void RemoveOrder(int OrderlineID) //runs in reference to the OrderID
        {
            string qry_deleteselected = "DELETE FROM Orderlines WHERE OrderlineID=" + OrderlineID;
            OleDbCommand cmd_deleteSelected = new OleDbCommand(qry_deleteselected, connection);
            OpenConnection();
            cmd_deleteSelected.ExecuteScalar();
        }

        private void btn_RemoveOrder_Click(object sender, RoutedEventArgs e)
        {
            if (dp_CustOrders.SelectedDate > DateTime.Now.AddDays(-1)) 
                //checks to see whether or not the date is valid
            {
                DataRowView dr_selected = (DataRowView)dg_CustomerOrders.SelectedItem;
                try { int OrderlineID = Convert.ToInt32(dr_selected.Row.ItemArray[0].ToString()); 
                RemoveOrder(OrderlineID);
                lbl_Output.Content = "Successfully removed order"; lbl_Output.Foreground = new SolidColorBrush(Colors.ForestGreen);
                GetCustomerOrders(false);
                }
                catch { /* used as a buffer so that the app doesn't crash when nothing is returned */}
            }
            else if (dp_CustOrders.SelectedDate == null) { lbl_Output.Content = "Please Select a date"; lbl_Output.Foreground = new SolidColorBrush(Colors.DarkRed); }
            else lbl_Output.Content = "Can't delete past orders"; lbl_Output.Foreground = new SolidColorBrush(Colors.DarkRed);
        }
    }
}
