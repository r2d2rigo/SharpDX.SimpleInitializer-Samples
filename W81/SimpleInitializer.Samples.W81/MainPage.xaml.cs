// The MIT License (MIT)
// 
// Copyright (c) 2014 Rodrigo 'r2d2rigo' Diaz
// Portions of this code Copyright (c) 2010-2013 Alexandre Mutel
//
// See LICENSE for full license.

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

namespace SimpleInitializer.Samples.W81
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        internal static Frame applicationFrame;

        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);

            if (e.NavigationMode == NavigationMode.Back)
            {
                Window.Current.Content = this.Frame;
                Window.Current.Activate();
            }
        }

        private void SwapChainPanel_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(SwapChainPanelPage));
        }

        private void SwapChainBackgroundPanel_Click(object sender, RoutedEventArgs e)
        {
            applicationFrame = this.Frame;

            Window.Current.Content = new SwapChainBackgroundPanelPage();
            Window.Current.Activate();
        }
    }
}
