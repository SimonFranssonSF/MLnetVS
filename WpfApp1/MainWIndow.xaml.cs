using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
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
using System.Windows.Forms;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //OpenFileDialog op = new OpenFileDialog();
            //op.Filter = "Excel Files | *.xlsx; *.xls; *.xlxm";

            //if (op.ShowDialog() == true)
            //{
            //    this.tb_path.Text = op.FileName;
            //}
            //string constr = "Provider=Microsoft.ACE.OLEDB.12.0.Data Source=" + tb_path.Text + ";Extended Properties = \"Excel 12.0; HDR=YES;\" ; ";
            //OleDbConnection db = new OleDbConnection(constr);
            //db.Open();
            //System.Windows.Forms.ComboBox = new;
            //dropDownSheet.DataSource = db.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
            //dropDownSheet.DisplayMember = "TABLE_NAME";
            //dropDownSheet.ValueMember = "TABLE_NAME";
            //lv1.
            //MessageBox.Show("Hello1.");

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("Hello2.");

        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {

            //MessageBox.Show("Hello3.");
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void DataGrid_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
