// The MIT License (MIT)
// 
// Copyright (c) 2014 Rodrigo 'r2d2rigo' Diaz
// Portions of this code Copyright (c) 2010-2013 Alexandre Mutel
//
// See LICENSE for full license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using SharpDX.SimpleInitializer;
using SampleInitializer.Samples.Common;

namespace SimpleInitializer.Samples.WP8
{
    public partial class DrawingSurfaceBackgroundGridPage : PhoneApplicationPage
    {
        private SharpDXContext context;
        private SceneRenderer renderer;

        public DrawingSurfaceBackgroundGridPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            this.context = new SharpDXContext();
            this.renderer = new SceneRenderer(this.context);
            this.context.Render += context_Render;

            this.context.BindToControl(this.LayoutRoot);
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            this.context.Render -= context_Render;
            this.renderer.Dispose();
            this.context.Dispose();
        }

        void context_Render(object sender, System.EventArgs e)
        {
            this.renderer.Render();
        }
    }
}