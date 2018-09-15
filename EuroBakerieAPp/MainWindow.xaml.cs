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
using EuroBakerieAPp.Windows;

namespace EuroBakerieAPp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>


    public partial class MainWindow : Window
    {
        
        string DB_Path = Properties.Settings.Default.DB_Source;
        OleDbConnection connection = null;
        public OpenFileDialog ofd;
        public FolderBrowserDialog fbd;
        string SelTable;
        string SelColm;
        string SelID;
        int id;

        public MainWindow()
        {
            InitializeComponent();
            Properties.Settings.Default.Reload(); //reloads the Default properties, making sure they're up to date and viable
            InitDB(); //calls the initDB functon which checks to see if the System User Property where the bool for the database is selected 
            InitCredentials();
            connection = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + DB_Path + "");

        }

        private void OpenConnection()
        {
            try { connection.Open(); } catch { connection.Close(); connection.Open(); }
        }

            private void InitCredentials()
        {
            tb_Username.Text = Properties.Settings.Default.Username;
            pb_Password.Password = Properties.Settings.Default.Password;
            cb_remember.IsChecked = Properties.Settings.Default.RememberMe;
        }

        private void InitDB()
        {
            if (Properties.Settings.Default.Database_Selected == true)
            {
                lbl_dbPATH.Foreground = new SolidColorBrush(Colors.ForestGreen);
                lbl_dbPATH.Content = Properties.Settings.Default.DB_Source;
            }
            else
            {
                lbl_dbPATH.Foreground = new SolidColorBrush(Colors.DarkRed);
                lbl_dbPATH.Content = "No Database Selected";
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(DB_Path == "")
            {
                System.Windows.MessageBox.Show("Choose Your EuroBakerie Database", "Error");
            }
            else
            {
                DB_Login("Staff", "Fullname");  DB_Login("Customers", "Contact"); tb_result.Text = "Username or Password Incorrect"; tb_result.Foreground = new SolidColorBrush(Colors.DarkRed);
                //this works by sequentially executing the DB_Login function passing the appropriate parameters. Since the function exits this form and loads a new one on a correct match it negates the need for any complex try statements.
                //If no result is yeilded by the two calls for the login function, it displays error text. This happens because it exits if theres a match, thus if there isn't one it continues on and displays the text.
                //I originally had the Staff Login checker call the Customer checker on fail, but that required a duplicated set of code and I didn't like that.
            }
            
        }

        private void RememberMe()
        {
            Properties.Settings.Default.Username = tb_Username.Text;
            Properties.Settings.Default.Password = pb_Password.Password;
            Properties.Settings.Default.RememberMe = true;
        }

        private void RemoveRememberMe()
        {
            Properties.Settings.Default.Username = null;
            Properties.Settings.Default.Password = null;
            Properties.Settings.Default.RememberMe = false;
        }

        private void DB_Login(string type, string criteria)
        {
            try
            {
                string cmdText = "select count(*) from " + type + " where " + criteria + "=" + "? and Password=?";
                OleDbCommand cmd = new OleDbCommand(cmdText, connection);
                {
                    OpenConnection();
                    cmd.Parameters.AddWithValue("@p1", tb_Username.Text);
                    cmd.Parameters.AddWithValue("@p2", pb_Password.Password);
                    int result = (int)cmd.ExecuteScalar();
                    if (result > 0)
                    {
                        if (cb_remember.IsChecked == true) { RememberMe(); }
                        else
                        {
                            RemoveRememberMe();
                        }
                        Properties.Settings.Default.DB_Source = DB_Path;
                        //Assigns the global properties of DB_Source for reference in other forms 
                        Properties.Settings.Default.Database_Selected = true;
                        //used so that when the app opens again it knows to fill in the DB_Source 
                        //Properties.Settings.Default.IsStaff = true; now redundant

                        Properties.Settings.Default.Save();
                        //Saves the user settings in properties.  

                        if (type == "Staff")
                        {
                            qryCustID("StaffID", "Staff", "Fullname");
                            Staff_Form frm_new = new Staff_Form(); frm_new.Show();
                        }
                        //if they are staff, it runs the query to get their ID with the correct referential criteria 
                        else if (type == "Customers")
                        {
                            qryCustID("CustomerID", "Customers","Contact"); Customer_Form frm_new = new Customer_Form(); frm_new.Show();
                        }
                        //if they are staff, it runs the query to get their ID with the correct referential
                        this.Close();
                    }
                    else return;
                }
                connection.Close();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("An Error Occured, please try again later", "Oops");
                createlogfile(ex.ToString());
            }
        }



        private void createlogfile(string logtext)
        {
            string filePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string now = DateTime.Now.ToString();
            string now1 = now.Replace(" ", String.Empty); string now2 = now1.Replace(":", String.Empty); string now3 = now2.Replace("/", String.Empty);
            filePath = (filePath + @"\EuroBakerie\LOGINCrashlog" + now3 + ".txt");
            string path = Convert.ToString(filePath);
            using (System.IO.StreamWriter file =
        new System.IO.StreamWriter(filePath, false))
            {
                file.WriteLine(logtext);
            }

        }

        private void btn_ChooseDBF_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog(); 
            //had to use Win32 option so it can be assigned a Nullable Bool, thus allowing for it to check completion... thanks VS...
            ofd.Filter = "MS Access Databases (*.accdb)|*.accdb";
            //ofd.Filter = "MS Access Databases (*.accdb)|*.accdb|All files (*.*)|*.*"; Did originally have "All Files" but that might result in "the mentally challenged" people trying to open a photo (╯°□°)╯︵ ┻━┻
            ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            Nullable<bool> result = ofd.ShowDialog();

            if (result == true)
            {
                string sFileName = ofd.FileName;
                DB_Path = sFileName; //Set global Database Path from Open File Dialog
                lbl_dbPATH.Content = DB_Path;
                lbl_dbPATH.Foreground = new SolidColorBrush(Colors.ForestGreen);
            }

        }

        private void qryCustID(string ID, string Table, string Column)
        {
            OpenConnection();
            string qry_findCustID = "select " + ID + " from " + Table + " where " + Column + "='" + tb_Username.Text + "'";
            OleDbCommand cmd_findCustID = new OleDbCommand(qry_findCustID, connection);
            {
                OpenConnection();
                id = (int)cmd_findCustID.ExecuteScalar();
                Properties.Settings.Default.ID = id;
                Console.WriteLine(id);
            }
        }

        private void pb_Password_KD(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Enter))
            {
                Button_Click(this, new RoutedEventArgs());
            }
        }
    }
    }
    

