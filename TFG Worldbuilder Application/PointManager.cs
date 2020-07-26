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
        public virtual long X
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
        public virtual long Y
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
        private string _color;
        public string color
        {
            get
            {
                return _color;
            }
            set
            {
                if (!string.Equals(value, "#F2F2F2"))
                    _color = value;
                _color = value;
                RaisePropertyChanged("color");
            }
        }

        /// <summary>
        /// creates a Point2D object
        /// </summary>
        public Point2D(long X, long Y)
        {
            this.X = X;
            this.Y = Y;
            color = "#F2F2F2";
        }

        /// <summary>
        /// copy constructor
        /// </summary>
        public Point2D(Point2D o) : this(o.X, o.Y)
        {
            this.color = o.color;
        }

        /// <summary>
        /// Converts a Windows Foundation Point into a Point2D
        /// </summary>
        public Point2D(Point o) : this((long)o.X, (long)o.Y)
        {
            ;
        }
        
        /// <summary>
        /// Converts an AbsolutePoint to a RenderedPoint
        /// </summary>
        public static RenderedPoint ConvertPoint(AbsolutePoint point)
        {
            return point.ToRenderedPoint();
        }

        /// <summary>
        /// Converts a RenderedPoint to an AbsolutePoint
        /// </summary>
        public static AbsolutePoint ConvertPoint(RenderedPoint point)
        {
            return point.ToAbsolutePoint();
        }

        /// <summary>
        /// Converts an ObservableCollection of AbsolutePoints to an ObservableCollection of RenderedPoints
        /// </summary>
        public static ObservableCollection<RenderedPoint> ConvertCollection(ObservableCollection<AbsolutePoint> list)
        {
            return AbsolutePoint.ToRenderedPoints(list);
        }

        /// <summary>
        /// Converts an ObservableCollection of RenderedPoints to an ObservableCollection of AbsolutePoints
        /// </summary>
        public static ObservableCollection<AbsolutePoint> ConvertCollection(ObservableCollection<RenderedPoint> list)
        {
            return RenderedPoint.ToAbsolutePoints(list);
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
        /// Returns the distance between this point and the passed point
        /// </summary>
        public long Distance(Point2D o)
        {
            return (this - o).Length();
        }

        /// <summary>
        /// Returns the exact between this point and the passed point
        /// </summary>
        public double TrueDistance(Point2D o)
        {
            long dx = o.X - X;
            long dy = o.Y - Y;
            return Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2));
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
        public override long X
        {
            get
            {
                return base.X;
            }
            set
            {
                base.X = value;
                RaisePropertyChanged("visibility");
            }
        }
        public override long Y
        {
            get
            {
                return base.Y;
            }
            set
            {
                base.Y = value;
                RaisePropertyChanged("visibility");
            }
        }

        public override Point pointstr
        {
            get
            {
                return Point2D.ToWindowsPoint(this);
            }
        }
        
        public string visibility
        {
            get
            {
                if (this.InFrame())
                    return "Visible";
                return "Collapsed";
            }
        }

        public RenderedPoint(long X, long Y) : base(X, Y)
        {
            ;
        }

        public RenderedPoint(RenderedPoint o) : base((Point2D)o)
        {
            this.color = o.color;
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
            this.color = o.color;
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

        /// <summary>
        /// Converts an ObservableCollection of RenderedPoints to an ObservableCollection of AbsolutePoints
        /// </summary>
        public static ObservableCollection<AbsolutePoint> ToAbsolutePoints(ObservableCollection<RenderedPoint> list)
        {
            ObservableCollection<AbsolutePoint> output = new ObservableCollection<AbsolutePoint>();
            for (int i = 0; i < list.Count; i++)
            {
                output.Add(list[i].ToAbsolutePoint());
            }
            return output;
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
        /// Checks whether this point can snap to another point, within the given range
        /// </summary>
        /// <param name="o">The point to snap to</param>
        /// <param name="snaprange">The range within which to snap</param>
        public bool SnapsTo(RenderedPoint o, long snaprange)
        {
            return this.Distance(o) <= snaprange;
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
        /// Returns the distance between this point and the passed point
        /// </summary>
        public long Distance(RenderedPoint o)
        {
            return (this - o).Length();
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
        
        /// <summary>
        /// Checks whether a given RenderedPoint in within the frame
        /// </summary>
        public static bool PointInFrame(RenderedPoint point)
        {
            return Polygon2D.Square2D.FramePolygon().WhereIsPoint(point) == Direction.Middle;
        }

        /// <summary>
        /// Checks whether a given RenderedPoint in within the frame
        /// </summary>
        public bool InFrame()
        {
            return PointInFrame(this);
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
            this.color = o.color;
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
            this.color = o.color;
        }

        public static explicit operator RenderedPoint(AbsolutePoint point)
        {
            return new RenderedPoint(point);
        }
        
        public RenderedPoint ToRenderedPoint()
        {
            return new RenderedPoint(this);
        }

        /// <summary>
        /// Converts an ObservableCollection of AbsolutePoints to an ObservableCollection of RenderedPoints
        /// </summary>
        public static ObservableCollection<RenderedPoint> ToRenderedPoints(ObservableCollection<AbsolutePoint> list)
        {
            ObservableCollection<RenderedPoint> output = new ObservableCollection<RenderedPoint>();
            for(int i=0; i<list.Count; i++)
            {
                output.Add(list[i].ToRenderedPoint());
            }
            return output;
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
        /// Returns the distance between this point and the passed point
        /// </summary>
        public long Distance(AbsolutePoint o)
        {
            return (this - o).Length();
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
                update_edges = true;
                RaisePropertyChanged("untrimmedvertices");
                RaisePropertyChanged("verticesr");
                RaisePropertyChanged("edges");
                RaisePropertyChanged("visibility");
            }
        }

        public ObservableCollection<RenderedPoint> untrimmedvertices
        {
            get
            {
                return Point2D.ApplyTransformation(vertices);
            }
        }
        public ObservableCollection<RenderedPoint> verticesr
        {
            get
            {
                return Polygon2D.TrimVertices(untrimmedvertices);
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
        /// string object used to set object visibility
        /// </summary>
        public string visibility
        {
            get
            {
                if (verticesr.Count > 0)
                {
                    return "Visible";
                }
                return "Collapsed";
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
        /// Forces an update to all point objects
        /// </summary>
        public void ForceUpdatePoints()
        {
            RaisePropertyChanged("vertices");
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
        /// Removes the last point from the polygon's vertices and returns it
        /// </summary>
        public AbsolutePoint RemovePoint()
        {
            if (vertices.Count <= 0)
                return null;
            AbsolutePoint output = new AbsolutePoint(vertices.Last());
            vertices.RemoveAt(vertices.Count - 1);
            RaisePropertyChanged("vertices");
            return output;
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
        /// Checks whether a given point is On any of the edges of the polygon
        /// </summary>
        public bool PointOnPolygon(AbsolutePoint point)
        {
            for(int i=0; i<edges.Count; i++)
            {
                if (edges[i].On(point))
                    return true;
            }
            return false;
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
        /// Checks whether two polygons intersect by checking their vertices and edges
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public bool IntersectsPolygon(Polygon2D o)
        {
            for(int i=0; i<vertices.Count; i++)
            {
                if (o.PointInPolygon(vertices[i]) && !o.PointOnPolygon(vertices[i]))
                    return true;
            }
            for(int i=0; i<o.vertices.Count; i++)
            {
                if (PointInPolygon(o.vertices[i]) && !PointOnPolygon(o.vertices[i]))
                    return true;
            }
            for(int i= 0; i<edges.Count; i++)
            {
                if (o.PointInPolygon(edges[i].center_a) && !o.PointOnPolygon(edges[i].center_a))
                    return true;
            }
            for (int i = 0; i < o.edges.Count; i++)
            {
                if (PointInPolygon(o.edges[i].center_a) && !PointOnPolygon(o.edges[i].center_a))
                    return true;
            }
            for (int i=0; i<edges.Count; i++)
            {
                for(int j=0; j<o.edges.Count; j++)
                {
                    if (!edges[i].SharesVertex(o.edges[j]) && !edges[i].IsParallel(o.edges[j]))
                    {
                        if (edges[i].Intersects(o.edges[j]))
                            return true;
                    }
                }
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
        public class Square2D
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

            public Point2D SquishToSide(Point2D point)
            {
                return new Point2D(Math.Max(Math.Min(point.X, Right), Left), Math.Max(Math.Min(point.Y, Bottom), Top));
            }

            public Point2D PinToSide(Point2D point, Point2D last)
            {
                if (point == last)
                    return null;
                Direction pinto = WhereIsPoint(point);
                if (pinto == Direction.Middle)
                    return point;
                if (WhereIsPoint(last) != Direction.Middle)
                    return SquishToSide(point);
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
            RenderedPoint last_added = null; //The untrimmed version of the most recently-added point to output
            for(int i=0; i<list.Count && !found_first_in; i++)
            {
                current_dir = Square2D.FramePolygon().WhereIsPoint(list[i % list.Count]);
                if (current_dir == Direction.Middle)
                {
                    found_first_in = true;
                    first_in = i;
                    output.Add(list[i]);
                    last_added = list[i];
                    last_dir = current_dir;
                    break;
                }
            }
            if (!found_first_in)
                return output;
            for(int i=(first_in + 1)%list.Count; i!=first_in; i = (i + 1) % list.Count) //Main loop; iterates through the list starting after first_in until it loops back around and reaches first_in
            {
                current_dir = Square2D.FramePolygon().WhereIsPoint(list[i % list.Count]);
                if (current_dir == Direction.Middle) //This point is within frame, and will be added
                {
                    if(last_dir != Direction.Middle) //The previous point was not in frame, and the intersection between that and the frame needs to be added
                    {
                        Point2D temp = Square2D.FramePolygon().PinToSide(last_added, list[i]);
                        if (temp != null)
                            output.Add(new RenderedPoint(temp.X, temp.Y));
                    }
                    output.Add(list[i]);
                    last_added = list[i];
                }
                else //This point is outside of frame, and it needs to be squished to the side if the last one was also out of frame, or the intersection between it and the last point with the frame needs to be added
                {
                    Point2D temp = Square2D.FramePolygon().PinToSide(list[i], last_added);
                    if (temp != null)
                        output.Add(new RenderedPoint(temp.X, temp.Y));
                    last_added = list[i];
                }
                last_dir = current_dir;
            }
            if (last_dir != Direction.Middle) //The previous point was not in frame, and the intersection between that and the frame needs to be added
            {
                Point2D temp = Square2D.FramePolygon().PinToSide(last_added, list[first_in]);
                if (temp != null)
                    output.Add(new RenderedPoint(temp.X, temp.Y));
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
                RaisePropertyChanged("visibility");
            }
        }
        private string _color = "Black";
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
        public string visibility
        {
            get
            {
                if (vertex1.InFrame() || vertex2.InFrame() || IntersectsFrame())
                    return "Visible";
                return "Collapsed";
            }
        }
        public double Length
        {
            get
            {
                return _vertex1.TrueDistance(_vertex2);
            }
        }
        public bool vertical
        {
            get
            {
                return dx == 0;
            }
        }
        public bool horizontal
        {
            get
            {
                return dy == 0;
            }
        }
        /// <summary>
        /// X such that this line would reach (X,0) if extended
        /// </summary>
        public double x_intercept
        {
            get
            {
                if (horizontal)
                    return 0;
                return _vertex1.X - _vertex1.Y * slope_y;
            }
        }
        /// <summary>
        /// Y such that this line would reach (0,Y) if extended
        /// </summary>
        public double y_intercept
        {
            get
            {
                if (vertical)
                    return 0;
                return _vertex1.Y - _vertex1.X * slope_x;
            }
        }
        /// <summary>
        /// The rise/run slope of the given line; multiply by change in X to get the change in Y
        /// </summary>
        public double slope_x
        {
            get
            {
                if (vertical)
                    return 0;
                return ((double)dy) / ((double)dx);
            }
        }
        /// <summary>
        /// The run/rise slope of the given line; multiply by change in Y to get the change in X
        /// </summary>
        public double slope_y
        {
            get
            {
                if (horizontal)
                    return 0;
                return ((double)dx) / ((double)dy);
            }
        }
        private long dx
        {
            get
            {
                return _vertex2.X - _vertex1.X;
            }
        }
        private long dy
        {
            get
            {
                return _vertex2.Y - _vertex1.Y;
            }
        }
        /// <summary>
        /// This line presented in the form: AX + BY = C
        /// </summary>
        public double A
        {
            get
            {
                return dy;
            }
        }
        /// <summary>
        /// This line presented in the form: AX + BY = C
        /// </summary>
        public double B
        {
            get
            {
                return dx;
            }
        }
        /// <summary>
        /// This line presented in the form: AX + BY = C
        /// </summary>
        public double C
        {
            get
            {
                return A * _vertex1.X + B * _vertex1.Y;
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
            if (v1 == v2)
                throw new ArgumentException("Line2D vertices may not be the same");
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

        public override string ToString()
        {
            return '(' + vertex1.ToString() + ',' + vertex2.ToString() + ')';
        }
        public void ForceUpdatePoints()
        {
            RaisePropertyChanged("vertex1");
            RaisePropertyChanged("vertex2");
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

        public bool SharesVertex(Line2D o)
        {
            if (_vertex1 == o._vertex1 || _vertex1 == o._vertex2)
                return true;
            if (_vertex2 == o._vertex1 || _vertex2 == o._vertex2)
                return true;
            return false;
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

        public static Line2D operator+(Line2D a, AbsolutePoint b)
        {
            return new Line2D(a._vertex1 + b, a._vertex2 + b);
        }

        public static Line2D operator-(Line2D a, Line2D b)
        {
            return new Line2D(a._vertex1, a._vertex2 - b.Centered()._vertex2);
        }

        public static Line2D operator-(Line2D a, AbsolutePoint b)
        {
            return new Line2D(a._vertex1 - b, a._vertex2 - b);
        }

        /// <summary>
        /// Returns true if the line intersects the render frame
        /// </summary>
        private bool IntersectsFrame()
        {
            AbsolutePoint Top_Left = (new RenderedPoint(0, 0)).ToAbsolutePoint();
            AbsolutePoint Top_Right = (new RenderedPoint(Global.CanvasSize.X, 0)).ToAbsolutePoint();
            AbsolutePoint Bottom_Right = Global.CanvasSize.ToAbsolutePoint();
            AbsolutePoint Bottom_Left = (new RenderedPoint(0, Global.CanvasSize.Y)).ToAbsolutePoint();
            if (Intersects(new Line2D(Top_Left, Top_Right)))
                return true;
            if (Intersects(new Line2D(Top_Right, Bottom_Right)))
                return true;
            if (Intersects(new Line2D(Bottom_Right, Bottom_Left)))
                return true;
            if (Intersects(new Line2D(Bottom_Left, Top_Left)))
                return true;
            return false;
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

        /// <summary>
        /// Creates a reversed version of the current line
        /// </summary>
        public Line2D Reverse()
        {
            return new Line2D(_vertex2, _vertex1);
        }

        /// <summary>
        /// Checks whether a point would fall on the line
        /// </summary>
        /// <param name="point">Point to check</param>
        public bool On(AbsolutePoint point)
        {
            return OnExtended(point) && point.X >= Math.Min(_vertex1.X, _vertex2.X) && point.X <= Math.Max(_vertex1.X, _vertex2.X) && point.Y >= Math.Min(_vertex1.Y, _vertex2.Y) && point.Y <= Math.Max(_vertex1.Y, _vertex2.Y);
        }

        /// <summary>
        /// Checks whether a point would fall on the line if it were extended infinitely
        /// </summary>
        /// <param name="point">Point to check</param>
        public bool OnExtended(AbsolutePoint point)
        {
            if (vertical)
            {
                return point.X == _vertex1.X;
            }
            if (horizontal)
            {
                return point.Y == _vertex1.Y;
            }
            return Math.Abs((point.X * slope_x + y_intercept)-point.Y) < 1;
        }

        /// <summary>
        /// Checks whether a point is above the line
        /// </summary>
        public bool Above(AbsolutePoint point)
        {
            if (vertical)
                return false;
            return point.Y < point.X * slope_x + y_intercept;
        }

        /// <summary>
        /// Checks whether a point is below the line
        /// </summary>
        public bool Below(AbsolutePoint point)
        {
            if (vertical)
                return false;
            return point.Y > point.X * slope_x + y_intercept;
        }

        /// <summary>
        /// Checks whether a point is left of the line
        /// </summary>
        public bool Left(AbsolutePoint point)
        {
            if (horizontal)
                return false;
            return point.X < point.Y * slope_y + x_intercept;
        }

        /// <summary>
        /// Checks whether a point is right of the line
        /// </summary>
        public bool Right(AbsolutePoint point)
        {
            if (horizontal)
                return false;
            return point.X > point.Y * slope_y + x_intercept;
        }
        
        /// <summary>
        /// Creates and returns a point on the line at the specified percent from vertex1
        /// </summary>
        /// <param name="percent"></param>
        public AbsolutePoint Get(double percent)
        {
            return _vertex1 + ((_vertex2 - _vertex1) * percent);
        }

        /// <summary>
        /// Returns true if the provided line is parallel
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public bool IsPerpendicular(Line2D line)
        {
            if (this.vertical)
                return line.horizontal;
            if (this.horizontal)
                return line.vertical;
            if (line.vertical)
                return horizontal;
            if (line.horizontal)
                return vertical;
            return this.slope_x * line.slope_x == 1;
        }

        /// <summary>
        /// Returns true if the provided line is parallel
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public bool IsParallel(Line2D line)
        {
            if (vertical)
                return line.vertical;
            return Math.Abs(slope_x - line.slope_x) < 0.01;
        }

        /// <summary>
        /// Returns true if the provided line intersects this line
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public bool Intersects(Line2D line)
        {
            if (IsParallel(line))
                return false;
            AbsolutePoint intersection = Intersection(line);
            return On(intersection) && line.On(intersection);
        }

        /// <summary>
        /// Returns the point of intersection between two lines
        /// Will throw an ArgumentException on parallel lines
        /// </summary>
        /// <param name="line">A non-parallel line</param>
        /// <returns></returns>
        public AbsolutePoint Intersection(Line2D line)
        {
            double delta = this.A * line.B - line.A * this.B;
            if (delta == 0)
                throw new ArgumentException("Lines are parallel");
            AbsolutePoint output = new AbsolutePoint((long)((line.B * this.C - this.B * line.C) / delta + .5), (long)((this.A * line.C - line.A * this.C) / delta + .5));
            output.Y = (long) (output.X * slope_x + y_intercept + 0.5);
            return output;
        }

        /// <summary>
        /// Returns the slope of a perpendicular line, if possible
        /// </summary>
        public double PerpendicularSlope()
        {
            if (horizontal)
                throw new DivideByZeroException("Slope does not exist for vertical lines");
            if (vertical)
                return 0;
            if (dy < 0 && dx < 0 && (slope_x < 0 || slope_y < 0))
                ;
            return -1 / slope_x;
        }

        public double TrueDistance(AbsolutePoint point)
        {
            if (On(point))
                return 0;
            return point.TrueDistance(GetClosestPoint(point));
        }

        public long RenderedDistance(RenderedPoint point)
        {
            if (On(point.ToAbsolutePoint()))
                return 0;
            return point.Distance(GetClosestPoint(point.ToAbsolutePoint()).ToRenderedPoint());
        }

        public long Distance(AbsolutePoint point)
        {
            if (On(point))
                return 0;
            return point.Distance(GetClosestPoint(point));
        }

        /// <summary>
        /// Gets and returns the closest point on this line to the presented point
        /// </summary>
        public AbsolutePoint GetClosestPoint(AbsolutePoint point)
        {
            if (On(point))
                return point;
            if (vertical)
            {
                if (_vertex1.Y > _vertex2.Y)
                    return this.Reverse().GetClosestPoint(point);
                if (point.Y > _vertex2.Y)
                    return _vertex2;
                if (point.Y < _vertex1.Y)
                    return _vertex1;
                return new AbsolutePoint(_vertex1.X, point.Y);
            }
            if (horizontal)
            {
                if (_vertex1.X > _vertex2.X)
                    return this.Reverse().GetClosestPoint(point);
                if (point.X > _vertex2.X)
                    return _vertex2;
                if (point.X < _vertex1.X)
                    return _vertex1;
                return new AbsolutePoint(point.X, _vertex1.Y);
            }
            AbsolutePoint v1, v2;
            if(_vertex1.X > _vertex2.X)
            {
                v1 = _vertex2;
                v2 = _vertex1;
            }else
            {
                v1 = _vertex1;
                v2 = _vertex2;
            }
            AbsolutePoint v1_2, v2_2;
            if(Math.Abs(slope_x) > Math.Abs(slope_y)) //The line is more vertical than horizontal; change the y by 1, and the x by slope_y
            {
                v1_2 = new AbsolutePoint((long)(v1.X + .5 + Math.Abs(slope_y)), v1.Y + 1); //This should be just right of the leftmost point
                v2_2 = new AbsolutePoint((long)(v2.X + .5 - Math.Abs(slope_y)), v2.Y - 1); //This should be just left of the rightmost point
            }
            else //The line is as or more horizontal than vertical; change the x by 1, and the y by slope_x
            {
                v1_2 = new AbsolutePoint(v1.X + 1, (long)(v1.Y + .5 + Math.Abs(slope_x))); //This should be just right of the leftmost point
                v2_2 = new AbsolutePoint(v2.X - 1, (long)(v2.Y + .5 - Math.Abs(slope_x))); //This should be just left of the rightmost point
            }
            if (point.TrueDistance(v1) < point.TrueDistance(v1_2)) //This checks whether the leftmost point is closer than the next leftmost point
                return v1;
            if (point.TrueDistance(v2) < point.TrueDistance(v2_2)) //This checks whether the rightmost point is closer than the next rightmost point
                return v2;
            return Intersection(CreatePerpendicularLine(point));
        }

        /// <summary>
        /// Creates a line perpendicular to the current line, intersecting it if possible
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public Line2D CreatePerpendicularLine(AbsolutePoint v1)
        {
            if (On(v1))
                throw new ArgumentException("Cannot create perpendicular line from a point on the current line");
            AbsolutePoint v2;
            if (vertical)
            {
                v2 = new AbsolutePoint(2 * _vertex1.X - v1.X, v1.Y);
                return new Line2D(v1, v2);
            }
            if (horizontal)
            {
                v2 = new AbsolutePoint(v1.X, 2 * _vertex1.Y - v1.Y);
                return new Line2D(v1, v2);
            }
            v2 = new AbsolutePoint(v1);
            double slope = PerpendicularSlope();
            if(Above(v1) ^ slope > 0)
            {
                //Case 1: Above(v1) && slope < 0 ==> v2.Y - n*slope is Below (Opposite!)
                //Case 2: Below(v1) && slope > 0 ==> v2.Y - n*slope is Above (Opposite!)
                int i = 1;
                do
                {
                    if(Math.Abs(slope) > 20)
                        v2 = v1 + new AbsolutePoint(i * i, (long)(i * i * -1 * slope));
                    else
                        v2 = v1 + new AbsolutePoint(i * i * 100, (long)(i * i * -100 * slope));
                    i++;
                } while (Above(v1) == Above(v2));
            } else
            {
                //Case 1: Above(v1) && slope > 0 ==> v2.Y + n*slope is Below (Opposite!)
                //Case 2: Below(v1) && slope < 0 ==> v2.Y + n*slope is Above (Opposite!)
                int i = 1;
                do
                {
                    if (Math.Abs(slope) > 20)
                        v2 = v1 + new AbsolutePoint(i * i, (long)(i * i * slope));
                    else
                        v2 = v1 + new AbsolutePoint(i * i * 100, (long)(i * i * 100 * slope));
                    i++;
                } while (Above(v1) == Above(v2));
            }
            if (Intersects(new Line2D(v1, v2)))
            {
                v2 = Intersection(new Line2D(v1, v2));
                v2 = 2 * v2 - v1;
            }
            return new Line2D(v1, v2);
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

            if (string.Equals(str, "points"))
            {
                RaisePropertyChanged("_points");
            }
            if (PropertyChanged != null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(str));
            }
        }
        private string _base_color = "#F2F2F2";
        public string base_color
        {
            get
            {
                return _base_color;
            }
            set
            {
                ReplaceColors(value, _base_color);
                _base_color = value;
                RaisePropertyChanged("base_color");
            }
        }

        public int Count
        {
            get
            {
                return _points.Count;
            }
        }

        public ObservableCollection<AbsolutePoint> _points;

        public ObservableCollection<RenderedPoint> points
        {
            get
            {
                return Polygon2D.TrimVertices(AbsolutePoint.ToRenderedPoints(_points));
            }
        }

        private void ReplaceColors(string new_color, string old_color)
        {
            for(int i=0; i<_points.Count; i++)
            {
                if (string.Equals(_points[i].color, old_color))
                    _points[i].color = new_color;
            }
        }

        public MyPointCollection()
        {
            this._points = new ObservableCollection<AbsolutePoint>();
        }

        /// <summary>
        /// Appends a point to the end of the list
        /// </summary>
        /// <param name="point">The point to add</param>
        /// <returns>The added point</returns>
        public AbsolutePoint AppendPoint(AbsolutePoint point)
        {
            if (this._points == null)
                this._points = new ObservableCollection<AbsolutePoint>();
            this._points.Add(new AbsolutePoint(point));
            RaisePropertyChanged("points");
            return point;
        }

        /// <summary>
        /// Removes the last points in the list
        /// </summary>
        /// <returns>The removed points, or null</returns>
        public AbsolutePoint RemovePoint()
        {
            if (this._points == null || this._points.Count <= 0)
                return null;
            AbsolutePoint output = new AbsolutePoint(this._points.Last());
            this._points.RemoveAt(this._points.Count - 1);
            RaisePropertyChanged("points");
            return output;
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
