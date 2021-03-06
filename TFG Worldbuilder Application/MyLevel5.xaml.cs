﻿using System;
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
    public sealed partial class MyLevel5 : UserControl
    {
        public Level5 Level
        {
            get
            {
                return (Level5)GetValue(levelproperty);
            }
            set
            {
                SetValue(levelproperty, (Level5)value);
            }
        }

        public Object StrokeThickness
        {
            get
            {
                return GetValue(strokethicknessproperty);
            }
            set
            {
                SetValue(strokethicknessproperty, value);
            }
        }

        public string Stroke
        {
            get
            {
                return (string)GetValue(strokeproperty);
            }
            set
            {
                SetValue(strokeproperty, value);
            }
        }

        public MyLevel5()
        {
            this.InitializeComponent();
        }

        public static readonly DependencyProperty levelproperty = DependencyProperty.Register("Level", typeof(Level5), typeof(MyLevel5), new PropertyMetadata(0));
        public static readonly DependencyProperty strokethicknessproperty = DependencyProperty.Register("StrokeThickness", typeof(uint), typeof(MyLevel5), new PropertyMetadata(0));
        public static readonly DependencyProperty strokeproperty = DependencyProperty.Register("Stroke", typeof(Object), typeof(MyLevel5), new PropertyMetadata(0));
    }
}
