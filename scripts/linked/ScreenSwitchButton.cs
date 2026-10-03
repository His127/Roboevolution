using Godot;
using System;

public partial class ScreenSwitchButton : Button
{
    
    [Export] 
    private string ScreenName;

    [Export]
    private bool IsOverlay = true;

    public override void _Ready() {
        Pressed += switchScreen;
    }

    private void switchScreen() {
        if(!IsOverlay) GameManager.Instance.screenManager.SwitchScreen(ScreenName);
        else GameManager.Instance.screenManager.ToggleOverlay(ScreenName);
    }



}