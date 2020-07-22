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

        private Canvas _mapcanvas;
        public Canvas MapCanvas
        {
            get
            {
                return _mapcanvas;
            }
            set
            {
                _mapcanvas = value;
                Global.RenderCanvas = value;
            }
        }
        private int LevelNum = 0;
        private int LevelStep = 0;
        private string name = "";
        private string label = "";
        private Polygon2D vertices = new Polygon2D();
        private LevelType type = LevelType.Invalid;
        private string subtype = "";
        private string ActiveJob = "";
        private bool ForceClose = false;
        public ActiveContext Context;
        public ObservableCollection<Level1> Worlds;

        public MapPage()
        {
            this.InitializeComponent();
            ForceClose = false;
            this.FileNameBlock.Text = Global.ActiveFile.FileName();
            this.Worlds = Global.ActiveFile.Worlds;
            this.Context = new ActiveContext(Global.ActiveFile.Worlds);
            this.DataContext = this.Context;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            this.MapCanvas = WorldCanvas;
            Context.RaisePropertyChanged("CenterX");
            Context.RaisePropertyChanged("CenterY");
            Context.RaisePropertyChanged("MaxX");
            Context.RaisePropertyChanged("MaxY");
            if (Worlds.Count > 0)
            {
                Context.SetWorld(Worlds[0].name, Worlds[0].subtype);
                Context.UpdateAll();
            }
            UpdateSaveState();
            ResetZoom();
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
                if (Global.ActiveFile.MatchesSave() || ForceClose)
                {
                    // Application now has read/write access to the picked file
                    Global.ActiveFile = new FileManager(file, true);
                    await Global.ActiveFile.ReadyFile();
                    this.Frame.Navigate(typeof(MapPage));
                }
            }
            else
            {
                ActiveJob = "New File";
                OpenUnsavedWorkAlert();
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
        /// Opens the Unsaved Work Alert
        /// </summary>
        private void OpenUnsavedWorkAlert()
        {
            UnsavedWorkAlertText.Text = "You have unsaved work.\nContinue anyway?";
            UnsavedWorkAlert.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Checks if the current project has unsaved work, then opens the requested file
        /// </summary>
        private async void File_Open_Click(object sender, RoutedEventArgs e)
        {
            if(Global.ActiveFile.MatchesSave() || ForceClose)
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
            else
            {
                ActiveJob = "Open File";
                OpenUnsavedWorkAlert();
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
            if (Global.ActiveFile.MatchesSave() || ForceClose)
            {
                this.Frame.Navigate(typeof(MainPage));
            } else
            {
                ActiveJob = "Close File";
                OpenUnsavedWorkAlert();
            }
        }

        /// <summary>
        /// Exits the program
        /// </summary>
        private void File_Exit_Click(object sender, RoutedEventArgs e)
        {
            if (Global.ActiveFile.MatchesSave() || ForceClose)
            {
                CoreApplication.Exit();
            } else {
                ActiveJob = "Exit";
                OpenUnsavedWorkAlert();
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
            LevelStep = 1;
            ActiveJob = "Create";
            vertices = new Polygon2D();
            Context.ClearPoints();
            OpenTapPrompt("Enter at least 3 points for Greater Region");
        }

        /// <summary>
        /// Opens the tap prompt with the given label (label will be updated as points are added)
        /// </summary>
        /// <param name="label"></param>
        private void OpenTapPrompt(string label)
        {
            this.label = label;
            vertices.vertices.Clear();
            TapPromptTab.Text = label + ": " + vertices.Size() + " points";
            TapPrompt.Visibility = Visibility.Visible;
            Context.SetPoints(vertices.vertices);
        }

        /// <summary>
        /// Creates a new world object with the given name and subtype
        /// </summary>
        private void NewWorld(string name, string subtype)
        {
            if(Global.ActiveFile.HasWorld(name, subtype))
            {
                OpenPopupAlert("Error: " + Enum.GetName(typeof(LevelType), type) + " with name \"" + name + "\" already exists");
            } else
            {
                Context.SetActive(new Level1(name, subtype));
                Global.ActiveFile.Worlds.Add((Level1) Context.ActiveLevel);
            }
            ActiveJob = "None";
            UpdateSaveState();
        }

        /// <summary>
        /// Creates a new greater region object with the given name, type, subtype, and border
        /// </summary>
        private void NewGreaterRegion(string name, LevelType type, string subtype, Polygon2D border) {
            Context.ClearPoints();
            if (Context.ActiveLevel.HasSublevelWithName(name))
            {
                OpenPopupAlert("Error: " + Enum.GetName(typeof(LevelType), type) + " sublevel with name \"" + name + "\" already exists");
            } else
            {
                Level2 new_level = new Level2(name, type, subtype, (Level1)Context.ActiveLevel, border);
                if (!Context.ActiveLevel.AddSublevel(new_level))
                {
                    OpenPopupAlert("Error: unknown error adding level");
                } else
                {
                    Context.UpdateAll();
                }
            }
            ActiveJob = "None";
            UpdateSaveState();
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

        /// <summary>
        /// Displays a flyout menu of options when a Point is clicked
        /// </summary>
        private void WorldCanvas_ClickPoint(RenderedPoint point)
        {

        }

        /// <summary>
        /// Displays a flyout menu of options when a Line is clicked
        /// </summary>
        private void WorldCanvas_ClickLine(Line2D line)
        {

        }

        /// <summary>
        /// Displays a flyout menu of options when a Level6 object is clicked
        /// </summary>
        private void WorldCanvas_ClickLevel6(Level6 level)
        {

        }

        /// <summary>
        /// Displays a flyout menu of options when a Level5 object is clicked
        /// </summary>
        private void WorldCanvas_ClickLevel5(Level5 level)
        {

        }

        /// <summary>
        /// Displays a flyout menu of options when a BorderLevel object is clicked
        /// </summary>
        private void WorldCanvas_ClickBorderLevel(BorderLevel level)
        {
            Context.SetSelected(level);
        }

        /// <summary>
        /// Handles clicking on an object
        /// </summary>
        private void WorldCanvas_ClickObject(RenderedPoint point)
        {
            if (Context.SnapsToSomething(point))
            {
                Object obj = Context.GetObjectContainingPoint(point);
                try
                {
                    if (obj.GetType() == typeof(RenderedPoint))
                        WorldCanvas_ClickPoint((RenderedPoint)obj);
                    else if (obj.GetType() == typeof(Level6))
                        WorldCanvas_ClickLine((Line2D)obj);
                    else if (obj.GetType() == typeof(Level6))
                        WorldCanvas_ClickLevel6((Level6)obj);
                    else if (obj.GetType() == typeof(Level5))
                        WorldCanvas_ClickLevel5((Level5)obj);
                    else
                    {
                        BorderLevel level = (BorderLevel)obj;
                        WorldCanvas_ClickBorderLevel(level);
                    }
                }
                catch (InvalidCastException)
                {
                    ;
                }
            }
        }

        /// <summary>
        /// Moves the focus to point
        /// </summary>
        /// <param name="point">an AbsolutePoint object representing the translated click location</param>
        private void WorldCanvas_Refocus(AbsolutePoint point)
        {
            Global.Center = point; //Sets the center to the abolute coordinates of the point
            ForceUpdatePoints();
        }

        /// <summary>
        /// Adds a point to the working list of vertices
        /// </summary>
        /// <param name="point">an AbsolutePoint object representing the translated click location</param>
        private void WorldCanvas_Add_Point(AbsolutePoint point)
        {
            if(LevelNum > 1 && LevelNum < 5)
            {
                for(int i=0; i<vertices.Count; i++)
                {
                    if(point == vertices[i])
                    {
                        OpenPopupAlert("Point " + point.ToString() + " already added");
                        return;
                    }
                }
                if (Context.ActiveLevel != null)
                {
                    if (Context.ActiveLevel.CanFitPoint(point))
                    {
                        vertices.AppendPoint(point);
                        Context.ExtraPoints.AppendPoint(point);
                        Context.RaisePropertyChanged("ExtraPoints");
                    }
                    else
                    {
                        OpenPopupAlert("Point " + point.ToString() + " not in range");
                    }
                }
                TapPromptTab.Text = label + ": " + vertices.Size() + " points";
            }
        }
        
        /// <summary>
        /// Performs the standard operations for a normal canvas click
        /// </summary>
        /// <param name="point">A RenderedPoint that will automatically snap to the nearest ExtraPoint or Vertex within range</param>
        private void Canvas_Clicked(RenderedPoint point)
        {
            point = Context.SnapToAPoint(point);
            if (string.Equals(ActiveJob, "Create"))
            {
                WorldCanvas_Add_Point(point.ToAbsolutePoint());
            }
            else if (Context.SnapsToSomething(point))
            {
                WorldCanvas_ClickObject(point);
            }
            else
            {
                WorldCanvas_Refocus(point.ToAbsolutePoint());
            }
        }

        private void WorldCanvas_Tapped(object sender, TappedRoutedEventArgs e)
        {
            RenderedPoint point = new RenderedPoint(e.GetPosition((Windows.UI.Xaml.UIElement)sender));
            Canvas_Clicked(point);
        }

        private void WorldCanvas_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            RenderedPoint point = new RenderedPoint(e.GetPosition((Windows.UI.Xaml.UIElement)sender));
            WorldCanvas_Refocus(point.ToAbsolutePoint());
        }

        private void Tap_Prompt_Cancel_Click(object sender, RoutedEventArgs e)
        {
            TapPrompt.Visibility = Visibility.Collapsed;
            ActiveJob = "None";
            Context.ClearPoints();
        }

        private bool Tap_Prompt_Confirm_Greater_Region()
        {
            if (vertices.Size() < 3)
            {
                OpenPopupAlert("Requires at least 3 points");
                return false;
            }
            LevelStep++;
            TapPrompt.Visibility = Visibility.Collapsed;
            OpenTextPrompt("What type of " + Enum.GetName(typeof(LevelType), type) + " region are you creating?\nEnter a subtype:");
            return true;
        }

        private void Tap_Prompt_Confirm_Click(object sender, RoutedEventArgs e)
        {
            if(string.Equals(ActiveJob, "Create")){
                if (LevelNum == 2)
                {
                    Tap_Prompt_Confirm_Greater_Region();
                }
            }
            else if(string.Equals(ActiveJob, "Move"))
            {

            }
        }

        /// <summary>
        /// Forces all points to update their positions
        /// </summary>
        private void ForceUpdatePoints()
        {
            Context.ForceUpdatePoints();
        }

        private void Zoom_In_Button_Click(object sender, RoutedEventArgs e)
        {
            Global.Zoom = Math.Min(Global.MaxZoom, Global.Zoom * 1.1);
            Context.Zoom = Global.Zoom;
            ForceUpdatePoints();
            Zoom_Out_Button.IsEnabled = true;
            if(Global.Zoom == Global.MaxZoom)
            {
                Zoom_In_Button.IsEnabled = false;
            }
        }

        private void Zoom_Out_Button_Click(object sender, RoutedEventArgs e)
        {
            Global.Zoom = Math.Max(Global.MinZoom, Global.Zoom / 1.1);
            Context.Zoom = Global.Zoom;
            ForceUpdatePoints();
            Zoom_In_Button.IsEnabled = true;
            if (Global.Zoom == Global.MinZoom)
            {
                Zoom_Out_Button.IsEnabled = false;
            }
        }

        private void ResetZoom()
        {
            Global.Center = new AbsolutePoint(Global.RenderedCenter.X, Global.RenderedCenter.Y);
            Global.Zoom = Global.DefaultZoom;
            Context.Zoom = Global.Zoom;
        }

        private void Reset_Zoom_Button_Click(object sender, RoutedEventArgs e)
        {
            ResetZoom();
            ForceUpdatePoints();
        }

        private void Unsaved_Work_Cancel_Click(object sender, RoutedEventArgs e)
        {
            UnsavedWorkAlert.Visibility = Visibility.Collapsed;
        }

        private void Unsaved_Work_Confirm_Click(object sender, RoutedEventArgs e)
        {
            UnsavedWorkAlert.Visibility = Visibility.Collapsed;
            ForceClose = true;
            if(string.Equals(ActiveJob, "New File")){
                File_New_Click(sender, e);
            } else if(string.Equals(ActiveJob, "Open File"))
            {
                File_Open_Click(sender, e);
            } else if(string.Equals(ActiveJob, "Close File"))
            {
                File_Close_Click(sender, e);
            } else if(string.Equals(ActiveJob, "Exit"))
            {
                File_Exit_Click(sender, e);
            } else
            {
                OpenPopupAlert("Unrecognized Click Event - Please recall original event");
            }
        }
    }
}
