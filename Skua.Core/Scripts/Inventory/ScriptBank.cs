﻿using Skua.Core.Interfaces;
using Skua.Core.Models.Items;
using Skua.Core.Flash;

namespace Skua.Core.Scripts;
public partial class ScriptBank : IScriptBank
{
    private readonly Lazy<IFlashUtil> _lazyFlash;
    private readonly Lazy<IScriptSend> _lazySend;
    private readonly Lazy<IScriptMap> _lazyMap;
    private readonly Lazy<IScriptWait> _lazyWait;
    private readonly Lazy<IScriptInventory> _lazyInventory;
    private readonly Lazy<IScriptOption> _lazyOptions;
    private readonly Lazy<IScriptManager> _lazyManager;
    private readonly Lazy<IScriptPlayer> _lazyPlayer;

    private IFlashUtil Flash => _lazyFlash.Value;
    private IScriptSend Send => _lazySend.Value;
    private IScriptMap Map => _lazyMap.Value;
    private IScriptWait Wait => _lazyWait.Value;
    private IScriptInventory Inventory => _lazyInventory.Value;
    private IScriptOption Options => _lazyOptions.Value;
    private IScriptManager Manager => _lazyManager.Value;
    private IScriptPlayer Player => _lazyPlayer.Value;

    public ScriptBank(
        Lazy<IFlashUtil> flash,
        Lazy<IScriptSend> send,
        Lazy<IScriptMap> map,
        Lazy<IScriptWait> wait,
        Lazy<IScriptInventory> inventory,
        Lazy<IScriptOption> options,
        Lazy<IScriptManager> manager,
        Lazy<IScriptPlayer> player)
    {
        _lazyFlash = flash;
        _lazySend = send;
        _lazyMap = map;
        _lazyWait = wait;
        _lazyInventory = inventory;
        _lazyOptions = options;
        _lazyManager = manager;
        _lazyPlayer = player;
    }
    public bool Loaded { get; set; } = false;
    [ObjectBinding("world.bankinfo.items", Default = "new()")]
    private List<InventoryItem> _items;
    [ObjectBinding("world.myAvatar.objData.iBankSlots")]
    private int _slots;
    [ObjectBinding("world.myAvatar.iBankCount")]
    private int _usedSlots;

    [MethodCallBinding("world.toggleBank", GameFunction = true)]
    public void _open() { }

    public void Load(bool waitForLoad = true)
    {
        Send.Packet($"%xt%zm%loadBank%{Map.RoomID}%All%");
        if (waitForLoad)
            Wait.ForBankLoad(20);
    }

    public bool Swap(string invItem, string bankItem)
    {
        if (((IScriptBank)this).TryGetItem(bankItem, out InventoryItem? bank) && Inventory.TryGetItem(invItem, out InventoryItem? inv))
            Swap(bank!, inv!);
        return false;
    }

    public bool Swap(int invItem, int bankItem)
    {
        if (((IScriptBank)this).TryGetItem(bankItem, out InventoryItem? bank) && Inventory.TryGetItem(invItem, out InventoryItem? inv))
            return Swap(bank!, inv!);
        return false;
    }

    public bool Swap(InventoryItem bankItem, InventoryItem invItem)
    {
        Send.Packet($"%xt%zm%bankSwapInv%{Map.RoomID}%{invItem.ID}%{invItem.CharItemID}%{bankItem.ID}%{bankItem.CharItemID}%");
        if (Options.SafeTimings)
            Wait.ForInventoryToBank(invItem.ID);
        return Inventory.Contains(bankItem.ID);
    }

    public void EnsureSwap(string invItem, string bankItem, bool loadBank = true)
    {
        if (loadBank)
            Load();
        int i = 0;
        while (!Swap(invItem, bankItem) && !Manager.ShouldExit && Player.Playing && ++i < Options.MaximumTries)
            Thread.Sleep(Options.ActionDelay);
    }

    public void EnsureSwap(int invItem, int bankItem, bool loadBank = true)
    {
        if (loadBank)
            Load();
        int i = 0;
        while (!Swap(invItem, bankItem) && !Manager.ShouldExit && Player.Playing && ++i < Options.MaximumTries)
            Thread.Sleep(Options.ActionDelay);
    }

    public bool ToInventory(InventoryItem item)
    {
        Send.Packet($"%xt%zm%bankToInv%{Map.RoomID}%{item.ID}%{item.CharItemID}%");
        if (Options.SafeTimings)
            Wait.ForBankToInventory(item!.Name);
        return Inventory.Contains(item.ID);
    }

    public void EnsureToInventory(string item, bool loadBank = true)
    {
        if (loadBank)
            Load();
        int i = 0;
        while (!((IScriptBank)this).ToInventory(item) && !Manager.ShouldExit && Player.Playing && ++i < Options.MaximumTries)
            Thread.Sleep(Options.ActionDelay);
    }

    public void EnsureToInventory(int id, bool loadBank = true)
    {
        if (loadBank)
            Load();
        int i = 0;
        while (!((IScriptBank)this).ToInventory(id) && !Manager.ShouldExit && Player.Playing && ++i < Options.MaximumTries)
            Thread.Sleep(Options.ActionDelay);
    }
}
