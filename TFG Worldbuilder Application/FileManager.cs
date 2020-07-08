using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFG_Worldbuilder_Application
{
    /// <summary>
    /// 2D Point object
    /// </summary>
    public class Point2D
    {
        public long X;
        public long Y;

        /// <summary>
        /// creates a Point2D object
        /// </summary>
        public Point2D(long X, long Y)
        {
            this.X = X;
            this.Y = Y;
        }

        /// <summary>
        /// copy constructor
        /// </summary>
        public Point2D(Point2D o)
        {
            X = o.X;
            Y = o.Y;
        }

        /// <summary>
        /// Attempts to convert a string in the format "(X,Y)" to a Point2D object
        /// </summary>
        public static Point2D FromString(string str)
        {
            if (str.Trim().IndexOf('(') == 0 && str.Trim().IndexOf(')') == str.Trim().Length-1 && str.IndexOf(',') > 0)
            {
                str = str.Trim().Substring(1, str.Trim().Length - 2).Trim();
                long X, Y;
                try
                {
                    X = Convert.ToInt64(str.Substring(0, str.IndexOf(',')));
                    Y = Convert.ToInt64(str.Substring(str.IndexOf(',') + 1));
                    return new Point2D(X, Y);
                } catch (InvalidCastException)
                {
                    return null;
                }
            }
            return null;
        }

        /// <summary>
        /// adds two Point2D objects together
        /// </summary>
        public static Point2D operator+(Point2D a, Point2D b)
        {
            return new Point2D(a.X + b.X, a.Y + b.Y);
        }

        /// <summary>
        /// takes the difference of two Point2D objects
        /// </summary>
        public static Point2D operator -(Point2D a, Point2D b)
        {
            return new Point2D(a.X - b.X, a.Y - b.Y);
        }
        
        /// <summary>
        /// multiplies a Point2D object by a scalar
        /// </summary>
        public static Point2D operator *(Point2D a, long s)
        {
            return new Point2D(a.X * s, a.Y * s);
        }

        /// <summary>
        /// multiplies a Point2D object by a scalar
        /// </summary>
        public static Point2D operator *(long s, Point2D a)
        {
            return new Point2D(a.X * s, a.Y * s);
        }

        /// <summary>
        /// divides a Point2D object by a scalar
        /// </summary>
        public static Point2D operator /(Point2D a, long s)
        {
            return new Point2D(a.X / s, a.Y / s);
        }

        /// <summary>
        /// divides a scalar by a Point2D object
        /// </summary>
        public static Point2D operator /(long s, Point2D a)
        {
            return new Point2D(s / a.X, s / a.Y);
        }

        /// <summary>
        /// checks equality between two Point2D objects
        /// </summary>
        public static bool operator ==(Point2D a, Point2D b)
        {
            return a.X == b.X && a.Y == b.Y;
        }

        /// <summary>
        /// checks equality between two Point2D objects
        /// </summary>
        public override bool Equals(object that)
        {
            return this.GetType()==that.GetType() && this == (Point2D) that;
        }

        /// <summary>
        /// Generates a possible hash value for any Point2D object
        /// </summary>
        public override int GetHashCode()
        {
            int sign = 1;
            long hashl = this.X ^ this.Y;
            if (hashl < 0)
                sign = -1;
            return (int)Math.Sqrt(Math.Abs(hashl)) * sign;
        }

        /// <summary>
        /// checks inequality between two Point2D objects
        /// </summary>
        public static bool operator !=(Point2D a, Point2D b)
        {
            return a.X != b.X || a.Y != b.Y;
        }

        /// <summary>
        /// puts the Point2D object in a readable format
        /// </summary>
        public override string ToString()
        {
            return "(" + X.ToString() + "," + Y.ToString() + ")";
        }

        /// <summary>
        /// Returns the "length" of a point
        /// </summary>
        public long Length()
        {
            return (long)Math.Sqrt(Math.Pow(X, 2) + Math.Pow(Y, 2));
        }

        /// <summary>
        /// Compares to another Point2D object and returns true if this one is above the other
        /// </summary>
        public bool Above(Point2D o)
        {
            return this.Y > o.Y;
        }

        /// <summary>
        /// Compares to another Point2D object and returns true if this one is below the other
        /// </summary>
        public bool Below(Point2D o)
        {
            return this.Y < o.Y;
        }

        /// <summary>
        /// Compares to another Point2D object and returns true if this one is left of the other
        /// </summary>
        public bool Left(Point2D o)
        {
            return this.X < o.X;
        }

        /// <summary>
        /// Compares to another Point2D object and returns true if this one is right of the other
        /// </summary>
        public bool Right(Point2D o)
        {
            return this.X > o.X;
        }
    }

    /// <summary>
    /// Polygon2D object of Point2D points
    /// </summary>
    public class Polygon2D
    {
        public List<Point2D> vertices;

        /// <summary>
        /// Basic constructor
        /// </summary>
        public Polygon2D()
        {
            vertices = new List<Point2D>();
        }

        /// <summary>
        /// Copy Constructor
        /// </summary>
        public Polygon2D(Polygon2D o)
        {
            vertices = new List<Point2D>();
            for(int i=0; i<o.vertices.Count; i++)
            {
                vertices.Add(new Point2D(o.vertices[i]));
            }
        }

        /// <summary>
        /// Returns the number of vertices in the polygon
        /// </summary>
        public long Size()
        {
            return this.vertices.Count;
        }

        /// <summary>
        /// Appends a new point to the end of the polygon's vertices
        /// </summary>
        public Point2D AppendPoint(Point2D point)
        {
            vertices.Add(new Point2D(point));
            return point;
        }

        /// <summary>
        /// Creates a new point between two existing points
        /// </summary>
        public Point2D NewPoint(Point2D a, Point2D b)
        {
            for(int i = 0; i < vertices.Count - 1; i++)
            {
                if ((vertices[i] == a && vertices[i+1] == b) || (vertices[i] == b && vertices[i + 1] == a))
                {
                    vertices.Insert(i + 1, (a + b) / 2);
                    return (a + b) / 2;
                }
            }
            return null;
        }

        /// <summary>
        /// Moves a point to another
        /// </summary>
        public Point2D MovePoint(Point2D old_position, Point2D new_position)
        {
            for (int i = 0; i < vertices.Count; i++)
            {
                if (vertices[i] == old_position)
                {
                    vertices[i] = new Point2D(new_position);
                    return vertices[i];
                }
            }
            return null;
        }

        /// <summary>
        /// Checks whether a given point is constrained by a polygon
        /// </summary>
        public bool PointInPolygon(Point2D point)
        {
            int i, j;
            bool c = false;
            for (i = 0, j = vertices.Count-1; i < vertices.Count; j = i++)
            {
                //The point is checked against each edge. The first line of the test succeeds if the y-coord is within scope, and the second line succeeds if it is left
                if (((vertices[i].Y >= point.Y) != (vertices[j].Y >= point.Y)) &&
                 (point.X <= (vertices[j].X - vertices[i].X) * (point.Y - vertices[i].Y) / (vertices[j].Y - vertices[i].Y) + vertices[i].X))
                    c = !c;
            }
            return c;
        }

        /// <summary>
        /// Checks whether a given polygon is constrained by a polygon
        /// </summary>
        public bool PolygonInPolygon(Polygon2D polygon)
        {
            for (int i=0; i<polygon.vertices.Count; i++)
            {
                if (!PointInPolygon(polygon.vertices[i]))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Checks whether the polygon has a given point
        /// </summary>
        public bool HasPoint(Point2D point)
        {
            for(int i = 0; i < vertices.Count; i++)
            {
                if (point == vertices[i])
                    return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Superclass that stores the information of a Level object
    /// </summary>
    public class SuperLevel
    {
        private string name;
        private int level;
        private string leveltype;
        private List<SuperLevel> sublevels;
        public SuperLevel parent;
        public List<string> leveldata;

        /// <summary>
        /// empty constructor; do not use
        /// </summary>
        public SuperLevel()
        {
            this.name = "null";
            this.level = -1;
            this.leveltype = "null";
            this.sublevels = null;
            this.parent = null;
            this.leveldata = null;
        }

        /// <summary>
        /// Basic constructor, creates a level given a name, level number, and level type
        /// </summary>
        protected SuperLevel(string name, int level, string leveltype, SuperLevel parent)
        {
            this.name = name;
            this.level = level;
            this.leveltype = leveltype;
            this.sublevels = new List<SuperLevel>();
            if (parent!=null && parent.GetLevel() < level)
                this.parent = parent;
            else
                this.parent = null;
            this.leveldata = new List<string>();
        }

        /// <summary>
        /// basic destructor
        /// </summary>
        ~SuperLevel()
        {
            this.sublevels.Clear();
        }

        /// <summary>
        /// Returns true if the current level is a valid level
        /// </summary>
        public bool Valid()
        {
            bool toReturn = level > 0 && level <= 6 && !string.Equals(name, "null") && !string.Equals(leveltype, "null");
            if (parent != null)
                toReturn = toReturn && level > parent.GetLevel();
            for(int i=0; i<sublevels.Count && toReturn; i++)
            {
                toReturn = toReturn && sublevels[i].Valid();
            }
            return toReturn;
        }

        /// <summary>
        /// Returns the level name
        /// </summary>
        public string GetName()
        {
            return this.name;
        }

        /// <summary>
        /// Prepares the level for printings with the given delimiters
        /// </summary>
        public string PrepareString(char inner_delimiter, char outer_delimiter)
        {
            string Text = "Start Level" + inner_delimiter + level + outer_delimiter;
            Text += "Level Name" + inner_delimiter + name + outer_delimiter;
            Text += "Level Type" + inner_delimiter + leveltype + outer_delimiter;
            Polygon2D border = null;
            switch (level) //Appends the special properties of each level to Text
            {
                case 1:
                    break;
                case 2:
                    border = ((Level2)this).GetBorder();
                    for (int i = 0; i < border.Size(); i++)
                    {
                        Text += "Border Vertex" + inner_delimiter + border.vertices[i].ToString() + outer_delimiter;
                    }
                    break;
                case 3:
                    border = ((Level3)this).GetBorder();
                    for (int i = 0; i < border.Size(); i++)
                    {
                        Text += "Border Vertex" + inner_delimiter + border.vertices[i].ToString() + outer_delimiter;
                    }
                    break;
                case 4:
                    border = ((Level4)this).GetBorder();
                    for (int i = 0; i < border.Size(); i++)
                    {
                        Text += "Border Vertex" + inner_delimiter + border.vertices[i].ToString() + outer_delimiter;
                    }
                    break;
                case 5:
                    Text += "Center" + inner_delimiter + ((Level5)this).GetCenter().ToString() + outer_delimiter;
                    Text += "Radius" + inner_delimiter + ((Level5)this).radius.ToString() + outer_delimiter;
                    break;
                case 6:
                    Text += "Center" + inner_delimiter + ((Level6)this).GetCenter().ToString() + outer_delimiter;
                    break;
                default:
                    break;
            } //Now done with the special properties of each level
            for (int i = 0; i < leveldata.Count; i++) //Add the leveldata
            {
                Text += leveldata[i].Trim() + outer_delimiter;
            }
            for (int i = 0; i < sublevels.Count; i++)
            {
                sublevels[i].PrepareString(inner_delimiter, outer_delimiter);
            }
            Text += "End Level" + inner_delimiter + level + outer_delimiter;
            return Text;
        }

        /// <summary>
        /// Prepares the level for printings with the default delimiters
        /// </summary>
        public override string ToString()
        {
            return PrepareString(' ', '\n');
        }

        /// <summary>
        /// Returns the list of sublevels
        /// </summary>
        public List<SuperLevel> GetSublevels()
        {
            return this.sublevels;
        }

        /// <summary>
        /// Returns the level number
        /// </summary>
        public int GetLevel()
        {
            return this.level;
        }

        /// <summary>
        /// Returns the level type
        /// </summary>
        public string GetLevelType()
        {
            return this.leveltype;
        }
        
        /// <summary>
        /// Compares the level numbers
        /// </summary>
        public static bool operator<(SuperLevel s1, SuperLevel s2)
        {
            return s1.GetLevel() < s2.GetLevel();
        }

        /// <summary>
        /// Compares the level numbers
        /// </summary>
        public static bool operator >(SuperLevel s1, SuperLevel s2)
        {
            return s1.GetLevel() > s2.GetLevel();
        }
        
        /// <summary>
        /// Checks if the current level has a sublevel with a given name
        /// </summary>
        public bool HasSublevelWithName(string name)
        {
            if (string.Equals(name, "null"))
                return true;
            for (int i = 0; i < this.sublevels.Count; i++)
            {
                if (string.Equals(name, this.sublevels[i].GetName()))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Adds a passed sublevel if it is of a lower level than the current level; returns true on success
        /// </summary>
        public bool AddSublevel(SuperLevel o)
        {
            if (o.GetLevel() > this.GetLevel())
            {
                if (HasSublevelWithName(o.GetName()))
                    return false;
                if (!o.SetParent(this))
                    return false;
                this.sublevels.Add(o);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Sets the parent of the level
        /// </summary>
        public bool SetParent(SuperLevel parent)
        {
            if (parent != null && !string.Equals(parent.GetName(), "null") && !string.Equals(parent.GetType(), "null"))
            {
                this.parent = parent;
            }
            return false;
        }

        /// <summary>
        /// Finds and returns a level with the name
        /// </summary>
        public SuperLevel GetLevel(string name)
        {
            for(int i=0; i<this.sublevels.Count; i++)
            {
                if(string.Equals(name, this.sublevels[i].GetName())){
                    return this.sublevels[i];
                }
            }
            return null;
        }

        /// <summary>
        /// Finds and returns a level with the name and level
        /// </summary>
        public SuperLevel GetLevel(string name, int level)
        {
            for (int i = 0; i < this.sublevels.Count; i++)
            {
                if (string.Equals(name, this.sublevels[i].GetName()))
                {
                    if(level == this.sublevels[i].level)
                        return this.sublevels[i];
                    break;
                }
            }
            return null;
        }

        /// <summary>
        /// Finds and returns a level with the name, level, and leveltype
        /// </summary>
        public SuperLevel GetLevel(string name, int level, string leveltype)
        {
            for (int i = 0; i < this.sublevels.Count; i++)
            {
                if (string.Equals(name, this.sublevels[i].GetName()))
                {
                    if (level == this.sublevels[i].level && string.Equals(leveltype, this.sublevels[i].GetLevelType()))
                        return this.sublevels[i];
                    break;
                }
            }
            return null;
        }

        /// <summary>
        /// Retrieves a list of all levels with the given name
        /// </summary>
        public List<SuperLevel> FindLevels(string name)
        {
            List<SuperLevel> results = new List<SuperLevel>();
            for(int i = 0; i < this.sublevels.Count; i++)
            {
                if(string.Equals(name, this.sublevels[i].GetName())){
                    results.Add(this.sublevels[i]);
                    break;
                }
            }
            for(int i = 0; i < this.sublevels.Count; i++)
            {
                results.Concat<SuperLevel>(this.sublevels[i].FindLevels(name));
            }
            return results;
        }

        /// <summary>
        /// Retrieves a list of all levels with the given name and level number
        /// </summary>
        public List<SuperLevel> FindLevels(string name, int level)
        {
            List<SuperLevel> results = new List<SuperLevel>();
            for (int i = 0; i < this.sublevels.Count; i++)
            {
                if (string.Equals(name, this.sublevels[i].GetName())){
                    if(level == this.sublevels[i].GetLevel())
                        results.Add(this.sublevels[i]);
                    break;
                }
            }
            if (level > this.level + 1)
            {
                for (int i = 0; i < this.sublevels.Count; i++)
                {
                    if(level > this.sublevels[i].GetLevel()) {
                        results.Concat<SuperLevel>(this.sublevels[i].FindLevels(name, level));
                    }
                }
            }
            return results;
        }

        /// <summary>
        /// Retrieves a list of all levels with the given name, level number, and level type
        /// </summary>
        public List<SuperLevel> FindLevels(string name, int level, string leveltype)
        {
            List<SuperLevel> results = new List<SuperLevel>();
            for (int i = 0; i < this.sublevels.Count; i++)
            {
                if (string.Equals(name, this.sublevels[i].GetName()))
                {
                    if (level == this.sublevels[i].GetLevel() && string.Equals(leveltype, this.sublevels[i].GetLevelType()))
                        results.Add(this.sublevels[i]);
                    break;
                }
            }
            if (level > this.level + 1)
            {
                for (int i = 0; i < this.sublevels.Count; i++)
                {
                    if (level > this.sublevels[i].GetLevel())
                    {
                        results.Concat<SuperLevel>(this.sublevels[i].FindLevels(name, level, leveltype));
                    }
                }
            }
            return results;
        }
    };

    /// <summary>
    /// Level 1 object class - Worlds
    /// </summary>
    public class Level1 : SuperLevel
    {
        /// <summary>
        /// Basic constructor, creates a level 1 object given a name and level type
        /// </summary>
        public Level1(string name, string leveltype) : base(name, 1, leveltype, null)
        {
            
        }
    }

    /// <summary>
    /// Level 2 object class - Greater Regions
    /// </summary>
    public class Level2 : SuperLevel
    {
        private Polygon2D border;

        /// <summary>
        /// Basic constructor, creates a level 2 object given a name and level type
        /// </summary>
        public Level2(string name, string leveltype, Level1 parent, Polygon2D border) : base(name, 2, leveltype, parent)
        {
            this.border = new Polygon2D(border);
        }

        /// <summary>
        /// Makes a copy of the border property to return
        /// </summary>
        /// <returns></returns>
        public Polygon2D GetBorder()
        {
            return new Polygon2D(border);
        }

        /// <summary>
        /// Returns the number of vertices in the border
        /// </summary>
        public long Size()
        {
            return this.border.Size();
        }

        /// <summary>
        /// Appends a new point to the end of the borders's vertices
        /// </summary>
        public Point2D AppendPoint(Point2D point)
        {
            return border.AppendPoint(point);
        }

        /// <summary>
        /// Creates a new point between two existing points
        /// </summary>
        public Point2D NewPoint(Point2D a, Point2D b)
        {
            return border.NewPoint(a, b);
        }

        /// <summary>
        /// Moves a point to another
        /// </summary>
        public Point2D MovePoint(Point2D old_position, Point2D new_position)
        {
            return border.MovePoint(old_position, new_position);
        }

        /// <summary>
        /// Checks whether a given point is constrained by a polygon
        /// </summary>
        public bool PointInPolygon(Point2D point)
        {
            return border.PointInPolygon(point);
        }

        /// <summary>
        /// Checks whether a given polygon is constrained by a polygon
        /// </summary>
        public bool PolygonInPolygon(Polygon2D polygon)
        {
            return border.PolygonInPolygon(polygon);
        }

        /// <summary>
        /// Checks whether the polygon has a given point
        /// </summary>
        public bool HasPoint(Point2D point)
        {
            return border.HasPoint(point);
        }
    }

    /// <summary>
    /// Level 3 object class - Regions
    /// </summary>
    public class Level3 : SuperLevel
    {
        private Polygon2D border; //Not constrained by Greater Region Boundaries

        /// <summary>
        /// Basic constructor, creates a level 3 object given a name and level type
        /// </summary>
        public Level3(string name, string leveltype, SuperLevel parent, Polygon2D border) : base(name, 3, leveltype, parent)
        {
            this.border = new Polygon2D(border);
        }

        /// <summary>
        /// Makes a copy of the border property to return
        /// </summary>
        /// <returns></returns>
        public Polygon2D GetBorder()
        {
            return new Polygon2D(border);
        }

        /// <summary>
        /// Returns the number of vertices in the border
        /// </summary>
        public long Size()
        {
            return this.border.Size();
        }

        /// <summary>
        /// Appends a new point to the end of the borders's vertices
        /// </summary>
        public Point2D AppendPoint(Point2D point)
        {
            return border.AppendPoint(point);
        }

        /// <summary>
        /// Creates a new point between two existing points
        /// </summary>
        public Point2D NewPoint(Point2D a, Point2D b)
        {
            return border.NewPoint(a, b);
        }

        /// <summary>
        /// Moves a point to another
        /// </summary>
        public Point2D MovePoint(Point2D old_position, Point2D new_position)
        {
            return border.MovePoint(old_position, new_position);
        }

        /// <summary>
        /// Checks whether a given point is constrained by a polygon
        /// </summary>
        public bool PointInPolygon(Point2D point)
        {
            return border.PointInPolygon(point);
        }

        /// <summary>
        /// Checks whether a given polygon is constrained by a polygon
        /// </summary>
        public bool PolygonInPolygon(Polygon2D polygon)
        {
            return border.PolygonInPolygon(polygon);
        }

        /// <summary>
        /// Checks whether the polygon has a given point
        /// </summary>
        public bool HasPoint(Point2D point)
        {
            return border.HasPoint(point);
        }
    }

    /// <summary>
    /// Level 4 object class - Subregions
    /// </summary>
    public class Level4 : SuperLevel
    {
        private Polygon2D border; //Constrained by Greater Region Boundaries

        /// <summary>
        /// Basic constructor, creates a level 4 object given a name and level type
        /// </summary>
        public Level4(string name, string leveltype, SuperLevel parent, Polygon2D border) : base(name, 4, leveltype, parent)
        {
            this.border = new Polygon2D(border);
        }

        /// <summary>
        /// Makes a copy of the border property to return
        /// </summary>
        /// <returns></returns>
        public Polygon2D GetBorder()
        {
            return new Polygon2D(border);
        }

        /// <summary>
        /// Returns the number of vertices in the border
        /// </summary>
        public long Size()
        {
            return this.border.Size();
        }

        /// <summary>
        /// Returns true if the parent is of the correct type and the given point fits within the constraints of the parent, or if the parent is not of the correct type
        /// </summary>
        public bool PointInParent(Point2D point)
        {
            if (this.parent.GetLevel() == 3)
                return ((Level3)this.parent).GetBorder().PointInPolygon(point);
            return true;
        }

        /// <summary>
        /// Appends a new point to the end of the borders's vertices, but only if that point fits within the constraints of the parent's borders
        /// </summary>
        public Point2D AppendPoint(Point2D point)
        {
            if(PointInParent(point))
                return border.AppendPoint(point);
            return null;
        }

        /// <summary>
        /// Creates a new point between two existing points
        /// </summary>
        public Point2D NewPoint(Point2D a, Point2D b)
        {
            return border.NewPoint(a, b);
        }

        /// <summary>
        /// Moves a point to another, but only if that point fits within the constraints of the parent's borders
        /// </summary>
        public Point2D MovePoint(Point2D old_position, Point2D new_position)
        {
            if (PointInParent(new_position))
                return border.MovePoint(old_position, new_position);
            return null;
        }

        /// <summary>
        /// Checks whether a given point is constrained by a polygon
        /// </summary>
        public bool PointInPolygon(Point2D point)
        {
            return border.PointInPolygon(point);
        }

        /// <summary>
        /// Checks whether a given polygon is constrained by a polygon
        /// </summary>
        public bool PolygonInPolygon(Polygon2D polygon)
        {
            return border.PolygonInPolygon(polygon);
        }

        /// <summary>
        /// Checks whether the polygon has a given point
        /// </summary>
        public bool HasPoint(Point2D point)
        {
            return border.HasPoint(point);
        }
    }

    /// <summary>
    /// Level 5 object class - Locations
    /// </summary>
    public class Level5 : SuperLevel
    {
        private Point2D center;
        public long radius;

        /// <summary>
        /// Basic constructor, creates a level 5 object given a name and level type
        /// </summary>
        public Level5(string name, string leveltype, SuperLevel parent, Point2D center, long radius) : base(name, 5, leveltype, parent)
        {
            if (PointInParent(center))
            {
                this.center = new Point2D(center);
                this.radius = radius;
            }
            else
            {
                this.center = null;
                this.radius = 0;
            }
        }

        /// <summary>
        /// Returns true if the parent is of the correct type and the given point fits within the constraints of the parent, or if the parent is not of the correct type
        /// </summary>
        public bool PointInParent(Point2D point)
        {
            if (this.parent.GetLevel() == 3)
                return ((Level3)this.parent).GetBorder().PointInPolygon(point);
            else if (this.parent.GetLevel() == 4)
                return ((Level4)this.parent).GetBorder().PointInPolygon(point);
            return true;
        }

        /// <summary>
        /// Returns the center
        /// </summary>
        public Point2D GetCenter()
        {
            return new Point2D(this.center);
        }

        /// <summary>
        /// Moves the center somewhere else
        /// </summary>
        public bool MoveCenter(Point2D point)
        {
            if (PointInParent(point))
            {
                this.center = new Point2D(point);
                return true;
            }
            return false;
        }

    }

    /// <summary>
    /// Level 6 object class - Structures
    /// </summary>
    public class Level6 : SuperLevel
    {
        private Point2D center;

        /// <summary>
        /// Basic constructor, creates a level 6 object given a name and level type
        /// </summary>
        public Level6(string name, string leveltype, SuperLevel parent, Point2D center) : base(name, 6, leveltype, parent)
        {
            if (PointInParent(center))
            {
                this.center = new Point2D(center);
            }
            else
            {
                this.center = null;
            }
        }

        /// <summary>
        /// Returns true if the parent is of the correct type and the given point fits within the constraints of the parent, or if the parent is not of the correct type
        /// </summary>
        public bool PointInParent(Point2D point)
        {
            if (this.parent.GetLevel() == 3)
                return ((Level3)this.parent).GetBorder().PointInPolygon(point);
            else if (this.parent.GetLevel() == 4)
                return ((Level4)this.parent).GetBorder().PointInPolygon(point);
            else if (this.parent.GetLevel() == 5)
                return (this.center - ((Level5)this.parent).GetCenter()).Length() <= ((Level5)this.parent).radius;
            return true;
        }

        /// <summary>
        /// Returns the center
        /// </summary>
        public Point2D GetCenter()
        {
            return new Point2D(this.center);
        }

        /// <summary>
        /// Moves the center somewhere else
        /// </summary>
        public bool MoveCenter(Point2D point)
        {
            if (PointInParent(point))
            {
                this.center = new Point2D(point);
                return true;
            }
            return false;
        }

    }

    /// <summary>
    /// A class that simplifies reading/writing of the active file
    /// </summary>
    public class FileManager
    {
        private Windows.Storage.StorageFile ActiveFile;
        private bool Readable;
        private bool Writable;
        private bool ValidFile;
        private bool NewFile;
        string Header;
        string Version;
        string Text;
        string Original;
        char inner_delimiter = ':';
        char outer_delimiter = '\n';
        List<string> NationalDirectory;
        List<string> GeographicalDirectory;
        List<string> ClimateDirectory;
        List<string> FactionalDirectory;
        List<string> CulturalDirectory;
        List<string> BiologicalDirectory;
        List<string> Keywords;
        public ObservableCollection<Level1> Worlds;
        private List<string> active_lines;
        private List<string> invalid_lines;

        public static ObservableCollection<Level1> WorldList()
        {
            ObservableCollection<Level1> WorldList = new ObservableCollection<Level1>()
            {
                
            };
            return WorldList;
        }


        enum DirectoryMode : int
        {
            None = 0,
            National = 1,
            Geographical = 2,
            Climate = 3,
            Factional = 4,
            Cultural = 5,
            Biological = 6
        }


        public FileManager(Windows.Storage.StorageFile file, bool newfile)
        {
            this.NewFile = newfile;
            this.ActiveFile = file;
            this.Readable = false;
            this.Writable = false;
            this.ValidFile = false;
            Original = "";
            Text = "";
            active_lines = new List<string>();
            invalid_lines = new List<string>();
            NationalDirectory = new List<string>();
            GeographicalDirectory = new List<string>();
            ClimateDirectory = new List<string>();
            FactionalDirectory = new List<string>();
            CulturalDirectory = new List<string>();
            BiologicalDirectory = new List<string>();
            Keywords = new List<string>();
            Keywords.Add("National");
            Keywords.Add("Geographical");
            Keywords.Add("Climate");
            Keywords.Add("Factional");
            Keywords.Add("Cultural");
            Keywords.Add("Biological");
            Keywords.Add("Directory");
            Keywords.Add("Settings");
            Keywords.Add("Content");
            Keywords.Add("Start Level");
            Keywords.Add("Level Name");
            Keywords.Add("Level Type");
            Keywords.Add("Border Vertex");
            Keywords.Add("Center");
            Keywords.Add("Radius");
            Keywords.Add("End Level");
            Keywords.Add("Level Type");
            Keywords.Add("Invalid");
            Header = "Prime Worldbuilding File" + outer_delimiter + "Created by Michael Quinn" + outer_delimiter + "Version" + inner_delimiter + "1.0.0" + outer_delimiter;
            Worlds = FileManager.WorldList();
        }

        /// <summary>
        /// returns true if the supplied text contains any illegal keyword
        /// </summary>
        public bool HasKeyword(string text)
        {
            for (int i=0; i<Keywords.Count; i++)
            {
                if (text.Contains(Keywords[i]))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// returns the first illegal keyword in the supplied text
        /// </summary>
        public string GetKeyword(string text)
        {
            for (int i = 0; i < Keywords.Count; i++)
            {
                if (text.Contains(Keywords[i]))
                    return Keywords[i];
            }
            return null;
        }

        /// <summary>
        /// Returns true if there is a world that has the same name and type
        /// </summary>
        public bool HasWorld(string name, string type)
        {
            if (Readable) { 
                for (int i=0; i<Worlds.Count; i++)
                {
                    if (string.Equals(name, Worlds[i].GetName()) && string.Equals(type, Worlds[i].GetLevelType()))
                        return true;
                }
                return false;
            }
            return true;
        }

        public bool Valid()
        {
            return this.ValidFile;
        }

        public bool ReadReady()
        {
            return this.Readable;
        }

        public bool WriteReady()
        {
            return this.Writable;
        }

        public String FileName()
        {
            return this.ActiveFile.Name;
        }

        public String GetCopy()
        {
            UpdateText();
            return this.Text;
        }

        /// <summary>
        /// Accesses and replaces the i-th line of the text with the provided string
        /// </summary>
        private bool ReplaceLine(string line, int i)
        {
            if (this.Readable && i >= 0 && i < this.active_lines.Count) {
                if (line.Trim().Contains(outer_delimiter))
                    return false;
                this.active_lines[i] = line.Trim();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Accesses and returns the i-th line of the text
        /// </summary>
        private String GetLine(int i)
        {
            if (this.Readable && i >= 0 && i < this.active_lines.Count)
            {
                return this.active_lines[i];
            }
            return null;
        }

        /// <summary>
        /// Adds a line of text to the end of the active lines
        /// </summary>
        private bool AddLine(string line)
        {
            if (Writable)
            {
                if (line.Trim().Contains(outer_delimiter))
                    return false;
                this.active_lines.Add(line.Trim());
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Adds multiple lines of text to the end of the active lines
        /// </summary>
        private bool AddLines(string text)
        {
            if (Writable)
            {
                int index = 0;
                int length = 0;
                while(index < text.Length)
                {
                    length = text.Substring(index).IndexOf(outer_delimiter);
                    if (length < 0)
                        length = text.Substring(index).Length;
                    AddLine(text.Substring(index, length).Trim());
                    index += length + 1;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if there is a line in the current version
        /// </summary>
        private bool HasLine(string line)
        {
            if (Readable)
            {
                if (line.Trim().Contains(outer_delimiter))
                    return false;
                for(int i=0; i<active_lines.Count; i++)
                {
                    if (string.Equals(active_lines[i], Equals(line.Trim())))
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns the index of the first occurance of the given line starting at start and before end
        /// </summary>
        private int IndexOf(string line, int start, int end)
        {
            if (Readable)
            {
                if (line.Trim().Contains(outer_delimiter))
                    return -1;
                for (int i = start; i < active_lines.Count && i < end; i++)
                {
                    if (string.Equals(active_lines[i], line.Trim()))
                        return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Returns the index of the first occurance of the given line starting at start
        /// </summary>
        private int IndexOf(string line, int start)
        {
            return IndexOf(line, start, active_lines.Count);
        }

        /// <summary>
        /// Returns the index of the first occurance of the given line
        /// </summary>
        private int IndexOf(string line)
        {
            return IndexOf(line, 0, active_lines.Count);
        }

        /// <summary>
        /// Returns the index of the first line after the starting index but before the end index containing the given substring
        /// </summary>
        private int IndexOfSubstring(string substring, int start, int end)
        {
            if (Readable)
            {
                if (substring.Contains(outer_delimiter))
                    return -1;
                for (int i = start; i < active_lines.Count && i < end; i++)
                {
                    if (active_lines[i].Contains(substring))
                        return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Returns the index of the first line after the starting index containing the given substring
        /// </summary>
        private int IndexOfSubstring(string substring, int start)
        {
            return IndexOfSubstring(substring, start, active_lines.Count);
        }

        /// <summary>
        /// Returns the index of the first line containing the given substring
        /// </summary>
        private int IndexOfSubstring(string substring)
        {
            return IndexOfSubstring(substring, 0, active_lines.Count);
        }

        /// <summary>
        /// Returns the first line containing the passed substring
        /// </summary>
        private string FindLineBySubstring(string substring)
        {
            if (Readable)
            {
                if (substring.Trim().Contains(outer_delimiter))
                    return null;
                for(int i=0; i<active_lines.Count; i++)
                {
                    if (active_lines[i].Contains(substring))
                        return active_lines[i];
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the first line containing the passed substring after the starting index
        /// </summary>
        private string FindLineBySubstring(string substring, int start)
        {
            if (Readable)
            {
                if (substring.Trim().Contains(outer_delimiter))
                    return null;
                for (int i = start; i < active_lines.Count; i++)
                {
                    if (active_lines[i].Contains(substring))
                        return active_lines[i];
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the first line containing the passed substring after the starting index and before the ending index
        /// </summary>
        private string FindLineBySubstring(string substring, int start, int end)
        {
            if (Readable)
            {
                if (substring.Trim().Contains(outer_delimiter))
                    return null;
                for (int i = start; i < active_lines.Count && i < end; i++)
                {
                    if (active_lines[i].Contains(substring))
                        return active_lines[i];
                }
            }
            return null;
        }

        /// <summary>
        /// Retrieves the lines between the starting index to the ending index, inclusive
        /// </summary>
        private List<string> GetLines(int start, int end)
        {
            if (Readable)
            {
                if (start < 0 || end >= active_lines.Count || end < start)
                    return null;
                return active_lines.GetRange(start, end - start + 1);
            }
            return null;
        }

        /// <summary>
        /// Inserts a line of text at the given position
        /// </summary>
        private bool InsertLine(string text, int i)
        {
            if (Writable && i >= 0 && i < active_lines.Count)
            {
                if (text.Trim().Contains(outer_delimiter))
                    return false;
                this.active_lines.Insert(i, text.Trim());
            }
            return false;
        }

        /// <summary>
        /// converts a string into a list of strings based on outer_delimiter
        /// </summary>
        private List<string> ConvertText(string text)
        {
            List<string> output = new List<string>();
            int index = 0;
            int length = 0;
            while(text!=null && index < text.Length)
            {
                length = text.Substring(index).IndexOf(outer_delimiter);
                if (length < 0)
                    length = text.Substring(index).Length;
                output.Add(text.Substring(index, length));
                index += length + 1;
            }
            return output;
        }

        /// <summary>
        /// converts a list of strings into a string broken up by outer_delimiter
        /// </summary>
        private string ConvertText(List<string> text)
        {
            string output = "";
            for(int i=0; text!=null && i<text.Count; i++)
            {
                output += text[i] + outer_delimiter;
            }
            return output;
        }

        /// <summary>
        /// Parses the text passed from GetDirectories to produce level objects; should not include the start and end lines for the level, and the level number should be passed as well
        /// </summary>
        private SuperLevel ParseLevels(String ActiveText, int level_num, SuperLevel parent)
        {
            string level_name = "null";
            string level_type = "null";
            string line = "";
            Polygon2D border = new Polygon2D();
            Point2D center = null;
            long radius = 0;
            int index = 0;
            int length = 0;
            SuperLevel level = null;
            bool got_name = false;
            bool got_type = false;
            bool got_border = false;
            bool got_center = false;
            bool got_radius = false;
            bool make_level = false;
            bool made_level = false;

            //Main loop; begins by checking whether the level information is available, and provides it; then looks for content processing
            while (index < ActiveText.Length)
            {
                length = ActiveText.Substring(index).IndexOf(outer_delimiter);
                line = ActiveText.Substring(index, length).Trim();
                bool do_it = true;
                if (!got_name && line.IndexOf("Level Name") == 0) //Beginning of level information processing; starting with Level Name
                {
                    do_it = false;
                    level_name = line.Substring("Level Name".Length + 1).Trim();
                    got_name = true;
                }
                else if (!got_type && line.IndexOf("Level Type") == 0) //Level information processing for Level Type
                {
                    do_it = false;
                    level_type = line.Substring("Level Type".Length + 1).Trim();
                    got_type = true;
                }
                else if (line.IndexOf("Border Vertex") == 0)//Level information processing for Border Vertices
                {
                    do_it = false;
                    line = line.Substring("Border Vertex".Length + 1).Trim();
                    Point2D point = Point2D.FromString(line);
                    if (point != null)
                    {
                        border.AppendPoint(point);
                        if (!got_border && border.Size() >= 3)
                        {
                            got_border = true;
                        }
                    }
                }
                else if (!got_center && line.IndexOf("Center") == 0) //Level information processing for Center point
                {
                    do_it = false;
                    line = line.Substring("Center".Length + 1).Trim();
                    center = Point2D.FromString(line);
                    if (center != null)
                    {
                        got_center = true;
                    }
                }
                else if (!got_radius && line.IndexOf("Radius") == 0) //Level information processing for Radius
                {
                    do_it = false;
                    line = line.Substring("Radius".Length + 1).Trim();
                    try
                    {
                        radius = Convert.ToInt64(line);
                        got_radius = true;
                    }
                    catch (InvalidCastException)
                    {
                        ;
                    }
                }
                if(do_it || index + length + 1 >= ActiveText.Length) //If the current line does not correspond to level information processing, then it is supposed to be level content
                {
                    if (!made_level && !make_level) //sees that the level has not been made and hasn't been proven to be makable
                    {
                        make_level = got_name && got_type;
                        switch (level_num) //Does the necessary checks depending on the level number
                        {
                            case 1:
                                break;
                            case 2:
                                make_level = make_level && got_border;
                                break;
                            case 3:
                                make_level = make_level && got_border;
                                break;
                            case 4:
                                make_level = make_level && got_border;
                                break;
                            case 5:
                                make_level = make_level && got_center && got_radius;
                                break;
                            case 6:
                                make_level = make_level && got_center;
                                break;
                            default:
                                make_level = false;
                                break;
                        }
                    }
                    if (!made_level && make_level) //Now checks if the level is makable and hasn't been made yet
                    {
                        switch (level_num)
                        {
                            case 1:
                                level = new Level1(level_name, level_type);
                                break;
                            case 2:
                                level = new Level2(level_name, level_type, (Level1)parent, border);
                                break;
                            case 3:
                                level = new Level3(level_name, level_type, parent, border);
                                break;
                            case 4:
                                level = new Level4(level_name, level_type, parent, border);
                                break;
                            case 5:
                                level = new Level5(level_name, level_type, parent, center, radius);
                                break;
                            case 6:
                                level = new Level6(level_name, level_type, parent, center);
                                break;
                            default:
                                make_level = false;
                                break;
                        }
                        if (make_level)
                            made_level = true;
                        make_level = false;
                    }
                    if (made_level) //Now for the actual content processing of the level
                    {
                        if (line.IndexOf("Start Level") == 0) //If there appears to be a sublevel here
                        {
                            try
                            {
                                int new_level_num = Convert.ToInt32(line.Substring("Start Level".Length + 1).Trim());
                                if (new_level_num <= 6 && new_level_num > level_num) //Ensures that the level is valid
                                {
                                    line = ActiveText.Substring(index).Trim();
                                    if (line.IndexOf("End Level" + inner_delimiter + new_level_num.ToString()) >= 0) //Ensures that there is an end to the level
                                    {
                                        int partial_length = line.IndexOf("End Level" + inner_delimiter + new_level_num.ToString()) - 1;
                                        line = line.Substring(line.IndexOf(outer_delimiter) + 1, partial_length).Trim();
                                        SuperLevel sublevel = ParseLevels(line, new_level_num, level);
                                        if (sublevel != null) //Ensures that the generated world is valid
                                            level.AddSublevel(sublevel);
                                        length += partial_length;
                                    } //End sublevel processing
                                }
                                else //If the number is invalid
                                {
                                    length = Math.Max(length, line.IndexOf("End Level" + inner_delimiter + new_level_num.ToString()));
                                }
                            }
                            catch (InvalidCastException)
                            {
                                ;
                            }
                        }//End potential sublevel processing
                        else //If there is unknown level data
                        {
                            level.leveldata.Add(line);
                        }
                    } //End content processing
                } //End of Else Block for content processing
                index += length + 1;
            }
            return level;
        }

        /// <summary>
        /// Parses the file line-by-line to determine where each directory starts and ends, adding them to the directory list
        /// </summary>
        private void GetDirectories()
        {
            if (Readable && ValidFile)
            {
                this.active_lines = ConvertText(this.Text);
                this.invalid_lines = new List<string>();
                string line = active_lines[0];
                int index = 0;
                //Heading section
                if (!string.Equals(line, "Prime Worldbuilding File"))
                {
                    ValidFile = false;
                }
                else
                {
                    for (index = 0; index < this.active_lines.Count; index++)
                    {
                        line = active_lines[index];
                        if (string.Equals(line, "Settings"))
                            break;
                        if (!Header.Contains(line) && !line.Contains("Version" + inner_delimiter))
                            invalid_lines.Add(line);
                    }
                }
                //Settings Section
                if (!string.Equals(line, "Settings")){
                    ValidFile = false;
                }
                else
                {
                    List<string> activeDirectory = null;
                    for (index++; index < this.active_lines.Count; index++) //Loop to initiate settings
                    {
                        line = active_lines[index];
                        if (string.Equals(line, "National"))
                        {
                            activeDirectory = NationalDirectory;
                        }
                        else if (string.Equals(line, "Geographical"))
                        {
                            activeDirectory = GeographicalDirectory;
                        }
                        else if (string.Equals(line, "Climate"))
                        {
                            activeDirectory = ClimateDirectory;
                        }
                        else if (string.Equals(line, "Factional"))
                        {
                            activeDirectory = FactionalDirectory;
                        }
                        else if (string.Equals(line, "Cultural"))
                        {
                            activeDirectory = CulturalDirectory;
                        }
                        else if (string.Equals(line, "Biological"))
                        {
                            activeDirectory = BiologicalDirectory;
                        }
                        else if (string.Equals(line, "Content"))
                        {
                            break;
                        }
                        else //Manages adding information to directories
                        {
                            if(activeDirectory == null)
                            {
                                invalid_lines.Add(line);
                            }
                            else
                            {
                                activeDirectory.Add(line);
                            }
                        }
                    }
                }
                //Content Section
                if(!string.Equals(line, "Content"))
                {
                    ValidFile = false;
                }
                else
                {
                    for(index++; index < this.active_lines.Count; index++)
                    {
                        line = active_lines[index];
                        if(string.Equals(line, "Start Level" + inner_delimiter + "1"))
                        {
                            int end = IndexOf("End Level" + inner_delimiter + "1", index);
                            if(end >= index)
                            {
                                this.Worlds.Add((Level1) ParseLevels(ConvertText(GetLines(index + 1, end - 1)), 1, null));
                                index = end;
                            }
                            else //If there is no end to the level
                            {
                                invalid_lines.Add(line);
                            }
                        }
                        else if(string.Equals(line, "Invalid"))
                        {
                            break;
                        }
                        else
                        {
                            invalid_lines.Add(line);
                        }
                    }
                }
                //Invalid Section
                for(index++; index < active_lines.Count; index++)
                {
                    line = active_lines[index];
                    if (string.Equals(line, "Invalid"))
                        continue;
                    invalid_lines.Add(line);
                }
            }
        }

        /// <summary>
        /// Prepares the file for reading and writing
        /// </summary>
        public async Task ReadyFile()
        {
            if (!this.Readable)
            {
                this.Original = await Windows.Storage.FileIO.ReadTextAsync(ActiveFile);
                this.Text = this.Original;
                this.active_lines = ConvertText(this.Text);
                if (string.Equals(this.active_lines[0], "Prime Worldbuilding File"))
                {
                    this.ValidFile = true;
                    this.Readable = true;
                }
                else if (this.NewFile)
                {
                    await FormatNewFile();
                    this.Readable = true;
                }
                else
                {
                    this.ValidFile = false;
                }
            }
            if (!this.Writable && this.ValidFile)
            {
                this.Version = GetLine(2);
                this.Version = this.Version.Substring(this.Version.IndexOf(inner_delimiter) + 1);
                GetDirectories();
                this.active_lines = ConvertText(this.Text);
                this.Writable = true;
            }
        }
        
        /// <summary>
        /// Saves edits to the Text variable by overwriting the Text with the Header followed by the information contained in all directories. Does not update the file itself.
        /// </summary>
        public void UpdateText()
        {
            if ((this.ValidFile && this.Readable) || NewFile)
            {
                this.active_lines = ConvertText(Header);
                AddLine("Settings");
                AddLine("National");
                for(int i=0; i<NationalDirectory.Count; i++)
                {
                    AddLine(NationalDirectory[i]);
                }
                AddLine("Geographical");
                for (int i = 0; i < GeographicalDirectory.Count; i++)
                {
                    AddLine(GeographicalDirectory[i]);
                }
                AddLine("Climate");
                for (int i = 0; i < ClimateDirectory.Count; i++)
                {
                    AddLine(ClimateDirectory[i]);
                }
                AddLine("Factional");
                for (int i = 0; i < FactionalDirectory.Count; i++)
                {
                    AddLine(FactionalDirectory[i]);
                }
                AddLine("Cultural");
                for (int i = 0; i < CulturalDirectory.Count; i++)
                {
                    AddLine(CulturalDirectory[i]);
                }
                AddLine("Biological");
                for (int i = 0; i < BiologicalDirectory.Count; i++)
                {
                    AddLine(BiologicalDirectory[i]);
                }
                AddLine("Content");

                bool any_invalid = this.invalid_lines.Count > 0;
                for (int i = 0; i < this.Worlds.Count; i++)
                {
                    if (Worlds[i].Valid()) //Ensures the world is valid (every level and sublevel has a correct level number, functional name, and functional level type
                    {
                        AddLines(Worlds[i].PrepareString(inner_delimiter, outer_delimiter));
                    }
                    else
                    {
                        any_invalid = true;
                    }
                }
                if (any_invalid)
                {
                    AddLine("Invalid");
                    for(int i=0; i<this.Worlds.Count; i++)
                    {
                        if (!Worlds[i].Valid())
                            AddLines(Worlds[i].PrepareString(inner_delimiter, outer_delimiter));
                    }
                    for(int i=0; i<invalid_lines.Count; i++)
                    {
                        AddLine(invalid_lines[i]);
                    }
                }
                this.Text = this.ConvertText(active_lines);
            }
        }
        
        /// <summary>
        /// Checks if the working version matches the saved version; updates the working version
        /// </summary>
        public bool MatchesSave()
        {
            if(ValidFile && Readable && Writable)
            {
                UpdateText();
                return string.Equals(this.Original, this.Text);
            }
            return true;
        }

        /// <summary>
        /// Saves edits to the file by overwriting the Text with the Header followed by the information contained in all directories
        /// </summary>
        public async Task SaveFile()
        {
            if (this.ValidFile && this.Writable)
            {
                this.Text = ConvertText(this.active_lines);
                if (!MatchesSave())
                    await Windows.Storage.FileIO.WriteTextAsync(ActiveFile, this.Text);
                this.Original = this.Text;
                this.NewFile = false;
            }
        }

        /// <summary>
        /// Sets a new file to match the ideal format
        /// </summary>
        public async Task FormatNewFile()
        {
            if (!this.ValidFile && NewFile)
            {
                UpdateText();
                this.ValidFile = true;
                this.Writable = true;
                await SaveFile();
            }
        }
    }
}
