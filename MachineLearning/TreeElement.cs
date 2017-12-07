using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineLearning
{
    class TreeElement
    {
        private string name;
        private int depth;
        private int direction;
        private int numberOfChildren;
        private TreeElement parent = null;
        private Point location;
        private List<TreeElement> siblings = new List<TreeElement>();
        private string directFamilyString = "";


        public TreeElement(string name)
        {
            this.name = name;
        }
        public string Name
        {
            get { return this.name; }
        }

        public int Depth
        {
            get { return this.depth; }
            set { this.depth = value; }
        }

        public int Direction
        {
            get { return this.direction; }
            set { this.direction = value; }
        }

        public int NumberOfChildren
        {
            get { return this.numberOfChildren; }
            set { this.numberOfChildren = value; }
        }

        public TreeElement Parent
        {
            get { return this.parent; }
            set { this.parent = value; }
        }

        public Point Location
        {
            get { return this.location; }
            set { this.location = value; }
        }

        public List<TreeElement> Siblings
        {
            get { return this.siblings; }
            set { this.siblings = value; }
        }

        public string DirectFamilyString
        {
            get { return this.directFamilyString; }
            set { this.directFamilyString = value; }
        }
    }
}
