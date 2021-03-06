﻿using Microsoft.Toolkit.Uwp.UI.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
            Open = 12,
            Resize = 13,
            MoveLevel = 14,
            Recolor = 15,
            BasicRecolor = 16
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
        private int LevelNum
        {
            get
            {
                return Context.WorkingLevelnum;
            }
            set
            {
                Context.WorkingLevelnum = value;
            }
        }
        private int LevelStep = 0;
        private string name = "";
        private string label = "";
        private string level_type_name
        {
            get
            {
                switch (LevelNum)
                {
                    case 1:
                        return "world";
                    case 2:
                        return "greater region";
                    case 3:
                        return "region";
                    case 4:
                        return "subregion";
                    case 5:
                        return "location";
                    case 6:
                        return "structure";
                }
                return "<UNDEFINED>";
            }
        }
        private string color = SuperLevel.DefaultColor;
        private Polygon2D vertices = new Polygon2D();
        private LevelType type
        {
            get
            {
                return Context.WorkingType;
            }
            set
            {
                Context.WorkingType = value;
            }
        }
        private string subtype = "";
        private AbsolutePoint center = null;
        private double radius = -1;
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
            Global.mappage = this; 
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
            else
            {
                NavigationButton.IsEnabled = false;
                ViewButton.IsEnabled = false;
                Create_Greater_Region_Flyout.IsEnabled = false;
                Create_Region_Flyout.IsEnabled = false;
                Create_Subregion_Flyout.IsEnabled = false;
                Create_Location_Flyout.IsEnabled = false;
                Create_Structure_Flyout.IsEnabled = false;
            }
            UpdateSaveState();
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
                        case 1:
                            if(Context.SubtypeList.Conflicts(new Tuple<int, LevelType, string, string>(this.LevelNum, this.type, prompt_text, SuperLevel.DefaultColor))){
                                Tuple<int, LevelType, string, string> actual = Context.SubtypeList.Get(prompt_text);
                                OpenPopupAlert("Invalid subtype; conflicts with existing Subtype definition for level " + actual.Item1 + " of type " + Enum.GetName(typeof(LevelType), actual.Item2));
                            } else
                            {
                                this.subtype = prompt_text;
                                if (LevelNum == 1)
                                {
                                    this.LevelStep = 3;
                                    Context.AddColor(1, LevelType.World, subtype, SuperLevel.DefaultColor);
                                    OpenTextPrompt("Name your " + subtype + ":");
                                }
                                else
                                {
                                    OpenColorPicker("Pick a color to represent " + subtype + "s:");
                                }
                            }
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
                                    radius = Double.Parse(prompt_text);
                                    if(radius >= 0)
                                    {
                                        this.LevelStep++;
                                        TextPrompt.Visibility = Visibility.Collapsed;
                                        NewLocation(this.name, this.type, this.subtype, center, radius);
                                    } else
                                    {
                                        radius = -1;
                                        OpenPopupAlert("Invalid input - must be formatted as a positive number");
                                    }
                                }
                            } catch (ArgumentException)
                            {
                                radius = -1;
                                OpenPopupAlert("Invalid input - must be formatted as a positive number");
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
                else if ((ActiveJob == Job.Resize))
                {
                    try
                    {
                        if (this.LevelNum == 5)
                        {
                            radius = Double.Parse(prompt_text);
                            if (radius >= 0)
                            {
                                Resize_ActiveCircle(radius);
                                TextPrompt.Visibility = Visibility.Collapsed;
                            }
                            else
                            {
                                radius = -1;
                                OpenPopupAlert("Invalid input - must be formatted as a positive number");
                            }
                        }
                    }
                    catch (ArgumentException)
                    {
                        radius = -1;
                        OpenPopupAlert("Invalid input - must be formatted as a positive number");
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
            ActiveJob = Job.Create;
            OpenSubtypePrompt("What type of world are you creating?\nSelect a subtype:");
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
                NavigationButton.IsEnabled = true;
                ViewButton.IsEnabled = true;
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
        private void NewLocation(string name, LevelType type, string subtype, AbsolutePoint center, double radius)
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
            string name = ((MenuFlyoutItem)sender).Text.Trim();
            SuperLevel world = Context.GetWorld(name);
            if (world != null)
                SetActive(world);
        }

        /// <summary>
        /// Sets the ActiveLevel to the sublevel defined by the sender event
        /// </summary>
        private void Open_Sublevel_Click(object sender, RoutedEventArgs e)
        {
            string name = ((MenuFlyoutItem)sender).Text.Trim();
            SuperLevel sublevel = Context.ActiveLevel.GetLevel(name);
            if (sublevel != null)
                SetActive(sublevel);
        }

        private void Level_Back_Click(object sender, RoutedEventArgs e)
        {
            if (Context.ActiveLevel.parent != null)
            {
                SetActive(Context.ActiveLevel.parent);
            }
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
                ActiveJob = Job.Create;
                vertices = new Polygon2D();
                Context.ClearPoints();
                OpenSubtypePrompt("What type of " + Enum.GetName(typeof(LevelType), type) + " " + level_type_name + " are you creating?\nSelect a subtype:");
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
                ActiveJob = Job.Create;
                vertices = new Polygon2D();
                Context.ClearPoints(); OpenSubtypePrompt("What type of " + Enum.GetName(typeof(LevelType), type) + " " + level_type_name + " are you creating?\nSelect a subtype:");
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
                ActiveJob = Job.Create;
                vertices = new Polygon2D();
                Context.ClearPoints(); OpenSubtypePrompt("What type of " + Enum.GetName(typeof(LevelType), type) + " " + level_type_name + " are you creating?\nSelect a subtype:");
            }
        }
        /// <summary>
        /// Opens the popup and prepares to create a new location
        /// </summary>
        private void Create_Location()
        {
            if (Context.ActiveLevel != null && Context.ActiveLevel.level < 5)
            {
                LevelNum = 5;
                LevelStep = 1;
                ActiveJob = Job.Create;
                vertices = new Polygon2D();
                Context.ClearPoints();
                OpenSubtypePrompt("What type of " + Enum.GetName(typeof(LevelType), type) + " " + level_type_name + " are you creating?\nSelect a subtype:");
            }
        }

        /// <summary>
        /// Opens the popup and prepares to create a new structure
        /// </summary>
        private void Create_Structure()
        {
            if (Context.ActiveLevel != null && Context.ActiveLevel.level < 6)
            {
                LevelNum = 6;
                LevelStep = 1;
                ActiveJob = Job.Create;
                vertices = new Polygon2D();
                Context.ClearPoints();
                OpenSubtypePrompt("What type of " + Enum.GetName(typeof(LevelType), type) + " " + level_type_name + " are you creating?\nSelect a subtype:");
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
        /// Displays a flyout menu of options when a Level5 object is clicked
        /// </summary>
        private void WorldCanvas_ClickLevel5(Level5 level, RenderedPoint point)
        {
            foreach (object child in CirclesControl.Items)
            {
                try
                {
                    MyLevel5 circle = FindVisualChild<MyLevel5>(CirclesControl.ContainerFromItem(child) as DependencyObject);
                    if (circle != null)
                    {
                        FlyoutBase flyout = FlyoutBase.GetAttachedFlyout(circle as FrameworkElement);
                        if (flyout != null)
                        {
                            FlyoutShowOptions show_options = new FlyoutShowOptions();
                            show_options.Position = point.ToWindowsPoint();
                            flyout.ShowAt(CirclesControl, show_options);
                            Context.SetSelected(level);
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
            foreach (object child in PointsControl.Items)
            {
                try
                {
                    MyLevel6 circle = FindVisualChild<MyLevel6>(PointsControl.ContainerFromItem(child) as DependencyObject);
                    if (circle != null)
                    {
                        FlyoutBase flyout = FlyoutBase.GetAttachedFlyout(circle as FrameworkElement);
                        if (flyout != null)
                        {
                            FlyoutShowOptions show_options = new FlyoutShowOptions();
                            show_options.Position = point.ToWindowsPoint();
                            flyout.ShowAt(PointsControl, show_options);
                            Context.SetSelected(level);
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
        /// Displays a flyout menu of options when a BorderLevel object is clicked
        /// </summary>
        private void WorldCanvas_ClickBorderLevel(BorderLevel level, RenderedPoint point)
        {
            foreach(object child in ShapesControl.Items)
            {
                try
                {
                    MyBorderLevel shape = FindVisualChild<MyBorderLevel>(ShapesControl.ContainerFromItem(child) as DependencyObject);
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
                if (obj != null)
                {
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
        }

        /// <summary>
        /// Moves the focus to point
        /// </summary>
        /// <param name="point">an AbsolutePoint object representing the translated click location</param>g 
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
                    if (Context.ActiveLevel.CanFitPoint(point) && !Context.Conflicts(point, LevelNum, type) && (Context.SelectedLevel == null || Context.SelectedLevel.CanFitPoint(point)))
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
                    else if(Context.Conflicts(point, LevelNum, type))
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
            if ((ActiveJob == Job.MoveLevel && Context.TestMoveLevel(point)) || (ActiveJob == Job.Move && Context.TestMovePoint(point)))
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
            if (Context.ActiveShapePolygon != null)
                point = Context.ActiveShapePolygon.Constrain(point);
            if (Context.ActiveShapeCircle != null)
                point = Context.ActiveShapeCircle.Constrain(point);
            point = Context.SnapToAPoint(point);
            if (ActiveJob == Job.CreatePolygon || ActiveJob == Job.CreatePoint)
            {
                WorldCanvas_Add_Point(point.ToAbsolutePoint());
            } else if(ActiveJob == Job.Move || ActiveJob == Job.MoveLevel)
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
                        if(LevelNum >= 2 && LevelNum <= 6)
                        {
                            ActiveJob = Job.Create;
                            OpenTextPrompt("Name your " + subtype);
                        }
                    }
                }
            }
            else if(ActiveJob == Job.Move || ActiveJob == Job.MoveLevel)
            {
                if(vertices.Count > 0)
                {
                    if(ActiveJob == Job.Move)
                    {
                        Context.SetVertex(vertices[0]);
                    } 
                    else if(ActiveJob == Job.MoveLevel)
                    {
                        if (Context.SelectedLevel.HasCenterProperty())
                        {
                            try
                            {
                                ((PointLevel)Context.SelectedLevel).center = vertices[0];
                            } catch (InvalidCastException levelcastexception)
                            {
                                OpenPopupAlert("Invalid selected level:\n" + levelcastexception.Message);
                            }
                            
                        }
                        else
                        {
                            OpenPopupAlert("Invalid selected level");
                        }
                    }
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

            if (Context.ActiveLevel.parent != null)
                Level_Back.IsEnabled = true;
            else
                Level_Back.IsEnabled = false;
            ResetZoom();
            ForceUpdatePoints();
        }

        private void Shapes_Control_Focus_Click(object sender, RoutedEventArgs e)
        {
            SetActive(Context.SelectedLevel);
            Context.NullSelected();
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

        private void Resize_ActiveCircle(double radius)
        {
            if(Context.SelectedLevel!=null && Context.SelectedLevel.HasRadiusProperty())
            {
                try
                {
                    ((Level5)Context.SelectedLevel).radius = radius;
                    Context.NullSelected();
                    UpdateSaveState();
                }
                catch (InvalidCastException levelcastexception)
                {
                    OpenPopupAlert("Invalid level object; cannot resize\n" + levelcastexception.Message);
                }
            } else
            {
                OpenPopupAlert("Invalid level object; cannot resize");
            }
            ActiveJob = Job.None;
        }

        private void Circles_Control_Resize_Click(object sender, RoutedEventArgs e)
        {
            if (Context.SelectedLevel!=null && Context.SelectedLevel.HasRadiusProperty())
            {
                ActiveJob = Job.Resize;
                OpenTextPrompt("Input new radius for " + Context.SelectedLevel.name);
            }
        }

        private void CirclesControlFlyout_Closed(object sender, object e)
        {
            if (ActiveJob == Job.None)
                Context.NullSelected();
        }

        private void Circles_Control_Rename_Click(object sender, RoutedEventArgs e)
        {
            ActiveJob = Job.Rename;
            OpenTextPrompt("Enter new name for " + Context.SelectedLevel.name + ':');
        }

        private void Circles_Control_Focus_Click(object sender, RoutedEventArgs e)
        {
            SetActive(Context.SelectedLevel);
            Context.NullSelected();
        }

        private void Circles_Control_Delete_Click(object sender, RoutedEventArgs e)
        {
            Context.DeleteSelected();
            UpdateSaveState();
        }

        private void Circles_Control_Move_Click(object sender, RoutedEventArgs e)
        {
            ActiveJob = Job.MoveLevel;
            OpenTapPrompt("Click new position for " + Context.SelectedLevel.name);
        }

        private void PointsControlFlyout_Closed(object sender, object e)
        {
            if (ActiveJob == Job.None)
                Context.NullSelected();
        }

        private void Points_Control_Rename_Click(object sender, RoutedEventArgs e)
        {
            ActiveJob = Job.Rename;
            OpenTextPrompt("Enter new name for " + Context.SelectedLevel.name + ':');
        }

        private void Points_Control_Move_Click(object sender, RoutedEventArgs e)
        {
            ActiveJob = Job.MoveLevel;
            OpenTapPrompt("Click new position for " + Context.SelectedLevel.name);
        }

        private void Points_Control_Delete_Click(object sender, RoutedEventArgs e)
        {
            Context.DeleteSelected();
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
            Context.ClearPoints();
        }

        /// <summary>
        /// Opens the subtype prompt with the given message
        /// </summary>
        private void OpenSubtypePrompt(string message)
        {
            SubtypePromptTab.Text = message;
            SubtypePrompt.Visibility = Visibility.Visible;
        }

        private void Subtype_Prompt_Cancel_Click(object sender, RoutedEventArgs e)
        {
            ActiveJob = Job.None;
            SubtypePrompt.Visibility = Visibility.Collapsed;
            SubtypesFlyout.Hide();
            Context.ClearPoints();
        }

        private void Subtype_Prompt_New_Click(object sender, RoutedEventArgs e)
        {
            OpenTextPrompt("What type of " + Enum.GetName(typeof(LevelType), type) + " " + level_type_name + " are you creating?\nEnter a subtype:");
            SubtypePrompt.Visibility = Visibility.Collapsed;
            SubtypesFlyout.Hide();
        }

        private void Go_After_Subtype()
        {
            switch (LevelNum)
            {
                case 1:
                    ActiveJob = Job.Create;
                    LevelStep = 3;
                    OpenTextPrompt("Name your " + subtype + ":");
                    break;
                case 2:
                    ActiveJob = Job.CreatePolygon;
                    OpenTapPrompt("Enter at least 3 points for the border of your " + subtype);
                    break;
                case 3:
                    ActiveJob = Job.CreatePolygon;
                    OpenTapPrompt("Enter at least 3 points for the border of your " + subtype);
                    break;
                case 4:
                    ActiveJob = Job.CreatePolygon;
                    OpenTapPrompt("Enter at least 3 points for the border of your " + subtype);
                    break;
                case 5:
                    ActiveJob = Job.CreatePoint;
                    OpenTapPrompt("Enter a center for the " + subtype);
                    break;
                case 6:
                    ActiveJob = Job.CreatePoint;
                    OpenTapPrompt("Enter a point for the " + subtype);
                    break;
            }
        }

        private void Select_Subtype_Click(object sender, RoutedEventArgs e)
        {
            if (ActiveJob == Job.Create)
            {
                subtype = ((MenuFlyoutItem)sender).Text.Trim();
                SubtypePrompt.Visibility = Visibility.Collapsed;
                LevelStep++;
                color = Global.Subtypes.GetColor(subtype);
                Go_After_Subtype();
                SubtypesFlyout.Hide();
            }
        }

        /// <summary>
        /// Opens the color picker with the given message
        /// </summary>
        private void OpenColorPicker(string message)
        {
            OpenColorPicker(message, SuperLevel.DefaultColor);
        }

        /// <summary>
        /// Opens the color picker with the given message and to the given color
        /// </summary>
        private void OpenColorPicker(string message, string color)
        {
            ColorPromptTab.Text = message;
            ColorPrompt.Visibility = Visibility.Visible;
            ColorPrompt_ColorPicker.Color = (Windows.UI.Color) Windows.UI.Xaml.Markup.XamlBindingHelper.ConvertValue(typeof(Windows.UI.Color), color);
        }

        private void Color_Prompt_Confirm_Click(object sender, RoutedEventArgs e)
        {
            color = ColorPrompt_ColorPicker.Color.ToString();
            if (ActiveJob == Job.Create)
            {
                this.LevelStep++;
                Context.AddColor(LevelNum, type, subtype, color);
                Go_After_Subtype();
            } else if (ActiveJob == Job.BasicRecolor)
            {
                Context.SetColor(subtype, color);
                UpdateSaveState();
                ActiveJob = Job.None;
            }
            ColorPrompt.Visibility = Visibility.Collapsed;
        }

        private void Color_Prompt_Cancel_Click(object sender, RoutedEventArgs e)
        {
            ActiveJob = Job.None;
            ColorPrompt.Visibility = Visibility.Collapsed;
            Context.ClearPoints();
        }

        private void Recolor_Level_Subtype_Click(object sender, RoutedEventArgs e)
        {
            if(ActiveJob == Job.None)
            {
                subtype = ((MenuFlyoutItem)sender).Text.Trim();
                subtype = subtype.Substring(subtype.IndexOf("Recolor ") + "Recolor ".Length).Trim();
                ActiveJob = Job.BasicRecolor;
                OpenColorPicker("Set the new color for all " + subtype + "s:", Global.Subtypes.GetColor(subtype));
            }
        }

        public void UpdateView_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
