using Godot;
using System;
using System.Collections.Generic;

public partial class ScreenManager : Node{

	[Export] ScreenEntry[] screenEntries;

	public Node ScreenNode;

	private Dictionary<string, PackedScene> availableScreen = new();

    public override void _Ready(){
   
   		foreach(ScreenEntry screenEntry in screenEntries){
			availableScreen[screenEntry.Key] = screenEntry.Screen;

		}
   
    }





	




}
