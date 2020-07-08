using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MapPage : Page
    {

        private Canvas MapCanvas;
        private Level1 ActiveWorld = null;
        private int LevelNum = 0;
        private int LevelStep = 0;
        private string name = "";
        private string type = "";
        private string ActiveJob = "";
        

        /// <ToDo>
        /// Look into Data Bindings for Xaml; you'll need to master those to understand this
        /// </ToDo>

        public MapPage()
        {
            this.InitializeComponent();
            this.FileNameBlock.Text = Global.ActiveFile.FileName();
            this.MapCanvas = (Canvas)this.FindName("WorldCanvas");
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
            Global.ActiveFile.UpdateText();
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
                    Global.ActiveFile.FormatNewFile();
                    this.Frame.Navigate(typeof(MapPage));
                }
            }
        }

        /// <summary>
        /// Opens the Popup Alert with the given message
        /// </summary>
        private void OpenPopupAlert(string text)
        {
            ((TextBlock)this.FindName("PopupAlertText")).Text = text;
            ((Grid)this.FindName("PopupAlert")).Visibility = Visibility.Visible;
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
        private void File_Save_Click(object sender, RoutedEventArgs e)
        {
            Global.ActiveFile.SaveFile();
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
                Global.ActiveFile.FormatNewFile();
                this.Frame.Navigate(typeof(MapPage));
            }
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
                Application.Current.Exit();
            }
        }
        
        /// <summary>
        /// Closes the popup
        /// </summary>
        private void PopupAlertButton_Click(object sender, RoutedEventArgs e)
        {
            ((Grid)this.FindName("PopupAlert")).Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Opens the text prompt popup
        /// </summary>
        private void OpenTextPrompt(string Label)
        {
            ((TextBox)this.FindName("TextPromptBox")).Text = "";
            ((TextBlock)this.FindName("TextPromptTab")).Text = Label;
            ((Grid)this.FindName("TextPrompt")).Visibility = Visibility.Visible;
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
                            this.type = prompt_text;
                            this.LevelStep++;
                            OpenTextPrompt("Name your " + this.type + ":");
                            break;
                        case 2:
                            this.name = prompt_text;
                            switch (this.LevelNum)
                            {
                                case 1:
                                    NewWorld(this.name, this.type);
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
            ActiveJob = "Create";
            OpenTextPrompt("What are you creating?\nEnter a type:");
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
        /// Creates a new world object with the given name and type
        /// </summary>
        private void NewWorld(string name, string type)
        {
            if(Global.ActiveFile.HasWorld(name, type))
            {
                OpenPopupAlert("Error: " + type + " with name \"" + name + "\" already exists");
            } else
            {
                ActiveWorld = new Level1(name, type);
                Global.ActiveFile.Worlds.Add(ActiveWorld);
            }
            UpdateSaveState();
        }

        /// <summary>
        /// Sets the ActiveWorld to the saved world with the specified name
        /// </summary>
        private void OpenWorld(string name)
        {
            for(int i=0; i<Global.ActiveFile.Worlds.Count; i++)
            {
                if(string.Equals(name, Global.ActiveFile.Worlds[i].GetName()))
                {
                    ActiveWorld = Global.ActiveFile.Worlds[i];
                    break;
                }
            }
            UpdateSaveState();
        }

        /// <summary>
        /// Sets the ActiveWorld to the saved world with the specified name and type
        /// </summary>
        private void OpenWorld(string name, string type)
        {
            for (int i = 0; i < Global.ActiveFile.Worlds.Count; i++)
            {
                if (string.Equals(name, Global.ActiveFile.Worlds[i].GetName()) && string.Equals(type, Global.ActiveFile.Worlds[i].GetType()))
                {
                    ActiveWorld = Global.ActiveFile.Worlds[i];
                    break;
                }
            }
        }
    }
}
