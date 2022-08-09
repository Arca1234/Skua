﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging.Messages;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Input;
using Skua.Core.Interfaces;

namespace Skua.Core.ViewModels;
public partial class ToPickupDropsViewModel : ObservableObject
{
    private readonly char[] _dropsSeparator = { '|' };
    public ToPickupDropsViewModel(IScriptDrop drops, IScriptOption options)
    {
        WeakReferenceMessenger.Default.Register<ToPickupDropsViewModel, PropertyChangedMessage<IEnumerable<string>>>(this, ToPickupChanged);

        Drops = drops;
        Options = options;
        RemoveAllDropsCommand = new RelayCommand(Drops.Clear);
    }

    [ObservableProperty]
    private string _addDropInput = string.Empty;

    public List<string> ToPickup => Drops.ToPickup.ToList();
    public IScriptDrop Drops { get; }
    public IScriptOption Options { get; }
    public IRelayCommand RemoveAllDropsCommand { get; }

    [RelayCommand]
    private void RemoveDrops(IList<object>? items)
    {
        if (items is null)
            return;
        IEnumerable<string> drops = items.Cast<string>();
        if (drops.Any())
            Drops.Remove(drops.ToArray());
    }

    [RelayCommand]
    private async Task ToggleDrops()
    {
        if (Drops.Enabled)
            await Drops.StopAsync();
        else
            Drops.Start();
    }

    [RelayCommand]
    private void AddDrop()
    {
        if (string.IsNullOrWhiteSpace(AddDropInput))
            return;

        IEnumerable<string> drops = AddDropInput.Split(_dropsSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (drops.Any())
            Drops.Add(drops.ToArray());

        AddDropInput = string.Empty;
    }

    private void ToPickupChanged(ToPickupDropsViewModel recipient, PropertyChangedMessage<IEnumerable<string>> message)
    {
        if (message.PropertyName == nameof(IScriptDrop.ToPickup))
            recipient.OnPropertyChanged(nameof(recipient.ToPickup));
    }
}
