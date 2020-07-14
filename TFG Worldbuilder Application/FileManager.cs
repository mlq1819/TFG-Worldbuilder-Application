using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFG_Worldbuilder_Application
{
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
            Keywords.Add("Level Subtype");
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
        /// Returns true if there is a world that has the same name and subtype
        /// </summary>
        public bool HasWorld(string name, string subtype)
        {
            if (Readable) { 
                for (int i=0; i<Worlds.Count; i++)
                {
                    if (string.Equals(name, Worlds[i].GetName()) && string.Equals(subtype, Worlds[i].subtype))
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
            LevelType level_type = LevelType.Invalid;
            string level_subtype = "null";
            string line = "";
            Polygon2D border = new Polygon2D();
            Point2D center = null;
            long radius = 0;
            int index = 0;
            int length = 0;
            SuperLevel level = null;
            bool got_name = false;
            bool got_type = false;
            bool got_subtype = false;
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
                if (line.IndexOf("Level Name") == 0) //Beginning of level information processing; starting with Level Name
                {
                    do_it = false;
                    if (!got_name)
                    {
                        level_name = line.Substring("Level Name".Length + 1).Trim();
                        got_name = true;
                    }
                }
                else if (line.IndexOf("Level Type") == 0) //Level information processing for Level Type
                {
                    do_it = false;
                    if (!got_type)
                    {
                        line = line.Substring("Level Type".Length + 1).Trim();
                        for(short i = -1; i <= 6; i++)
                            {
                            if (string.Equals(line, Enum.GetName(typeof(LevelType), ((LevelType)i))))
                            {
                                level_type = (LevelType)i;
                                got_type = true;
                                break;
                            }
                        }
                        if (!got_type)
                        {
                            try
                            {
                                level_type = (LevelType)Convert.ToInt16(line);
                                got_type = true;
                            }
                            catch (FormatException)
                            {
                                ;
                            }
                        }
                    }
                }
                else if (line.IndexOf("Level Subtype") == 0) //Level information processing for Level Subtype
                {
                    do_it = false;
                    if (!got_subtype)
                    {
                        level_subtype = line.Substring("Level Subtype".Length + 1).Trim();
                        got_subtype = true;
                    }
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
                else if (line.IndexOf("Center") == 0) //Level information processing for Center point
                {
                    do_it = false;
                    if (!got_center)
                    {
                        line = line.Substring("Center".Length + 1).Trim();
                        center = Point2D.FromString(line);
                        if (center != null)
                        {
                            got_center = true;
                        }
                    }
                }
                else if (line.IndexOf("Radius") == 0) //Level information processing for Radius
                {
                    do_it = false;
                    if (!got_radius)
                    {
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
                                if(got_subtype)
                                    level = new Level1(level_name, level_subtype);
                                else
                                    level = new Level1(level_name);
                                break;
                            case 2:
                                if (got_subtype)
                                    level = new Level2(level_name, level_type, level_subtype, (Level1)parent, border);
                                else
                                    level = new Level2(level_name, level_type, (Level1)parent, border);
                                break;
                            case 3:
                                if (got_subtype)
                                    level = new Level3(level_name, level_type, level_subtype, parent, border);
                                else
                                    level = new Level3(level_name, level_type, parent, border);
                                break;
                            case 4:
                                if (got_subtype)
                                    level = new Level4(level_name, level_type, level_subtype, parent, border);
                                else
                                    level = new Level4(level_name, level_type, parent, border);
                                break;
                            case 5:
                                if(got_subtype)
                                    level = new Level5(level_name, level_type, level_subtype, parent, center, radius);
                                else
                                    level = new Level5(level_name, level_type, parent, center, radius);
                                break;
                            case 6:
                                if(got_subtype)
                                    level = new Level6(level_name, level_type, level_subtype, parent, center);
                                else
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
                    if (do_it && made_level) //Now for the actual content processing of the level
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
                            if(end >= index) //Index is not length; this *should* be >=
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
                if (this.active_lines.Count > 0 && string.Equals(this.active_lines[0], "Prime Worldbuilding File"))
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
        private async Task FormatNewFile()
        {
            if (!this.ValidFile && NewFile)
            {
                UpdateText();
                await SaveFile();
                this.ValidFile = true;
                this.Writable = true;
                NewFile = false;
            }
        }
    }
}
