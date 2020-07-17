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
    public enum Direction : short
    {
        Invalid = -1,
        Middle = 0,
        TopLeft = 1,
        Top = 2,
        TopRight = 3,
        Right = 4,
        BottomRight = 5,
        Bottom = 6,
        BottomLeft = 7,
        Left = 8
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
            public virtual Point pointstr
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
            /// Uses Global.Zoom, Global.Center, and Global.RenderedCenter to output the render coordinates for the point
            /// Produces coordinates by translating such that Global.Center becomes (0,0), then scaling by Global.Zoom, then translating such that (0,0) becomes Global.RenderedCenter
            /// </summary>
            /// <param name="point">Point2D object to transform</param>
            public static RenderedPoint ApplyTransformation(AbsolutePoint input)
            {
                AbsolutePoint translated_input = input - Global.Center; //translated_input is set to the input translated such that Global.Center becomes (0,0)
                RenderedPoint untraslated_output = new RenderedPoint((long)(translated_input.X * Global.Zoom), (long)(translated_input.Y * Global.Zoom)); //untraslated_output is set to a rescaling of translated_input based on Global.Zoom
                return untraslated_output + Global.RenderedCenter; //The returned point is untraslated_output translated such that (0,0) becomes Global.RenderedCenter
            }

            /// <summary>
            /// Uses Global.Zoom, Global.Center, and Global.RenderedCenter to output the render coordinates for the point
            /// </summary>
            /// <param name="point">Point object to transform</param>
            public static Point ApplyTransformation(Point input)
            {
                return Point2D.ToWindowsPoint(Point2D.ApplyTransformation(new AbsolutePoint(input)));
            }

            /// <summary>
            /// Applies the render coordinate transformation to all elements in an ObservableCollection<Point2D>
            /// </summary>
            /// <param name="points">PointCollection to perform the transformation on</param>
            public static ObservableCollection<RenderedPoint> ApplyTransformation(ObservableCollection<AbsolutePoint> input)
            {
                ObservableCollection<RenderedPoint> output = new ObservableCollection<RenderedPoint>();
                if (input == null)
                    return output;
                for (int i = 0; i < input.Count; i++)
                {
                    output.Add(ApplyTransformation(input[i]));
                }
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
            /// Uses Global.Zoom, Global.Center, and Global.RenderedCenter to output the absolute coordinates for the point
            /// /// Produces coordinates by translating such that Global.RenderedCenter becomes (0,0), then scaling by Global.Zoom, then translating such that (0,0) becomes Global.Center
            /// </summary>
            /// <param name="point">Point2D object to revert</param>
            public static AbsolutePoint RevertTransformation(RenderedPoint input)
            {
                RenderedPoint translated_input = input - Global.RenderedCenter; //translated_input is set to the input translated such that Global.RenderedCenter becomes (0,0)
                AbsolutePoint untraslated_output = new AbsolutePoint((long)(translated_input.X / Global.Zoom), (long)(translated_input.Y / Global.Zoom)); //untraslated_output is set to a rescaling of translated_input based on Global.Zoom
                return untraslated_output + Global.Center; //The returned point is untraslated_output translated such that (0,0) becomes Global.Center
            }

            /// <summary>
            /// Uses Global.Zoom, Global.Center, and Global.RenderedCenter to output the absolute coordinates for the point
            /// </summary>
            /// <param name="point">Point object to revert</param>
            public static Point RevertTransformation(Point input)
            {
                return Point2D.ToWindowsPoint(Point2D.RevertTransformation(new RenderedPoint(input)));
            }
        
            /// <summary>
            /// Reverts the render coordinate transformation to all elements in an ObservableCollection<Point2D>
            /// </summary>
            /// <param name="points">PointCollection to perform the transformation on</param>
            public static ObservableCollection<AbsolutePoint> RevertTransformation(ObservableCollection<RenderedPoint> input)
            {
                ObservableCollection<AbsolutePoint> output = new ObservableCollection<AbsolutePoint>();
                if (input == null)
                    return output;
                for (int i = 0; i < input.Count; i++)
                {
                    output.Add(RevertTransformation(input[i]));
                }
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
                try
                {
                    return this.GetType() == that.GetType() && this == (Point2D)that;
                }
                catch (InvalidCastException)
                {
                    return false;
                }
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
    /// Class for Point2D objects that are Rendered Points; their values represent the rendering values for objects they represent
    /// </summary>
    public class RenderedPoint : Point2D
    {
        public override Point pointstr
        {
            get
            {
                return Point2D.ToWindowsPoint(this);
            }
        }

        public RenderedPoint(long X, long Y) : base(X, Y)
        {
            ;
        }

        public RenderedPoint(RenderedPoint o) : base((Point2D)o)
        {
            ;
        }

        public RenderedPoint(Point o) : base(o)
        {
            ;
        }

        /// <summary>
        /// Creates a RenderedPoint by transforming an AbsolutePoint
        /// </summary>
        public RenderedPoint(AbsolutePoint o) : base((Point2D) Point2D.ApplyTransformation(o))
        {
            ;
        }
        
        public static explicit operator AbsolutePoint(RenderedPoint point)
        {
            return new AbsolutePoint(point);
        }

        public static explicit operator Point(RenderedPoint point)
        {
            return new Point(point.X, point.Y);
        }
        
        public AbsolutePoint ToAbsolutePoint()
        {
            return new AbsolutePoint(this);
        }

        public Point ToWindowsPoint()
        {
            return RenderedPoint.ToWindowsPoint(this);
        }

        public Point2D ToPoint2D()
        {
            return (Point2D)this;
        }

        /// <summary>
        /// Converts a RenderedPoint into a Point
        /// </summary>
        public static Point ToWindowsPoint(RenderedPoint point)
        {
            return new Point(point.X, point.Y);
        }

        /// <summary>
        /// Converts a Point2D IList to a Point List
        /// </summary>
        public static PointCollection ToWindowsPoints(IList<RenderedPoint> list)
        {
            PointCollection output = new PointCollection();
            for (int i = 0; i < list.Count; i++)
            {
                output.Add(RenderedPoint.ToWindowsPoint(list[i]));
            }
            return output;
        }

        /// <summary>
        /// adds two RenderedPoint objects together
        /// </summary>
        public static RenderedPoint operator +(RenderedPoint a, RenderedPoint b)
        {
            return new RenderedPoint(a.X + b.X, a.Y + b.Y);
        }

        /// <summary>
        /// takes the difference of two RenderedPoint objects
        /// </summary>
        public static RenderedPoint operator -(RenderedPoint a, RenderedPoint b)
        {
            return new RenderedPoint(a.X - b.X, a.Y - b.Y);
        }

        /// <summary>
        /// multiplies a RenderedPoint object by a scalar
        /// </summary>
        public static RenderedPoint operator *(RenderedPoint a, double s)
        {
            return new RenderedPoint((long)(a.X * s), (long)(a.Y * s));
        }

        /// <summary>
        /// multiplies a RenderedPoint object by a scalar
        /// </summary>
        public static RenderedPoint operator *(double s, RenderedPoint a)
        {
            return new RenderedPoint((long)(a.X * s), (long)(a.Y * s));
        }

        /// <summary>
        /// divides a RenderedPoint object by a scalar
        /// </summary>
        public static RenderedPoint operator /(RenderedPoint a, double s)
        {
            return new RenderedPoint((long)(a.X / s), (long)(a.Y / s));
        }

        /// <summary>
        /// divides a scalar by a RenderedPoint object
        /// </summary>
        public static RenderedPoint operator /(long s, RenderedPoint a)
        {
            return new RenderedPoint((long)(s / a.X), (long)(s / a.Y));
        }

        /// <summary>
        /// checks equality between two RenderedPoint objects
        /// </summary>
        public static bool operator ==(RenderedPoint a, RenderedPoint b)
        {
            if (((object)a) == null && ((object)b) == null)
                return true;
            if (((object)a) == null ^ ((object)b) == null)
                return false;
            return a.X == b.X && a.Y == b.Y;
        }

        /// <summary>
        /// checks inequality between two RenderedPoint objects
        /// </summary>
        public static bool operator !=(RenderedPoint a, RenderedPoint b)
        {
            if (((object)a) == null && ((object)b) == null)
                return false;
            if (((object)a) == null ^ ((object)b) == null)
                return true;
            return a.X != b.X || a.Y != b.Y;
        }

        /// <summary>
        /// checks equality between two RenderedPoint objects
        /// </summary>
        public override bool Equals(object that)
        {
            try
            {
                return this.GetType() == that.GetType() && this == (RenderedPoint)that;
            }
            catch (InvalidCastException)
            {
                return false;
            }
        }

        /// <summary>
        /// Generates a possible hash value for any RenderedPoint object
        /// </summary>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    /// <summary>
    /// Class for Point2D objects that are Absolute Points; their values represent the real, saved values of the objects they represent
    /// </summary>
    public class AbsolutePoint : Point2D
    {
        public AbsolutePoint(long X, long Y) : base(X, Y)
        {
            ;
        }

        public AbsolutePoint(AbsolutePoint o) : base((Point2D)o)
        {
            ;
        }

        public AbsolutePoint(Point o) : base(o)
        {
            ;
        }

        /// <summary>
        /// Creates an AbsolutePoint by reverting a RenderedPoint
        /// </summary>
        public AbsolutePoint(RenderedPoint o) : base((Point2D) Point2D.RevertTransformation(o))
        {
            ;
        }

        public static explicit operator RenderedPoint(AbsolutePoint point)
        {
            return new RenderedPoint(point);
        }
        
        public RenderedPoint ToRenderedPoint()
        {
            return new RenderedPoint(this);
        }

        public Point2D ToPoint2D()
        {
            return (Point2D)this;
        }

        /// <summary>
        /// Attempts to convert a string in the format "(X,Y)" to a Point2D object
        /// </summary>
        public static AbsolutePoint FromString(string str)
        {
            if (str.Trim().IndexOf('(') == 0 && str.Trim().IndexOf(')') == str.Trim().Length - 1 && str.IndexOf(',') > 0)
            {
                str = str.Trim().Substring(1, str.Trim().Length - 2).Trim();
                long X, Y;
                try
                {
                    X = Convert.ToInt64(str.Substring(0, str.IndexOf(',')));
                    Y = Convert.ToInt64(str.Substring(str.IndexOf(',') + 1));
                    return new AbsolutePoint(X, Y);
                }
                catch (InvalidCastException)
                {
                    return null;
                }
            }
            return null;
        }

        /// <summary>
        /// adds two AbsolutePoint objects together
        /// </summary>
        public static AbsolutePoint operator +(AbsolutePoint a, AbsolutePoint b)
        {
            return new AbsolutePoint(a.X + b.X, a.Y + b.Y);
        }

        /// <summary>
        /// takes the difference of two AbsolutePoint objects
        /// </summary>
        public static AbsolutePoint operator -(AbsolutePoint a, AbsolutePoint b)
        {
            return new AbsolutePoint(a.X - b.X, a.Y - b.Y);
        }

        /// <summary>
        /// multiplies a AbsolutePoint object by a scalar
        /// </summary>
        public static AbsolutePoint operator *(AbsolutePoint a, double s)
        {
            return new AbsolutePoint((long)(a.X * s), (long)(a.Y * s));
        }

        /// <summary>
        /// multiplies a AbsolutePoint object by a scalar
        /// </summary>
        public static AbsolutePoint operator *(double s, AbsolutePoint a)
        {
            return new AbsolutePoint((long)(a.X * s), (long)(a.Y * s));
        }

        /// <summary>
        /// divides a AbsolutePoint object by a scalar
        /// </summary>
        public static AbsolutePoint operator /(AbsolutePoint a, double s)
        {
            return new AbsolutePoint((long)(a.X / s), (long)(a.Y / s));
        }

        /// <summary>
        /// divides a scalar by a AbsolutePoint object
        /// </summary>
        public static AbsolutePoint operator /(long s, AbsolutePoint a)
        {
            return new AbsolutePoint((long)(s / a.X), (long)(s / a.Y));
        }

        /// <summary>
        /// checks equality between two AbsolutePoint objects
        /// </summary>
        public static bool operator ==(AbsolutePoint a, AbsolutePoint b)
        {
            if (((object)a) == null && ((object)b) == null)
                return true;
            if (((object)a) == null ^ ((object)b) == null)
                return false;
            return a.X == b.X && a.Y == b.Y;
        }

        /// <summary>
        /// checks inequality between two AbsolutePoint objects
        /// </summary>
        public static bool operator !=(AbsolutePoint a, AbsolutePoint b)
        {
            if (((object)a) == null && ((object)b) == null)
                return false;
            if (((object)a) == null ^ ((object)b) == null)
                return true;
            return a.X != b.X || a.Y != b.Y;
        }
        
        /// <summary>
        /// checks equality between two AbsolutePoint objects
        /// </summary>
        public override bool Equals(object that)
        {
            try
            {
                return this.GetType() == that.GetType() && this == (AbsolutePoint)that;
            } catch (InvalidCastException)
            {
                return false;
            }
        }

        /// <summary>
        /// Generates a possible hash value for any AbsolutePoint object
        /// </summary>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    /// <summary>
    /// Polygon2D object of AbsolutePoint points
    /// Is able to interface with RenderedPoint objects through conversions to AbsolutePoints
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
            if (string.Equals(str, "vertices"))
            {
                RaisePropertyChanged("verticesr");
                update_edges = false;
                RaisePropertyChanged("edges");
            }
        }

        public ObservableCollection<RenderedPoint> verticesr
        {
            get
            {
                return Polygon2D.TrimVertices(Point2D.ApplyTransformation(vertices));
            }
        }
        public ObservableCollection<AbsolutePoint> vertices;
        private bool update_edges = true;
        private ObservableCollection<Line2D> _edges;
        public ObservableCollection<Line2D> edges
        {
            get
            {
                update_edges = update_edges || _edges == null;
                if (update_edges)
                {
                    _edges = new ObservableCollection<Line2D>();
                    for (int i = 0; i < vertices.Count; i++)
                    {
                        _edges.Add(new Line2D(vertices[i], vertices[(i + 1) % vertices.Count]));
                    }
                    update_edges = false;
                }
                return _edges;
            }
        }
        public int Count
        {
            get
            {
                return this.Size();
            }
        }
        public AbsolutePoint this[int i]
        {
            get
            {
                return new AbsolutePoint(this.vertices[i]);
            }
            set
            {
                this.vertices[i] = value;
                RaisePropertyChanged("vertices");
                RaisePropertyChanged("verticesr");
            }
        }

        /// <summary>
        /// Basic constructor
        /// </summary>
        public Polygon2D()
        {
            vertices = new ObservableCollection<AbsolutePoint>();
        }

        /// <summary>
        /// Constructs a Polygon2D from a list of Point2D objects
        /// </summary>
        public Polygon2D(IList<AbsolutePoint> list) : this()
        {
            for (int i = 0; i < list.Count; i++)
            {
                this.vertices.Add(new AbsolutePoint(list[i]));
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
        public RenderedPoint AppendPoint(RenderedPoint point)
        {
            AppendPoint(new AbsolutePoint(point));
            return point;
        }

        /// <summary>
        /// Appends a new point to the end of the polygon's vertices
        /// </summary>
        public AbsolutePoint AppendPoint(AbsolutePoint point)
        {
            vertices.Add(new AbsolutePoint(point));
            RaisePropertyChanged("vertices");
            return point;
        }

        /// <summary>
        /// Creates a new point between two existing points
        /// </summary>
        public RenderedPoint NewPoint(RenderedPoint a, RenderedPoint b)
        {
            AbsolutePoint output = NewPoint(new AbsolutePoint(a), new AbsolutePoint(b));
            if (output != null)
                return new RenderedPoint(output);
            return null;
        }

        /// <summary>
        /// Creates a new point between two existing points
        /// </summary>
        public AbsolutePoint NewPoint(AbsolutePoint a, AbsolutePoint b)
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
        /// Creates a new point on a specific edge
        /// </summary>
        public AbsolutePoint NewPoint(Line2D line)
        {
            return NewPoint(line._vertex1, line._vertex2);
        }

        /// <summary>
        /// Moves a point to another
        /// </summary>
        public RenderedPoint MovePoint(RenderedPoint old_position, RenderedPoint new_position)
        {
            AbsolutePoint output = MovePoint(new AbsolutePoint(old_position), new AbsolutePoint(new_position));
            if (output != null)
                return new RenderedPoint(output);
            return null;
        }

        /// <summary>
        /// Moves a point to another
        /// </summary>
        public AbsolutePoint MovePoint(AbsolutePoint old_position, AbsolutePoint new_position)
        {
            for (int i = 0; i < vertices.Count; i++)
            {
                if (vertices[i] == old_position)
                {
                    vertices[i] = new AbsolutePoint(new_position);
                    RaisePropertyChanged("vertices");
                    return vertices[i];
                }
            }
            return null;
        }

        /// <summary>
        /// Checks whether a given point is constrained by a polygon
        /// </summary>
        public bool PointInPolygon(RenderedPoint point)
        {
            return PointInPolygon(new AbsolutePoint(point));
        }

        /// <summary>
        /// Checks whether a given point is constrained by a polygon
        /// </summary>
        public bool PointInPolygon(AbsolutePoint point)
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
        public bool HasPoint(AbsolutePoint point)
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
        public AbsolutePoint GetCenter()
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
            AbsolutePoint abs_center = new AbsolutePoint((minX + maxX) / 2, (minY + maxY) / 2);
            AbsolutePoint avg_center = new AbsolutePoint(sumX / vertices.Count, sumY / vertices.Count);
            if (PointInPolygon(abs_center))
                return abs_center;
            else if (PointInPolygon(avg_center))
            {
                AbsolutePoint inc_center = new AbsolutePoint(abs_center);
                while (!PointInPolygon(inc_center))
                {
                    inc_center = ((9 * inc_center) + avg_center) / 10;
                }
                return inc_center;
            }
            return abs_center;
        }

        /// <summary>
        /// Square class mostly made for figuring out rendering space
        /// </summary>
        private class Square2D
        {
            private long minX;
            private long minY;
            private long maxX;
            private long maxY;
            public long Top
            {
                get
                {
                    return minY;
                }
            }
            public long Bottom
            {
                get
                {
                    return maxY;
                }
            }
            public long Left
            {
                get
                {
                    return minX;
                }
            }
            public long Right
            {
                get
                {
                    return maxX;
                }
            }
            public Point2D TopLeft
            {
                get
                {
                    return new Point2D(minX, minY);
                }
            }
            public Point2D TopRight
            {
                get
                {
                    return new Point2D(maxX, minY);
                }
            }
            public Point2D BottomRight
            {
                get
                {
                    return new Point2D(maxX, maxY);
                }
            }
            public Point2D BottomLeft
            {
                get
                {
                    return new Point2D(minX, maxY);
                }
            }

            public Square2D(Point2D p1, Point2D p2) : base()
            {
                minX = Math.Min(p1.X, p2.X);
                minY = Math.Min(p1.Y, p2.Y);
                maxX = Math.Max(p1.X, p2.X);
                maxY = Math.Max(p1.Y, p2.Y);
            }

            public static Square2D FramePolygon()
            {
                return new Square2D(new Point2D(0,0), Global.CanvasSize);
            }

            public Direction WhereIsPoint(Point2D point)
            {
                bool left = point.X < Left;
                bool right = point.X > Right;
                bool top = point.Y < Top;
                bool bottom = point.Y > Bottom;
                if (top)
                {
                    if (left)
                        return Direction.TopLeft;
                    if (right)
                        return Direction.TopRight;
                    return Direction.Top;
                }
                if (bottom)
                {
                    if (left)
                        return Direction.BottomLeft;
                    if (right)
                        return Direction.BottomRight;
                    return Direction.Bottom;
                }
                if (left)
                    return Direction.Left;
                if (right)
                    return Direction.Right;
                return Direction.Middle;
            }

            public Point2D GetCorner(Direction a)
            {
                switch (a)
                {
                    case Direction.TopLeft:
                        return TopLeft;
                    case Direction.TopRight:
                        return TopRight;
                    case Direction.BottomRight:
                        return BottomRight;
                    case Direction.BottomLeft:
                        return BottomLeft;
                    default:
                        return null;
                }
            }

            public static bool IsCorner(Direction a)
            {
                return a == Direction.TopLeft || a == Direction.TopRight || a == Direction.BottomRight || a == Direction.BottomLeft;
            }

            public static bool IsSide(Direction a)
            {
                return a == Direction.Left || a == Direction.Top || a == Direction.Right || a == Direction.Bottom;
            }

            public static Direction GetHorizontal(Direction a)
            {
                if (a == Direction.Invalid)
                    return a;
                if (Enum.GetName(typeof(Direction), a).Contains("Left"))
                    return Direction.Left;
                else if (Enum.GetName(typeof(Direction), a).Contains("Right"))
                    return Direction.Right;
                return Direction.Middle;
            }

            public static Direction GetVertical(Direction a)
            {
                if (a == Direction.Invalid)
                    return a;
                if (Enum.GetName(typeof(Direction), a).Contains("Top"))
                    return Direction.Top;
                else if (Enum.GetName(typeof(Direction), a).Contains("Bottom"))
                    return Direction.Bottom;
                return Direction.Middle;
            }

            public Point2D PinToSide(Point2D point, Point2D last)
            {
                Direction pinto = WhereIsPoint(point);
                double changed = 0.0f;
                if(GetHorizontal(pinto) != Direction.Middle) //Either left or right; trimming X
                {
                    if (pinto == Direction.Left) //Trim Left
                    {
                        changed = Math.Max(changed, ((double)Left - last.X) / ((double)point.X - last.X));
                    }
                    else //Trim Right
                    {
                        changed = Math.Max(changed, ((double)Right - last.X) / ((double)point.X - last.X));
                    }
                }
                if(GetVertical(pinto) != Direction.Middle) //Either top or bottom; trimming Y
                {
                    if(pinto == Direction.Top) //Trim Top
                    {
                        changed = Math.Max(changed, ((double)Top - last.Y) / ((double)point.Y - last.Y));
                    }
                    else //Trim Bottom
                    {
                        changed = Math.Max(changed, ((double)Bottom - last.Y) / ((double)point.Y - last.Y));
                    }
                }
                Point2D output = ((point - last) * changed) + last;
                return output;
            }

            public static bool SharesSide(Direction a, Direction b)
            {
                return (GetVertical(a) == GetVertical(b) && GetVertical(a) != Direction.Middle) || (GetHorizontal(a) == GetHorizontal(b) && GetHorizontal(a) != Direction.Middle);
            }

            public static bool NeedsAdd(Direction a, Direction b)
            {
                if (a == b)
                    return false;
                if (b == Direction.Middle)
                    return false;
                if (a == Direction.Middle)
                    return true;
                if(SharesSide(a, b))
                {
                    return IsCorner(b); //Either b is a side and a is a corner, in which case they don't need new; or a is a side and b is a corner, in which case they do need new
                }
                return true;
            }
        }
        
        /// <summary>
        /// Returns the list of RenderedPoints only containing those within frame
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static ObservableCollection<RenderedPoint> TrimVertices(ObservableCollection<RenderedPoint> list)
        {
            ObservableCollection<RenderedPoint> output = new ObservableCollection<RenderedPoint>();
            Direction last_dir = Direction.Invalid;
            Direction current_dir;
            int first_in = 0;
            bool found_first_in = false;
            Point2D last_added = null;
            for(int i=0; i<list.Count; i++)
            {
                current_dir = Square2D.FramePolygon().WhereIsPoint(list[i%list.Count]);
                if (current_dir == Direction.Middle)
                {
                    if (!found_first_in)
                    {
                        first_in = i;
                        found_first_in = true;
                    }
                    if(last_dir != Direction.Invalid && last_dir != Direction.Middle)
                    {
                        Point2D temp;
                        temp = Square2D.FramePolygon().PinToSide(last_added, list[i % list.Count]);
                        output.Add(new RenderedPoint(temp.X, temp.Y));
                    }
                    if (i < list.Count)
                    {
                        output.Add(list[i % list.Count]);
                        last_added = list[i % list.Count];
                    }
                    last_dir = current_dir;
                }
                else if(last_dir != Direction.Invalid && last_dir != current_dir)
                {
                    if(Square2D.NeedsAdd(last_dir, current_dir))
                    {
                        Point2D temp;
                        temp = Square2D.FramePolygon().PinToSide(list[i % list.Count], last_added);
                        output.Add(new RenderedPoint(temp.X, temp.Y));
                        last_added = list[i % list.Count];
                        last_dir = current_dir;
                    }
                }
            }
            if (found_first_in)
            {
                for(int i=0; i<first_in; i++)
                {
                    current_dir = Square2D.FramePolygon().WhereIsPoint(list[i % list.Count]);
                    if (current_dir == Direction.Middle)
                    {
                        if (last_dir != Direction.Invalid && last_dir != Direction.Middle)
                        {
                            Point2D temp;
                            temp = Square2D.FramePolygon().PinToSide(last_added, list[i % list.Count]);
                            output.Add(new RenderedPoint(temp.X, temp.Y));
                        }
                        last_dir = current_dir;
                        break;
                    }
                    else if (last_dir != Direction.Invalid && last_dir != current_dir)
                    {
                        if (Square2D.NeedsAdd(last_dir, current_dir))
                        {
                            Point2D temp;
                            temp = Square2D.FramePolygon().PinToSide(list[i % list.Count], last_added);
                            output.Add(new RenderedPoint(temp.X, temp.Y));
                            last_added = list[i % list.Count];
                            last_dir = current_dir;
                        }
                    }
                }
            }
            return output;
        }
    }

    /// <summary>
    /// Line2D object of AbsolutePoint points
    /// Is able to output some RenderedPoint objects
    /// </summary>
    public class Line2D : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string str)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(str));
            }
            if (str.Contains("vertex")) //Raises changed property for other elements on behalf of vertex
            {
                try
                {
                    int number = Int32.Parse(str.Substring("vertex".Length).Trim());
                    RaisePropertyChanged("X" + number.ToString());
                    RaisePropertyChanged("Y" + number.ToString());
                } catch (FormatException)
                {
                    ;
                }
                RaisePropertyChanged("center_r");
            }
        }
        private AbsolutePoint __vertex1;
        private AbsolutePoint __vertex2;
        public AbsolutePoint _vertex1
        {
            get
            {
                return __vertex1;
            }
            set
            {
                __vertex1 = value;
                RaisePropertyChanged("vertex1");
            }
        }
        public AbsolutePoint _vertex2
        {
            get
            {
                return __vertex2;
            }
            set
            {
                __vertex2 = value;
                RaisePropertyChanged("vertex2");
            }
        }
        public RenderedPoint vertex1
        {
            get
            {
                return _vertex1.ToRenderedPoint();
            }
            set
            {
                _vertex1 = value.ToAbsolutePoint();
            }
        }
        public RenderedPoint vertex2
        {
            get
            {
                return _vertex2.ToRenderedPoint();
            }
            set
            {
                _vertex2 = value.ToAbsolutePoint();
            }
        }
        public AbsolutePoint center_a
        {
            get
            {
                return (_vertex1 + _vertex2) / 2;
            }
        }
        public RenderedPoint center_r
        {
            get
            {
                return (vertex1 + vertex2) / 2;
            }
        }
        public double Length
        {
            get
            {
                return (_vertex1 - _vertex2).Length();
            }
        }

        public string X1
        {
            get
            {
                return vertex1.X.ToString();
            }
        }
        public string Y1
        {
            get
            {
                return vertex1.Y.ToString();
            }
        }
        public string X2
        {
            get
            {
                return vertex2.X.ToString();
            }
        }
        public string Y2
        {
            get
            {
                return vertex2.Y.ToString();
            }
        }

        /// <summary>
        /// Creates a Line2D object from two AbsolutePoint objects
        /// </summary>
        /// <param name="v1">An AbsolutePoint object</param>
        /// <param name="v2">An AbsolutePoint object</param>
        public Line2D(AbsolutePoint v1, AbsolutePoint v2)
        {
            this._vertex1 = new AbsolutePoint(v1);
            this._vertex2 = new AbsolutePoint(v2);
        }

        /// <summary>
        /// Creates a Line2D object from two RenderedPoint objects
        /// </summary>
        /// <param name="v1">A RenderedPoint object</param>
        /// <param name="v2">A RenderedPoint object</param>
        public Line2D(RenderedPoint v1, RenderedPoint v2) : this(v1.ToAbsolutePoint(), v2.ToAbsolutePoint())
        {
            ;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="o">Another Line2D object</param>
        public Line2D(Line2D o) : this(o._vertex1, o._vertex2)
        {
            ;
        }

        /// <summary>
        /// Sets vertex1 to the passed point
        /// </summary>
        /// <param name="point">An AbsolutePoint object</param>
        public AbsolutePoint MoveVertex1(AbsolutePoint point)
        {
            _vertex1 = new AbsolutePoint(point);
            return point;
        }

        /// <summary>
        /// Sets vertex2 to the passed point
        /// </summary>
        /// <param name="point">An AbsolutePoint object</param>
        public AbsolutePoint MoveVertex2(AbsolutePoint point)
        {
            _vertex2 = new AbsolutePoint(point);
            return point;
        }

        /// <summary>
        /// Returns a line of Length==1 in the same direction as this
        /// </summary>
        public Line2D Normalized()
        {
            Line2D output = new Line2D(this);
            output._vertex1 = new AbsolutePoint(0, 0);
            output._vertex2 = (output._vertex2 / this.Length);
            return output;
        }

        /// <summary>
        /// Returns a line of the same length and direction but with _vertex1 set to (0,0)
        /// </summary>
        protected Line2D Centered()
        {
            return new Line2D(_vertex1, _vertex2 - _vertex1);
        }

        public static bool operator==(Line2D a, Line2D b)
        {
            if (((object)a) == null && ((object)b) == null)
                return true;
            if (((object)a) == null ^ ((object)b) == null)
                return false;
            return (a._vertex1 == b._vertex1 && a._vertex2 == b._vertex2) || (a._vertex1 == b._vertex2 && a._vertex1 == b._vertex1);
        }

        public static bool operator!=(Line2D a, Line2D b)
        {
            if (((object)a) == null && ((object)b) == null)
                return false;
            if (((object)a) == null ^ ((object)b) == null)
                return true;
            return (a._vertex1 != b._vertex1 && a._vertex1 != b._vertex2) || (a._vertex2 != b._vertex1 && a._vertex2 != b._vertex2);
        }

        public static Line2D operator+(Line2D a, Line2D b)
        {
            return new Line2D(a._vertex1, a._vertex2 + b.Centered()._vertex2);
        }

        public static Line2D operator-(Line2D a, Line2D b)
        {
            return new Line2D(a._vertex1, a._vertex2 - b.Centered()._vertex2);
        }
        
        /// <summary>
        /// checks equality between two objects
        /// </summary>
        public override bool Equals(object that)
        {
            try
            {
                return this.GetType() == that.GetType() && this == (Line2D)that;
            }
            catch (InvalidCastException)
            {
                return false;
            }
        }

        /// <summary>
        /// Generates a possible hash value for any Line2D object
        /// </summary>
        public override int GetHashCode()
        {
            return _vertex1.GetHashCode() ^ _vertex2.GetHashCode();
        }

        /// <summary>
        /// Takes an IList of Line2D objects and a Line2D object and returns true if the list contains the Line
        /// </summary>
        /// <param name="list">The list within in which to search for the line</param>
        /// <param name="line">The line to search for within the list</param>
        /// <returns>true if the list contains the line; false otherwise</returns>
        public static bool Contains(IList<Line2D> list, Line2D line)
        {
            if (list == null)
                return false;
            if (line == null)
                return true;
            for(int i=0; i<list.Count; i++)
            {
                if (list[i] == line)
                    return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Class that stores AbsolutePoints but is accessed as RenderedPoints
    /// </summary>
    public class MyPointCollection : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string str)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(str));
            }
        }

        private ObservableCollection<AbsolutePoint> _points;
        public ObservableCollection<RenderedPoint> points
        {
            get
            {
                return Point2D.ApplyTransformation(_points);
            }
        }

        public MyPointCollection()
        {
            this._points = new ObservableCollection<AbsolutePoint>();
        }

        public AbsolutePoint AppendPoint(AbsolutePoint point)
        {
            if (this._points == null)
                this._points = new ObservableCollection<AbsolutePoint>();
            this._points.Add(new AbsolutePoint(point));
            RaisePropertyChanged("points");
            return point;
        }

        public void ForceUpdatePoints()
        {
            RaisePropertyChanged("points");
        }

        public bool Contains(AbsolutePoint point)
        {
            if (point == null)
                return true;
            for (int i=0; i<_points.Count; i++)
            {
                if (point == _points[i])
                    return true;
            }
            return false;
        }
    }
}
