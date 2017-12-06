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
using System.Text.RegularExpressions;

namespace MachineLearning
{
    public partial class Form1 : Form
    {
        private DataTable theData;
        private ID3_1 id3Tree;
        private ArrayList comboboxes;
        private Label labelPrediction;

        private ArrayList drawingPoints;

        Panel inner = new Panel();
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
                // MessageBox.Show(err.Message);
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

            string rules = id3Tree.RuleText;
            parseTheRules(rules);
        }

        private void parseTheRules(string rules)
        {

            //string line = rules.Split(new[] { '\r', '\n' }).FirstOrDefault(); // a line from the rules
            //Debug.WriteLine("line: " + line);
            //int count = (line.Split('&').Length - 1)/2 + 1; // number of things to handle
            //Debug.WriteLine(count);
            //rules = RemoveFirstLines(rules, 1); // removes first line when going to read next
            //Debug.WriteLine("rules " + rules);
            //Debug.WriteLine("vene : " + Regex.Match(line, @"\(([^)]*)\)").Groups[1].Value); // first part of string to be handled
            //line = line.Replace(Regex.Match(line, @"\(([^)]*)\)").Groups[0].Value, ""); // removes the part that is handled
            //Debug.WriteLine(line);
            int numLines = rules.Split('\n').Length - 1; // -1?
            Debug.WriteLine("numLines: " + numLines);

            Dictionary<string, int[]> classesLoc = new Dictionary<string, int[]>();
            Dictionary<string, int> classesChildren = new Dictionary<string, int>();
            Dictionary<string, int> classesDirection = new Dictionary<string, int>();

            drawingPoints = new ArrayList();

            for (int i = 0; i < numLines; i++)
            {
                string line = rules.Split(new[] { '\r', '\n' }).FirstOrDefault();
                int count = (line.Split('&').Length - 1) / 2 + 1;
                rules = RemoveFirstLines(rules, 1);
                string parent = "";
                string root = "";
                for (int j = 0; j < count; j++)
                {
                    string toHandle = Regex.Match(line, @"\(([^)]*)\)").Groups[1].Value;
                    line = line.Replace(Regex.Match(line, @"\(([^)]*)\)").Groups[0].Value, "");
                    string[] toHandleFixed = toHandle.Split(new string[] { "==" }, StringSplitOptions.None);
                    int isMainClass = 1;

                    foreach (string toHandleS in toHandleFixed)
                    {
                        isMainClass += 1;
                        string final = toHandleS.Trim();
                        if(final == root)
                        {
                            // lägg till yes/no aka det som kommer först i strängen på varje rad här, location fås av classesLoc[final] sen adda i vanlig  ordning som i else if nedan.
                            parent = root;
                        }
                        // classesLoc.Add(final, [X, Y]);
                        if (!classesLoc.ContainsKey(final) && parent == "")
                        {
                            Label la = new Label();
                            la.Size = new Size(50, 30);
                            la.Location = new Point(300, 20);
                            labelPrediction.Font = new Font("Arial", 8, FontStyle.Bold);
                            la.Text = final;
                            la.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
                            treeView.Controls.Add(la);
                            int[] loc = { 300, 20 };
                            classesLoc.Add(final, loc);
                            classesChildren.Add(final, 0);
                            parent = final;
                            root = final;
                            classesDirection.Add(final, 0);
                        } else if (!classesLoc.ContainsKey(final) && parent != "")
                        {
                            Label la = new Label();
                            la.Size = new Size(50, 30);
                            int width;
                            if (classesChildren.ContainsKey(parent))
                            {
                                classesChildren[parent] += 1;
                                width = classesChildren[parent];
                            }
                            else
                            {
                                width = 1;
                                classesChildren.Add(parent, 1);
                            }

                            int addedWidth;
                            if (isMainClass % 2 == 0)
                            {
                                addedWidth = 50;
                            } else
                            {
                                addedWidth = 50;
                            }

                            int[] loc = new int[2];
                            if (width % 2 != classesDirection[parent])
                            {
                                if (classesDirection[parent] == 1)
                                    width -= 1;
                                la.Location = new Point(classesLoc[parent][0] + addedWidth * (width), classesLoc[parent][1] + 80); // 80*numberofChilds ska det vara här vänster
                                loc[0] = classesLoc[parent][0] + addedWidth * (width);                                              // 80*numberofChilds ska det vara här vänster
                                loc[1] = classesLoc[parent][1] + 80;
                                classesDirection.Add(final, 0);
                            }
                            else
                            {
                                if (classesDirection[parent] == 0)
                                    width -= 1;
                                la.Location = new Point(classesLoc[parent][0] - addedWidth * (width), classesLoc[parent][1] + 80); // 80*numberofChilds ska det vara här vänster
                                loc[0] = classesLoc[parent][0] - addedWidth * (width);                                                // 80*numberofChilds ska det vara här vänster
                                loc[1] = classesLoc[parent][1] + 80;
                                classesDirection.Add(final, 1);
                            }

                            labelPrediction.Font = new Font("Arial", 8, FontStyle.Bold);
                            la.Text = final;
                            la.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
                            treeView.Controls.Add(la);

                            int[] drawPoint = { loc[0]+25, loc[1], classesLoc[parent][0]+25, classesLoc[parent][1] +30};
                            drawingPoints.Add(drawPoint);

                            treeView.Refresh();
                            classesLoc.Add(final, loc);    
                            parent = final;
                        } else if (classesLoc.ContainsKey(final)) {
                            parent = final;
                        }
                    }
                    
                } 
            }
            
        }

        public static string RemoveFirstLines(string text, int linesCount)
        {
            var lines = Regex.Split(text, "\r\n|\r|\n").Skip(linesCount);
            return string.Join(Environment.NewLine, lines.ToArray());
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

            Label labelWhat = new Label();
            labelWhat.Font = new Font("Arial", 12, FontStyle.Bold);
            labelWhat.Text = "What to predict: ";
            labelWhat.Location = new Point(60, 20);
            labelWhat.Size = new Size(130, 50);
            customView.Controls.Add(labelWhat);

            // Combobox that says what column to predicts based on the data
            ComboBox combo = new ComboBox();
            combo.Location = new Point(200, 20);
            combo.Name = "combo";
            combo.Size = new Size(80, 25);
            foreach (DataColumn column in theData.Columns)
            {
                // Debug.WriteLine(column.ColumnName);
                combo.Items.Add(column.ColumnName);
            }
            combo.SelectedIndexChanged += new System.EventHandler(Combo_SelectedIndexChanged);
            customView.Controls.Add(combo);
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
            inner.Size = new Size(500, 500);
            string[] options = id3Tree.InputNamesArr;
            comboboxes = new ArrayList();             // resets comboboxes if new predicition topic chosen
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
                    if (colCell.ToString() != "")
                    {
                        combo1.Items.Add(colCell);
                    }
                    // Debug.WriteLine(colCell);
                }

                combo1.SelectedIndexChanged += new System.EventHandler(Combo1_SelectedIndexChanged);
                comboboxes.Add(combo1);
                inner.Controls.Add(combo1);

                Label label = new Label();
                label.Text = options[i];
                label.Location = new Point(100, 30 * i + 80);
                inner.Controls.Add(label);
            }

            labelPrediction = new Label();
            labelPrediction.Font = new Font("Arial", 24, FontStyle.Bold);
            labelPrediction.Location = new Point(200, 200);
            labelPrediction.Size = new Size(300, 150);
            inner.Controls.Add(labelPrediction);
            customView.Controls.Add(inner);
        }

        /*
         *  creates the tree and applies the rest of the customlayoutview. 
         *  The values in the option comboboxes of the custom view is generated to 
         *  match whatever is select to be predicted
         */
        private void Combo_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            // Debug.WriteLine(cb.SelectedItem);
            // Debug.WriteLine(cb.SelectedIndex);
            this.id3Tree = new ID3_1(this.theData);
            id3Tree.Train(cb.SelectedIndex);
            if (inner != null)
            {
                inner.Controls.Clear();
            }
            applyCustomViewLayout();
        }

        private void paint_tree(object sender, PaintEventArgs e)
        {
            Pen pen = new Pen(Color.FromArgb(255, 0, 0, 0));
            foreach (int[] drawingPoint in drawingPoints)
                e.Graphics.DrawLine(pen, drawingPoint[0], drawingPoint[1], drawingPoint[2], drawingPoint[3]);
        }
    }
}
