using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Media;

namespace TFG_Worldbuilder_Application
{

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
        public Point pointstr
        {
            get
            {
                return Point2D.ApplyTransformation(Point2D.ToWindowsPoint(this));
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
        public Point2D(Point o) : this((long)o.X, (long)o.Y)
        {
            ;
        }

        /// <summary>
        /// Converts a Point2D into a Point
        /// </summary>
        public static Point ToWindowsPoint(Point2D point)
        {
            return new Point(point.X, point.Y);
        }

        /// <summary>
        /// Converts a Point2D IList to a Point List
        /// </summary>
        public static PointCollection ToWindowsPoints(IList<Point2D> list)
        {
            PointCollection output = new PointCollection();
            for (int i = 0; i < list.Count; i++)
            {
                output.Add(Point2D.ToWindowsPoint(list[i]));
            }
            return output;
        }

        /// <summary>
        /// Uses Global.Zoom, Global.Center, and Global.Shift to output the render coordinates for the point
        /// </summary>
        /// <param name="point">Point2D object to transform</param>
        public static Point2D ApplyTransformation(Point2D input)
        {
            return new Point2D(Point2D.ApplyTransformation(Point2D.ToWindowsPoint(input)));
        }

        /// <summary>
        /// Uses Global.Zoom, Global.Center, and Global.Shift to output the render coordinates for the point
        /// </summary>
        /// <param name="point">Point object to transform</param>
        public static Point ApplyTransformation(Point input)
        {
            Point output = new Point(input.X, input.Y);
            output = Point2D.ToWindowsPoint(((new Point2D(input) - Global.Center) * Global.Zoom) + Global.OriginalCenter);
            return output;
        }

        /// <summary>
        /// Applies the render coordinate transformation to all elements in a PointCollection
        /// </summary>
        /// <param name="points">PointCollection to perform the transformation on</param>
        public static PointCollection ApplyTransformation(PointCollection input)
        {
            PointCollection output = new PointCollection();
            if (input == null)
                return output;
            for (int i = 0; i < input.Count; i++)
            {
                output.Add(ApplyTransformation(input[i]));
            }
            return output;
        }

        /// <summary>
        /// Uses Global.Zoom, Global.Center, and Global.Shift to output the absolute coordinates for the point
        /// </summary>
        /// <param name="point">Point2D object to revert</param>
        public static Point2D RevertTransformation(Point2D input)
        {
            return new Point2D(Point2D.RevertTransformation(Point2D.ToWindowsPoint(input)));
        }

        /// <summary>
        /// Uses Global.Zoom, Global.Center, and Global.Shift to output the absolute coordinates for the point
        /// </summary>
        /// <param name="point">Point object to revert</param>
        public static Point RevertTransformation(Point input)
        {
            Point output = new Point(input.X, input.Y);
            output = Point2D.ToWindowsPoint(((new Point2D(input) - Global.OriginalCenter) / Global.Zoom) + Global.Center);
            return output;
        }

        /// <summary>
        /// Reverts the render coordinate transformation to all elements in a PointCollection
        /// </summary>
        /// <param name="points">PointCollection to revert the transformation on</param>
        public static PointCollection RevertTransformation(PointCollection input)
        {
            PointCollection output = new PointCollection();
            if (input == null)
                return output;
            for (int i = 0; i < input.Count; i++)
            {
                output.Add(RevertTransformation(input[i]));
            }
            return output;
        }

        /// <summary>
        /// Converts a Point2D IList to a Point List
        /// </summary>
        public static ObservableCollection<Point> ToWindowsPoints2(IList<Point2D> list)
        {
            ObservableCollection<Point> output = new ObservableCollection<Point>();
            for (int i = 0; i < list.Count; i++)
            {
                output.Add(Point2D.ToWindowsPoint(list[i]));
            }
            return output;
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
        public static Point2D operator *(Point2D a, double s)
        {
            return new Point2D((long) (a.X * s), (long) (a.Y * s));
        }

        /// <summary>
        /// multiplies a Point2D object by a scalar
        /// </summary>
        public static Point2D operator *(double s, Point2D a)
        {
            return new Point2D((long)(a.X * s), (long)(a.Y * s));
        }

        /// <summary>
        /// divides a Point2D object by a scalar
        /// </summary>
        public static Point2D operator /(Point2D a, double s)
        {
            return new Point2D((long)(a.X / s), (long)(a.Y / s));
        }

        /// <summary>
        /// divides a scalar by a Point2D object
        /// </summary>
        public static Point2D operator /(long s, Point2D a)
        {
            return new Point2D((long)(s / a.X), (long)(s / a.Y));
        }

        /// <summary>
        /// checks equality between two Point2D objects
        /// </summary>
        public static bool operator ==(Point2D a, Point2D b)
        {
            if (((object)a) == null && ((object)b) == null)
                return true;
            if (((object)a) == null ^ ((object)b) == null)
                return false;
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
            if (((object)a) == null && ((object)b) == null)
                return false;
            if (((object)a) == null ^ ((object)b) == null)
                return true;
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
            for (int i = 0; i < list.Count; i++)
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

        /// <summary>
        /// Creates and returns a centerpoint for the polygon
        /// </summary>
        public Point2D GetCenter()
        {
            long minX, maxX, minY, maxY, sumX, sumY;
            minX = minY = Int64.MaxValue;
            maxX = maxY = Int64.MinValue;
            sumX = sumY = 0;
            for (int i = 0; i < vertices.Count; i++)
            {
                minX = Math.Min(minX, vertices[i].X);
                maxX = Math.Max(maxX, vertices[i].X);
                minY = Math.Min(minY, vertices[i].Y);
                maxY = Math.Max(maxY, vertices[i].Y);
                sumX += vertices[i].X;
                sumY += vertices[i].Y;
            }
            Point2D abs_center = new Point2D((minX + maxX) / 2, (minY + maxY) / 2);
            Point2D avg_center = new Point2D(sumX / vertices.Count, sumY / vertices.Count);
            if (PointInPolygon(abs_center))
                return abs_center;
            else if (PointInPolygon(avg_center))
            {
                Point2D inc_center = new Point2D(abs_center);
                while (!PointInPolygon(inc_center))
                {
                    inc_center = ((9 * inc_center) + avg_center) / 10;
                }
                return inc_center;
            }
            return abs_center;
        }
    }

    /// <summary>
    /// Interface that defines a collection of points with a valid points property that can be used for the intended purpose
    /// </summary>
    public interface MyPointCollection : INotifyPropertyChanged
    {
        PointCollection points
        {
            get;
        }

        Point2D AppendPoint(Point2D point);
    }

    public class GenericPointCollection : MyPointCollection
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string str)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(str));
            }
        }

        private PointCollection _points;
        public PointCollection points
        {
            get
            {
                return Point2D.ApplyTransformation(_points);
            }
        }

        public GenericPointCollection()
        {
            this._points = new PointCollection();
        }

        public Point2D AppendPoint(Point2D point)
        {
            if (this._points == null)
                this._points = new PointCollection();
            this._points.Add(Point2D.ToWindowsPoint(point));
            RaisePropertyChanged("points");
            return point;
        }
    }
}
