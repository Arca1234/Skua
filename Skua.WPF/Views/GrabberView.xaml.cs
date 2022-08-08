﻿using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;
using CommunityToolkit.Mvvm.DependencyInjection;
using Skua.Core.ViewModels;

namespace Skua.WPF.Views;
/// <summary>
/// Interaction logic for GrabberUserControl.xaml
/// </summary>
public partial class GrabberView : UserControl
{
    public GrabberView()
    {
        InitializeComponent();
        DataContext = Ioc.Default.GetService<GrabberViewModel>();
    }
}
