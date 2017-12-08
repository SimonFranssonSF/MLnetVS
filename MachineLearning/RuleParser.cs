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
        public RuleParser(string rules)
        {

        }

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

            foreach (TreeElement te in theTree)
            {
                if (te.Parent == null || te.Siblings == null || te.Children == null)
                    continue;
                //Debug.WriteLine("Name: " + te.Name + " Parent: " + te.Parent.Name);
                //Debug.WriteLine("Children: ");
                if(te.Name == "Humidity")
                {
                    //Debug.WriteLine("path: " + te.DirectFamilyString);
                    //Debug.WriteLine("Parent: " + te.Parent.Name);
                    //Debug.WriteLine("children: " );
                    foreach (TreeElement a in te.Children)
                    {
                        //Debug.WriteLine(a.Name);
                    }
                }
                foreach(TreeElement t in te.Children)
                {
                    //Debug.WriteLine("Child: " + t.Name);
                }
                //Debug.WriteLine("Siblings: ");
                foreach (TreeElement ta in te.Siblings)
                {
                    //Debug.WriteLine("Siblings: " + ta.Name);
                }
            }

            generateTreeLocations(theTree);
            return theTree;
        }

        public void generateTreeLocations(List<TreeElement> theTree)
        {
            TreeElement root = getRootElement(theTree);
            root.Location = new Point(300, 20);
            root.Direction = 0;
            recur(root, root);
        }

        public void recur(TreeElement te, TreeElement root)
        {
            int nodeWidth = 65;
            int nodeHeight = 40;

            if (te.NumberOfChildren == 0)
            {
                return;
            }

            for (int i = 0; i < te.NumberOfChildren; i++)
            {
                int factor = i+1;
                TreeElement child = te.Children[i];
                //int nbrOfOlderSiblings = olderSibs(child, root);
                if (i % 2 == te.Direction)
                {
                    if (te.Direction == 1)
                        factor -= 1;
                    child.LocationX = child.Parent.Location.X + nodeWidth * factor; //+ nbrOfOlderSiblings * nodeWidth + child.NumberOfChildren / 2 * nodeWidth; // TODO add number of older sibs + all their children
                    child.LocationY = child.Parent.Location.Y + nodeHeight;
                    child.Direction = 0;
                }
                else if(i % 2 != te.Direction)
                {
                    if (te.Direction == 0)
                        factor -= 1;
                    child.LocationX = child.Parent.Location.X - nodeWidth * factor;// - nbrOfOlderSiblings * nodeWidth - child.NumberOfChildren/2 * nodeWidth; // TODO add number of older sibs + all their children
                    child.LocationY = child.Parent.Location.Y + nodeHeight;
                    child.Direction = 1;
                }
                recur(child, root);
            }
        }

        private int olderSibs(TreeElement te, TreeElement root)
        {
            // TODO check older siblings by using location relative to root, if  location of some element is inbetween root and this element loc then its an older sibling
            // check the number  of  children that sibling have, add it to a variable, do this for all older sibs.
            // return the widthAddedFactor
            int olderSibling = 0;
            foreach(TreeElement sibling in te.Siblings)
            {
                if (sibling.LocationX  < root.LocationX && sibling.LocationX > te.LocationX)
                {
                    olderSibling += 1+sibling.Children.Count/2;
                }
                else if (sibling.LocationX > root.LocationX && sibling.LocationX < te.LocationX)
                {
                    olderSibling += 1+sibling.Children.Count/2;
                }
            }
            return olderSibling;
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
                        if(teSearched.Name == parent && te.DirectFamilyString == teSearched.DirectFamilyString + teSearched.Name+",")
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
                if (te.Siblings != null && te.Parent != null)
                {
                    te.Parent.NumberOfChildren = te.Siblings.Count + 1;
                    List<TreeElement> children = new List<TreeElement>(te.Siblings);
                    children.Add(te);
                    te.Parent.Children = children;
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

        public TreeElement getRootElement(List<TreeElement> theTree)
        {
            TreeElement root = null;
            foreach (TreeElement te in theTree)
            {
                if (te.Parent == null)
                {
                    root = te;
                }
            }
            return root;
        }
    }   
}
