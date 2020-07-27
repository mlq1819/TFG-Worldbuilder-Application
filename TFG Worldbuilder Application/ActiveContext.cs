﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Windows.Foundation;
using Windows.ApplicationModel.Contacts;

namespace TFG_Worldbuilder_Application
{

    /// <summary>
    /// DataContext bindable object for convenience sake; manages Active information for the level
    /// </summary>
    public class ActiveContext : INotifyPropertyChanged
    {
        public ObservableCollection<Level1> Worlds;
        public SuperLevel ActiveLevel;
        public bool HasActive
        {
            get
            {
                return SelectedLevel != null || SelectedPoint >= 0 || SelectedLine >= 0;
            }
        }
        private SuperLevel _selectedlevel = null;
        public SuperLevel SelectedLevel
        {
            get
            {
                return _selectedlevel;
            }
            set
            {
                _selectedlevel = value;
                RaisePropertyChanged("SelectedLevel");
            }
        }
        private int _selectedpoint = -1;
        public int SelectedPoint
        {
            get
            {
                return _selectedpoint;
            }
            set
            {
                _selectedpoint = value;
                RaisePropertyChanged("SelectedPoint");
                RaisePropertyChanged("CurrentPoint");
            }
        }
        private int _selectedline = -1;
        public RenderedPoint CurrentPoint
        {
            get
            {
                if(SelectedPoint >= 0)
                {
                    return Vertices.points[SelectedPoint];
                }
                return null;
            }
        }
        public int SelectedLine
        {
            get
            {
                return _selectedline;
            }
            set
            {
                _selectedline = value;
                RaisePropertyChanged("SelectedLine");
            }
        }
        private string BaseLevelColor = "#F2F2F2";
        private string BasePointColor = "LightCoral";
        private string BaseLineColor = "Black";
        public string ActiveShapeVisibility
        {
            get
            {
                if(ActiveShape == null)
                {
                    return "Collapsed";
                }
                return "Visible";
            }
        }
        private BorderLevel activeshape = null;
        public BorderLevel ActiveShape
        {
            get
            {
                return activeshape;
            }
            set
            {
                activeshape = value;
                RaisePropertyChanged("ActiveShape");
                RaisePropertyChanged("ActiveShapeVisibility");
            }
        }
        public ObservableCollection<BorderLevel> Shapes;
        public ObservableCollection<Level5> Circles;
        public ObservableCollection<Level6> Points;
        public MyPointCollection Vertices;
        public ObservableCollection<Line2D> Lines;
        public MyPointCollection ExtraPoints;
        private void SetExtraLines()
        {
            ExtraLines = new ObservableCollection<Line2D>();
            if (ExtraPoints.Count > 2)
            {
                for (int i = 0; i < ExtraPoints.Count; i++)
                {
                    ExtraLines.Add(new Line2D(ExtraPoints.points[i], ExtraPoints.points[(i + 1) % ExtraPoints.Count]));
                    ExtraLines.Last().ForceUpdatePoints();
                }
            }
            RaisePropertyChanged("ExtraLines");
        }
        public ObservableCollection<Line2D> ExtraLines;
        private double _Zoom = 1;
        public string ZoomStr
        {
            get
            {
                return ((int)(Zoom * 100 / Global.DefaultZoom)).ToString() + '%';
            }
        }
        public double Zoom
        {
            get
            {
                return _Zoom;
            }
            set
            {
                _Zoom = value;
                RaisePropertyChanged("Zoom");
                RaisePropertyChanged("ZoomStr");
            }
        }
        public long snap_range = 10;

        public string CenterX
        {
            get
            {
                return ((long)Global.CanvasSize.X / 2).ToString();
            }
        }
        public string CenterY
        {
            get
            {
                return ((long)Global.CanvasSize.Y / 2).ToString();
            }
        }
        public string MaxX
        {
            get
            {
                return Global.CanvasSize.X.ToString();
            }
        }
        public string MaxY
        {
            get
            {
                return Global.CanvasSize.Y.ToString();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string str)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(str));
                if (string.Equals(str, "ExtraPoints"))
                    SetExtraLines();
            }
        }

        public ActiveContext()
        {
            this.Worlds = new ObservableCollection<Level1>();
            this.ActiveLevel = null;
            this.SelectedLevel = null;
            this.Shapes = new ObservableCollection<BorderLevel>();
            this.Circles = new ObservableCollection<Level5>();
            this.Points = new ObservableCollection<Level6>();
            this.Vertices = new MyPointCollection();
            this.Lines = new ObservableCollection<Line2D>();
            this.ExtraPoints = new MyPointCollection();
            SetExtraLines();
        }

        public ActiveContext(ObservableCollection<Level1> Worlds) : this()
        {
            this.Worlds = Worlds;
        }

        public void NullSelected()
        {
            if (SelectedLevel != null)
            {
                SelectedLevel.color = BaseLevelColor;
                SelectedLevel = null;
                RaisePropertyChanged("Shapes");
            }
            if(SelectedPoint > -1)
            {
                Vertices._points[SelectedPoint].color = BasePointColor;
                SelectedPoint = -1;
                RaisePropertyChanged("Vertices");
            }
            if(SelectedLine > -1)
            {
                Lines[SelectedLine].color = BaseLineColor;
                SelectedLine = -1;
                RaisePropertyChanged("Lines");
            }
        }

        public void SetSelected(SuperLevel level)
        {
            NullSelected();
            SelectedLevel = level;
            if (SelectedLevel != null)
            {
                SelectedLevel.color = "LightSkyBlue";
                RaisePropertyChanged("Shapes");
            }
        }

        public void SetSelected(RenderedPoint point)
        {
            NullSelected();
            for(int i=0; i<Vertices.points.Count; i++)
            {
                if (Vertices.points[i] == point)
                {
                    SelectedPoint = i;
                    Vertices._points[SelectedPoint].color = "LightSkyBlue";
                    RaisePropertyChanged("Vertices");
                    return;
                }
            }
        }

        public void SetSelected(Line2D line)
        {
            NullSelected();
            for (int i = 0; i < Lines.Count; i++)
            {
                if (Lines[i] == line)
                {
                    SelectedLine = i;
                    Lines[SelectedLine].color = "Navy";
                    RaisePropertyChanged("Lines");
                    return;
                }
            }
        }

        /// <summary>
        /// Sets the ActiveLevel to the passed level and updates Shapes and Points
        /// </summary>
        public void SetActive(SuperLevel level)
        {
            if(ActiveLevel != null)
                ActiveLevel.color = "#F2F2F2";
            ActiveLevel = level;
            if (ActiveLevel != null)
            {
                if (ActiveLevel.HasBorderProperty())
                {
                    try
                    {
                        ActiveShape = (BorderLevel)ActiveLevel;
                    } catch (InvalidCastException)
                    {
                        ActiveShape = null;
                    }
                }
                ActiveLevel.color = "LightSkyBlue";
                Global.DefaultZoom = ActiveLevel.GetMedZoom();
                Global.DefaultCenter = ActiveLevel.GetCenter();
                ForceUpdatePoints();
                UpdateAll();
            } else //If ActiveLevel *is* null
            {
                ActiveShape = null;
                Global.DefaultCenter = new AbsolutePoint(Global.RenderedCenter.X, Global.RenderedCenter.Y);
                Shapes = new ObservableCollection<BorderLevel>();
                Circles = new ObservableCollection<Level5>();
                Points = new ObservableCollection<Level6>();
                RaisePropertyChanged("Shapes");
                RaisePropertyChanged("Circles");
                RaisePropertyChanged("Points");
            }
            RaisePropertyChanged("ActiveLevel");
        }

        /// <summary>
        /// Sets the SelectedPoint to new_pos
        /// </summary>
        /// <param name="new_pos">The position the vertex is to be changed to</param>
        /// <returns>true if there is a SelectedPoint, false otherwise</returns>
        public bool SetVertex(RenderedPoint new_pos)
        {
            return SetVertex(new_pos.ToAbsolutePoint());
        }

        /// <summary>
        /// Sets the SelectedPoint to new_pos
        /// </summary>
        /// <param name="new_pos">The position the vertex is to be changed to</param>
        /// <returns>true if there is a SelectedPoint, false otherwise</returns>
        public bool SetVertex(AbsolutePoint new_pos)
        {
            if(SelectedPoint >= 0)
            {
                for(int i=0; i<Shapes.Count; i++)
                {
                    Shapes[i].MovePoint(Vertices._points[SelectedPoint], new_pos);
                }
                RaisePropertyChanged("Shapes");
                UpdateVertices();
                UpdateLines();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Sets the vertex at old_pos to be at new_pos
        /// </summary>
        /// <param name="old_post">The current position of the vertex to be changed</param>
        /// <param name="new_pos">The position the vertex is to be changed to</param>
        /// <returns>true if the vertex exists, false otherwise</returns>
        public bool SetVertex(RenderedPoint old_pos, RenderedPoint new_pos)
        {
            return SetVertex(old_pos.ToAbsolutePoint(), new_pos.ToAbsolutePoint());
        }

        /// <summary>
        /// Sets the vertex at old_pos to be at new_pos
        /// </summary>
        /// <param name="old_post">The current position of the vertex to be changed</param>
        /// <param name="new_pos">The position the vertex is to be changed to</param>
        /// <returns>true if the vertex exists, false otherwise</returns>
        public bool SetVertex(AbsolutePoint old_pos, AbsolutePoint new_pos)
        {
            for(int i=0; i<Vertices.Count; i++)
            {
                if (Vertices._points[i] == old_pos)
                {
                    for(int j=0; j<Shapes.Count; j++)
                    {
                        Shapes[i].MovePoint(old_pos, new_pos);
                    }
                    RaisePropertyChanged("Shapes");
                    UpdateVertices();
                    UpdateLines();
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if the SelectedPoint would work if moved to new_pos
        /// </summary>
        public bool TestMovePoint(AbsolutePoint new_pos)
        {
            if (SelectedPoint < 0)
                return false;
            ObservableCollection<BorderLevel> TestShapes = new ObservableCollection<BorderLevel>();
            List<int> updatedvalues = new List<int>();
            for(int i=0; i<Shapes.Count; i++)
            {
                TestShapes.Add(new BorderLevel(Shapes[i]));
                if (TestShapes[i].HasPoint(CurrentPoint.ToAbsolutePoint()))
                {
                    TestShapes[i].MovePoint(CurrentPoint.ToAbsolutePoint(), new_pos);
                    updatedvalues.Add(i);
                }
            }
            foreach(int i in updatedvalues)
            {
                if (Intersects(TestShapes[i].border, TestShapes[i].level, TestShapes))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Updates the Shapes to match those in ActiveLevel
        /// </summary>
        public void UpdateShapes()
        {
            IList<SuperLevel> temp = SuperLevel.Filter(ActiveLevel.GetSublevels(), 2);
            temp.Concat<SuperLevel>(SuperLevel.Filter(ActiveLevel.GetSublevels(), 3));
            temp.Concat<SuperLevel>(SuperLevel.Filter(ActiveLevel.GetSublevels(), 4));
            Shapes = new ObservableCollection<BorderLevel>();
            for (int i = 0; i < temp.Count; i++)
            {
                try
                {
                    if(((BorderLevel)temp[i]).border.verticesr.Count > 0)
                        Shapes.Add((BorderLevel)temp[i]);
                }
                catch (InvalidCastException)
                {
                    ;
                }
            }
            RaisePropertyChanged("Shapes");
            UpdateVertices();
            UpdateLines();
        }

        /// <summary>
        /// Updates the Circles to match those in ActiveLevel
        /// </summary>
        public void UpdateCircles()
        {
            IList<SuperLevel> temp = SuperLevel.Filter(ActiveLevel.GetSublevels(), 5);
            for (int i = 0; i < temp.Count; i++)
            {
                try
                {
                    Circles.Add((Level5)temp[i]);
                }
                catch (InvalidCastException)
                {
                    ;
                }
            }
            RaisePropertyChanged("Circles");
        }

        /// <summary>
        /// Updates the Points to match those in ActiveLevel
        /// </summary>
        public void UpdatePoints()
        {
            IList<SuperLevel> temp = SuperLevel.Filter(ActiveLevel.GetSublevels(), 6);
            for (int i = 0; i < temp.Count; i++)
            {
                try
                {
                    Points.Add((Level6)temp[i]);
                }
                catch (InvalidCastException)
                {
                    ;
                }
            }
            RaisePropertyChanged("Points");
        }

        /// <summary>
        /// Updates the Vertices to match those of the Shapes
        /// </summary>
        public void UpdateVertices()
        {
            Vertices = new MyPointCollection();
            for (int i = 0; i < Shapes.Count; i++)
            {
                for (int j = 0; j < Shapes[i].border.Count; j++)
                {
                    if (!Vertices.Contains(Shapes[i].border[j]))
                        Vertices.AppendPoint(Shapes[i].border[j]);
                }
            }
            Vertices.base_color = "LightCoral";
            RaisePropertyChanged("Vertices");
        }

        /// <summary>
        /// Updates the Lines to match those of the Shapes
        /// </summary>
        public void UpdateLines()
        {
            Lines = new ObservableCollection<Line2D>();
            for(int i=0; i<Shapes.Count; i++)
            {
                ObservableCollection<Line2D> edges = Shapes[i].border.edges;
                for(int j=0; j<edges.Count; j++)
                {
                    if (!Line2D.Contains(Lines, edges[j]))
                    {
                        Lines.Add(new Line2D(edges[j]));
                        Lines.Last().ForceUpdatePoints();
                    }
                }
            }
        }

        /// <summary>
        /// Updates the Shapes, Circles, and Points to match the ActiveWorld
        /// </summary>
        public void UpdateAll()
        {
            NullSelected();
            UpdateShapes();
            UpdateCircles();
            UpdatePoints();
        }

        /// <summary>
        /// Checks whether a given point fits within any existing sublevels of the activelevel
        /// </summary>
        /// <param name="point">The point to check against</param>
        /// <param name="levelnum">The current level number to check for</param>
        /// <param name="activelevel">The activelevel to check the sublevels of</param>
        /// <returns></returns>
        private static bool Conflicts(AbsolutePoint point, int levelnum, SuperLevel activelevel)
        {
            if (levelnum > 1 && levelnum < 6 && activelevel != null)
            {
                for (int i = 0; i < activelevel.sublevels.Count; i++)
                {
                    if (activelevel.sublevels[i].level == levelnum)
                    {
                        try
                        {
                            if (activelevel.sublevels[i].HasBorderProperty())
                            {
                                BorderLevel sublevel = (BorderLevel)activelevel.sublevels[i];
                                if (sublevel.PointInPolygon(point) && !sublevel.PointOnPolygon(point))
                                    return true;

                            }
                            else if (activelevel.sublevels[i].HasRadiusProperty())
                            {
                                Level5 sublevel = (Level5)activelevel.sublevels[i];
                                if (sublevel.CanFitPoint(point))
                                    return true;
                            }
                        }
                        catch (InvalidCastException)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Checks whether a given point fits within any existing sublevels of the ActiveLevel
        /// </summary>
        /// <param name="point">The point to check against</param>
        /// <param name="levelnum">The current level number to check for</param>
        /// <returns></returns>
        public bool Conflicts(AbsolutePoint point, int levelnum)
        {
            return Conflicts(point, levelnum, ActiveLevel);
        }

        /// <summary>
        /// Checks whether a given polygon intersects with any existing shapes of the same level number
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="levelnum"></param>
        /// <param name="shapes"></param>
        /// <returns></returns>
        private static bool Intersects(Polygon2D polygon, int levelnum, IList<BorderLevel> shapes)
        {
            for (int i = 0; i < shapes.Count; i++)
            {
                if (shapes[i].level == levelnum)
                {
                    if (shapes[i].border.IntersectsPolygon(polygon))
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks whether a given polygon intersects with any existing sublevels of the current activelevel of the same level number
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="levelnum"></param>
        /// <returns></returns>
        public bool Intersects(Polygon2D polygon, int levelnum)
        {
            return Intersects(polygon, levelnum, Shapes);
        }

        /// <summary>
        /// Forces all point-related objects to update
        /// </summary>
        public void ForceUpdatePoints()
        {
            if (ActiveLevel != null)
            {
                ActiveLevel.ForceUpdatePoints();
                RaisePropertyChanged("ActiveLevel");
            }
            if (ExtraPoints != null)
            {
                ExtraPoints.ForceUpdatePoints();
                RaisePropertyChanged("ExtraPoints");
            }
            RaisePropertyChanged("Shapes");
            RaisePropertyChanged("Circles");
            RaisePropertyChanged("Points");
            RaisePropertyChanged("Vertices");
            if(Lines != null)
            {
                for(int i=0; i<Lines.Count; i++)
                {
                    Lines[i].ForceUpdatePoints();
                }
            }
            RaisePropertyChanged("Lines");
            RaisePropertyChanged("ExtraPoints");
        }

        /// <summary>
        /// Sets this.ExtraPoints to the list
        /// </summary>
        public void SetPoints(IList<AbsolutePoint> list)
        {
            this.ExtraPoints = new MyPointCollection();
            for(int i=0; i<list.Count; i++)
            {
                this.ExtraPoints.AppendPoint(new AbsolutePoint(list[i].X, list[i].Y));
            }
            RaisePropertyChanged("ExtraPoints");
        }

        /// <summary>
        /// Clears the ExtraPoints
        /// </summary>
        public void ClearPoints()
        {
            this.ExtraPoints = new MyPointCollection();
            this.ExtraPoints.ForceUpdatePoints();
            RaisePropertyChanged("ExtraPoints");
        }

        /// <summary>
        /// Sets the ActiveWorld to the World of the given name and updates Shapes and Points
        /// </summary>
        public bool SetWorld(string name)
        {
            if (ActiveLevel != null && string.Equals(ActiveLevel.GetName(), name))
                return true;
            for (int i = 0; i < Worlds.Count; i++)
            {
                if (string.Equals(Worlds[i].GetName(), name))
                {
                    SetActive(Worlds[i]);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Sets the ActiveWorld to the World of the given name and subtype and updates Shapes and Points
        /// </summary>
        public bool SetWorld(string name, string subtype)
        {
            if (ActiveLevel!=null && string.Equals(ActiveLevel.GetName(), name) && string.Equals(ActiveLevel.subtype, subtype))
                return true;
            for (int i = 0; i < Worlds.Count; i++)
            {
                if (string.Equals(Worlds[i].GetName(), name) && string.Equals(Worlds[i].subtype, subtype))
                {
                    SetActive(Worlds[i]);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks whether the given point will snap to an element of ExtraPoints
        /// </summary>
        public bool SnapsToExtraPoint(RenderedPoint point)
        {
            for(int i=0; i<ExtraPoints.points.Count; i++)
            {
                if (point.SnapsTo(ExtraPoints.points[i], snap_range))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Checks whether the given point will snap to an element of Vertices
        /// </summary>
        public bool SnapsToVertices(RenderedPoint point)
        {
            for (int i = 0; i < Vertices.points.Count; i++)
            {
                if (point.SnapsTo(Vertices.points[i], snap_range))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Checks whether the given point will snap to an element of Points
        /// </summary>
        public bool SnapsToPoints(RenderedPoint point)
        {
            for(int i=0; i < Points.Count; i++)
            {
                if (point.SnapsTo(Points[i].center.ToRenderedPoint(), snap_range))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Checks whether the given point will snap to an element of Circles
        /// </summary>
        public bool SnapsToCircle(RenderedPoint point)
        {
            for(int i=0; i < Circles.Count; i++)
            {
                if (Circles[i].PointInRadius(point.ToAbsolutePoint()))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Checks whether the given point will snap to an element of Lines
        /// </summary>
        public bool SnapsToLine(RenderedPoint point)
        {
            for (int i = 0; i < Lines.Count; i++)
            {
                long distance = Lines[i].RenderedDistance(point);
                Line2D temp = Lines[i];
                if (Lines[i].RenderedDistance(point) <= snap_range)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Checks whether the given point will snap to an element of Shapes
        /// </summary>
        public bool SnapsToShape(RenderedPoint point)
        {
            for(int i=0; i < Shapes.Count; i++)
            {
                if (Shapes[i].PointInPolygon(point.ToAbsolutePoint()))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Snaps the point to the closest ExtraPoint element in range
        /// </summary>
        public RenderedPoint SnapToExtraPoint(RenderedPoint point)
        {
            if (!SnapsToExtraPoint(point))
                return point;
            long distance = snap_range;
            for(int i=0; i<ExtraPoints.points.Count; i++)
            {
                distance = Math.Min(distance, point.Distance(ExtraPoints.points[i]));
            }
            for(int i=0; i<ExtraPoints.points.Count; i++)
            {
                if (point.Distance(ExtraPoints.points[i]) == distance)
                    return ExtraPoints.points[i];
            }
            return point;
        }

        /// <summary>
        /// Snaps the point to the closest Vertices element in range
        /// </summary>
        public RenderedPoint SnapToVertices(RenderedPoint point)
        {
            if (!SnapsToVertices(point))
                return point;
            long distance = snap_range;
            for (int i = 0; i < Vertices.points.Count; i++)
            {
                distance = Math.Min(distance, point.Distance(Vertices.points[i]));
            }
            for (int i = 0; i < Vertices.points.Count; i++)
            {
                if (point.Distance(Vertices.points[i]) == distance)
                    return Vertices.points[i];
            }
            return point;
        }

        /// <summary>
        /// Snaps the point to the closest Points element in range
        /// </summary>
        public RenderedPoint SnapToPoints(RenderedPoint point)
        {
            if (!SnapsToVertices(point))
                return point;
            long distance = snap_range;
            for (int i = 0; i < Points.Count; i++)
            {
                distance = Math.Min(distance, point.Distance(Points[i].center.ToRenderedPoint()));
            }
            for (int i = 0; i < Points.Count; i++)
            {
                if (point.Distance(Points[i].center.ToRenderedPoint()) == distance)
                    return Points[i].center.ToRenderedPoint();
            }
            return point;
        }

        /// <summary>
        /// Snaps the point to the closest Circles element in range
        /// </summary>
        public RenderedPoint SnapToCircles(RenderedPoint point)
        {
            if (!SnapsToVertices(point))
                return point;
            long distance = long.MaxValue;
            for (int i = 0; i < Points.Count; i++)
            {
                distance = Math.Min(distance, point.Distance(Circles[i].center.ToRenderedPoint()));
            }
            for (int i = 0; i < Points.Count; i++)
            {
                if (point.Distance(Circles[i].center.ToRenderedPoint()) == distance)
                    return Circles[i].center.ToRenderedPoint();
            }
            return point;
        }

        /// <summary>
        /// Snaps the point to the closest Circles element in range
        /// </summary>
        public RenderedPoint SnapToLines(RenderedPoint point)
        {
            if (!SnapsToLine(point))
                return point;
            long distance = snap_range + 1;
            for(int i=0; i<Lines.Count; i++)
            {
                distance = Math.Min(distance, Lines[i].RenderedDistance(point));
            }
            for(int i=0; i<Lines.Count; i++)
            {
                if (Lines[i].RenderedDistance(point) == distance)
                {
                    return Lines[i].GetClosestPoint(point.ToAbsolutePoint()).ToRenderedPoint();
                }
                    
            }
            return point;
        }

        /// <summary>
        /// Gets the closest element of ExtraPoint in range
        /// </summary>
        public RenderedPoint GetExtraPoint(RenderedPoint point)
        {
            if (!SnapsToExtraPoint(point))
                throw new ArgumentException("Point not within range of any elements of ExtraPoints");
            return SnapToExtraPoint(point);
        }

        /// <summary>
        /// Gets the closest element of Vertices in range
        /// </summary>
        public RenderedPoint GetVertex(RenderedPoint point)
        {
            if (!SnapsToVertices(point))
                throw new ArgumentException("Point not within range of any elements of Vertices");
            return SnapToVertices(point);
        }

        /// <summary>
        /// Gets the closest element of Points in range
        /// </summary>
        public Level6 GetPoint(RenderedPoint point)
        {
            List<Level6> list = GetPointsByPoint(point);
            if (list.Count == 0)
                throw new ArgumentException("Point not within range of any elements of Points");
            return list[0];
        }

        /// <summary>
        /// Gets the closest element of Lines in range
        /// </summary>
        public Line2D GetLine(RenderedPoint point)
        {
            long min_distance = snap_range + 1;
            for (int i=0; i<Lines.Count; i++)
            {
                min_distance = Math.Min(min_distance, Lines[i].RenderedDistance(point));
            }
            if(min_distance <= snap_range)
            {
                for(int i=0; i<Lines.Count; i++)
                {
                    if (Lines[i].RenderedDistance(point) == min_distance)
                        return Lines[i];
                }
            }
            throw new ArgumentException("Point not within range of any elements of Lines");
        }

        /// <summary>
        /// Gets the closest element of Circles in range
        /// </summary>
        public Level5 GetCirlce(RenderedPoint point)
        {
            if (SnapsToCircle(point))
            {
                long min_distance = long.MaxValue;
                for (int i = 0; i < Circles.Count; i++)
                {
                    if (Circles[i].PointInRadius(point.ToAbsolutePoint()))
                        min_distance = Math.Min(min_distance, point.Distance(Circles[i].center.ToRenderedPoint()));
                }
                for (int i = 0; i < Lines.Count; i++)
                {
                    if (point.Distance(Circles[i].center.ToRenderedPoint()) == min_distance)
                        return Circles[i];
                }
            }
            throw new ArgumentException("Point not within range of any elements of Circles");
        }

        /// <summary>
        /// Gets the closest element of Shapes in range
        /// </summary>
        public BorderLevel GetShape(RenderedPoint point)
        {
            if (SnapsToShape(point))
            {
                long min_distance = long.MaxValue;
                for (int i = 0; i < Shapes.Count; i++)
                {
                    if (Shapes[i].PointInPolygon(point.ToAbsolutePoint()))
                    {
                        min_distance = Math.Min(min_distance, point.Distance(Shapes[i]._center));
                    }
                }
                for (int i = 0; i < Shapes.Count; i++)
                {
                    if (Shapes[i].PointInPolygon(point.ToAbsolutePoint()) && point.Distance(Shapes[i]._center) == min_distance)
                    {
                        return Shapes[i];
                    }
                }
            }
            throw new ArgumentException("Point not within range of any elements of Shapes");
        }

        /// <summary>
        /// Creates and returns a list of elements in ExtraPoints that point can snap to
        /// </summary>
        public List<RenderedPoint> GetExtraPointsByPoint(RenderedPoint point)
        {
            List<RenderedPoint> output = new List<RenderedPoint>();
            for(int i=0; i<ExtraPoints.points.Count; i++)
            {
                if (point.SnapsTo(ExtraPoints.points[i], snap_range))
                    output.Add(ExtraPoints.points[i]);
            }
            return output;
        }

        /// <summary>
        /// Creates and returns a list of elements in Vertices that point can snap to
        /// </summary>
        public List<RenderedPoint> GetVerticesByPoint(RenderedPoint point)
        {
            List<RenderedPoint> output = new List<RenderedPoint>();
            for (int i = 0; i < Vertices.points.Count; i++)
            {
                if (point.SnapsTo(Vertices.points[i], snap_range))
                    output.Add(Vertices.points[i]);
            }
            return output;
        }

        /// <summary>
        /// Creates and returns a list of elements in Points that point can snap to
        /// </summary>
        public List<Level6> GetPointsByPoint(RenderedPoint point)
        {
            List<Level6> output = new List<Level6>();
            for (int i = 0; i < Points.Count; i++)
            {
                if (point.SnapsTo(Points[i].center.ToRenderedPoint(), snap_range))
                    output.Add(Points[i]);
            }
            return output;
        }

        /// <summary>
        /// Creates and returns a list of elements in Circles that point can snap to
        /// </summary>
        public List<Level5> GetCirclesByPoint(RenderedPoint point)
        {
            List<Level5> output = new List<Level5>();
            for (int i = 0; i < Circles.Count; i++)
            {
                if (Circles[i].PointInRadius(point.ToAbsolutePoint()))
                    output.Add(Circles[i]);
            }
            return output;
        }

        /// <summary>
        /// Creates and returns a list of elements in Shapes that point can snap to
        /// </summary>
        public List<BorderLevel> GetShapesByPoint(RenderedPoint point)
        {
            List<BorderLevel> output = new List<BorderLevel>();
            for (int i = 0; i < Shapes.Count; i++)
            {
                if (Shapes[i].PointInPolygon(point.ToAbsolutePoint()))
                    output.Add(Shapes[i]);
            }
            return output;
        }

        /// <summary>
        /// Creates and returns a list of elements in Lines that point can snap to
        /// </summary>
        public List<Line2D> GetLinesByPoint(RenderedPoint point)
        {
            List<Line2D> output = new List<Line2D>();
            for (int i = 0; i < Lines.Count; i++)
            {
                if (Lines[i].RenderedDistance(point) <= snap_range)
                    output.Add(Lines[i]);
            }
            return output;
        }

        /// <summary>
        /// Attempts to snap the given point to a point within range
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public RenderedPoint SnapToAPoint(RenderedPoint point)
        {
            if (SnapsToExtraPoint(point))
                return SnapToExtraPoint(point);
            if (SnapsToVertices(point))
                return SnapToVertices(point);
            if (SnapsToLine(point))
                return SnapToLines(point);
            return point;
        }

        /// <summary>
        /// Creates and returns the set of objects in the current ActiveContext that are close enough to the given point
        /// </summary>
        public List<Object> GetObjectsContainingPoint(RenderedPoint point)
        {
            List<Object> output = new List<Object>();
            output.Concat<Object>(GetPointsByPoint(point));
            output.Concat<Object>(GetCirclesByPoint(point));
            output.Concat<Object>(GetVerticesByPoint(point));
            output.Concat<Object>(GetLinesByPoint(point));
            output.Concat<Object>(GetShapesByPoint(point));
            return output;
        }

        /// <summary>
        /// Check whether the given point snaps to anything
        /// </summary>
        public bool SnapsToSomething(RenderedPoint point)
        {
            return SnapsToPoints(point) || SnapsToCircle(point) || SnapsToVertices(point) || SnapsToLine(point) || SnapsToShape(point);
        }

        /// <summary>
        /// Gets the object best suited for point
        /// </summary>
        public Object GetObjectContainingPoint(RenderedPoint point)
        {
            if (SnapsToPoints(point))
                return GetPoint(point);
            if (SnapsToCircle(point))
                return GetCirlce(point);
            if (SnapsToVertices(point))
                return GetVertex(point);
            if (SnapsToLine(point))
                return GetLine(point);
            if (SnapsToShape(point))
                return GetShape(point);
            return null;
        }
    }
}
