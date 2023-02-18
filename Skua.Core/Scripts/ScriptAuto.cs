﻿using System.Diagnostics;
using System.Diagnostics.Contracts;
using CommunityToolkit.Mvvm.ComponentModel;
using Skua.Core.Interfaces;
using Skua.Core.Models.Skills;

namespace Skua.Core.Scripts;
public partial class ScriptAuto : ObservableObject, IScriptAuto
{
    public ScriptAuto(
        ILogService logger,
        Lazy<IScriptPlayer> player,
        Lazy<IScriptDrop> drops,
        Lazy<IScriptSkill> skills,
        Lazy<IScriptBoost> boosts,
        Lazy<IScriptOption> options,
        Lazy<IScriptMonster> monsters,
        Lazy<IScriptKill> kill,
        Lazy<IScriptHunt> hunt,
        Lazy<IScriptWait> wait)
    {
        _logger = logger;
        _lazyPlayer = player;
        _lazyDrops = drops;
        _lazySkills = skills;
        _lazyBoosts = boosts;
        _lazyOptions = options;
        _lazyMonsters = monsters;
        _lazyKill = kill;
        _lazyHunt = hunt;
        _lazyWait = wait;
    }
   
    private readonly ILogService _logger;
    private readonly Lazy<IScriptPlayer> _lazyPlayer;
    private readonly Lazy<IScriptDrop> _lazyDrops;
    private readonly Lazy<IScriptSkill> _lazySkills;
    private readonly Lazy<IScriptBoost> _lazyBoosts;
    private readonly Lazy<IScriptOption> _lazyOptions;
    private readonly Lazy<IScriptKill> _lazyKill;
    private readonly Lazy<IScriptMonster> _lazyMonsters;
    private readonly Lazy<IScriptHunt> _lazyHunt;
    private readonly Lazy<IScriptWait> _lazyWait;
    private IScriptPlayer _player => _lazyPlayer.Value;
    private IScriptDrop _drops => _lazyDrops.Value;
    private IScriptSkill _skills => _lazySkills.Value;
    private IScriptBoost _boosts => _lazyBoosts.Value;
    private IScriptOption _options => _lazyOptions.Value;
    private IScriptKill _kill => _lazyKill.Value;
    private IScriptMonster _monsters => _lazyMonsters.Value;
    private IScriptHunt _hunt => _lazyHunt.Value;
    private IScriptWait _wait => _lazyWait.Value;

    [ObservableProperty]
    private bool _isRunning;

    private Task? _autoTask;
    private CancellationTokenSource? _ctsAuto;

    public void StartAutoAttack(string? className = null, ClassUseMode classUseMode = ClassUseMode.Base)
    {
        _ctsAuto = new CancellationTokenSource();
        _DoActionAuto(hunt: false, className, classUseMode);
    }
    
    public void StartAutoHunt(string? className = null, ClassUseMode classUseMode = ClassUseMode.Base)
    {
        _ctsAuto = new CancellationTokenSource();
        _DoActionAuto(hunt: true, className, classUseMode);
    }

    public void Stop()
    {
        if(_ctsAuto is null)
        {
            IsRunning = false;
            return;
        }
        
        _ctsAuto?.Cancel();
        _wait.ForTrue(() => _ctsAuto is null, 20);
        _autoTask?.Dispose();
        IsRunning = false;
    }

    public async ValueTask StopAsync()
    {
        if(_ctsAuto is null)
        {
            IsRunning = false;
            return;
        }
        
        _ctsAuto?.Cancel();
        await _wait.ForTrueAsync(() => _ctsAuto is null, 20);
        _autoTask?.Dispose();
        IsRunning = false;
    }

    private void _DoActionAuto(bool hunt, string? className = null, ClassUseMode classUseMode = ClassUseMode.Base)
    {
        if (_autoTask != null && !_autoTask.IsCompleted)
            return;

        if (!_player.LoggedIn)
            return;

        CheckDropsandBoosts();
        if (className is not null)
            _skills.StartAdvanced(className, true, classUseMode);
        else
            _skills.StartAdvanced(_player.CurrentClass?.Name ?? "Generic", true);

        _autoTask = Task.Run(async () =>
        {
            try
            {
                if (hunt)
                    await _Hunt(_ctsAuto!.Token);
                else
                    await _Attack(_ctsAuto!.Token);
            }
            catch { }
            finally
            {
                _drops.Stop();
                _skills.Stop();
                _boosts.Stop();
                _ctsAuto?.Dispose();
                _ctsAuto = null;
                IsRunning = false;
            }
        });
        IsRunning = true;
    }

    private string _target = "";
    private async Task _Attack(CancellationToken token)
    {
        Trace.WriteLine("Auto attack started.");
        _player.SetSpawnPoint();
        while (!token.IsCancellationRequested)
        {
            if (!_options.AttackWithoutTarget)
                _kill.Monster("*", token);
            try
            {
                await Task.Delay(500, token);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
        Trace.WriteLine("Auto attack stopped.");
    }

    private async Task _Hunt(CancellationToken token)
    {
        Trace.WriteLine("Auto hunt started.");

        if (_player.HasTarget)
            _target = _player.Target?.Name ?? "*";
        else
        {
            List<string> monsters = _monsters.CurrentMonsters.Select(m => m.Name).ToList();
            _target = string.Join('|', monsters);
        }

        _logger.ScriptLog($"[Auto Hunt] Hunting for {_target}");
        while (!token.IsCancellationRequested)
        {
            _hunt.Monster(_target, token);
            try
            {
                await Task.Delay(500, token);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
        Trace.WriteLine("Auto hunt stopped.");
    }

    private void CheckDropsandBoosts()
    {
        if (_drops.ToPickup.Any())
            _drops.Start();

        if (_boosts.UsingBoosts)
            _boosts.Start();
    }
}
