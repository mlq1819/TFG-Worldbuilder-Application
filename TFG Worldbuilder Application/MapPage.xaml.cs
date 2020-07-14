using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TFG_Worldbuilder_Application
{
    /// <summary>
    /// DataContext bindable object for convenience sake
    /// </summary>
    public class ActiveContext : INotifyPropertyChanged
    {
        public ObservableCollection<Level1> Worlds;
        public SuperLevel ActiveLevel;
        public ObservableCollection<BorderLevel> Shapes;
        public ObservableCollection<PointLevel> Points;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string str)
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

        private void SetActive(SuperLevel level)
        {
            ActiveLevel = level;
            SetWorld();
            RaisePropertyChanged("ActiveLevel");
        }

        /// <summary>
        /// Updates the Shapes and Points to match the ActiveWorld
        /// </summary>
        private void SetWorld()
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
                    Points.Add((PointLevel)temp[i]);
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
            if (string.Equals(ActiveLevel.GetName(), name))
                return true;
            for(int i=0; i<Worlds.Count; i++)
            {
                if(string.Equals(Worlds[i].GetName(), name))
                {
                    ActiveLevel = Worlds[i];
                    SetWorld();
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
            if (string.Equals(ActiveLevel.GetName(), name) && string.Equals(ActiveLevel.subtype, subtype))
                return true;
            for (int i = 0; i < Worlds.Count; i++)
            {
                if (string.Equals(Worlds[i].GetName(), name) && string.Equals(Worlds[i].subtype, subtype))
                {
                    ActiveLevel = Worlds[i];
                    SetWorld();
                    return true;
                }
            }
            return false;
        }
    }


/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MapPage : Page
    {

        private Canvas MapCanvas;
        private Level1 ActiveWorld = null;
        private int LevelNum = 0;
        private int LevelStep = 0;
        private string name = "";
        private LevelType type = LevelType.Invalid;
        private string subtype = "";
        private string ActiveJob = "";
        public ActiveContext Context;
        public ObservableCollection<Level1> Worlds;
        
        public MapPage()
        {
            this.InitializeComponent();
            this.FileNameBlock.Text = Global.ActiveFile.FileName();
            this.MapCanvas = (Canvas)this.FindName("WorldCanvas");
            this.Worlds = Global.ActiveFile.Worlds;
            this.Context = new ActiveContext(Global.ActiveFile.Worlds);
            this.DataContext = this.Worlds;
            if (Worlds.Count > 0)
            {
                
                OpenWorld(Worlds[0].name, Worlds[0].subtype);
            }
            UpdateSaveState();
        }

        /// <summary>
        /// TODO :: Warns the user that their current file is unsaved, and asks if they'd like to either save it, lose the changes, or cancel the operation. Returns false on cancelled operation.
        /// </summary>
        private bool WarnClose()
        {
            return true;
        }

        /// <summary>
        /// Updates the current save status of the file
        /// </summary>
        private void UpdateSaveState()
        {
            if (Global.ActiveFile.MatchesSave())
            {
                this.FileNameBlock.Text = Global.ActiveFile.FileName();
            } else
            {
                this.FileNameBlock.Text = Global.ActiveFile.FileName() + '*';
            }
        }

        /// <summary>
        /// Creates a new file and navigates to it if the user decides to do so
        /// </summary>
        private async void File_New_Click(object sender, RoutedEventArgs e)
        {
            var savePicker = new Windows.Storage.Pickers.FileSavePicker();
            savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            // Dropdown of file types the user can save the file as
            savePicker.FileTypeChoices.Add("Prime Worldbuilding File", new List<string>() { ".prm_world" });
            // Default file name if the user does not type one in or select a file to replace
            savePicker.SuggestedFileName = "New Project";
            Windows.Storage.StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                if (Global.ActiveFile.MatchesSave() || WarnClose())
                {
                    // Application now has read/write access to the picked file
                    Global.ActiveFile = new FileManager(file, true);
                    await Global.ActiveFile.ReadyFile();
                    this.Frame.Navigate(typeof(MapPage));
                }
            }
        }

        /// <summary>
        /// Opens the Popup Alert with the given message
        /// </summary>
        private void OpenPopupAlert(string text)
        {
            PopupAlertText.Text = text;
            PopupAlert.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Checks if the current project has unsaved work, then opens the requested file
        /// </summary>
        private async void File_Open_Click(object sender, RoutedEventArgs e)
        {
            if(Global.ActiveFile.MatchesSave() || WarnClose())
            {
                var picker = new Windows.Storage.Pickers.FileOpenPicker();
                picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
                picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
                picker.FileTypeFilter.Add(".prm_world");
                Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
                if (file != null)
                {
                    // Application now has read/write access to the picked file
                    Global.ActiveFile = new FileManager(file, false);
                    await Global.ActiveFile.ReadyFile();
                    if (Global.ActiveFile.Valid())
                        this.Frame.Navigate(typeof(MapPage));
                    else
                    {
                        OpenPopupAlert("File not formatted for Worldbuilding - Invalid File");
                    }
                }
            }
        }

        /// <summary>
        /// Saves the active file
        /// </summary>
        private async void File_Save_Click(object sender, RoutedEventArgs e)
        {
            await Global.ActiveFile.SaveFile();
            UpdateSaveState();
        }

        /// <summary>
        /// Creates a copy of the active file with a new name
        /// </summary>
        private async void File_SaveAs_Click(object sender, RoutedEventArgs e)
        {
            var savePicker = new Windows.Storage.Pickers.FileSavePicker();
            savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            // Dropdown of file types the user can save the file as
            savePicker.FileTypeChoices.Add("Prime Worldbuilding File", new List<string>() { ".prm_world" });
            // Default file name if the user does not type one in or select a file to replace
            savePicker.SuggestedFileName = Global.ActiveFile.FileName();
            Windows.Storage.StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                await Windows.Storage.FileIO.WriteTextAsync(file, Global.ActiveFile.GetCopy());
                // Application now has read/write access to the picked file
                Global.ActiveFile = new FileManager(file, true);
                await Global.ActiveFile.ReadyFile();
                this.Frame.Navigate(typeof(MapPage));
            }
            UpdateSaveState();
        }

        /// <summary>
        /// Closes the file and returns to the main menu
        /// </summary>
        private void File_Close_Click(object sender, RoutedEventArgs e)
        {
            if (Global.ActiveFile.MatchesSave() || WarnClose())
            {
                this.Frame.Navigate(typeof(MainPage));
            }
        }

        /// <summary>
        /// Exits the program
        /// </summary>
        private void File_Exit_Click(object sender, RoutedEventArgs e)
        {
            if (Global.ActiveFile.MatchesSave() || WarnClose())
            {
                CoreApplication.Exit();
            }
        }
        
        /// <summary>
        /// Closes the popup
        /// </summary>
        private void PopupAlertButton_Click(object sender, RoutedEventArgs e)
        {
            PopupAlert.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Opens the text prompt popup
        /// </summary>
        private void OpenTextPrompt(string Label)
        {
            TextPromptBox.Text = "";
            TextPromptTab.Text = Label;
            TextPrompt.Visibility = Visibility.Visible;
            TextPromptBox.Focus(FocusState.Programmatic);
        }

        private void Text_Prompt_Cancel_Click(object sender, RoutedEventArgs e)
        {
            ((Grid)this.FindName("TextPrompt")).Visibility = Visibility.Collapsed;
        }

        private void Text_Prompt_Confirm_Click(object sender, RoutedEventArgs e)
        {
            string prompt_text = ((TextBox)this.FindName("TextPromptBox")).Text.Trim();
            if (prompt_text.Length > 255)
                prompt_text = prompt_text.Substring(0, 255).Trim();
            if(prompt_text.Length == 0)
            {
                OpenPopupAlert("Text must contain at least one (1) character");
            }
            else if (Global.ActiveFile.HasKeyword(prompt_text))
            {
                OpenPopupAlert("Supplied text contains illegal keyword: \"" + Global.ActiveFile.GetKeyword(prompt_text) + "\"");
            }
            else
            {
                ((Grid)this.FindName("TextPrompt")).Visibility = Visibility.Collapsed;
                if (string.Equals(ActiveJob, "Create"))
                {
                    switch (this.LevelStep)
                    {
                        case 1:
                            this.subtype = prompt_text;
                            this.LevelStep++;
                            OpenTextPrompt("Name your " + this.subtype + ":");
                            break;
                        case 2:
                            this.name = prompt_text;
                            switch (this.LevelNum)
                            {
                                case 1:
                                    NewWorld(this.name, this.subtype);
                                    break;
                                case 2:
                                    //ToDo: add call to CreateGreaterRegion
                                    break;
                                    //ToDo: add more cases for other levels
                            }
                            break;
                    }
                }
                else if (string.Equals(ActiveJob, "Open"))
                {
                    switch (this.LevelNum)
                    {
                        case 1:
                            OpenWorld(prompt_text);
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Opens the popup and prepares to create a new continent
        /// </summary>
        private void Create_World_Click(object sender, RoutedEventArgs e)
        {
            LevelNum = 1;
            LevelStep = 1;
            type = LevelType.World;
            ActiveJob = "Create";
            OpenTextPrompt("What type of world are you creating?\nEnter a subtype:");
        }

        /// <summary>
        /// Opens the popup and prepares to create a new continent
        /// </summary>
        private void Create_Continent_Click(object sender, RoutedEventArgs e)
        {
            LevelNum = 2;
            LevelStep = 1;
            ActiveJob = "Create";
            OpenTextPrompt("What are you creating?\nEnter a type:");
        }

        /// <summary>
        /// Creates a new world object with the given name and subtype
        /// </summary>
        private void NewWorld(string name, string subtype)
        {
            if(Global.ActiveFile.HasWorld(name, subtype))
            {
                OpenPopupAlert("Error: " + type + " with name \"" + name + "\" already exists");
            } else
            {
                ActiveWorld = new Level1(name, subtype);
                ActiveWorld.color = "LightSkyBlue";
                Global.ActiveFile.Worlds.Add(ActiveWorld);
                for(int i=0; i<Worlds.Count-1; i++)
                {
                    Worlds[i].color = "#F2F2F2";
                }
            }
            UpdateSaveState();
        }

        /// <summary>
        /// Sets the ActiveWorld to the saved world with the specified name
        /// </summary>
        private void OpenWorld(string name)
        {
            bool found_world = false;
            for(int i=0; i<Global.ActiveFile.Worlds.Count; i++)
            {
                if (!found_world && string.Equals(name, Global.ActiveFile.Worlds[i].GetName()))
                {
                    found_world = true;
                    if (ActiveWorld == null || !string.Equals(ActiveWorld.name, Worlds[i].name))
                    {
                        ActiveWorld = Worlds[i];
                        //Update the load state -- this does not affect the save state
                    }
                    Worlds[i].color = "LightSkyBlue";
                }
                else
                    Worlds[i].color = "#F2F2F2";
            }
        }

        /// <summary>
        /// Sets the ActiveWorld to the saved world with the specified name and type
        /// </summary>
        private void OpenWorld(string name, string subtype)
        {
            bool found_world = false;
            for (int i = 0; i < Global.ActiveFile.Worlds.Count; i++)
            {
                if (!found_world && string.Equals(name, Global.ActiveFile.Worlds[i].GetName()) && string.Equals(subtype, Global.ActiveFile.Worlds[i].subtype))
                {
                    found_world = true;
                    if (ActiveWorld == null || !string.Equals(ActiveWorld.name, Worlds[i].name))
                    {
                        ActiveWorld = Worlds[i];
                        //Update the load state -- this does not affect the save state
                    }
                    Worlds[i].color = "LightSkyBlue";
                }
                else
                    Worlds[i].color = "#F2F2F2";
            }
        }

        /// <summary>
        /// Sets the ActiveWorld to the saved world defined by the sender event
        /// </summary>
        private void Open_World_Click(object sender, RoutedEventArgs e)
        {
            WorldsMenu.Hide();
            string name = ((MenuFlyoutItem)sender).Text.Trim();
            OpenWorld(name);
        }

        /// <summary>
        /// Sets focus to the prompt text box if available when the create menu is closed
        /// </summary>
        private void CreateMenu_Closed(object sender, object e)
        {
            if(((Grid)this.FindName("TextPrompt")).Visibility == Visibility.Visible)
                ((TextBox)this.FindName("TextPromptBox")).Focus(FocusState.Programmatic);
        }
    }
}
