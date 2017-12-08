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
        private int numberOfChildren = 0;
        private TreeElement parent = null;
        private Point location = new Point();
        private List<TreeElement> siblings = new List<TreeElement>();
        private string directFamilyString = "";
        private List<TreeElement> children = new List<TreeElement>();


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

        public int LocationX
        {
            get { return this.location.X; }
            set { this.location.X = value; }
        }

        public int LocationY
        {
            get { return this.location.Y; }
            set { this.location.Y = value; }
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
        public List<TreeElement> Children
        {
            get { return this.children;  }
            set { this.children = value; }
        }
    }
}
