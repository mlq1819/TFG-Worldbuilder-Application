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
        public Level2Subtypes Level2
        {
            get
            {
                return (Level2Subtypes)GetValue(level2property);
            }
            set
            {
                SetValue(level2property, (Level2Subtypes)value);
            }
        }
        public Level3Subtypes Level3
        {
            get
            {
                return (Level3Subtypes)GetValue(level3property);
            }
            set
            {
                SetValue(level3property, (Level3Subtypes)value);
            }
        }
        public Level4Subtypes Level4
        {
            get
            {
                return (Level4Subtypes)GetValue(level4property);
            }
            set
            {
                SetValue(level4property, (Level4Subtypes)value);
            }
        }
        public Level5Subtypes Level5
        {
            get
            {
                return (Level5Subtypes)GetValue(level5property);
            }
            set
            {
                SetValue(level5property, (Level5Subtypes)value);
            }
        }
        public Level6Subtypes Level6
        {
            get
            {
                return (Level6Subtypes)GetValue(level6property);
            }
            set
            {
                SetValue(level6property, (Level6Subtypes)value);
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
        public string Enabled
        {
            get
            {
                if (Level2.Count + Level3.Count + Level4.Count + Level5.Count + Level6.Count > 0)
                    return "True";
                return "False";
            }
        }

        public ViewMenuControl()
        {
            this.InitializeComponent();
        }

        public static readonly DependencyProperty level2property = DependencyProperty.Register("Level2", typeof(Level2Subtypes), typeof(ViewMenuControl), new PropertyMetadata(0));
        public static readonly DependencyProperty level3property = DependencyProperty.Register("Level3", typeof(Level3Subtypes), typeof(ViewMenuControl), new PropertyMetadata(0));
        public static readonly DependencyProperty level4property = DependencyProperty.Register("Level4", typeof(Level4Subtypes), typeof(ViewMenuControl), new PropertyMetadata(0));
        public static readonly DependencyProperty level5property = DependencyProperty.Register("Level5", typeof(Level5Subtypes), typeof(ViewMenuControl), new PropertyMetadata(0));
        public static readonly DependencyProperty level6property = DependencyProperty.Register("Level6", typeof(Level6Subtypes), typeof(ViewMenuControl), new PropertyMetadata(0));
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
