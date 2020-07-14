using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace TFG_Worldbuilder_Application
{
    public enum LevelType : short
    {
        Invalid = -1,
        World = 0,
        National = 1,
        Geographical = 2,
        Climate = 3,
        Factional = 4,
        Cultural = 5,
        Biological = 6
    }

    /// <summary>
    /// 2D Point object
    /// </summary>
    public class Point2D : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string str)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(str));
            }
        }

        private long _X;
        public long X
        {
            get
            {
                return _X;
            }
            set
            {
                _X = value;
                RaisePropertyChanged("X");
                RaisePropertyChanged("pointstr");
            }
        }
        private long _Y;
        public long Y
        {
            get
            {
                return _Y;
            }
            set
            {
                _Y = value;
                RaisePropertyChanged("Y");
                RaisePropertyChanged("pointstr");
            }
        }
        public string pointstr
        {
            get
            {
                return X.ToString() + ',' + Y.ToString();
            }
            set
            {
                ;
            }
        }

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
        public Point2D(Point2D o) : this(o.X, o.Y)
        {
            ;
        }

        /// <summary>
        /// Converts a Windows Foundation Point into a Point2D
        /// </summary>
        public Point2D(Point o) : this((long) o.X, (long) o.Y)
        {
            ;
        }

        /// <summary>
        /// Attempts to convert a string in the format "(X,Y)" to a Point2D object
        /// </summary>
        public static Point2D FromString(string str)
        {
            if (str.Trim().IndexOf('(') == 0 && str.Trim().IndexOf(')') == str.Trim().Length - 1 && str.IndexOf(',') > 0)
            {
                str = str.Trim().Substring(1, str.Trim().Length - 2).Trim();
                long X, Y;
                try
                {
                    X = Convert.ToInt64(str.Substring(0, str.IndexOf(',')));
                    Y = Convert.ToInt64(str.Substring(str.IndexOf(',') + 1));
                    return new Point2D(X, Y);
                }
                catch (InvalidCastException)
                {
                    return null;
                }
            }
            return null;
        }

        /// <summary>
        /// adds two Point2D objects together
        /// </summary>
        public static Point2D operator +(Point2D a, Point2D b)
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
            return this.GetType() == that.GetType() && this == (Point2D)that;
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
    public class Polygon2D : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string str)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(str));
            }
        }

        public ObservableCollection<Point2D> vertices;
        public int Count
        {
            get
            {
                return this.Size();
            }
        }
        public Point2D this[int i]
        {
            get
            {
                return new Point2D(this.vertices[i]);
            }
            set
            {
                this.vertices[i] = value;
                RaisePropertyChanged("vertices");
            }
        }

        /// <summary>
        /// Basic constructor
        /// </summary>
        public Polygon2D()
        {
            vertices = new ObservableCollection<Point2D>();
        }

        /// <summary>
        /// Constructs a Polygon2D from a list of Point2D objects
        /// </summary>
        public Polygon2D(IList<Point2D> list) : this()
        {
            for(int i=0; i<list.Count; i++)
            {
                this.vertices.Add(new Point2D(list[i]));
            }
        }

        /// <summary>
        /// Copy Constructor
        /// </summary>
        public Polygon2D(Polygon2D o) : this(o.vertices)
        {
            ;
        }

        /// <summary>
        /// Returns the number of vertices in the polygon
        /// </summary>
        public int Size()
        {
            return this.vertices.Count;
        }

        /// <summary>
        /// Appends a new point to the end of the polygon's vertices
        /// </summary>
        public Point2D AppendPoint(Point2D point)
        {
            vertices.Add(new Point2D(point));
            RaisePropertyChanged("vertices");
            return point;
        }

        /// <summary>
        /// Creates a new point between two existing points
        /// </summary>
        public Point2D NewPoint(Point2D a, Point2D b)
        {
            for (int i = 0; i < vertices.Count - 1; i++)
            {
                if ((vertices[i] == a && vertices[i + 1] == b) || (vertices[i] == b && vertices[i + 1] == a))
                {
                    vertices.Insert(i + 1, (a + b) / 2);
                    RaisePropertyChanged("vertices");
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
                    RaisePropertyChanged("vertices");
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
            for (i = 0, j = vertices.Count - 1; i < vertices.Count; j = i++)
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
            for (int i = 0; i < polygon.vertices.Count; i++)
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
            for (int i = 0; i < vertices.Count; i++)
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
    public class SuperLevel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string str)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(str));
                if (parent != null)
                {
                    parent.RaisePropertyChanged(this.name + '.' + str);
                }
            }
        }

        private string _name;
        public string name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                RaisePropertyChanged("name");
            }
        }
        private int _level;
        public int level
        {
            get
            {
                return _level;
            }
            set
            {
                _level = value;
                RaisePropertyChanged("level");
            }
        }
        private LevelType _leveltype;
        public LevelType leveltype
        {
            get
            {
                return _leveltype;
            }
            set
            {
                _leveltype = value;
                RaisePropertyChanged("leveltype");
            }
        }
        private string _subtype;
        public string subtype
        {
            get
            {
                return _subtype;
            }
            set
            {
                _subtype = value;
                RaisePropertyChanged("subtype");
            }
        }
        private string _color;
        public string color
        {
            get
            {
                return _color;
            }
            set
            {
                _color = value;
                RaisePropertyChanged("color");
            }
        }
        public ObservableCollection<SuperLevel> sublevels;
        public SuperLevel parent;
        public List<string> leveldata;

        /// <summary>
        /// empty constructor; do not use
        /// </summary>
        public SuperLevel()
        {
            this.name = "null";
            this.level = -1;
            this.leveltype = LevelType.Invalid;
            this.sublevels = null;
            this.parent = null;
            this.leveldata = null;
        }

        /// <summary>
        /// Basic constructor, creates a level given a name, level number, level type, and parent level
        /// </summary>
        /// <param name="name">Level name, must be a unique identifier among its siblings</param>
        /// <param name="level">Level number, between 1 and 6; generally determined by the constructor used</param>
        /// <param name="leveltype">Level type, provides specific context that indicates what type of level it is of the 6 basic types or World</param>
        /// <param name="parent">Level parent, must be of same type as child or World</param>
        protected SuperLevel(string name, int level, LevelType leveltype, SuperLevel parent)
        {
            this.name = name;
            this.level = level;
            this.leveltype = leveltype;
            this.subtype = "";
            this.sublevels = new ObservableCollection<SuperLevel>();
            if (parent != null && parent.GetLevel() < level)
            {
                this.parent = parent;
                if (this.level > 2)
                {
                    this.leveltype = parent.GetLevelType();
                }
            }
            else
                this.parent = null;
            this.leveldata = new List<string>();
            color = "#F2F2F2";
        }

        /// <summary>
        /// /// Advanced constructor that uses sublevel; leveltype is overwritten by parent's type, if applicable
        /// </summary>
        /// <param name="name">Level name, must be a unique identifier among its siblings</param>
        /// <param name="level">Level number, between 1 and 6; generally determined by the constructor used</param>
        /// <param name="leveltype">Level type, provides specific context that indicates what type of level it is of the 6 basic types or World</param>
        /// <param name="subtype">Level subtype, can be more customized by the user</param>
        /// <param name="parent">Level parent, must be of same type as child or World</param>
        protected SuperLevel(string name, int level, LevelType leveltype, string subtype, SuperLevel parent) : this(name, level, leveltype, parent)
        {
            this.subtype = subtype;
        }

        /// <summary>
        /// basic destructor
        /// </summary>
        ~SuperLevel()
        {
            this.sublevels.Clear();
        }

        /// <summary>
        /// Filters a passed list by level
        /// </summary>
        public static IList<SuperLevel> Filter(IList<SuperLevel> lst, int level)
        {
            List<SuperLevel> output = new List<SuperLevel>();
            for(int i=0; i<lst.Count; i++)
            {
                if (lst[i].GetLevel() == level)
                    output.Add(lst[i]);
            }
            return output;
        }

        /// <summary>
        /// Filters a passed list by level and type
        /// </summary>
        public static IList<SuperLevel> Filter(IList<SuperLevel> lst, int level, LevelType type)
        {
            List<SuperLevel> output = new List<SuperLevel>();
            for (int i = 0; i < lst.Count; i++)
            {
                if (lst[i].GetLevel() == level && LevelType.Equals(lst[i].GetType(), type))
                    output.Add(lst[i]);
            }
            return output;
        }

        /// <summary>
        /// Filters a passed list by level, type, and subtype
        /// </summary>
        public static IList<SuperLevel> Filter(IList<SuperLevel> lst, int level, LevelType type, string subtype)
        {
            List<SuperLevel> output = new List<SuperLevel>();
            for (int i = 0; i < lst.Count; i++)
            {
                if (lst[i].GetLevel() == level && LevelType.Equals(lst[i].GetType(), type) && string.Equals(lst[i].subtype, subtype))
                    output.Add(lst[i]);
            }
            return output;
        }

        /// <summary>
        /// Filters a passed list by type
        /// </summary>
        public static IList<SuperLevel> Filter(IList<SuperLevel> lst, LevelType type)
        {
            List<SuperLevel> output = new List<SuperLevel>();
            for (int i = 0; i < lst.Count; i++)
            {
                if (LevelType.Equals(lst[i].GetType(), type))
                    output.Add(lst[i]);
            }
            return output;
        }

        /// <summary>
        /// Filters a passed list by type and subtype
        /// </summary>
        public static IList<SuperLevel> Filter(IList<SuperLevel> lst, LevelType type, string subtype)
        {
            List<SuperLevel> output = new List<SuperLevel>();
            for (int i = 0; i < lst.Count; i++)
            {
                if (LevelType.Equals(lst[i].GetType(), type) && string.Equals(lst[i].subtype, subtype))
                    output.Add(lst[i]);
            }
            return output;
        }

        /// <summary>
        /// Returns true if the current level is a valid level
        /// </summary>
        public bool Valid()
        {
            bool toReturn = level > 0 && level <= 6 && !string.Equals(name, "null") && !string.Equals(leveltype, "null");
            if (parent != null)
                toReturn = toReturn && level > parent.GetLevel();
            for (int i = 0; i < sublevels.Count && toReturn; i++)
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
            Text += "Level Type" + inner_delimiter + Enum.GetName(typeof(LevelType), leveltype) + outer_delimiter;
            if(subtype.Length>0)
                Text += "Level Subtype" + inner_delimiter + subtype + outer_delimiter;
            if(level >= 2 && level <= 4) //Border Levels
            {
                try
                {
                    Polygon2D border = null;
                    border = ((BorderLevel)this).GetBorder();
                    for(int i=0; i<border.vertices.Count; i++)
                    {
                        Text += "Border Vertex" + inner_delimiter + border.vertices[i].ToString() + outer_delimiter;
                    }
                }
                catch (InvalidCastException) {}
            } 
            if(level >= 5)//Point Levels
            {
                try
                {
                    Point2D center = ((PointLevel)this).GetCenter();
                    Text += "Center" + inner_delimiter + center.ToString() + outer_delimiter;
                }
                catch (InvalidCastException) { }
            } 
            if(level == 5)//Level 5's radius
            {
                try
                {
                    string rad = ((Level5)this).radius.ToString();
                    Text += "Radius" + inner_delimiter + rad + outer_delimiter;
                }
                catch (InvalidCastException) { }
            }
            //Now done with the special properties of each level
            for (int i = 0; i < leveldata.Count; i++) //Add the leveldata
            {
                Text += leveldata[i].Trim() + outer_delimiter;
            }
            for (int i = 0; i < sublevels.Count; i++)
            {
                Text += sublevels[i].PrepareString(inner_delimiter, outer_delimiter);
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
        public ObservableCollection<SuperLevel> GetSublevels()
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
        public LevelType GetLevelType()
        {
            return this.leveltype;
        }

        /// <summary>
        /// Compares the level numbers
        /// </summary>
        public static bool operator <(SuperLevel s1, SuperLevel s2)
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
        public virtual bool AddSublevel(SuperLevel o)
        {
            if (o.GetLevel() > this.GetLevel())
            {
                if (HasSublevelWithName(o.GetName()))
                    return false;
                if (!o.SetParent(this))
                    return false;
                this.sublevels.Add(o);
                RaisePropertyChanged("sublevels");
                return true;
            }
            return false;
        }

        /// <summary>
        /// Attempts to sets the parent of the level; will fail if the level types are mismatched or if the parent has invalid information
        /// </summary>
        public virtual bool SetParent(SuperLevel parent)
        {
            if (parent != null && !string.Equals(parent.GetName(), "null") && !LevelType.Equals(parent.GetType(), LevelType.Invalid) && (parent.GetLevel() == 1 || LevelType.Equals(this.leveltype, LevelType.Invalid) || LevelType.Equals(parent.GetLevelType(), this.GetLevelType())))
            {
                this.parent = parent;
                if (parent.GetLevel() != 1)
                    this.leveltype = parent.GetLevelType();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Finds and returns a level with the name
        /// </summary>
        public SuperLevel GetLevel(string name)
        {
            for (int i = 0; i < this.sublevels.Count; i++)
            {
                if (string.Equals(name, this.sublevels[i].GetName()))
                {
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
                    if (level == this.sublevels[i].level)
                        return this.sublevels[i];
                    break;
                }
            }
            return null;
        }

        /// <summary>
        /// Finds and returns a level with the name, level, and subtype
        /// </summary>
        public SuperLevel GetLevel(string name, int level, string subtype)
        {
            for (int i = 0; i < this.sublevels.Count; i++)
            {
                if (string.Equals(name, this.sublevels[i].GetName()))
                {
                    if (level == this.sublevels[i].level && string.Equals(subtype, this.sublevels[i].subtype))
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
            for (int i = 0; i < this.sublevels.Count; i++)
            {
                if (string.Equals(name, this.sublevels[i].GetName()))
                {
                    results.Add(this.sublevels[i]);
                    break;
                }
            }
            for (int i = 0; i < this.sublevels.Count; i++)
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
                if (string.Equals(name, this.sublevels[i].GetName()))
                {
                    if (level == this.sublevels[i].GetLevel())
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
                        results.Concat<SuperLevel>(this.sublevels[i].FindLevels(name, level));
                    }
                }
            }
            return results;
        }

        /// <summary>
        /// Retrieves a list of all levels with the given name, level number, and level type
        /// </summary>
        public List<SuperLevel> FindLevels(string name, int level, LevelType leveltype)
        {
            List<SuperLevel> results = new List<SuperLevel>();
            for (int i = 0; i < this.sublevels.Count; i++)
            {
                if (string.Equals(name, this.sublevels[i].GetName()))
                {
                    if (level == this.sublevels[i].GetLevel() && LevelType.Equals(leveltype, this.sublevels[i].GetLevelType()))
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

        /// <summary>
        /// Returns true if the passed point fits within the boundaries and level
        /// </summary>
        public virtual bool CanFitPoint(Point2D point)
        {
            return true;
        }

        /// <summary>
        /// Returns true if the passed point fits within the boundaries
        /// </summary>
        public virtual bool FitsPoint(Point2D point)
        {
            return false;
        }

        /// <summary>
        /// Returns true if the parent is of the correct type and the given point fits within the constraints of the parent, or if the parent is not of the correct type
        /// </summary>
        public bool PointInParent(Point2D point)
        {
            if (this.level == 1)
                return true;
            if (this.parent == null)
                return true;
            return this.parent.CanFitPoint(point);
        }

        /// <summary>
        /// Returns false, as this function only returns true for regions with defined locations
        /// </summary>
        public virtual bool IsWithinRegion(Polygon2D region)
        {
            return false;
        }

        /// <summary>
        /// Returns a list of sublevels in the region
        /// </summary>
        public List<SuperLevel> SublevelsInRegion(Polygon2D region)
        {
            List<SuperLevel> output = new List<SuperLevel>();
            if (IsWithinRegion(region))
                output.Add(this);
            for(int i=0; i<sublevels.Count; i++)
            {
                output.Concat<SuperLevel>(this.sublevels[i].SublevelsInRegion(region));
            }
            return output;
        }

        /// <summary>
        /// Returns true if the target has the border property
        /// </summary>
        public virtual bool HasBorderProperty()
        {
            return false;
        }

        /// <summary>
        /// Returns true if a SuperLevel of the given level should have the border property
        /// </summary>
        public static bool HasBorderProperty(int level)
        {
            if (level >= 2 && level <= 4)
                return true;
            return false;
        }

        /// <summary>
        /// Returns true if the target has the center property
        /// </summary>
        public virtual bool HasCenterProperty()
        {
            return false;
        }
        
        /// <summary>
        /// Returns true if a SuperLevel of the given level should have the center property
        /// </summary>
        public static bool HasCenterProperty(int level)
        {
            if (level >= 5 && level <= 6)
                return true;
            return false;
        }
        
        /// <summary>
        /// Returns true if the target has the radius property
        /// </summary>
        public virtual bool HasRadiusProperty()
        {
            return false;
        }
        
        /// <summary>
        /// Returns true if a SuperLevel of the given level should have the radius property
        /// </summary>
        public static bool HasRadiusProperty(int level)
        {
            return level == 5;
        }

    };

    /// <summary>
    /// Superclass for levels with a defined border
    /// </summary>
    public class BorderLevel : SuperLevel
    {
        private Polygon2D border;
        
        public string points
        {
            get
            {
                return GetPoints();
            }
            set
            {
                ;
            }
        }

        /// <summary>
        /// Extended constructor, creates a level given a name, level number, level type, parent level, and border object
        /// </summary>
        /// <param name="name">Level name, must be a unique identifier among its siblings</param>
        /// <param name="level">Level number, between 1 and 6; generally determined by the constructor used</param>
        /// <param name="leveltype">Level type, provides specific context that indicates what type of level it is of the 6 basic types or World</param>
        /// <param name="parent">Level parent, must be of same type as child or World</param>
        /// <param name="border">Level border, must have at least 3 points</param>
        protected BorderLevel(string name, int level, LevelType type, SuperLevel parent, Polygon2D border) : base(name, level, type, parent)
        {
            this.border = new Polygon2D();
            for (int i = 0; i < border.Count; i++)
            {
                if (PointInParent(border.vertices[i]))
                    this.border.AppendPoint(border.vertices[i]);
            }
            if (this.border.Count > 0 && this.border.Count < 3)
                this.border = new Polygon2D();
        }

        /// <summary>
        /// Extended constructor, creates a level given a name, level number, level type, sublevel, parent level, and border object
        /// </summary>
        /// <param name="name">Level name, must be a unique identifier among its siblings</param>
        /// <param name="level">Level number, between 1 and 6; generally determined by the constructor used</param>
        /// <param name="leveltype">Level type, provides specific context that indicates what type of level it is of the 6 basic types or World</param>
        /// <param name="subtype">Level subtype, can be more customized by the user</param>
        /// <param name="parent">Level parent, must be of same type as child or World</param>
        /// <param name="border">Level border, must have at least 3 points</param>
        protected BorderLevel(string name, int level, LevelType type, string sublevel, SuperLevel parent, Polygon2D border) : base(name, level, type, sublevel, parent)
        {
            this.border = new Polygon2D();
            for (int i = 0; i < border.Count; i++)
            {
                if (PointInParent(border.vertices[i]))
                    this.border.AppendPoint(border.vertices[i]);
            }
            if (this.border.Count > 0 && this.border.Count < 3)
                this.border = new Polygon2D();
        }
        
        /// <summary>
        /// Returns true if the passed point fits within the boundaries and level
        /// </summary>
        public override bool CanFitPoint(Point2D point)
        {
            return PointInPolygon(point);
        }

        /// <summary>
        /// Returns true if the passed point fits within the boundaries
        /// </summary>
        public override bool FitsPoint(Point2D point)
        {
            return PointInPolygon(point);
        }
        
        /// <summary>
        /// Generates a XAML-Ready string of all vertices in the object
        /// </summary>
        private string GetPoints()
        {
            string output = "";
            for(int i=0; i<border.vertices.Count; i++)
            {
                if (i > 0)
                    output += ' ';
                output += border.vertices[i].X + ',' + border.vertices[i].Y;
            }
            return output;
        }

        /// <summary>
        /// Returns all available information within Worlds that applies to this region
        /// </summary>
        public List<SuperLevel> GetAllWithinRegion(List<Level1> Worlds)
        {
            List<SuperLevel> output = new List<SuperLevel>();
            for(int i=0; i<Worlds.Count; i++)
            {
                output.Concat<SuperLevel>(Worlds[i].SublevelsInRegion(border));
            }
            return output;
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
            Point2D output = this.border.AppendPoint(point);
            RaisePropertyChanged("points");
            return output;
        }

        /// <summary>
        /// Creates a new point between two existing points
        /// </summary>
        public Point2D NewPoint(Point2D a, Point2D b)
        {
            Point2D output = border.NewPoint(a, b);
            RaisePropertyChanged("points");
            return output;
        }

        /// <summary>
        /// Moves a point to another, but only if the new position is also within the parent or if the old position was outside the parents
        /// </summary>
        public Point2D MovePoint(Point2D old_position, Point2D new_position)
        {
            if (PointInParent(new_position) || !PointInParent(old_position))
            {
                Point2D output = border.MovePoint(old_position, new_position);
                if (output != null)
                    RaisePropertyChanged("points");
                return output;
            }
            return old_position;
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
        /// Sets the parent of the level, taking into account its borders and the parent's
        /// </summary>
        public override bool SetParent(SuperLevel parent)
        {
            if(this.level <= 3)
            {
                return base.SetParent(parent);
            }
            for(int i=0; i<this.Size(); i++)
            {
                if (!PointInParent(this.border.vertices[i]))
                    return false;
            }
            return base.SetParent(parent);
        }

        /// <summary>
        /// Checks whether the polygon has a given point
        /// </summary>
        public bool HasPoint(Point2D point)
        {
            return border.HasPoint(point);
        }

        /// <summary>
        /// Returns true if border region is within the region
        /// </summary>
        public override bool IsWithinRegion(Polygon2D region)
        {
            return region.PolygonInPolygon(this.border);
        }

        /// <summary>
        /// Returns true if the target has the border property
        /// </summary>
        public override bool HasBorderProperty()
        {
            return true;
        }
    }

    /// <summary>
    /// Superclass for levels with a defined center
    /// </summary>
    public class PointLevel : SuperLevel
    {
        private Point2D _center;
        public Point2D center
        {
            get
            {
                return _center;
            }
            set
            {
                _center = value;
                RaisePropertyChanged("center");
            }
        }

        /// <summary>
        /// Extended constructor, creates a level given a name, level number, level type, parent level, and point object
        /// </summary>
        /// <param name="name">Level name, must be a unique identifier among its siblings</param>
        /// <param name="level">Level number, between 1 and 6; generally determined by the constructor used</param>
        /// <param name="leveltype">Level type, provides specific context that indicates what type of level it is of the 6 basic types or World</param>
        /// <param name="parent">Level parent, must be of same type as child or World</param>
        /// <param name="center">Level center</param>
        protected PointLevel(string name, int level, LevelType leveltype, SuperLevel parent, Point2D center) : base(name, level, leveltype, parent)
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
        /// Extended constructor, creates a level given a name, level number, level type, sublevel, parent level, and point object
        /// </summary>
        /// <param name="name">Level name, must be a unique identifier among its siblings</param>
        /// <param name="level">Level number, between 1 and 6; generally determined by the constructor used</param>
        /// <param name="leveltype">Level type, provides specific context that indicates what type of level it is of the 6 basic types or World</param>
        /// <param name="subtype">Level subtype, can be more customized by the user</param>
        /// <param name="parent">Level parent, must be of same type as child or World</param>
        /// <param name="center">Level center</param>
        protected PointLevel(string name, int level, LevelType leveltype, string sublevel, SuperLevel parent, Point2D center) : base(name, level, leveltype, sublevel, parent)
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
        /// Sets the parent of the level, taking into account its borders and the parent's
        /// </summary>
        public override bool SetParent(SuperLevel parent)
        {
            if (this.level <= 3)
            {
                return base.SetParent(parent);
            }
            if (!PointInParent(this.center))
                return false;
            return base.SetParent(parent);
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

        /// <summary>
        /// Returns true if the point is in the region
        /// </summary>
        public override bool IsWithinRegion(Polygon2D region)
        {
            return region.PointInPolygon(this.center);
        }
        
        /// <summary>
        /// Returns true if the target has the center property
        /// </summary>
        public override bool HasCenterProperty()
        {
            return true;
        }
    }

    /// <summary>
    /// Level 1 object class - Worlds
    /// </summary>
    public class Level1 : SuperLevel
    {

        /// <summary>
        /// Basic constructor, creates a level 1 object given a name
        /// </summary>
        /// <param name="name">Level name, must be a unique identifier among its siblings</param>
        public Level1(string name) : base(name, 1, LevelType.World, null)
        {

        }

        /// <summary>
        /// Advanced constructor; takes a name and a subtype for the world
        /// </summary>
        /// <param name="name">Level name, must be a unique identifier among its siblings</param>
        /// <param name="subtype">Level subtype, can be more customized by the user</param>
        public Level1(string name, string subtype) : base (name, 1, LevelType.World, subtype, null)
        {

        }
    }

    /// <summary>
    /// Border Level 2 object class - Greater Regions
    /// </summary>
    public class Level2 : BorderLevel
    {
        /// <summary>
        /// Basic constructor, creates a level 2 object given a name, level type, parent, and border
        /// </summary>
        /// <param name="name">Level name, must be a unique identifier among its siblings</param>
        /// <param name="leveltype">Level type, provides specific context that indicates what type of level it is of the 6 basic types or World</param>
        /// <param name="parent">Level parent, must be of same type as child or World</param>
        /// <param name="border">Level border, must have at least 3 points</param>
        public Level2(string name, LevelType leveltype, Level1 parent, Polygon2D border) : base(name, 2, leveltype, parent, border)
        {
            
        }

        /// <summary>
        /// Basic constructor, creates a level 2 object given a name, level type, sublevel, parent, and border
        /// </summary>
        /// <param name="name">Level name, must be a unique identifier among its siblings</param>
        /// <param name="leveltype">Level type, provides specific context that indicates what type of level it is of the 6 basic types or World</param>
        /// <param name="subtype">Level subtype, can be more customized by the user</param>
        /// <param name="parent">Level parent, must be of same type as child or World</param>
        /// <param name="border">Level border, must have at least 3 points</param>
        public Level2(string name, LevelType leveltype, string sublevel, Level1 parent, Polygon2D border) : base(name, 2, leveltype, sublevel, parent, border)
        {

        }
        
        /// <summary>
        /// Returns true if the passed point fits within the boundaries and level
        /// </summary>
        public override bool CanFitPoint(Point2D point)
        {
            return true;
        }
    }

    /// <summary>
    /// Border Level 3 object class - Regions
    /// </summary>
    public class Level3 : BorderLevel
    {
        /// <summary>
        /// Basic constructor, creates a level 3 object given a name, level type, parent, and border
        /// </summary>
        /// <param name="name">Level name, must be a unique identifier among its siblings</param>
        /// <param name="leveltype">Level type, provides specific context that indicates what type of level it is of the 6 basic types or World</param>
        /// <param name="parent">Level parent, must be of same type as child or World</param>
        /// <param name="border">Level border, must have at least 3 points</param>
        public Level3(string name, LevelType leveltype, SuperLevel parent, Polygon2D border) : base(name, 3, leveltype, parent, border)
        {
            
        }

        /// <summary>
        /// Basic constructor, creates a level 3 object given a name, level type, sublevel, parent, and border
        /// </summary>
        /// <param name="name">Level name, must be a unique identifier among its siblings</param>
        /// <param name="leveltype">Level type, provides specific context that indicates what type of level it is of the 6 basic types or World</param>
        /// <param name="subtype">Level subtype, can be more customized by the user</param>
        /// <param name="parent">Level parent, must be of same type as child or World</param>
        /// <param name="border">Level border, must have at least 3 points</param>
        public Level3(string name, LevelType leveltype, string sublevel, SuperLevel parent, Polygon2D border) : base(name, 3, leveltype, sublevel, parent, border)
        {

        }
    }

    /// <summary>
    /// Border Level 4 object class - Subregions
    /// </summary>
    public class Level4 : BorderLevel
    {

        /// <summary>
        /// Basic constructor, creates a level 4 object given a name, level type, parent, and border
        /// </summary>
        /// <param name="name">Level name, must be a unique identifier among its siblings</param>
        /// <param name="leveltype">Level type, provides specific context that indicates what type of level it is of the 6 basic types or World</param>
        /// <param name="parent">Level parent, must be of same type as child or World</param>
        /// <param name="border">Level border, must have at least 3 points</param>
        public Level4(string name, LevelType leveltype, SuperLevel parent, Polygon2D border) : base(name, 4, leveltype, parent, border)
        {

        }

        /// <summary>
        /// Advanced constructor, creates a level 4 object given a name, level type, sublevel, parent, and border
        /// </summary>
        /// <param name="name">Level name, must be a unique identifier among its siblings</param>
        /// <param name="leveltype">Level type, provides specific context that indicates what type of level it is of the 6 basic types or World</param>
        /// <param name="subtype">Level subtype, can be more customized by the user</param>
        /// <param name="parent">Level parent, must be of same type as child or World</param>
        /// <param name="border">Level border, must have at least 3 points</param>
        public Level4(string name, LevelType leveltype, string sublevel, SuperLevel parent, Polygon2D border) : base(name, 4, leveltype, sublevel, parent, border)
        {

        }
    }

    /// <summary>
    /// Point Level 5 object class - Locations
    /// </summary>
    public class Level5 : PointLevel
    {
        private long _radius;
        public long radius
        {
            get
            {
                return _radius;
            }
            set
            {
                _radius = value;
                RaisePropertyChanged("radius");
            }
        }

        /// <summary>
        /// Basic constructor, creates a level 5 object given a name, level type, parent, center, and radius
        /// </summary>
        /// <param name="name">Level name, must be a unique identifier among its siblings</param>
        /// <param name="leveltype">Level type, provides specific context that indicates what type of level it is of the 6 basic types or World</param>
        /// <param name="parent">Level parent, must be of same type as child or World</param>
        /// <param name="center">Level center</param>
        /// <param name="radius">Level radius, indicates region in which sublevels can be placed surrounding center</param>
        public Level5(string name, LevelType leveltype, SuperLevel parent, Point2D center, long radius) : base(name, 5, leveltype, parent, center)
        {
            if (PointInParent(center))
            {
                this.radius = radius;
            }
            else
            {
                this.radius = 0;
            }
        }

        /// <summary>
        /// Advanced constructor, creates a level 5 object given a name, level type, sublevel, parent, center, and radius
        /// </summary>
        /// <param name="name">Level name, must be a unique identifier among its siblings</param>
        /// <param name="leveltype">Level type, provides specific context that indicates what type of level it is of the 6 basic types or World</param>
        /// <param name="subtype">Level subtype, can be more customized by the user</param>
        /// <param name="parent">Level parent, must be of same type as child or World</param>
        /// <param name="center">Level center</param>
        /// <param name="radius">Level radius, indicates region in which sublevels can be placed surrounding center</param>
        public Level5(string name, LevelType leveltype, string sublevel, SuperLevel parent, Point2D center, long radius) : base(name, 5, leveltype, sublevel, parent, center)
        {
            if (PointInParent(center))
            {
                this.radius = radius;
            }
            else
            {
                this.radius = 0;
            }
        }

        /// <summary>
        /// Returns true if the point is within the radius of the center
        /// </summary>
        public bool PointInRadius(Point2D point)
        {
            return (point - center).Length() <= radius;
        }
        
        /// <summary>
        /// Returns true if the passed point fits within the boundaries and level
        /// </summary>
        public override bool CanFitPoint(Point2D point)
        {
            return PointInRadius(point);
        }

        /// <summary>
        /// Returns true if the passed point fits within the boundaries
        /// </summary>
        public override bool FitsPoint(Point2D point)
        {
            return PointInRadius(point);
        }
        
        /// <summary>
        /// Returns true if the target has the radius property
        /// </summary>
        public override bool HasRadiusProperty()
        {
            return true;
        }
    }

    /// <summary>
    /// Point Level 6 object class - Structures
    /// </summary>
    public class Level6 : PointLevel
    {
        /// <summary>
        /// Basic constructor, creates a level 6 object given a name, level type, parent, and center
        /// </summary>
        /// <param name="name">Level name, must be a unique identifier among its siblings</param>
        /// <param name="leveltype">Level type, provides specific context that indicates what type of level it is of the 6 basic types or World</param>
        /// <param name="parent">Level parent, must be of same type as child or World</param>
        /// <param name="center">Level center</param>
        public Level6(string name, LevelType leveltype, SuperLevel parent, Point2D center) : base(name, 6, leveltype, parent, center)
        {

        }

        /// <summary>
        /// Advanced constructor, creates a level 6 object given a name, level type, sublevel, parent, and center
        /// </summary>
        /// <param name="name">Level name, must be a unique identifier among its siblings</param>
        /// <param name="leveltype">Level type, provides specific context that indicates what type of level it is of the 6 basic types or World</param>
        /// <param name="subtype">Level subtype, can be more customized by the user</param>
        /// <param name="parent">Level parent, must be of same type as child or World</param>
        /// <param name="center">Level center</param>
        public Level6(string name, LevelType leveltype, string sublevel, SuperLevel parent, Point2D center) : base(name, 6, leveltype, sublevel, parent, center)
        {

        }
    }
}
