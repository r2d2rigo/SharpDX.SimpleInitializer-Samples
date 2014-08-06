// The MIT License (MIT)
// 
// Copyright (c) 2014 Rodrigo 'r2d2rigo' Diaz
// Portions of this code Copyright (c) 2010-2013 Alexandre Mutel
//
// See LICENSE for full license.

using SampleInitializer.Samples.Common;
using SharpDX.SimpleInitializer;
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
    public sealed partial class SwapChainBackgroundPanelPage : Page
    {
        private SharpDXContext context;
        private SceneRenderer renderer;

        public SwapChainBackgroundPanelPage()
        {
            this.InitializeComponent();

            this.context = new SharpDXContext();
            this.renderer = new SceneRenderer(this.context);
            this.context.BindToControl(this.LayoutRoot);

            this.context.Render += context_Render;
        }

        void context_Render(object sender, System.EventArgs e)
        {
            this.renderer.Render();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.context.Render -= context_Render;
            this.renderer.Dispose();
            this.context.Dispose();

            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);

            Window.Current.Content = MainPage.applicationFrame;
            Window.Current.Activate();
        }
    }
}
