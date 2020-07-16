using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Windows.Foundation;

namespace TFG_Worldbuilder_Application
{

    /// <summary>
    /// DataContext bindable object for convenience sake; manages Active information for the level
    /// </summary>
    public class ActiveContext : INotifyPropertyChanged
    {
        public ObservableCollection<Level1> Worlds;
        public SuperLevel ActiveLevel;
        public ObservableCollection<BorderLevel> Shapes;
        public ObservableCollection<Level5> Circles;
        public ObservableCollection<Level6> Points;
        public MyPointCollection ExtraPoints;

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string str)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(str));
            }
        }

        public ActiveContext()
        {
            this.Worlds = new ObservableCollection<Level1>();
            this.ActiveLevel = null;
            this.Shapes = new ObservableCollection<BorderLevel>();
            this.Circles = new ObservableCollection<Level5>();
            this.Points = new ObservableCollection<Level6>();
            this.ExtraPoints = new MyPointCollection();
        }

        public ActiveContext(ObservableCollection<Level1> Worlds)
        {
            this.Worlds = Worlds;
            this.ActiveLevel = null;
            this.Shapes = new ObservableCollection<BorderLevel>();
            this.Circles = new ObservableCollection<Level5>();
            this.Points = new ObservableCollection<Level6>();
            this.ExtraPoints = new MyPointCollection();
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
                ActiveLevel.color = "LightSkyBlue";
                UpdateShapesAndPoints();
            } else
            {
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
        /// Updates the Shapes, Circles, and Points to match the ActiveWorld
        /// </summary>
        public void UpdateShapesAndPoints()
        {
            IList<SuperLevel> temp = SuperLevel.Filter(ActiveLevel.GetSublevels(), 2);
            temp.Concat<SuperLevel>(SuperLevel.Filter(ActiveLevel.GetSublevels(), 3));
            temp.Concat<SuperLevel>(SuperLevel.Filter(ActiveLevel.GetSublevels(), 4));
            Shapes = new ObservableCollection<BorderLevel>();
            for (int i = 0; i < temp.Count; i++)
            {
                try
                {
                    Shapes.Add((BorderLevel)temp[i]);
                }
                catch (InvalidCastException)
                {
                    ;
                }
            }
            RaisePropertyChanged("Shapes");
            temp = SuperLevel.Filter(ActiveLevel.GetSublevels(), 5);
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
            temp = SuperLevel.Filter(ActiveLevel.GetSublevels(), 6);
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
        }

        /// <summary>
        /// Sets this.ExtraPoints to the list
        /// </summary>
        public void SetPoints(IList<Point2D> list)
        {
            this.ExtraPoints = new MyPointCollection();
            for(int i=0; i<list.Count; i++)
            {
                this.ExtraPoints.AppendPoint(new Point2D(list[i].X, list[i].Y));
            }
            RaisePropertyChanged("ExtraPoints");
        }

        /// <summary>
        /// Clears the ExtraPoints
        /// </summary>
        public void ClearPoints()
        {
            this.ExtraPoints = new MyPointCollection();
            ((MyPointCollection) this.ExtraPoints).RaisePropertyChanged("points");
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
    }
}
