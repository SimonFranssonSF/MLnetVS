using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows;
using Microsoft.Win32;
using System.IO;
using System.Data.OleDb;
using System.Diagnostics;
using System.Collections;

namespace MachineLearning
{
    public partial class Form1 : Form
    {
        private DataTable theData;
        private ID3_1 id3Tree;
        private ArrayList comboboxes = new ArrayList();
        private Label labelPrediction;
        public Form1()
        {
            InitializeComponent();
        }

        /*
         * openButtonClicked opens a filedialog where the user should open xml files of any kind.
         * If the file exists and the status is ok then it also populates the data into the UI components dataGrid, textBoxPathFile, and dropdownSheet.
         * 
         */
        private void openButtonClicked(object sender, EventArgs e)
        {
            // OpenFiledialog non-multuselect filtering xml files
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = "XML Files (*.xml; *.xls; *.xlsx; *.xlsm; *.xlsb) |*.xml; *.xls; *.xlsx; *.xlsm; *.xlsb";
            openDialog.Multiselect = false;
            openDialog.Title = "Open training set data";
            openDialog.InitialDirectory = @"Desktop";

            // If "openDialog" has  a return value then:
                // stores the path in string path,
                // extracts the fileName
            if (openDialog.ShowDialog() == DialogResult.OK) {
                textBoxPathFile.Text = openDialog.FileName;             // Set the text of the inputField "textBoxPathFile"

                // Check the extension and build the corrext dbString depending on it
                FileInfo file = new FileInfo(textBoxPathFile.Text);                     
                if (!file.Exists)
                {
                    throw new Exception(message: "Couldn't find file" + textBoxPathFile.Text);
                }

                // open the connection and populate the combobox "dropdownSheets" with the excel-files all sheets
                OleDbConnection conn = openConnDb();
                dropdownSheets.DataSource = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                dropdownSheets.DisplayMember = "TABLE_NAME";
                dropdownSheets.ValueMember = "TABLE_NAME";

                // Populates the grid "dataGrid" with the selected datasheet in "dropdownSheets"
                this.theData = populateGrid(conn);
                dataGrid.DataSource = this.theData;
                conn.Close();

                this.id3Tree = new ID3_1(this.theData);
                id3Tree.Train();
            }
        }

        /*
         * populateGrid returns a DataTable which can be used to populate dataGrid
         * 
         */
        public DataTable populateGrid(OleDbConnection conn)
        {
            OleDbDataAdapter dbAdapter = new OleDbDataAdapter("Select * From [" + dropdownSheets.SelectedValue + "]", conn);
            DataTable dataTable = new DataTable();
            dbAdapter.Fill(dataTable);
            return dataTable;
        }

        /*
         * openConnDb returns a OleDbConnection db connection/open connection to a data source with properties and filepath of textBoxPathFile
         */
        public OleDbConnection openConnDb() {
            // string used to create a new db connection
            string dbString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + textBoxPathFile.Text + ";Extended Properties='Excel 12.0;HDR=Yes;'";

            // opens the dbconnection and populates the combobox "dropdownSheets" with all the sheets of selected files
            OleDbConnection conn = new OleDbConnection(dbString);
            conn.Open();
            return conn;
        }

        /*
         * dropdownSheets populates dataGrid when selected index of dropdownSheet changed
         */
        private void dropdownSheets_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                OleDbConnection conn = openConnDb();
                this.theData = populateGrid(conn);
                dataGrid.DataSource = this.theData;
                conn.Close();
            }
            catch (Exception err)
            {
                //MessageBox.Show(err.Message);
            }
        }

        /*
         * treeButton_click should read the data and train from it, then display a tree based on the training 
         */
        private void treeButton_Click(object sender, EventArgs e)
        {
            mainView.Visible = false;
            treeView.Visible = true;
            customView.Visible = false;

            treeButton.FlatAppearance.BorderSize = 1;
            treeButton.FlatAppearance.BorderColor = Color.FromArgb(205, 222, 206);

            customInput.TabStop = false;
            customInput.FlatStyle = FlatStyle.Flat;
            customInput.FlatAppearance.BorderSize = 0;

            trainingSetButton.TabStop = false;
            trainingSetButton.FlatStyle = FlatStyle.Flat;
            trainingSetButton.FlatAppearance.BorderSize = 0;

        }

        /*
         * When the training is done you should be able to enter custom input to see what the computer predicts 
         * This function presents the customView, calls the applyCustomLayout function
         */
        private void customInput_Click(object sender, EventArgs e)
        {
            // show correct view
            mainView.Visible = false;
            treeView.Visible = false;
            customView.Visible = true;

            // customize the upper menue buttons accordingly
            customInput.FlatAppearance.BorderSize = 1;
            customInput.FlatAppearance.BorderColor = Color.FromArgb(205, 222, 206);

            treeButton.TabStop = false;
            treeButton.FlatStyle = FlatStyle.Flat;
            treeButton.FlatAppearance.BorderSize = 0;

            trainingSetButton.TabStop = false;
            trainingSetButton.FlatStyle = FlatStyle.Flat;
            trainingSetButton.FlatAppearance.BorderSize = 0;

            // Creates the comboxes, stores the in an array, adds events, creates labels belonging to each combobox
            applyCustomViewLayout();
        }

        /*
         * Queries the tree if all comboboxes has an selected index, presents the answer in a label 
         */
        private void Combo1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // ComboBox c = sender as ComboBox;     // which combobox was changed? (not important but good to remember)
                                                    // Debug.Write(c.Name);

            ArrayList query = new ArrayList();      // To hold the query 

            // gets all values of the comboboxes to the form a query ArrayList
            foreach (ComboBox cb in comboboxes)
            {
                if (cb.SelectedIndex < 0)
                    continue;
                query.Add(cb.SelectedItem);
                // Debug.WriteLine(cb.SelectedItem);
            }

            if (query.Count == comboboxes.Count)
            {
                string prediction = id3Tree.Query(query);
                // Debug.Write(prediction);
                labelPrediction.Text = id3Tree.ToPredict + ": " + prediction;       // Present answer in label
            }
        }

        /*
         * trainingSetButton_Click presents the view where u enter the training-set file
         */
        private void trainingSetButton_Click(object sender, EventArgs e)
        {
            mainView.Visible = true;
            treeView.Visible = false;
            customView.Visible = false;

            trainingSetButton.FlatAppearance.BorderSize = 1;
            trainingSetButton.FlatAppearance.BorderColor = Color.FromArgb(205, 222, 206);

            treeButton.TabStop = false;
            treeButton.FlatStyle = FlatStyle.Flat;
            treeButton.FlatAppearance.BorderSize = 0;

            customInput.TabStop = false;
            customInput.FlatStyle = FlatStyle.Flat;
            customInput.FlatAppearance.BorderSize = 0;
        }

        /*
         * ApplyCumstViewLayout: creates the layout correctly depending of the data added from excel
         * creates, comboboxes, labels, evenhandlers on selectedindexchanged
         */
        private void applyCustomViewLayout()
        {
            string[] options = id3Tree.InputNamesArr;
            for (int i = 0; i < options.Length; i++)
            {
                // Debug.Write(options.Length + "\nf\n");
                ComboBox combo1 = new ComboBox();
                combo1.Location = new Point(200, 30 * i + 80);
                combo1.Name = "comboB" + i;
                combo1.Size = new Size(80, 25);

                DataView view = new DataView(theData);
                DataTable uniqueValues = view.ToTable(true, options[i]);
                foreach (DataRow row in uniqueValues.Rows)
                {
                    object colCell = row[options[i]];
                    combo1.Items.Add(colCell);
                    // Debug.WriteLine(colCell);
                }

                combo1.SelectedIndexChanged += new System.EventHandler(Combo1_SelectedIndexChanged);
                comboboxes.Add(combo1);
                customView.Controls.Add(combo1);

                Label label = new Label();
                label.Text = options[i];
                label.Location = new Point(100, 30 * i + 80);
                customView.Controls.Add(label);
            }

            labelPrediction = new Label();
            labelPrediction.Font = new Font("Arial", 24, FontStyle.Bold);
            labelPrediction.Location = new Point(200, 200);
            labelPrediction.Size = new Size(300, 150);
            customView.Controls.Add(labelPrediction);
        }
    }
}
