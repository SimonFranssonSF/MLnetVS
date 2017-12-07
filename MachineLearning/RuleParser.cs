using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Windows;
using Microsoft.Win32;
using System.IO;
using System.Data.OleDb;

using System.Text.RegularExpressions;

namespace MachineLearning
{
    class RuleParser
    {
        
        //private string rules;
        //private int numLines;

        public RuleParser(string rules)
        {
            //this.rules = rules;
            //this.numLines = rules.Split('\n').Length - 1;
        }


        /*
         * Parses this instance's rules and returns them as a dict which can be used to draw them
         * Also prepares another  dict with points to draw lines inbetween the nodes in the decision tree.
         */
        /*public ArrayList ParseTheRules(int height)
        {
            Dictionary<string, int[]> classesLoc = new Dictionary<string, int[]>();
            Dictionary<string, int> classesChildren = new Dictionary<string, int>();
            Dictionary<string, int> classesDirection = new Dictionary<string, int>();
            ArrayList drawingPoints = new ArrayList();
 
            for (int i = 0; i < numLines; i++)
            {
                string line = rules.Split(new[] { '\r', '\n' }).FirstOrDefault();
                string output = line.Substring(0, line.IndexOf(' '));
                line += " && " +  "(" + output + ")";
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
                    // Debug.WriteLine(toHandleFixed.Length);
                    //Debug.WriteLine(toHandleFixed[0]  +" depth: " + j + " or depth: " + j+1);
                    foreach (string toHandleS in toHandleFixed)
                    {
                        isMainClass += 1;
                        string final = toHandleS.Trim();
                        
                        if (final == root)
                        {
                            parent = root;
                        }
                        if (!classesLoc.ContainsKey(final) && parent == "")
                        {
                            int[] loc = { 300, 20 };
                            classesLoc.Add(final, loc);
                            classesChildren.Add(final, 0);
                            parent = final;
                            root = final;
                            classesDirection.Add(final, 0);
                            object[] drawPoint = { loc[0], loc[1], loc[0], loc[1], final };
                            drawingPoints.Add(drawPoint);
                        }
                        else if (!classesLoc.ContainsKey(final) && parent != "" || toHandleFixed.Length == 1)
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
                            if (isMainClass % 2 == 1)
                            {
                                addedWidth = 80;
                            }
                            else
                            {
                                addedWidth = 80;
                            }

                            int[] loc = new int[2];
                            if (toHandleFixed.Length == 1)
                            {
                                la.Location = new Point(classesLoc[parent][0], classesLoc[parent][1] + height);
                                loc[0] = classesLoc[parent][0];
                                loc[1] = classesLoc[parent][1] + height;
                            }
                            else if (width % 2 != classesDirection[parent])
                            {
                                if (classesDirection[parent] == 1)
                                    width -= 1;
                                la.Location = new Point(classesLoc[parent][0] + addedWidth * (width), classesLoc[parent][1] + height); 
                                loc[0] = classesLoc[parent][0] + addedWidth * (width);                                             
                                loc[1] = classesLoc[parent][1] + height;
                                if(toHandleFixed.Length != 1)
                                {
                                    classesDirection.Add(final, 0);
                                }
                            }
                            else
                            {
                                if (classesDirection[parent] == 0)
                                    width -= 1;
                                la.Location = new Point(classesLoc[parent][0] - addedWidth * (width), classesLoc[parent][1] + height); 
                                loc[0] = classesLoc[parent][0] - addedWidth * (width);                                                
                                loc[1] = classesLoc[parent][1] + height;
                                if (toHandleFixed.Length != 1)
                                {
                                    classesDirection.Add(final, 1);
                                }
                            }
                            object[] drawPoint = { loc[0], loc[1], classesLoc[parent][0], classesLoc[parent][1], final };
                            drawingPoints.Add(drawPoint);

                            if (toHandleFixed.Length != 1)
                            {
                                classesLoc.Add(final, loc);
                            }
                            parent = final;
                        }
                        else if (classesLoc.ContainsKey(final))
                        {
                            parent = final;
                        }
                    }

                }
            }

            return drawingPoints;
        }*/

        public List<TreeElement> ParseTheRules2(string rules)
        {
            int numLines = rules.Split('\n').Length - 1;
            List<List<TreeElement>> allElements = new List<List<TreeElement>>();

            for (int i = 0; i < numLines; i++)
            {
                List<TreeElement> lineElements = GetLineElements(i, rules); 
                addPath(lineElements);
                allElements.Add(lineElements);
            }

            List<TreeElement> theTree = makeDistinct(allElements);
            addSiblings(theTree);
            addParent(theTree);
            addChildren(theTree);
            addDepth(theTree);

            /****TODO ***/
            /* Now all data is present except location and direction. figure out how the location  (and direction) will be decided and how I will loop this out
             */
            return null;
        }

        /*
         * Removes a line from thje rules and returns an array with the elements from that line
         * 
         */
        private List<TreeElement> GetLineElements(int lineNum, string rules)
        {
            List<TreeElement> elements = new List<TreeElement>();
            string line = GetLine(rules, lineNum);
            string output = line.Substring(0, line.IndexOf(' '));
            line += " && " + "(" + output + ")";
            int count = (line.Split('&').Length - 1) / 2 + 1;

            for (int j = 0; j < count; j++)
            {
                string toHandle = Regex.Match(line, @"\(([^)]*)\)").Groups[1].Value;
                line = line.Replace(Regex.Match(line, @"\(([^)]*)\)").Groups[0].Value, "");
                string[] toHandleFixed = toHandle.Split(new string[] { "==" }, StringSplitOptions.None);
                foreach (string toHandleS in toHandleFixed)
                {
                    string element = toHandleS.Trim();
                    TreeElement te = new TreeElement(element);
                    elements.Add(te);
                }
            }
            return elements;
        }

        /*
         * Gets a line from a text IN: lineNumber IN: string Text (with multiple rows/rules in this case) 
         */
        private static string GetLine(string text, int lineNum)
        {
            string[] lines = text.Replace("\r", "").Split('\n');
            return lines.Length >= lineNum ? lines[lineNum] : null;
        }

        /*
         * Adds the elements depth/level in the tree
         * 
         */ 
        private void addDepth(List<TreeElement> theTree)
        {
            foreach (TreeElement te in theTree)
            {
                List<string> family = te.DirectFamilyString.TrimEnd(',').Split(',').ToList<string>();
                te.Depth = family.Count;
            }
        }

        /*
         * Adds each elements direct path to root aka direct family as well as parent
         * 
         */
        private void addPath(List<TreeElement> treeElements)
        {
            for (int i = 0; i < treeElements.Count; i++)
            {
                TreeElement t = treeElements[i];
                for (int j = 0; j < i; j++)
                {
                    t.DirectFamilyString += treeElements[j].Name+",";
                }
            }
        }

        /*
         * Fixes sibling references to the treeElements
         *
         */
        private void addSiblings(List<TreeElement> theTree)
        {
            for (int i = 0; i < theTree.Count; i++)
            {
                TreeElement te = theTree[i];
                for (int j = 0; j < theTree.Count; j++)
                {
                    TreeElement teSearched = theTree[j];
                        
                    if (te.DirectFamilyString == teSearched.DirectFamilyString && te.Name != teSearched.Name)
                    {
                        te.Siblings.Add(teSearched);
                    }
                }
            }
        }

        /*
         * Adds a treeElement reference to the treeElements parent  
         */
        private void addParent(List<TreeElement> theTree)
        {
            foreach(TreeElement te in theTree)
            {
                List<string> family = te.DirectFamilyString.TrimEnd(',').Split(',').ToList<string>();
                if (family != null)
                {
                    string parent = family[family.Count - 1];
                    foreach(TreeElement teSearched in theTree)
                    {
                        if(teSearched.Name == parent)
                        {
                            te.Parent = teSearched;
                        }
                    }
                }
            }
        }

        /**
         * Adds number of children each parent have. 
         * 
         */
        public void addChildren(List<TreeElement> theTree)
        {
            foreach (TreeElement te in theTree)
            {
                if (te.Siblings != null && te.Parent != null && te.Parent.NumberOfChildren != null)
                {
                    te.Parent.NumberOfChildren = te.Siblings.Count + 1;
                }

            }  
        }

        /*
         *
         * Removes treeElements with the same path and name, i.e redundant data
         * 
         */
        public List<TreeElement> makeDistinct(List<List<TreeElement>> allElements)
        {
            List<TreeElement> theTree = new List<TreeElement>();
            for (int i = 0; i < allElements.Count; i++)
            {
                List<TreeElement> line = allElements[i];
                for (int j = 0; j < line.Count; j++)
                {
                    TreeElement te = line[j];
                    bool found = false;
                    foreach (TreeElement teSearched in theTree)
                    {
                        if (te.DirectFamilyString == teSearched.DirectFamilyString && te.Name == teSearched.Name)
                        {
                            found = true;
                        }
                    }

                    if (!found)
                    {
                        theTree.Add(te);
                    }
                }
            }
            return theTree;
        }
    }   
}
