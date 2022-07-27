﻿using Microsoft.Toolkit.Mvvm.ComponentModel;
using Skua.Core.Interfaces;
using Skua.Core.Models.Skills;
using Skua.Core.Utils;

namespace Skua.Core.Skills;

public class AdvancedSkillContainer : ObservableRecipient, IAdvancedSkillContainer
{
    private List<AdvancedSkill> _loadedSkills = new();
    public List<AdvancedSkill> LoadedSkills
    {
        get { return _loadedSkills; }
        set { SetProperty(ref _loadedSkills, value, true); }
    }

    public AdvancedSkillContainer()
    {
        LoadSkills();
    }

    public void Add(AdvancedSkill skill)
    {
        _loadedSkills.Add(skill);
        Save();
    }
    public void Remove(AdvancedSkill skill)
    {
        _loadedSkills.Remove(skill);
        Save();
    }
    public void TryOverride(AdvancedSkill skill)
    {
        if (!_loadedSkills.Contains(skill))
        {
            _loadedSkills.Add(skill);
            Save();
            return;
        }

        int index = _loadedSkills.IndexOf(skill);
        _loadedSkills[index] = skill;
        Save();
    }

    public void LoadSkills()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "AdvancedSkills.txt");
        if (!File.Exists(path))
            return;

        LoadedSkills.Clear();
        foreach (string line in File.ReadAllLines(path))
        {
            string[] parts = line.Split(new[] { '=' }, 4);
            if (parts.Length == 3)
                _loadedSkills.Add(new AdvancedSkill(parts[1].Trim(), parts[2].Trim(), 250, parts[0].Trim(), "WaitForCooldown"));
            else if (parts.Length == 4)
            {
                bool useIfAvailble = int.TryParse(parts[3].RemoveLetters(), out int result);
                _loadedSkills.Add(new AdvancedSkill(parts[1].Trim(), parts[2].Trim(), useIfAvailble ? 250 : result, parts[0].Trim(), useIfAvailble ? SkillUseMode.UseIfAvailable : SkillUseMode.WaitForCooldown));
            }
        }
        OnPropertyChanged(nameof(LoadedSkills));
        Broadcast(new(), _loadedSkills, nameof(LoadedSkills));
    }

    public void Save()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "AdvancedSkills.txt");
        File.WriteAllLines(path, _loadedSkills.OrderBy(s => s.ClassName).Select(s => s.SaveString));
        LoadSkills();
    }
}
