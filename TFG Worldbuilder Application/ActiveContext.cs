using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public ObservableCollection<Point2D> Points;

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
            this.Points = new ObservableCollection<PointLevel>();
        }

        public ActiveContext(ObservableCollection<Level1> Worlds)
        {
            this.Worlds = Worlds;
            this.ActiveLevel = null;
            this.Shapes = new ObservableCollection<BorderLevel>();
            this.Points = new ObservableCollection<PointLevel>();
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
                Shapes.Clear();
                Points.Clear();
            }
            RaisePropertyChanged("ActiveLevel");
        }

        /// <summary>
        /// Updates the Shapes and Points to match the ActiveWorld
        /// </summary>
        public void UpdateShapesAndPoints()
        {

            IList<SuperLevel> temp = SuperLevel.Filter(ActiveLevel.GetSublevels(), 2);
            temp.Concat<SuperLevel>(SuperLevel.Filter(ActiveLevel.GetSublevels(), 3));
            temp.Concat<SuperLevel>(SuperLevel.Filter(ActiveLevel.GetSublevels(), 4));
            Shapes.Clear();
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
            temp = SuperLevel.Filter(ActiveLevel.GetSublevels(), 5);
            temp.Concat<SuperLevel>(SuperLevel.Filter(ActiveLevel.GetSublevels(), 6));
            Points.Clear();
            for (int i = 0; i < temp.Count; i++)
            {
                try
                {
                    Points.Add(((PointLevel)temp[i]).GetCenter());
                }
                catch (InvalidCastException)
                {
                    ;
                }
            }
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
