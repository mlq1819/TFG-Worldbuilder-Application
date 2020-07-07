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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TFG_Worldbuilder_Application
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MapPage : Page
    {

        public MapPage()
        {
            this.InitializeComponent();
            this.FileNameBlock.Text = Global.ActiveFile.FileName();
        }

        /// <summary>
        /// Warns the user that their current file is unsaved, and asks if they'd like to either save it, lose the changes, or cancel the operation. Returns false on cancelled operation.
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
                    Global.ActiveFile = new FileManager(file);
                    Global.ActiveFile.FormatNewFile();
                    this.Frame.Navigate(typeof(MapPage));
                }
            }
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
                    Global.ActiveFile = new FileManager(file);
                    this.Frame.Navigate(typeof(MapPage));
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
                Global.ActiveFile = new FileManager(file);
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
        
    }
}
