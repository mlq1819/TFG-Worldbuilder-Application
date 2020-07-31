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
        public ActiveContext Context
        {
            get
            {
                return (ActiveContext)GetValue(contextproperty);
            }
            set
            {
                SetValue(contextproperty, (ActiveContext)value);
            }
        }

        public ViewMenuControl()
        {
            this.InitializeComponent();
        }

        public static readonly DependencyProperty contextproperty = DependencyProperty.Register("Context", typeof(ActiveContext), typeof(ViewMenuControl), new PropertyMetadata(0));
    }
}
