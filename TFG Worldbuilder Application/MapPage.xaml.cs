using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MapPage : Page
    {

        private Canvas MapCanvas;
        private int LevelNum = 0;
        private int LevelStep = 0;
        private string name = "";
        private string label = "";
        private Polygon2D vertices = new Polygon2D();
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
            this.DataContext = this.Context;
            if (Worlds.Count > 0)
            {
                Context.SetWorld(Worlds[0].name, Worlds[0].subtype);
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
            TextPrompt.Visibility = Visibility.Collapsed;
            ActiveJob = "None";
        }

        private void Text_Prompt_Confirm_Click(object sender, RoutedEventArgs e)
        {
            string prompt_text = TextPromptBox.Text.Trim();
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
                TextPrompt.Visibility = Visibility.Collapsed;
                if (string.Equals(ActiveJob, "Create"))
                {
                    switch (this.LevelStep) // Control for level step
                    {
                        case 2:
                            this.subtype = prompt_text;
                            this.LevelStep++;
                            OpenTextPrompt("Name your " + this.subtype + ":");
                            break;
                        case 3:
                            this.name = prompt_text;
                            switch (this.LevelNum)
                            {
                                case 1:
                                    NewWorld(this.name, this.subtype);
                                    break;
                                case 2:
                                    NewGreaterRegion(this.name, this.type, this.subtype, this.vertices);
                                    break;
                                    //ToDo: add more cases for other levels
                            }
                            break;
                    }
                }
                else if (string.Equals(ActiveJob, "Open"))
                {
                    switch (this.LevelNum) //Control for level type
                    {
                        case 1:
                            Context.SetWorld(prompt_text);
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
            LevelStep = 2;
            type = LevelType.World;
            ActiveJob = "Create";
            OpenTextPrompt("What type of world are you creating?\nEnter a subtype:");
        }

        /// <summary>
        /// Opens the popup and prepares to create a new greater region
        /// </summary>
        private void Create_Greater_Region()
        {
            LevelNum = 2;
            LevelStep = 2;
            ActiveJob = "Create";
            OpenTapPrompt("Enter at least 3 points for Greater Region");
        }

        private void OpenTapPrompt(string label)
        {
            this.label = label;
            vertices.vertices.Clear();
            TapPromptTab.Text = label + ": " + vertices.Size() + " points";
            TapPrompt.Visibility = Visibility.Visible;
            Context.Points = vertices.vertices;
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
                Context.SetActive(new Level1(name, subtype));
                Context.ActiveLevel.color = "LightSkyBlue";
                Global.ActiveFile.Worlds.Add((Level1) Context.ActiveLevel);
                for(int i=0; i<Worlds.Count-1; i++)
                {
                    Worlds[i].color = "#F2F2F2";
                }
            }
            ActiveJob = "None";
            UpdateSaveState();
        }

        /// <summary>
        /// Creates a new greater region object with the given name, type, subtype, and border
        /// </summary>
        private void NewGreaterRegion(string name, LevelType type, string subtype, Polygon2D border) {

        }

        /// <summary>
        /// Sets the ActiveWorld to the saved world defined by the sender event
        /// </summary>
        private void Open_World_Click(object sender, RoutedEventArgs e)
        {
            WorldsMenu.Hide();
            string name = ((MenuFlyoutItem)sender).Text.Trim();
            Context.SetWorld(name);
        }

        /// <summary>
        /// Sets focus to the prompt text box if available when the create menu is closed
        /// </summary>
        private void CreateMenu_Closed(object sender, object e)
        {
            if(TextPrompt.Visibility == Visibility.Visible)
                TextPromptBox.Focus(FocusState.Programmatic);
        }

        private void Create_Greater_Region_National_Click(object sender, RoutedEventArgs e)
        {
            type = LevelType.National;
            Create_Greater_Region();
        }

        private void Create_Greater_Region_Geographical_Click(object sender, RoutedEventArgs e)
        {
            type = LevelType.Geographical;
            Create_Greater_Region();
        }

        private void Create_Greater_Region_Climate_Click(object sender, RoutedEventArgs e)
        {
            type = LevelType.Climate;
            Create_Greater_Region();
        }

        private void Create_Greater_Region_Factional_Click(object sender, RoutedEventArgs e)
        {
            type = LevelType.Factional;
            Create_Greater_Region();
        }

        private void Create_Greater_Region_Cultural_Click(object sender, RoutedEventArgs e)
        {
            type = LevelType.Cultural;
            Create_Greater_Region();
        }

        private void Create_Greater_Region_Biological_Click(object sender, RoutedEventArgs e)
        {
            type = LevelType.Biological;
            Create_Greater_Region();
        }

        private void WorldCanvas_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (string.Equals(ActiveJob, "Create") && LevelNum > 1 && LevelNum < 5)
            {
                Point2D point = new Point2D(e.GetPosition((Windows.UI.Xaml.UIElement)sender));
                if (Context.ActiveLevel != null)
                {
                    if(Context.ActiveLevel.GetLevel() > 1 && Context.ActiveLevel.GetLevel() < 5)
                    {
                        try
                        {
                            if (((BorderLevel)Context.ActiveLevel).PointInPolygon(point))
                            {
                                vertices.AppendPoint(point);
                                Context.RaisePropertyChanged("Points");
                            }
                        } catch (InvalidCastException)
                        {
                            ;
                        }
                    }
                    else if (Context.ActiveLevel.GetLevel() == 5)
                    {
                        try
                        {
                            if (((Level5)Context.ActiveLevel).PointInRadius(point))
                            {
                                vertices.AppendPoint(point);
                                Context.RaisePropertyChanged("Points");
                            }
                        } catch (InvalidCastException)
                        {
                            ;
                        }
                    }
                }
                TapPromptTab.Text = label + ": " + vertices.Size() + " points";
            }
        }

        private void Tap_Prompt_Cancel_Click(object sender, RoutedEventArgs e)
        {
            TapPrompt.Visibility = Visibility.Collapsed;
            ActiveJob = "None";
            Context.UpdateShapesAndPoints();
        }

        private void Tap_Prompt_Confirm_Click(object sender, RoutedEventArgs e)
        {
            if(vertices.Size() < 3)
            {
                OpenPopupAlert("Requires at least 3 points");
            } else
            {
                LevelStep++;
                Context.UpdateShapesAndPoints();
                OpenTextPrompt("What type of" + Enum.GetName(typeof(LevelType), type) + "are you creating?\nEnter a subtype:");
            }
        }
    }
}
