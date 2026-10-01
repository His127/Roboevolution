using Godot;
using System;

public partial class ScreenSwitchButton : Button
{
    
    [Export] string nameScreen;

    public override void _Ready()
    {
        
        Pressed += switchScreen;


    }

    private void switchScreen()
    {
        
        GameManager.Instance.screenManager.SwitchScreen(nameScreen);


    }



}