﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using System.Diagnostics;
using Accord.MachineLearning.VectorMachines.Learning;
using Accord.Math.Optimization.Losses;
using Accord.Statistics;
using Accord.Statistics.Kernels;
using Accord.MachineLearning.DecisionTrees;
using Accord.Statistics.Filters;
using Accord.Math;
using Accord.MachineLearning.DecisionTrees.Learning;
using System.Collections;
using System.Windows.Forms;
using System.Linq.Expressions;
using Accord.MachineLearning.DecisionTrees.Rules;

namespace MachineLearning
{
    class ID3_1
    {
        private DataTable theData;          // The data from excel
        private DecisionTree tree;          // The trained tree
        private Codification codebook;      // The codebook
        private string[] inputNamesArr;     // The headers of the data
        private string toPredict;           // What to predict
        string ruleText;                    // the rules of the tree represented as a string
        public ID3_1(DataTable theData)
        {
            this.theData = theData;
        }

        // property Datatable the data
        public DataTable TheData
        {
            get{ return this.theData; }
            set{ this.theData = value; }
        }

        // property InputNamesArrm what other classifications than what to predict?
        public string[] InputNamesArr
        {
            get { return inputNamesArr; }
        }

        // property toPredict getter, what to classify
        public string ToPredict
        {
            get { return toPredict; }
        }

        // property ruleText containg the rules of the tree
        public string RuleText
        {
            get { return ruleText; }
        }

        /*
         * Takes a Datatable with the training data
         * translates the data to ints
         * trains using the training data
         * The last col of the datatable input is the thing to predicted
         */
        public void Train(int index)
        {
            DataTable dataTable = this.theData;

            // Debug.Write("DataTable size: ");
            // Debug.Write("Rows: " + dataTable.Rows.Count);
            // Debug.Write("Cols: " + dataTable.Columns.Count);

            ArrayList inputNames = new ArrayList();
            foreach (DataColumn column in dataTable.Columns)
            {
                inputNames.Add(column.ColumnName);
            }
            this.toPredict = (string)inputNames[index];                                         // The column to predict
            inputNames.RemoveAt(index);                                                         // the data input data (predict column removed)
            this.inputNamesArr = (string[])inputNames.ToArray(typeof(string));

            // Debug.Write("Input arr size: " + inputNamesArr.Length);

            // Using Accord.Statistics.Filters to present the data as integers, 
            // as integers are more efficient
            this.codebook = new Codification(dataTable) { DefaultMissingValueReplacement = 0 };  // codebook object that can convert  strings to ints, null/missing value will be defaulted to 0
            DataTable symbols = codebook.Apply(dataTable);                                       // applying our data to the codebook
            int[][] inputs = symbols.ToJagged<int>(inputNamesArr);                               // The conversion to ints
            int[] outputs = symbols.ToArray<int>(toPredict);                                     // The conversion to ints

            // Debug.Write("Array size: ");
            // Debug.Write("inputs: " + inputs.Length);
            // Debug.Write("outputs: " + outputs.Length);

            // Debug.Write("Test");

            var id3 = new ID3Learning()                                                          // the id3 algo
            {                                                          
                Attributes = DecisionVariable.FromCodebook(codebook, inputNamesArr)              // the trees decision attributes/headers from excel, second argument could be given saying what columns it should be               

            };

            this.tree = id3.Learn(inputs, outputs);                                              // Learn using the inputs and output defined above

            // transform the rules of the tree into a string
            DecisionSet treeRules = tree.ToRules();
            ruleText = treeRules.ToString(codebook, toPredict,
                System.Globalization.CultureInfo.InvariantCulture);
            Debug.WriteLine(ruleText);
        }


        /*
         * Query asks the tree to predict based on an input given in an arraylist
         * Returns the answer
         */
        public string Query(ArrayList values) {
            string[] keys = inputNamesArr;

            // Debug.WriteLine(inputNamesArr.Length);
            // Debug.WriteLine(values.Count);

            // Transform the two arrays inputNamesArr and values into a string[,]
            var queryArr = new String[inputNamesArr.Length, 2];
            for (int i = 0; i < values.Count; i++)
            {
                queryArr[i, 0] = keys.ElementAt(i);
                queryArr[i, 1] = (string)values[i];
            }

            int[] query = codebook.Transform(queryArr);                                             // Transforms the query to a query of ints

            int predictedInt = tree.Decide(query);                                                  // Get the answer in ints
            string predictedString = codebook.Revert(toPredict, predictedInt);                      // convert the answer back to a string

            // Debug.Write(predictedString);

            return predictedString;
        }
    }

}
