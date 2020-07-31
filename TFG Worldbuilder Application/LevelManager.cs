using Microsoft.Graphics.Canvas.Effects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Media;

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
                if (string.Equals(str, "name") || string.Equals(str, "level") || string.Equals(str, "subtype"))
                    RaisePropertyChanged("prop_str");
                if(string.Equals(str, "sublevels"))
                {
                    RaisePropertyChanged("SubShapes");
                    RaisePropertyChanged("SubCircles");
                    RaisePropertyChanged("SubPoints");
                }
            }
        }

        public virtual double Level_MaxX
        {
            get
            {
                double output = double.MinValue;
                foreach(SuperLevel sublevel in sublevels)
                {
                    output = Math.Max(output, sublevel.Level_MaxX);
                }
                return output;
            }
        }
        public virtual double Level_MaxY
        {
            get
            {
                double output = double.MinValue;
                foreach (SuperLevel sublevel in sublevels)
                {
                    output = Math.Max(output, sublevel.Level_MaxY);
                }
                return output;
            }
        }
        public virtual double Level_MinX
        {
            get
            {
                double output = double.MaxValue;
                foreach (SuperLevel sublevel in sublevels)
                {
                    output = Math.Min(output, sublevel.Level_MinX);
                }
                return output;
            }
        }
        public virtual double Level_MinY
        {
            get
            {
                double output = double.MaxValue;
                foreach (SuperLevel sublevel in sublevels)
                {
                    output = Math.Min(output, sublevel.Level_MinY);
                }
                return output;
            }
        }


        public SuperLevel thisSuperLevel
        {
            get
            {
                return this;
            }
        }
        public ObservableCollection<BorderLevel> SubShapes
        {
            get
            {
                ObservableCollection<BorderLevel> output = new ObservableCollection<BorderLevel>();
                foreach (SuperLevel sublevel in sublevels)
                {
                    if (sublevel != null)
                    {
                        if (sublevel.HasBorderProperty())
                        {
                            try
                            {
                                BorderLevel subshape = (BorderLevel)sublevel;
                                output.Add(subshape);
                            }
                            catch (InvalidCastException)
                            {
                                ;
                            }
                        }
                    }
                }
                return output;
            }
        }
        public ObservableCollection<Level5> SubCircles
        {
            get
            {
                ObservableCollection<Level5> output = new ObservableCollection<Level5>();
                foreach (SuperLevel sublevel in sublevels)
                {
                    if (sublevel != null)
                    {
                        if (sublevel.HasCenterProperty() && sublevel.HasRadiusProperty() && sublevel.level == 5)
                        {
                            try
                            {
                                Level5 subshape = (Level5)sublevel;
                                output.Add(subshape);
                            }
                            catch (InvalidCastException)
                            {
                                ;
                            }
                        }
                    }
                }
                return output;
            }
        }
        public ObservableCollection<Level6> SubPoints
        {
            get
            {
                ObservableCollection<Level6> output = new ObservableCollection<Level6>();
                foreach (SuperLevel sublevel in sublevels)
                {
                    if (sublevel != null)
                    {
                        if (sublevel.HasCenterProperty() && !sublevel.HasRadiusProperty() && sublevel.level == 6)
                        {
                            try
                            {
                                Level6 subshape = (Level6)sublevel;
                                output.Add(subshape);
                            }
                            catch (InvalidCastException)
                            {
                                ;
                            }
                        }
                    }
                }
                return output;
            }
        }

        public string Opacity
        {
            get
            {
                switch (leveltype)
                {
                    case LevelType.Geographical:
                        return "1";
                    case LevelType.National:
                        return "0.8";
                    default:
                        return "0.5";
                }
            }
        }

        public static readonly string DefaultColor = "#F2F2F2";

        public string prop_str
        {
            get
            {
                return name + ": Level " + level.ToString() + " " + subtype;
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
                if(parent != null && parent.HasSublevelWithName(value))
                    throw new ArgumentException("Name Conflict - sibling level already has name");
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
                RaisePropertyChanged("recolorsubtype");
            }
        }
        public string recolorsubtype
        {
            get
            {
                return "Recolor " + subtype;
            }
        }
        private string _basecolor;
        public string basecolor
        {
            get
            {
                return _basecolor;
            }
            set
            {
                bool update_color = string.Equals(color, basecolor);
                _basecolor = value;
                if (update_color)
                    color = basecolor;
                RaisePropertyChanged("basecolor");
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
            basecolor = SuperLevel.DefaultColor;
            color = basecolor;
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
            basecolor = Global.Subtypes.GetColor(subtype);
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        public SuperLevel(SuperLevel o) : this(o.name, o.level, o.leveltype, o.subtype, o.parent){
            for(int i=0; i<o.sublevels.Count; i++)
            {
                AddSublevel(o.sublevels[i].Copy());
            }
            for(int i=0; i<o.leveldata.Count; i++)
            {
                this.leveldata.Add(o.leveldata[i]);
            }
            this.basecolor = o.basecolor;
            this.color = o.color;
        }
        public virtual SuperLevel Copy()
        {
            return new SuperLevel(this);
        }

        /// <summary>
        /// basic destructor
        /// </summary>
        ~SuperLevel()
        {
            this.sublevels.Clear();
        }

        /// <summary>
        /// Recolors any level of the given subtype
        /// </summary>
        public void Recolor(string subtype)
        {
            if(string.Equals(this.subtype.ToLower(), subtype.ToLower()))
            {
                basecolor = Global.Subtypes.GetColor(subtype);
            }
            else
            {
                foreach (SuperLevel sublevel in sublevels)
                {
                    sublevel.Recolor(subtype);
                }
            }
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
            if(HasBorderProperty()) //Border Levels
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
            if(HasCenterProperty())//Point Levels
            {
                try
                {
                    AbsolutePoint center = ((PointLevel)this).GetCenter();
                    Text += "Center" + inner_delimiter + center.ToString() + outer_delimiter;
                }
                catch (InvalidCastException) { }
            } 
            if(HasRadiusProperty())//Level 5's radius
            {
                try
                {
                    string rad = ((Level5)this).radius.ToString();
                    Text += "Radius" + inner_delimiter + rad + outer_delimiter;
                }
                catch (InvalidCastException) { }
            }
            if(!string.Equals(basecolor, SuperLevel.DefaultColor))
                Text += "Level Color" + inner_delimiter + basecolor + outer_delimiter;
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
        /// Deletes the passed SuperLevel from the list of sublevels, or returns false
        /// </summary>
        public bool DeleteSublevel(SuperLevel o)
        {
            for(int i=0; i<sublevels.Count; i++)
            {
                if(string.Equals(sublevels[i].name, o.name) && sublevels[i].GetType() == o.GetType() && string.Equals(sublevels[i].subtype, o.subtype))
                {
                    sublevels.RemoveAt(i);
                    RaisePropertyChanged("sublevels");
                    return true;
                }
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
                ListConcat<SuperLevel>.Concat(results, this.sublevels[i].FindLevels(name));
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
                        ListConcat<SuperLevel>.Concat(results, this.sublevels[i].FindLevels(name, level));
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
                        ListConcat<SuperLevel>.Concat(results, this.sublevels[i].FindLevels(name, level, leveltype));
                    }
                }
            }
            return results;
        }

        /// <summary>
        /// Returns true if the passed point fits within the boundaries and level
        /// </summary>
        public bool CanFitPoint(RenderedPoint point)
        {
            return CanFitPoint(point.ToAbsolutePoint());
        }

        /// <summary>
        /// Returns true if the passed point fits within the boundaries and level
        /// </summary>
        public virtual bool CanFitPoint(AbsolutePoint point)
        {
            return true;
        }

        /// <summary>
        /// Returns true if the passed point fits within the boundaries
        /// </summary>
        public bool FitsPoint(RenderedPoint point)
        {
            return FitsPoint(point.ToAbsolutePoint());
        }

        /// <summary>
        /// Returns true if the passed point fits within the boundaries
        /// </summary>
        public virtual bool FitsPoint(AbsolutePoint point)
        {
            return false;
        }
        
        /// <summary>
        /// Returns true if the parent is of the correct type and the given point fits within the constraints of the parent, or if the parent is not of the correct type
        /// </summary>
        public bool PointInParent(RenderedPoint point)
        {
            return PointInParent(point.ToAbsolutePoint());
        }

        /// <summary>
        /// Returns true if the parent is of the correct type and the given point fits within the constraints of the parent, or if the parent is not of the correct type
        /// </summary>
        public bool PointInParent(AbsolutePoint point)
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
                ListConcat<SuperLevel>.Concat(output, this.sublevels[i].SublevelsInRegion(region));
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

        /// <summary>
        /// Forces an update to all point objects
        /// </summary>
        public virtual void ForceUpdatePoints()
        {
            RaisePropertyChanged("Opacity");
            RaisePropertyChanged("Level_MaxX");
            RaisePropertyChanged("Level_MinX");
            RaisePropertyChanged("Level_MaxY");
            RaisePropertyChanged("Level_MinY");
            if (sublevels != null)
            {
                for(int i=0; i<sublevels.Count; i++)
                {
                    sublevels[i].ForceUpdatePoints();
                }
            }
        }
        
        /// <summary>
        /// Returns the MedZoom for the region such that being zoomed in will fully encompass the region
        /// </summary>
        public virtual double GetMedZoom()
        {
            double output = 1.0f;
            if(Level_MaxX > Level_MinX && Level_MaxY > Level_MinY)
            {
                double zoom_x = Global.CanvasSize.X / (Level_MaxX - Level_MinX);
                double zoom_y = Global.CanvasSize.Y / (Level_MaxY - Level_MinY);
                output = Math.Min(Math.Min(zoom_x, zoom_y), output);
                if (output <= 0)
                    output = 1.0f;
            }
            if (parent == null)
                return output;
            return Math.Max(output, parent.GetMedZoom());
        }

        public virtual AbsolutePoint GetCenter()
        {
            if (Level_MaxX > Level_MinX && Level_MaxY > Level_MinY)
            {
                return new AbsolutePoint((Level_MaxX + Level_MinX) / 2, (Level_MaxY + Level_MinY) / 2);
            }
            return new AbsolutePoint(Global.CanvasSize.X / 2, Global.CanvasSize.Y / 2);
        }
    };

    /// <summary>
    /// Superclass for levels with a defined border
    /// </summary>
    public class BorderLevel : SuperLevel
    {
        public Polygon2D border;

        public override double Level_MaxX
        {
            get
            {
                double output = base.Level_MaxX;
                foreach(AbsolutePoint vertex in border.vertices)
                {
                    output = Math.Max(output, vertex.X);
                }
                return output;
            }
        }
        public override double Level_MaxY
        {
            get
            {
                double output = base.Level_MaxY;
                foreach (AbsolutePoint vertex in border.vertices)
                {
                    output = Math.Max(output, vertex.Y);
                }
                return output;
            }
        }
        public override double Level_MinX
        {
            get
            {
                double output = base.Level_MinX;
                foreach (AbsolutePoint vertex in border.vertices)
                {
                    output = Math.Min(output, vertex.X);
                }
                return output;
            }
        }
        public override double Level_MinY
        {
            get
            {
                double output = base.Level_MinY;
                foreach (AbsolutePoint vertex in border.vertices)
                {
                    output = Math.Min(output, vertex.Y);
                }
                return output;
            }
        }

        public BorderLevel thisBorderLevel
        {
            get
            {
                return this;
            }
        }

        public PointCollection truepoints
        {
            get
            {
                return RenderedPoint.ToWindowsPoints(border.untrimmedvertices);
            }
        }

        public PointCollection points
        {
            get
            {
                return RenderedPoint.ToWindowsPoints(border.verticesr);
            }
        }
        
        public RenderedPoint _center
        {
            get
            {
                return border.GetCenter().ToRenderedPoint();
            }
        }
        /// <summary>
        /// center object used for rendering
        /// </summary>
        public Point center
        {
            get
            {
                return _center.ToWindowsPoint();
            }
        }
        /// <summary>
        /// string object used to set object visibility
        /// </summary>
        public string visibility
        {
            get
            {
                if (border.verticesr.Count > 0)
                    return "Visible";
                return "Collapsed";
            }
        }
        /// <summary>
        /// string object used to set name visibility
        /// </summary>
        public string name_visibility
        {
            get
            {
                if(border.verticesr.Count>0)
                    return _center.visibility;
                return "Collapsed";
            }
        }
        /// <summary>
        /// string designed to be set to the margins of a textblock
        /// </summary>
        public string margin
        {
            get
            {
                int intended_width = 50;
                int intended_height = 20;
                Point contextual_center = border.GetContextualCenter().ToRenderedPoint().ToWindowsPoint();
                string left, top, right, bottom;
                left = (contextual_center.X - intended_width).ToString();
                top = (contextual_center.Y - intended_height).ToString();
                right = ((Global.CanvasSize.X - contextual_center.X) - intended_width).ToString();
                bottom = ((Global.CanvasSize.Y - contextual_center.Y) - intended_height).ToString();
                return left + ',' + top + ',' + right + ',' + bottom;
            }
        }

        /// <summary>
        /// Forces an update to all point objects
        /// </summary>
        public override void ForceUpdatePoints()
        {
            RaisePropertyChanged("points");
            RaisePropertyChanged("truepoints");
            RaisePropertyChanged("center");
            RaisePropertyChanged("margin");
            RaisePropertyChanged("visibility");
            RaisePropertyChanged("name_visibility");
            border.ForceUpdatePoints();
            base.ForceUpdatePoints();
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
        /// Copy constructor
        /// </summary>
        public BorderLevel(BorderLevel o) : base(o)
        {
            this.border = new Polygon2D();
            for (int i = 0; i < border.Count; i++)
            {
                border.AppendPoint(o.border.vertices[i]);
            }
        }
        public override SuperLevel Copy()
        {
            return new BorderLevel(this);
        }

        /// <summary>
        /// Returns true if the passed point fits within the boundaries and level
        /// </summary>
        public override bool CanFitPoint(AbsolutePoint point)
        {
            return PointInPolygon(point);
        }

        /// <summary>
        /// Returns true if the passed point fits within the boundaries
        /// </summary>
        public override bool FitsPoint(AbsolutePoint point)
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
                ListConcat<SuperLevel>.Concat(output, Worlds[i].SublevelsInRegion(border));
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
        public AbsolutePoint AppendPoint(AbsolutePoint point)
        {
            AbsolutePoint output = this.border.AppendPoint(point);
            RaisePropertyChanged("points");
            RaisePropertyChanged("truepoints");
            RaisePropertyChanged("center");
            RaisePropertyChanged("margin");
            RaisePropertyChanged("visibility");
            RaisePropertyChanged("name_visibility");
            return output;
        }
        
        /// <summary>
        /// Deletes the passed point from the list of vertices, or returns false
        /// </summary>
        public bool DeletePoint(AbsolutePoint point)
        {
            return border.DeletePoint(point);
        }

        /// <summary>
        /// Creates a new point between two existing points
        /// </summary>
        public AbsolutePoint NewPoint(AbsolutePoint a, AbsolutePoint b)
        {
            AbsolutePoint output = border.NewPoint(a, b);
            RaisePropertyChanged("points");
            RaisePropertyChanged("truepoints");
            RaisePropertyChanged("visibility");
            RaisePropertyChanged("name_visibility");
            return output;
        }

        /// <summary>
        /// Moves a point to another, but only if the new position is also within the parent or if the old position was outside the parents
        /// </summary>
        public AbsolutePoint MovePoint(AbsolutePoint old_position, AbsolutePoint new_position)
        {
            if (PointInParent(new_position) || !PointInParent(old_position))
            {
                AbsolutePoint output = border.MovePoint(old_position, new_position);
                if (output != null)
                {
                    RaisePropertyChanged("points");
                    RaisePropertyChanged("truepoints");
                    RaisePropertyChanged("center");
                    RaisePropertyChanged("margin");
                    RaisePropertyChanged("visibility");
                    RaisePropertyChanged("name_visibility");
                }
                return output;
            }
            return old_position;
        }

        /// <summary>
        /// Checks whether a given point is constrained by a polygon
        /// </summary>
        public bool PointInPolygon(AbsolutePoint point)
        {
            return border.PointInPolygon(point);
        }

        /// <summary>
        /// Checks whether a given point is On any of the edges of the polygon
        /// </summary>
        public bool PointOnPolygon(AbsolutePoint point)
        {
            return border.PointOnPolygon(point);
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
        public bool HasPoint(AbsolutePoint point)
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
        
        /// <summary>
        /// Returns the MedZoom for the region such that being zoomed in will fully encompass the region
        /// </summary>
        public override double GetMedZoom()
        {
            double myZoom = 1.0f;
            double MinX, MinY, MaxX, MaxY;
            MinX = MinY = Double.MaxValue;
            MaxX = MaxY = Double.MinValue;
            for(int i=0; i<border.vertices.Count; i++)
            {
                MinX = Math.Min(MinX, border.vertices[i].X);
                MinY = Math.Min(MinY, border.vertices[i].Y);
                MaxX = Math.Max(MaxX, border.vertices[i].X);
                MaxY = Math.Max(MaxY, border.vertices[i].Y);
            }
            double width_percent, height_percent;
            width_percent = ((double)(MaxX - MinX)) / Global.CanvasSize.X;
            height_percent = ((double)(MaxY - MinY)) / Global.CanvasSize.Y;
            myZoom = 1 / Math.Max(width_percent, height_percent);
            if (parent == null)
                return myZoom;
            return Math.Max(myZoom, parent.GetMedZoom());
        }

        public override AbsolutePoint GetCenter()
        {
            return border.GetCenter();
        }

        /// <summary>
        /// If the point is outside the boundaries of the level, returns the closest point within the boundaries
        /// </summary>
        public RenderedPoint Constrain(RenderedPoint point)
        {
            if (!PointInPolygon(point.ToAbsolutePoint()))
            {
                double distance = double.MaxValue;
                foreach(Line2D line in border.edges)
                {
                    distance = Math.Min(distance, line.RenderedDistance(point));
                }
                foreach(Line2D line in border.edges)
                {
                    if (Double.Equals(line.RenderedDistance(point), distance))
                        return line.GetClosestPoint(point.ToAbsolutePoint()).ToRenderedPoint();
                }
            }
            return point;
        }
    }

    /// <summary>
    /// Superclass for levels with a defined center
    /// </summary>
    public class PointLevel : SuperLevel
    {
        private AbsolutePoint _center;
        public AbsolutePoint center
        {
            get
            {
                return _center;
            }
            set
            {
                _center = value;
                RaisePropertyChanged("center");
                RaisePropertyChanged("center_r");
            }
        }

        public override double Level_MaxX
        {
            get
            {
                return Math.Max(base.Level_MaxX, center.X);
            }
        }
        public override double Level_MaxY
        {
            get
            {
                return Math.Max(base.Level_MaxY, center.Y);
            }
        }
        public override double Level_MinX
        {
            get
            {
                return Math.Min(base.Level_MinX, center.X);
            }
        }
        public override double Level_MinY
        {
            get
            {
                return Math.Min(base.Level_MinY, center.Y);
            }
        }

        public Point center_r
        {
            get
            {
                return center.ToRenderedPoint().ToWindowsPoint();
            }
        }

        public string margin
        {
            get
            {
                int intended_width = 50;
                int intended_height = 20;
                Point __center = center_r;
                string left, top, right, bottom;
                left = (__center.X - intended_width).ToString();
                top = (__center.Y - intended_height).ToString();
                right = ((Global.CanvasSize.X - __center.X) - intended_width).ToString();
                bottom = ((Global.CanvasSize.Y - __center.Y) - intended_height).ToString();
                return left + ',' + top + ',' + right + ',' + bottom;
            }
        }

        public string name_visibility
        {
            get
            {
                return center.ToRenderedPoint().visibility;
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
        protected PointLevel(string name, int level, LevelType leveltype, SuperLevel parent, AbsolutePoint center) : base(name, level, leveltype, parent)
        {
            if (PointInParent(center))
            {
                this.center = new AbsolutePoint(center);
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
        protected PointLevel(string name, int level, LevelType leveltype, string sublevel, SuperLevel parent, AbsolutePoint center) : base(name, level, leveltype, sublevel, parent)
        {
            if (PointInParent(center))
            {
                this.center = new AbsolutePoint(center);
            }
            else
            {
                this.center = null;
            }
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        public PointLevel(PointLevel o) : base(o)
        {
            this.center = o.center;
        }

        public override SuperLevel Copy()
        {
            return new PointLevel(this);
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
        public override AbsolutePoint GetCenter()
        {
            return new AbsolutePoint(this.center);
        }

        /// <summary>
        /// Moves the center somewhere else
        /// </summary>
        public bool MoveCenter(AbsolutePoint point)
        {
            if (PointInParent(point))
            {
                this.center = new AbsolutePoint(point);
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

        /// <summary>
        /// Forces an update to all point objects
        /// </summary>
        public override void ForceUpdatePoints()
        {
            RaisePropertyChanged("center");
            RaisePropertyChanged("center_r");
            RaisePropertyChanged("margin");
            RaisePropertyChanged("name_visibility");
            base.ForceUpdatePoints();
        }

        /// <summary>
        /// Returns the MedZoom for the region such that being zoomed in will fully encompass the region
        /// </summary>
        public override double GetMedZoom()
        {
            double myZoom = 1.0f;
            double width_percent, height_percent;
            width_percent = ((double) 20) / Global.CanvasSize.X;
            height_percent = ((double) 20) / Global.CanvasSize.Y;
            myZoom = 1 / Math.Max(width_percent, height_percent);

            if (parent == null)
                return myZoom;
            return Math.Max(myZoom, parent.GetMedZoom());
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
        public override bool CanFitPoint(AbsolutePoint point)
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
        private double _radius;
        public double radius
        {
            get
            {
                return _radius;
            }
            set
            {
                _radius = value;
                RaisePropertyChanged("radius");
                RaisePropertyChanged("radius_r");
                RaisePropertyChanged("radius_str");
            }
        }
        public double radius_r
        {
            get
            {
                return radius * Global.Zoom;
            }
        }
        public string radius_str
        {
            get
            {
                return radius_r.ToString();
            }
        }

        public override double Level_MaxX
        {
            get
            {
                return Math.Max(base.Level_MaxX, center.X + radius);
            }
        }
        public override double Level_MaxY
        {
            get
            {
                return Math.Max(base.Level_MaxY, center.Y + radius);
            }
        }
        public override double Level_MinX
        {
            get
            {
                return Math.Min(base.Level_MinX, center.X - radius);
            }
        }
        public override double Level_MinY
        {
            get
            {
                return Math.Min(base.Level_MinY, center.Y - radius);
            }
        }

        public Level5 thisLevel5
        {
            get
            {
                return this;
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
        public Level5(string name, LevelType leveltype, SuperLevel parent, AbsolutePoint center, double radius) : base(name, 5, leveltype, parent, center)
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
        public Level5(string name, LevelType leveltype, string sublevel, SuperLevel parent, AbsolutePoint center, double radius) : base(name, 5, leveltype, sublevel, parent, center)
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
        /// Copy constructor
        /// </summary>
        public Level5(Level5 o) : base(o)
        {
            this.radius = o.radius;
        }

        public override void ForceUpdatePoints()
        {
            RaisePropertyChanged("radius");
            RaisePropertyChanged("radius_r");
            RaisePropertyChanged("radius_str");
            base.ForceUpdatePoints();
        }

        public override SuperLevel Copy()
        {
            return new Level5(this);
        }

        /// <summary>
        /// Returns true if the point is within the radius of the center
        /// </summary>
        public bool PointInRadius(AbsolutePoint point)
        {
            return center.Distance(point) <= radius + 0.1;
        }
        
        /// <summary>
        /// Returns true if the passed point fits within the boundaries and level
        /// </summary>
        public override bool CanFitPoint(AbsolutePoint point)
        {
            return PointInRadius(point);
        }

        /// <summary>
        /// Returns true if the passed point fits within the boundaries
        /// </summary>
        public override bool FitsPoint(AbsolutePoint point)
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

        /// <summary>
        /// Returns the MedZoom for the region such that being zoomed in will fully encompass the region
        /// </summary>
        public override double GetMedZoom()
        {
            double myZoom = 1.0f;
            double width_percent, height_percent;
            width_percent = ((double)(radius * 2)) / Global.CanvasSize.X;
            height_percent = ((double)(radius * 2)) / Global.CanvasSize.Y;
            myZoom = 1 / Math.Max(width_percent, height_percent);
            if (parent == null)
                return myZoom;
            return Math.Max(myZoom, parent.GetMedZoom());
        }

        /// <summary>
        /// Checks whether the point can be snapped to the edge of the circle
        /// </summary>
        public bool SnapsToEdge(RenderedPoint point, double snap_range)
        {
            if(radius_r < 20)
                return false;
            return Math.Abs(Math.Abs(center.ToRenderedPoint().Distance(point)) - radius_r) <= snap_range;
        }

        /// <summary>
        /// Returns the point snapped to the edge of the circle, if possible
        /// </summary>
        public RenderedPoint SnapToEdge(RenderedPoint point, double snap_range)
        {
            if(SnapsToEdge(point, snap_range))
            {
                RenderedPoint direction = point - center.ToRenderedPoint();
                direction = (direction * radius_r) / direction.Length();
                direction = direction + center.ToRenderedPoint();
                if (point.Distance(direction) <= snap_range)
                {
                    return direction;
                }
            }
            return point;
        }

        /// <summary>
        /// If the point is outside the boundaries of the level, returns the closest point within the boundaries
        /// </summary>
        public RenderedPoint Constrain(RenderedPoint point)
        {
            if (!PointInRadius(point.ToAbsolutePoint()))
            {
                RenderedPoint output = RenderedPoint.Normalize(center.ToRenderedPoint() - point);
                output = output * radius_r;
                output = output + center.ToRenderedPoint();
                return output;
            }
            return point;
        }
    }

    /// <summary>
    /// Point Level 6 object class - Structures
    /// </summary>
    public class Level6 : PointLevel
    {
        public Level6 thisLevel6
        {
            get
            {
                return this;
            }
        }

        /// <summary>
        /// Basic constructor, creates a level 6 object given a name, level type, parent, and center
        /// </summary>
        /// <param name="name">Level name, must be a unique identifier among its siblings</param>
        /// <param name="leveltype">Level type, provides specific context that indicates what type of level it is of the 6 basic types or World</param>
        /// <param name="parent">Level parent, must be of same type as child or World</param>
        /// <param name="center">Level center</param>
        public Level6(string name, LevelType leveltype, SuperLevel parent, AbsolutePoint center) : base(name, 6, leveltype, parent, center)
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
        public Level6(string name, LevelType leveltype, string sublevel, SuperLevel parent, AbsolutePoint center) : base(name, 6, leveltype, sublevel, parent, center)
        {

        }
    }
}
