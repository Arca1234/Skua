﻿using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Skua.Core.ViewModels;

namespace Skua.WPF.Views;
/// <summary>
/// Interaction logic for ScriptRepoView.xaml
/// </summary>
public partial class ScriptRepoView : UserControl
{
    ICollectionView _collectionView;
    public ScriptRepoView()
    {
        InitializeComponent();
        DataContext = Ioc.Default.GetService<ScriptRepoViewModel>()!;
        _collectionView = CollectionViewSource.GetDefaultView(((ScriptRepoViewModel)DataContext).Scripts);
        _collectionView.Filter = Search;
    }

    private bool Search(object obj)
    {
        if(string.IsNullOrWhiteSpace(SearchBox.Text))
            return true;

        return obj is ScriptInfoViewModel script && script.FileName.Contains(SearchBox.Text);
    }

    private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        _collectionView.Refresh();
    }
}
