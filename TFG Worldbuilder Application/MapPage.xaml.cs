using Microsoft.Toolkit.Uwp.UI.Extensions;
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

        public enum Job : int
        {
            None = 0,
            NewFile = 1,
            OpenFile = 2,
            ReloadFile = 3,
            CloseFile = 4,
            Exit = 5,
            Create = 6,
            CreatePolygon = 7,
            CreatePoint = 8,
            Type = 9,
            Move = 10,
            Rename = 11,
            Open = 12

        }

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
        private AbsolutePoint center = null;
        private long radius = -1;
        private Job activejob = Job.None;
        private Job ActiveJob
        {
            get
            {
                return activejob;
            }
            set
            {
                if (activejob != Job.None && value == Job.None)
                    Context.NullSelected();
                activejob = value;
            }
        }
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
                SetActive(Worlds[0]);
                //Context.SetWorld(Worlds[0].name, Worlds[0].subtype);
                //Context.UpdateAll();
            }
            UpdateSaveState();
            ResetZoom();
            ForceUpdatePoints();
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {

            Context.RaisePropertyChanged("CenterX");
            Context.RaisePropertyChanged("CenterY");
            Context.RaisePropertyChanged("MaxX");
            Context.RaisePropertyChanged("MaxY");
            ResetZoom();
            ForceUpdatePoints();
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
                ActiveJob = Job.NewFile;
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

        private async void OpenFile(FileManager file)
        {
            if(file != null)
            {
                Global.ActiveFile = file;
                await Global.ActiveFile.ReadyFile();
                if (Global.ActiveFile.Valid())
                    this.Frame.Navigate(typeof(MapPage));
                else
                {
                    OpenPopupAlert("File not formatted for Worldbuilding - Invalid File");
                }
            }
            else
            {
                OpenPopupAlert("Invalid File");
            }
        }

        /// <summary>
        /// Checks if the current project has unsaved work, then opens the requested file
        /// </summary>
        private async void File_Open_Click(object sender, RoutedEventArgs e)
        {
            if (Global.ActiveFile.MatchesSave() || ForceClose)
            {
                var picker = new Windows.Storage.Pickers.FileOpenPicker();
                picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
                picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
                picker.FileTypeFilter.Add(".prm_world");
                Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
                OpenFile(new FileManager(file, false));
            }
            else
            {
                ActiveJob = Job.OpenFile;
                OpenUnsavedWorkAlert();
            }
        }

        /// <summary>
        /// Checks if the current project has unsaved work, then reloads the current file
        /// </summary>
        private void File_Reload_Click(object sender, RoutedEventArgs e)
        {
            if (Global.ActiveFile.MatchesSave() || ForceClose)
            {
                OpenFile(Global.ActiveFile.Reload());
            }
            else
            {
                ActiveJob = Job.ReloadFile;
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
                ActiveJob = Job.CloseFile;
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
                ActiveJob = Job.Exit;
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
            if ((ActiveJob == Job.CreatePolygon || ActiveJob == Job.CreatePoint) && LevelNum > 1)
            {
                Context.ClearPoints();
            }
            ActiveJob = Job.None;
        }

        private void Text_Prompt_Confirm_Click(object sender, RoutedEventArgs e)
        {
            string prompt_text = TextPromptBox.Text.Trim();
            if (prompt_text.Length > 255)
                prompt_text = prompt_text.Substring(0, 255).Trim();
            if (prompt_text.Length == 0)
            {
                OpenPopupAlert("Text must contain at least one (1) character");
            }
            else if (Global.ActiveFile.HasKeyword(prompt_text))
            {
                OpenPopupAlert("Supplied text contains illegal keyword: \"" + Global.ActiveFile.GetKeyword(prompt_text) + "\"");
            }
            else
            {
                if (ActiveJob == Job.Create)
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
                            this.LevelStep++;
                            switch (this.LevelNum)
                            {
                                case 1:
                                    TextPrompt.Visibility = Visibility.Collapsed;
                                    NewWorld(this.name, this.subtype);
                                    break;
                                case 2:
                                    TextPrompt.Visibility = Visibility.Collapsed;
                                    NewGreaterRegion(this.name, this.type, this.subtype, this.vertices);
                                    break;
                                case 3:
                                    TextPrompt.Visibility = Visibility.Collapsed;
                                    NewRegion(this.name, this.type, this.subtype, this.vertices);
                                    break;
                                case 4:
                                    TextPrompt.Visibility = Visibility.Collapsed;
                                    NewSubregion(this.name, this.type, this.subtype, this.vertices);
                                    break;
                                case 5:
                                    OpenTextPrompt("Enter a radius:");
                                    break;
                                case 6:
                                    TextPrompt.Visibility = Visibility.Collapsed;
                                    NewStructure(this.name, this.type, this.subtype, center);
                                    break;
                            }
                            break;
                        case 4:
                            try
                            {
                                if(this.LevelNum == 5)
                                {
                                    radius = Int64.Parse(prompt_text);
                                    if(radius >= 0)
                                    {
                                        this.LevelStep++;
                                        TextPrompt.Visibility = Visibility.Collapsed;
                                        NewLocation(this.name, this.type, this.subtype, center, radius);
                                    } else
                                    {
                                        radius = -1;
                                        OpenPopupAlert("Invalid input - must be formatted as a positive whole number");
                                    }
                                }
                            } catch (ArgumentException)
                            {
                                radius = -1;
                                OpenPopupAlert("Invalid input - must be formatted as a positive whole number");
                            }
                            break;
                    }
                }
                else if (ActiveJob == Job.Open)
                {
                    TextPrompt.Visibility = Visibility.Collapsed;
                    switch (this.LevelNum) //Control for level type
                    {
                        case 1:
                            SuperLevel world = Context.GetWorld(prompt_text);
                            if (world != null)
                                SetActive(world);
                            break;
                    }
                } else if ((ActiveJob == Job.Rename) && Context.SelectedLevel != null)
                {
                    try
                    {
                        Context.SelectedLevel.name = prompt_text;
                        TextPrompt.Visibility = Visibility.Collapsed;
                        Context.NullSelected();
                        UpdateSaveState();
                        ActiveJob = Job.None;
                    } catch (ArgumentException rename_exception)
                    {
                        OpenPopupAlert(rename_exception.Message);
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
            ActiveJob = Job.Create;
            OpenTextPrompt("What type of world are you creating?\nEnter a subtype:");
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
            if (vertices.Count > 0)
                Tap_Prompt_Back.IsEnabled = true;
            else
                Tap_Prompt_Back.IsEnabled = false;
        }

        /// <summary>
        /// Creates a new world object with the given name and subtype
        /// </summary>
        private void NewWorld(string name, string subtype)
        {
            if (Global.ActiveFile.HasWorld(name, subtype))
            {
                OpenPopupAlert("Error: " + Enum.GetName(typeof(LevelType), type) + " with name \"" + name + "\" already exists");
            } else
            {
                SetActive(new Level1(name, subtype));
                Global.ActiveFile.Worlds.Add((Level1)Context.ActiveLevel);
            }
            ActiveJob = Job.None;
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
            ActiveJob = Job.None;
            UpdateSaveState();
        }

        /// <summary>
        /// Creates a new region object with the given name, type, subtype, and border
        /// </summary>
        private void NewRegion(string name, LevelType type, string subtype, Polygon2D border)
        {
            Context.ClearPoints();
            if (Context.ActiveLevel.HasSublevelWithName(name))
            {
                OpenPopupAlert("Error: " + Enum.GetName(typeof(LevelType), type) + " sublevel with name \"" + name + "\" already exists");
            }
            else
            {
                Level3 new_level = new Level3(name, type, subtype, Context.ActiveLevel, border);
                if (!Context.ActiveLevel.AddSublevel(new_level))
                {
                    OpenPopupAlert("Error: unknown error adding level");
                }
                else
                {
                    Context.UpdateAll();
                }
            }
            ActiveJob = Job.None;
            UpdateSaveState();
        }

        /// <summary>
        /// Creates a new subregion object with the given name, type, subtype, and border
        /// </summary>
        private void NewSubregion(string name, LevelType type, string subtype, Polygon2D border)
        {
            Context.ClearPoints();
            if (Context.ActiveLevel.HasSublevelWithName(name))
            {
                OpenPopupAlert("Error: " + Enum.GetName(typeof(LevelType), type) + " sublevel with name \"" + name + "\" already exists");
            }
            else
            {
                Level4 new_level = new Level4(name, type, subtype, Context.ActiveLevel, border);
                if (!Context.ActiveLevel.AddSublevel(new_level))
                {
                    OpenPopupAlert("Error: unknown error adding level");
                }
                else
                {
                    Context.UpdateAll();
                }
            }
            ActiveJob = Job.None;
            UpdateSaveState();
        }

        /// <summary>
        /// Creates a new location object with the given name, type, subtype, center, and radius
        /// </summary>
        private void NewLocation(string name, LevelType type, string subtype, AbsolutePoint center, long radius)
        {
            Context.ClearPoints();
            if (Context.ActiveLevel.HasSublevelWithName(name))
            {
                OpenPopupAlert("Error: " + Enum.GetName(typeof(LevelType), type) + " sublevel with name \"" + name + "\" already exists");
            }
            else
            {
                Level5 new_level = new Level5(name, type, subtype, Context.ActiveLevel, center, radius);
                if (!Context.ActiveLevel.AddSublevel(new_level))
                {
                    OpenPopupAlert("Error: unknown error adding level");
                }
                else
                {
                    Context.UpdateAll();
                }
            }
            ActiveJob = Job.None;
            UpdateSaveState();
        }

        /// <summary>
        /// Creates a new structure object with the given name, type, subtype, and center
        /// </summary>
        private void NewStructure(string name, LevelType type, string subtype, AbsolutePoint center)
        {
            Context.ClearPoints();
            if (Context.ActiveLevel.HasSublevelWithName(name))
            {
                OpenPopupAlert("Error: " + Enum.GetName(typeof(LevelType), type) + " sublevel with name \"" + name + "\" already exists");
            }
            else
            {
                Level6 new_level = new Level6(name, type, subtype, Context.ActiveLevel, center);
                if (!Context.ActiveLevel.AddSublevel(new_level))
                {
                    OpenPopupAlert("Error: unknown error adding level");
                }
                else
                {
                    Context.UpdateAll();
                }
            }
            ActiveJob = Job.None;
            UpdateSaveState();
        }

        /// <summary>
        /// Sets the ActiveLevel to the saved world defined by the sender event
        /// </summary>
        private void Open_World_Click(object sender, RoutedEventArgs e)
        {
            WorldsMenu.Hide();
            string name = ((MenuFlyoutItem)sender).Text.Trim();
            SuperLevel world = Context.GetWorld(name);
            if (world != null)
                SetActive(world);
            ResetZoom();
            ForceUpdatePoints();
        }

        /// <summary>
        /// Sets the ActiveLevel to the sublevel defined by the sender event
        /// </summary>
        private void Open_Sublevel_Click(object sender, RoutedEventArgs e)
        {
            SublevelsMenu.Hide();
            string name = ((MenuFlyoutItem)sender).Text.Trim();
            SuperLevel sublevel = Context.ActiveLevel.GetLevel(name);
            if (sublevel != null)
                SetActive(sublevel);
            ResetZoom();
            ForceUpdatePoints();
        }

        /// <summary>
        /// Sets focus to the prompt text box if available when the create menu is closed
        /// </summary>
        private void CreateMenu_Closed(object sender, object e)
        {
            if (TextPrompt.Visibility == Visibility.Visible)
                TextPromptBox.Focus(FocusState.Programmatic);
        }


        private void Create_Greater_Region_Click(object sender, RoutedEventArgs e)
        {
            LevelNum = 2;
            if(Context.ActiveLevel != null && Context.ActiveLevel.leveltype != LevelType.Invalid && Context.ActiveLevel.leveltype != LevelType.World)
            {
                type = Context.ActiveLevel.leveltype;
                Create_Greater_Region();
            } else
            {
                ActiveJob = Job.Type;
                OpenTypePrompt("Define Greater Region Type");
            }
        }

        private void Create_Region_Click(object sender, RoutedEventArgs e)
        {
            LevelNum = 3;
            if (Context.ActiveLevel != null && Context.ActiveLevel.leveltype != LevelType.Invalid && Context.ActiveLevel.leveltype != LevelType.World)
            {
                type = Context.ActiveLevel.leveltype;
                Create_Region();
            }
            else
            {
                ActiveJob = Job.Type;
                OpenTypePrompt("Define Region Type");
            }
        }

        private void Create_Subregion_Click(object sender, RoutedEventArgs e)
        {
            LevelNum = 4;
            if (Context.ActiveLevel != null && Context.ActiveLevel.leveltype != LevelType.Invalid && Context.ActiveLevel.leveltype != LevelType.World)
            {
                type = Context.ActiveLevel.leveltype;
                Create_Subregion();
            }
            else
            {
                ActiveJob = Job.Type;
                OpenTypePrompt("Define Subregion Type");
            }
        }

        private void Create_Location_Click(object sender, RoutedEventArgs e)
        {
            LevelNum = 5;
            if (Context.ActiveLevel != null && Context.ActiveLevel.leveltype != LevelType.Invalid && Context.ActiveLevel.leveltype != LevelType.World)
            {
                type = Context.ActiveLevel.leveltype;
                Create_Location();
            }
            else
            {
                ActiveJob = Job.Type;
                OpenTypePrompt("Define Location Type");
            }
        }

        private void Create_Structure_Click(object sender, RoutedEventArgs e)
        {
            LevelNum = 6;
            if (Context.ActiveLevel != null && Context.ActiveLevel.leveltype != LevelType.Invalid && Context.ActiveLevel.leveltype != LevelType.World)
            {
                type = Context.ActiveLevel.leveltype;
                Create_Structure();
            }
            else
            {
                ActiveJob = Job.Type;
                OpenTypePrompt("Define Structure Type");
            }
        }


        /// <summary>
        /// Opens the popup and prepares to create a new greater region
        /// </summary>
        private void Create_Greater_Region()
        {
            if (Context.ActiveLevel != null && Context.ActiveLevel.level < 2)
            {
                LevelNum = 2;
                LevelStep = 1;
                ActiveJob = Job.CreatePolygon;
                vertices = new Polygon2D();
                Context.ClearPoints();
                OpenTapPrompt("Enter at least 3 points for Greater Region");
            }
        }

        /// <summary>
        /// Opens the popup and prepares to create a new region
        /// </summary>
        private void Create_Region()
        {
            if (Context.ActiveLevel != null && Context.ActiveLevel.level < 3)
            {
                LevelNum = 3;
                LevelStep = 1;
                ActiveJob = Job.CreatePolygon;
                vertices = new Polygon2D();
                Context.ClearPoints();
                OpenTapPrompt("Enter at least 3 points for Region");
            }
        }

        /// <summary>
        /// Opens the popup and prepares to create a new subregion
        /// </summary>
        private void Create_Subregion()
        {
            if (Context.ActiveLevel != null && Context.ActiveLevel.level < 4)
            {
                LevelNum = 4;
                LevelStep = 1;
                ActiveJob = Job.CreatePolygon;
                vertices = new Polygon2D();
                Context.ClearPoints();
                OpenTapPrompt("Enter at least 3 points for Subregion");
            }
        }
        /// <summary>
        /// Opens the popup and prepares to create a new location
        /// </summary>
        private void Create_Location()
        {
            if (Context.ActiveLevel != null && Context.ActiveLevel.level < 4)
            {
                LevelNum = 5;
                LevelStep = 1;
                ActiveJob = Job.CreatePoint;
                vertices = new Polygon2D();
                Context.ClearPoints();
                OpenTapPrompt("Enter a point for the center");
            }
        }

        /// <summary>
        /// Opens the popup and prepares to create a new structure
        /// </summary>
        private void Create_Structure()
        {
            if (Context.ActiveLevel != null && Context.ActiveLevel.level < 4)
            {
                LevelNum = 6;
                LevelStep = 1;
                ActiveJob = Job.CreatePoint;
                vertices = new Polygon2D();
                Context.ClearPoints();
                OpenTapPrompt("Enter a point for the center");
            }
        }

        private childItem FindVisualChild<childItem>(DependencyObject obj) where childItem : DependencyObject
        {
            if (obj == null)
                return null;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is childItem)
                    return (childItem)child;
                else
                {
                    childItem childOfChild = FindVisualChild<childItem>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }


        /// <summary>
        /// Displays a flyout menu of options when a Vertex is clicked
        /// </summary>
        private void WorldCanvas_ClickVertex(RenderedPoint point)
        {
            foreach (object child in ShapesVerticesControl.Items)
            {
                try
                {
                    Windows.UI.Xaml.Shapes.Path vertex = FindVisualChild<Windows.UI.Xaml.Shapes.Path>(ShapesVerticesControl.ContainerFromItem(child) as DependencyObject);
                    if (vertex != null)
                    {
                        FlyoutBase flyout = FlyoutBase.GetAttachedFlyout(vertex as FrameworkElement);
                        if (flyout != null)
                        {
                            Context.SetSelected(point);
                            FlyoutShowOptions show_options = new FlyoutShowOptions();
                            show_options.Position = point.ToWindowsPoint();
                            flyout.ShowAt(ShapesVerticesControl, show_options);
                            return;
                        }
                    }
                }
                catch (InvalidCastException)
                {
                    ;
                }
            }
        }

        /// <summary>
        /// Displays a flyout menu of options when a Line is clicked
        /// </summary>
        private void WorldCanvas_ClickLine(Line2D line, RenderedPoint point)
        {
            foreach (object child in ShapesEdgesControl.Items)
            {
                try
                {
                    Windows.UI.Xaml.Shapes.Line edge = FindVisualChild<Windows.UI.Xaml.Shapes.Line>(ShapesEdgesControl.ContainerFromItem(child) as DependencyObject);
                    if (edge != null)
                    {
                        FlyoutBase flyout = FlyoutBase.GetAttachedFlyout(edge as FrameworkElement);
                        if (flyout != null)
                        {
                            Context.SetSelected(line);
                            FlyoutShowOptions show_options = new FlyoutShowOptions();
                            show_options.Position = point.ToWindowsPoint();
                            flyout.ShowAt(ShapesEdgesControl, show_options);
                            return;
                        }
                    }
                }
                catch (InvalidCastException)
                {
                    ;
                }
            }
        }

        /// <summary>
        /// Displays a flyout menu of options when a Level6 object is clicked
        /// </summary>
        private void WorldCanvas_ClickLevel6(Level6 level, RenderedPoint point)
        {

        }

        /// <summary>
        /// Displays a flyout menu of options when a Level5 object is clicked
        /// </summary>
        private void WorldCanvas_ClickLevel5(Level5 level, RenderedPoint point)
        {

        }
        /// <summary>
        /// Displays a flyout menu of options when a BorderLevel object is clicked
        /// </summary>
        private void WorldCanvas_ClickBorderLevel(BorderLevel level, RenderedPoint point)
        {
            foreach(object child in ShapesControl.Items)
            {
                try
                {
                    Polygon shape = FindVisualChild<Polygon>(ShapesControl.ContainerFromItem(child) as DependencyObject);
                    if (shape != null)
                    {
                        FlyoutBase flyout = FlyoutBase.GetAttachedFlyout(shape as FrameworkElement);
                        if (flyout != null)
                        {
                            FlyoutShowOptions show_options = new FlyoutShowOptions();
                            show_options.Position = point.ToWindowsPoint();
                            flyout.ShowAt(shape, show_options);
                            Context.SetSelected(level);
                            return;
                        }
                    }
                } catch (InvalidCastException)
                {
                    ;
                }
            }
        }

        /// <summary>
        /// Handles clicking on an object
        /// </summary>
        private void WorldCanvas_ClickObject(RenderedPoint point)
        {
            if (Context.SnapsToSomething(point))
            {
                Context.NullSelected();
                Object obj = Context.GetObjectContainingPoint(point);
                try
                {
                    if (obj.GetType() == typeof(RenderedPoint))
                        WorldCanvas_ClickVertex((RenderedPoint)obj);
                    else if (obj.GetType() == typeof(Line2D))
                        WorldCanvas_ClickLine((Line2D)obj, point);
                    else if (obj.GetType() == typeof(Level6))
                        WorldCanvas_ClickLevel6((Level6)obj, point);
                    else if (obj.GetType() == typeof(Level5))
                        WorldCanvas_ClickLevel5((Level5)obj, point);
                    else
                    {
                        BorderLevel level = (BorderLevel)obj;
                        WorldCanvas_ClickBorderLevel(level, point);
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
            if(ActiveJob == Job.CreatePolygon || ActiveJob == Job.CreatePoint)
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
                    if (Context.ActiveLevel.CanFitPoint(point) && !Context.Conflicts(point, LevelNum) && (Context.SelectedLevel == null || Context.SelectedLevel.CanFitPoint(point)))
                    {
                        if(ActiveJob == Job.CreatePolygon || vertices.Count == 0)
                        {
                            vertices.AppendPoint(point);
                            Context.ExtraPoints.AppendPoint(point);
                        } else
                        {
                            vertices.vertices[0] = point;
                            Context.ExtraPoints._points[0] = point;
                        }
                        Context.RaisePropertyChanged("ExtraPoints");
                    }
                    else if(Context.Conflicts(point, LevelNum))
                    {
                        OpenPopupAlert("Point " + point.ToString() + " within an existing object");
                    }
                    else
                    {
                        OpenPopupAlert("Point " + point.ToString() + " not in range");
                    }
                }
                TapPromptTab.Text = label + ": " + vertices.Size() + " points";
                if(vertices.Count > 0)
                    Tap_Prompt_Back.IsEnabled = true;
                else
                    Tap_Prompt_Back.IsEnabled = false;
            }
        }

        /// <summary>
        /// Changes the moving point for the tap prompt
        /// </summary>
        /// <param name="point"></param>
        private void WorldCanvas_Move_Point(AbsolutePoint point)
        {
            if (Context.TestMovePoint(point))
            {
                if (vertices.Count < 1)
                    vertices.AppendPoint(point);
                else
                    vertices.vertices[0] = new AbsolutePoint(point);
                if (Context.ExtraPoints.Count < 1)
                    Context.ExtraPoints.AppendPoint(point);
                else
                    Context.ExtraPoints._points[0] = point;
                Context.RaisePropertyChanged("ExtraPoints");
                TapPromptTab.Text = label + ": " + vertices.Size() + " points";
                if (vertices.Count > 0)
                    Tap_Prompt_Back.IsEnabled = true;
                else
                    Tap_Prompt_Back.IsEnabled = false;
                Tap_Prompt_Confirm_Click(null, null);
            } else
            {
                OpenPopupAlert("Cannot move point to that position");
            }

        }
        
        /// <summary>
        /// Performs the standard operations for a normal canvas click
        /// </summary>
        /// <param name="point">A RenderedPoint that will automatically snap to the nearest ExtraPoint or Vertex within range</param>
        private void Canvas_Clicked(RenderedPoint point)
        {
            point = Context.SnapToAPoint(point);
            if (ActiveJob == Job.CreatePolygon || ActiveJob == Job.CreatePoint)
            {
                WorldCanvas_Add_Point(point.ToAbsolutePoint());
            } else if(ActiveJob == Job.Move)
            {
                WorldCanvas_Move_Point(point.ToAbsolutePoint());
            }
            else if (ActiveJob == Job.None){
                if (Context.SnapsToSomething(point))
                {
                    WorldCanvas_ClickObject(point);
                }
                else
                {
                    if (Context.HasActive)
                        Context.NullSelected();
                }
            }
        }

        private void WorldCanvas_Tapped(object sender, TappedRoutedEventArgs e)
        {
            RenderedPoint point = new RenderedPoint(e.GetPosition((Windows.UI.Xaml.UIElement)sender));
            Canvas_Clicked(point);
        }


        private void WorldCanvas_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            RenderedPoint point = new RenderedPoint(e.GetPosition((Windows.UI.Xaml.UIElement)sender));
            WorldCanvas_Refocus(point.ToAbsolutePoint());
        }

        private void Tap_Prompt_Cancel_Click(object sender, RoutedEventArgs e)
        {
            TapPrompt.Visibility = Visibility.Collapsed;
            ActiveJob = Job.None;
            Context.ClearPoints();
        }

        private void Tap_Prompt_Back_Click(object sender, RoutedEventArgs e)
        {
            if (vertices.Count > 0)
            {
                vertices.RemovePoint();
                Context.ExtraPoints.RemovePoint();
                Context.RaisePropertyChanged("ExtraPoints");
            }
            if(vertices.Count <= 0)
                Tap_Prompt_Back.IsEnabled = false;
            TapPromptTab.Text = label + ": " + vertices.Size() + " points";
        }

        private void Tap_Prompt_Confirm_Click(object sender, RoutedEventArgs e)
        {
            if(ActiveJob == Job.CreatePolygon || ActiveJob == Job.CreatePoint){
                if(Context.Intersects(vertices, LevelNum))
                {
                    OpenPopupAlert("Current object intersects with an existing object");
                } else
                {
                    if(ActiveJob == Job.CreatePolygon && vertices.Size() < 3)
                    {
                        OpenPopupAlert("Requires at least 3 points");
                    } else if(ActiveJob == Job.CreatePoint && vertices.Size() < 1)
                    {
                        OpenPopupAlert("Requires at least 1 point");
                    } else
                    {
                        if (ActiveJob == Job.CreatePoint)
                            center = vertices[0];
                        TapPrompt.Visibility = Visibility.Collapsed;
                        LevelStep++;
                        string level_type_name = "<UNDEFINED>";
                        switch (LevelNum)
                        {
                            case 2:
                                level_type_name = "greater region";
                                break;
                            case 3:
                                level_type_name = "region";
                                break;
                            case 4:
                                level_type_name = "subregion";
                                break;
                            case 5:
                                level_type_name = "location";
                                break;
                            case 6:
                                level_type_name = "structure";
                                break;
                            default:
                                break;
                        }
                        if(LevelNum >= 2 && LevelNum <= 6)
                        {
                            ActiveJob = Job.Create;
                            OpenTextPrompt("What type of " + Enum.GetName(typeof(LevelType), type) + " " + level_type_name + " are you creating?\nEnter a subtype:");
                        }
                    }
                }
            }
            else if(ActiveJob == Job.Move)
            {
                if(vertices.Count > 0)
                {
                    Context.SetVertex(vertices[0]);
                    Context.ClearPoints();
                    TapPrompt.Visibility = Visibility.Collapsed;
                    ActiveJob = Job.None;
                    UpdateSaveState();
                } else
                {
                    OpenPopupAlert("No new position mapped to move to");
                }
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
            Global.Center = new AbsolutePoint(Global.DefaultCenter);
            Global.Zoom = Global.DefaultZoom;
            Context.Zoom = Global.Zoom;
            Zoom_In_Button.IsEnabled = true;
            Zoom_Out_Button.IsEnabled = true;
        }

        private void Reset_Zoom_Button_Click(object sender, RoutedEventArgs e)
        {
            Context.NullSelected();
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
            if(ActiveJob == Job.NewFile){
                File_New_Click(sender, e);
            } else if(ActiveJob == Job.OpenFile)
            {
                File_Open_Click(sender, e);
            } else if(ActiveJob == Job.CloseFile)
            {
                File_Close_Click(sender, e);
            } else if(ActiveJob == Job.Exit)
            {
                File_Exit_Click(sender, e);
            } else if(ActiveJob == Job.ReloadFile)
            {
                File_Reload_Click(sender, e);
            }
            else
            {
                OpenPopupAlert("Unrecognized Click Event - Please recall original event");
            }
        }

        private void Shapes_Control_Rename_Click(object sender, RoutedEventArgs e)
        {
            ActiveJob = Job.Rename;
            OpenTextPrompt("Enter new name for " + Context.SelectedLevel.name + ':');
        }

        private void Shapes_Control_Delete_Click(object sender, RoutedEventArgs e)
        {
            Context.DeleteSelected();
            UpdateSaveState();
        }

        private void SetActive(SuperLevel level)
        {
            if(level.level >= 2)
                Create_Greater_Region_Flyout.IsEnabled = false; 
            else
                Create_Greater_Region_Flyout.IsEnabled = true;
            if (level.level >= 3)
                Create_Region_Flyout.IsEnabled = false;
            else
                Create_Region_Flyout.IsEnabled = true;
            if (level.level >= 4)
                Create_Subregion_Flyout.IsEnabled = false;
            else
                Create_Subregion_Flyout.IsEnabled = true;
            if (level.level >= 5)
                Create_Location_Flyout.IsEnabled = false;
            else
                Create_Location_Flyout.IsEnabled = true;
            if (level.level >= 6)
                Create_Structure_Flyout.IsEnabled = false;
            else
                Create_Structure_Flyout.IsEnabled = true;
            Context.SetActive(level);
        }

        private void Shapes_Control_Focus_Click(object sender, RoutedEventArgs e)
        {
            SetActive(Context.SelectedLevel);
            Context.NullSelected();
            ResetZoom();
            ForceUpdatePoints();
        }

        private void Vertices_Control_Delete_Click(object sender, RoutedEventArgs e)
        {
            Context.DeleteSelected();
            UpdateSaveState();
        }

        private void Vertices_Control_Move_Click(object sender, RoutedEventArgs e)
        {
            ActiveJob = Job.Move;
            OpenTapPrompt("Click new position for " + Context.CurrentPoint.ToString());
        }

        private void ShapesControlFlyout_Closed(object sender, object e)
        {
            if(ActiveJob == Job.None)
                Context.NullSelected();
        }

        private void VerticesControlFlyout_Closed(object sender, object e)
        {
            if (ActiveJob == Job.None)
                Context.NullSelected();
        }

        private void EdgesControlFlyout_Closed(object sender, object e)
        {
            if (ActiveJob == Job.None)
                Context.NullSelected();
        }

        private void Edges_Control_Split_Click(object sender, RoutedEventArgs e)
        {
            Context.SplitLine();
            UpdateSaveState();
        }

        /// <summary>
        /// Opens the Popup Alert with the given message
        /// </summary>
        private void OpenTypePrompt(string text)
        {
            TypePromptTab.Text = text;
            TypePrompt.Visibility = Visibility.Visible;
        }

        private void Type_Prompt_Confirm()
        {
            TypePrompt.Visibility = Visibility.Collapsed;
            switch (LevelNum)
            {
                case 2:
                    Create_Greater_Region();
                    break;
                case 3:
                    Create_Region();
                    break;
                case 4:
                    Create_Subregion();
                    break;
                case 5:
                    Create_Location();
                    break;
                case 6:
                    Create_Structure();
                    break;
                default:
                    break;
            }
        }

        private void Type_Prompt_National_Click(object sender, RoutedEventArgs e)
        {
            type = LevelType.National;
            Type_Prompt_Confirm();
        }

        private void Type_Prompt_Geographical_Click(object sender, RoutedEventArgs e)
        {
            type = LevelType.Geographical;
            Type_Prompt_Confirm();
        }

        private void Type_Prompt_Climate_Click(object sender, RoutedEventArgs e)
        {
            type = LevelType.Climate;
            Type_Prompt_Confirm();
        }

        private void Type_Prompt_Factional_Click(object sender, RoutedEventArgs e)
        {
            type = LevelType.Factional;
            Type_Prompt_Confirm();
        }

        private void Type_Prompt_Cultural_Click(object sender, RoutedEventArgs e)
        {
            type = LevelType.Cultural;
            Type_Prompt_Confirm();
        }

        private void Type_Prompt_Biological_Click(object sender, RoutedEventArgs e)
        {
            type = LevelType.Biological;
            Type_Prompt_Confirm();
        }

        private void Type_Prompt_Cancel_Click(object sender, RoutedEventArgs e)
        {
            ActiveJob = Job.None;
            TypePrompt.Visibility = Visibility.Collapsed;
        }

    }
}
