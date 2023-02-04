﻿using Skua.Core.Utils;

namespace Skua.Core.ViewModels;
public class AboutViewModel : BotControlViewModelBase
{
    private string _markDownContent = "Loading content...";
    
    public AboutViewModel() : base("About")
    {
        _markDownContent = string.Empty;
    }

    protected override void OnActivated()
    {
        GetAboutContent();
    }

    private async Task GetAboutContent()
    {
        var response = await HttpClients.Default.GetAsync("https://raw.githubusercontent.com/BrenoHenrike/Skua/op-version/about.md");
        if (!response.IsSuccessStatusCode)
            MarkdownDoc = "### No content found. Please check your internet connection.";

        MarkdownDoc = await response.Content.ReadAsStringAsync();
    }
    
    public string MarkdownDoc
    {
        get { return _markDownContent; }
        set { SetProperty(ref _markDownContent, value); }
    }
}
