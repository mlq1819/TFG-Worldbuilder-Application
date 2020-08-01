using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFG_Worldbuilder_Application
{
    public class SubtypeArchive
    {
        public ObservableCollection<Tuple<int, LevelType, string, string>> subtypes;

        public int Count
        {
            get
            {
                if (subtypes == null)
                    return -1;
                return subtypes.Count;
            }
        }

        public SubtypeArchive()
        {
            subtypes = new ObservableCollection<Tuple<int, LevelType, string, string>>();
        }

        public SubtypeArchive(IList<Tuple<int, LevelType, string, string>> list) : this()
        {
            foreach (Tuple<int, LevelType, string, string> item in list)
            {
                Add(item);
            }
        }

        public SubtypeArchive(SubtypeArchive o) : this(o.subtypes)
        {
            ;
        }

        /// <summary>
        /// Attempts to add the given item to the list of subtypes; returns true on success
        /// </summary>
        public bool Add(Tuple<int, LevelType, string, string> item)
        {
            if (item.Item1 <= 1 || item.Item1 > 6)
                return false;
            if (item.Item2 == LevelType.Invalid)
                return false;
            if (Has(item.Item3))
                return false;
            subtypes.Add(new Tuple<int, LevelType, string, string>(item.Item1, item.Item2, Capitalize(item.Item3), item.Item4));
            return true;
        }

        private int Add(SuperLevel level)
        {
            int count = 0;
            if (Add(new Tuple<int, LevelType, string, string>(level.level, level.leveltype, level.subtype, level.basecolor)))
                count++;
            else
            {
                Tuple<int, LevelType, string, string> current = Get(level.subtype);
                if(current.Item1 == level.level && current.Item2 == level.leveltype)
                {
                    level.Recolor(level.subtype);
                }
            }
            foreach (SuperLevel sublevel in level.sublevels)
            {
                count += Add(sublevel);
            }
            return count;
        }

        public int Add(IList<Level1> Worlds)
        {
            int count = 0;
            foreach (Level1 world in Worlds)
            {
                count += Add(world);
            }
            return count;
        }

        /// <summary>
        /// Checks whether a given subtype is in the list
        /// </summary>
        public bool Has(string subtype)
        {
            subtype = Capitalize(subtype);
            foreach (Tuple<int, LevelType, string, string> item in subtypes)
            {
                if (string.Equals(item.Item3, subtype))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Checks for an entry conflict; entry conflicts occur when a new entry shares a name with a member of subtypes but is not otherwise identical
        /// </summary>
        public bool Conflicts(Tuple<int, LevelType, string> entry)
        {
            return Conflicts(new Tuple<int, LevelType, string, string>(entry.Item1, entry.Item2, entry.Item3, SuperLevel.DefaultColor));
        }

        /// <summary>
        /// Checks for an entry conflict; entry conflicts occur when a new entry shares a name with a member of subtypes but is not otherwise identical
        /// </summary>
        public bool Conflicts(Tuple<int, LevelType, string, string> entry)
        {
            if (!Has(entry.Item3))
                return false;
            foreach (Tuple<int, LevelType, string, string> item in subtypes)
            {
                if (string.Equals(entry.Item3, item.Item3))
                {
                    return entry.Item1 != item.Item1 || entry.Item2 != item.Item2;
                }
            }
            return true;
        }

        /// <summary>
        /// Returns the tuple containing the given subtype, or null
        /// </summary>
        public Tuple<int, LevelType, string, string> Get(string subtype)
        {
            subtype = Capitalize(subtype);
            if (Has(subtype))
            {
                foreach (Tuple<int, LevelType, string, string> item in subtypes)
                {
                    if (string.Equals(subtype, item.Item3))
                        return item;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the type of the given subtype, or Invalid
        /// </summary>
        public LevelType GetType(string subtype)
        {
            Tuple<int, LevelType, string, string> output = Get(subtype);
            if (output != null)
            {
                return output.Item2;
            }
            return LevelType.Invalid;
        }

        /// <summary>
        /// Returns the level of the given subtype, or -1
        /// </summary>
        public int GetLevel(string subtype)
        {
            Tuple<int, LevelType, string, string> output = Get(subtype);
            if (output != null)
            {
                return output.Item1;
            }
            return -1;
        }

        /// <summary>
        /// Returns the color of the given subtype, or SuperLevel.DefaultColor
        /// </summary>
        public string GetColor(string subtype)
        {
            Tuple<int, LevelType, string, string> output = Get(subtype);
            if (output != null)
            {
                return output.Item4;
            }
            return SuperLevel.DefaultColor;
        }

        /// <summary>
        /// Sets the color of the given subtype to the given color; returns true on success
        /// </summary>
        public bool SetColor(string subtype, string color)
        {
            subtype = Capitalize(subtype);
            if (Has(subtype))
            {
                for (int i = 0; i < subtypes.Count; i++)
                {
                    if (string.Equals(subtypes[i].Item3, subtype))
                    {
                        subtypes[i] = new Tuple<int, LevelType, string, string>(subtypes[i].Item1, subtypes[i].Item2, subtypes[i].Item3, color);
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Returns a list of strings consisting of all subtypes of that type
        /// </summary>
        public ObservableCollection<StringContainer> GetSubtypes(int level, LevelType leveltype)
        {
            ObservableCollection<StringContainer> output = new ObservableCollection<StringContainer>();
            if (leveltype != LevelType.Invalid)
            {
                foreach (Tuple<int, LevelType, string, string> item in subtypes)
                {
                    if (item.Item1 == level && item.Item2 == leveltype)
                        output.Add(new StringContainer(item.Item3));
                }
            }
            return output;
        }

        public int CountType(int level, LevelType leveltype)
        {
            return GetSubtypes(level, leveltype).Count;
        }

        /// <summary>
        /// Capitalizes the first letter of each word, and de-capitalizes all other letters of each word, and trims extraneous whitespace
        /// </summary>
        public static string Capitalize(string str)
        {
            string output = "";
            bool last_space = true;
            foreach (char c in str)
            {
                if (c == ' ' || c == '\t' || c == '\n' || c == '-')
                {
                    if (!last_space)
                    {
                        if (c == '-')
                            output += '-';
                        else
                            output += ' ';
                    }
                    last_space = true;
                }
                else
                {
                    if (last_space)
                    {
                        output += char.ToUpper(c);
                    }
                    else
                    {
                        output += char.ToLower(c);
                    }
                    last_space = false;
                }
            }
            return output.Trim();
        }

        /// <summary>
        /// Generates a SubtypeArchive containing default subtypes
        /// </summary>
        public static SubtypeArchive DefaultSubtypes()
        {
            SubtypeArchive output = new SubtypeArchive();
            //National
            output.Add(new Tuple<int, LevelType, string, string>(2, LevelType.Geographical, "Empire", "#FFCC00"));

            output.Add(new Tuple<int, LevelType, string, string>(3, LevelType.Geographical, "Country", "#FF3385"));
            output.Add(new Tuple<int, LevelType, string, string>(3, LevelType.Geographical, "Nation", "#FF3385"));
            output.Add(new Tuple<int, LevelType, string, string>(3, LevelType.Geographical, "Kingdom", "#8533FF"));

            output.Add(new Tuple<int, LevelType, string, string>(4, LevelType.Geographical, "Territory", "#4747D1"));
            output.Add(new Tuple<int, LevelType, string, string>(4, LevelType.Geographical, "State", "#4747D1"));
            output.Add(new Tuple<int, LevelType, string, string>(4, LevelType.Geographical, "Province", "#4747D1"));
            output.Add(new Tuple<int, LevelType, string, string>(4, LevelType.Geographical, "Commonwealth", "#4747D1"));

            output.Add(new Tuple<int, LevelType, string, string>(5, LevelType.Geographical, "Capital", "#E6AC00"));
            output.Add(new Tuple<int, LevelType, string, string>(5, LevelType.Geographical, "City", "#FF4D4D"));
            output.Add(new Tuple<int, LevelType, string, string>(5, LevelType.Geographical, "Town", "#FF8080"));
            output.Add(new Tuple<int, LevelType, string, string>(5, LevelType.Geographical, "Village", "#FFB366"));

            output.Add(new Tuple<int, LevelType, string, string>(6, LevelType.Geographical, "Courthouse", "#E6E6E6"));
            output.Add(new Tuple<int, LevelType, string, string>(6, LevelType.Geographical, "Parliament", "#9999FF"));
            output.Add(new Tuple<int, LevelType, string, string>(6, LevelType.Geographical, "Congress", "#9999FF"));
            output.Add(new Tuple<int, LevelType, string, string>(6, LevelType.Geographical, "Town Hall", "#6666FF"));
            output.Add(new Tuple<int, LevelType, string, string>(6, LevelType.Geographical, "Jail", "#002DB3"));
            output.Add(new Tuple<int, LevelType, string, string>(6, LevelType.Geographical, "Prison", "#002DB3"));

            //Geographical
            output.Add(new Tuple<int, LevelType, string, string>(2, LevelType.Geographical, "Continent", "#4D9900"));
            output.Add(new Tuple<int, LevelType, string, string>(2, LevelType.Geographical, "Ocean", "#0040FF"));

            output.Add(new Tuple<int, LevelType, string, string>(3, LevelType.Geographical, "Sub-Continent", "#59B300"));
            output.Add(new Tuple<int, LevelType, string, string>(3, LevelType.Geographical, "Sea", "#3399FF"));
            output.Add(new Tuple<int, LevelType, string, string>(3, LevelType.Geographical, "Continental Island", "#99CC00"));

            output.Add(new Tuple<int, LevelType, string, string>(4, LevelType.Geographical, "Peninsula", "#66CC00"));
            output.Add(new Tuple<int, LevelType, string, string>(4, LevelType.Geographical, "Bay", "#33FFD6"));
            output.Add(new Tuple<int, LevelType, string, string>(4, LevelType.Geographical, "Lake", "#33CCFF"));
            output.Add(new Tuple<int, LevelType, string, string>(4, LevelType.Geographical, "Mountain Range", "#A6A6A6"));
            output.Add(new Tuple<int, LevelType, string, string>(4, LevelType.Geographical, "Plain", "#CCFF66"));
            output.Add(new Tuple<int, LevelType, string, string>(4, LevelType.Geographical, "Canyon", "#E67300"));
            output.Add(new Tuple<int, LevelType, string, string>(4, LevelType.Geographical, "Island", "#ACE600"));

            output.Add(new Tuple<int, LevelType, string, string>(5, LevelType.Geographical, "Mountain", "#8C8C8C"));
            output.Add(new Tuple<int, LevelType, string, string>(5, LevelType.Geographical, "Volcano", "#B18F81"));
            output.Add(new Tuple<int, LevelType, string, string>(5, LevelType.Geographical, "Pond", "#80DFFF"));
            output.Add(new Tuple<int, LevelType, string, string>(5, LevelType.Geographical, "Valley", "#FF9933"));

            output.Add(new Tuple<int, LevelType, string, string>(6, LevelType.Geographical, "Mountain Peak", "#E6E6E6"));
            output.Add(new Tuple<int, LevelType, string, string>(6, LevelType.Geographical, "Cliff", "#7575A3"));

            //Climate


            //Factional


            //Cultural


            //Biological


            return output;
        }
    }
}
