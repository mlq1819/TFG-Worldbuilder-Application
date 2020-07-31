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

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace TFG_Worldbuilder_Application
{
    public sealed partial class ViewMenuControl : UserControl
    {
        public SubtypesContainer Levels
        {
            get
            {
                return (SubtypesContainer)GetValue(levelsproperty);
            }
            set
            {
                SetValue(levelsproperty, (SubtypesContainer)value);
            }
        }
        public Level2Subtypes Level2
        {
            get
            {
                return Levels.Level2;
            }
        }
        public Level3Subtypes Level3
        {
            get
            {
                return Levels.Level3;
            }
        }
        public Level4Subtypes Level4
        {
            get
            {
                return Levels.Level4;
            }
        }
        public Level5Subtypes Level5
        {
            get
            {
                return Levels.Level5;
            }
        }
        public Level6Subtypes Level6
        {
            get
            {
                return Levels.Level6;
            }
        }

        public string SubMenuName
        {
            get
            {
                return (string)GetValue(submenunameproperty);
            }
            set
            {
                SetValue(submenunameproperty, (string)value);
            }
        }

        public ViewMenuControl()
        {
            this.InitializeComponent();
        }

        public static readonly DependencyProperty levelsproperty = DependencyProperty.Register("Levels", typeof(SubtypesContainer), typeof(ViewMenuControl), new PropertyMetadata(0));
        public static readonly DependencyProperty submenunameproperty = DependencyProperty.Register("SubMenuName", typeof(string), typeof(ViewMenuControl), new PropertyMetadata(0));

        private void UpdateView_Click(object sender, RoutedEventArgs e)
        {
            if(Global.mappage!=null)
            {
                Global.mappage.UpdateView_Click(sender, e);
            }
        }
    }
}
